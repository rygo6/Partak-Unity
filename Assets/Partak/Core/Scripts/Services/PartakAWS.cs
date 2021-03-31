using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Util;
using GeoTetra.GTCommon.Extensions;
using GeoTetra.GTPooling;
using GeoTetra.Partak;
using UnityEngine;
using UnityEngine.Networking;

namespace GeoTetra.GTBackend
{
    [Serializable]
    public class PartakAWSRef : ServiceObjectReferenceT<PartakAWS>
    {
        public PartakAWSRef(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/PartakAWS")]
    public class PartakAWS : ServiceObject
    {
        // [SerializeField] private string _accessKeyId = "";
        // [SerializeField] private string _secretAccessKey = "";

        [SerializeField] private string _regionEndpoint  = "us-west-2";
        [SerializeField] private string _tableName = "Partak";
        [SerializeField] private string _s3Bucket = "partak";
        [SerializeField] private string _levelImagesFolder = "LevelImages";
        [SerializeField] private string _createdAtIndex = "pk-created_at-index";
        [SerializeField] private string _thumbsUpIndex = "pk-thumbs_up-index";
        [SerializeField] private string _aggregateThumbIndex = "pk-thumbs_aggregate-index";
        
        public static class LevelFields
        {
            public const string PkKey = "pk";
            public const string IdKey = "id";
            public const string AuthorKey = "author";
            public const string LevelDataKey = "level_data";
            public const string ThumbsUpKey = "thumbs_up";
            public const string AggregateThumbKey = "thumbs_aggregate"; 
            public const string ThumbsDownKey = "thumbs_down";
            public const string CreatedAtKey = "created_at";
            public const string DownloadedCountKey = "download_count";
            public const string DeleteCountKey = "delete_count";
            public const string PlayCountKey = "play_count";
            public const string LastPlayedKey = "last_played";
            public const string PkLevelValue = "level";
        } 
        
        private Table _table;
        private AmazonS3Client _s3Client;
        private AmazonDynamoDBClient _dbClient;
        private TransferUtility _transferUtility;
        private string _accessKeyId = "";
        private string _secretAccessKey = "";

        protected override async Task OnServiceStart()
        {
            await Connect();
            await base.OnServiceStart();
        }
        
        protected override void OnServiceEnd()
        {
            _table = null;
            _s3Client = null;
            _dbClient = null;
            _transferUtility = null;
            base.OnServiceEnd();
        }

        /// <summary>
        /// Connect is called before every operation in order to allow lazy initialization of the AWS calls.
        /// </summary>
        [ContextMenu("Connect")]
        private async Task Connect()
        {
            if (_dbClient == null || _table == null || _s3Client == null || _transferUtility == null)
            {
                Debug.Log("Connecting to AWS");
                LoggingConfig loggingConfig = AWSConfigs.LoggingConfig;
                loggingConfig.LogTo = LoggingOptions.Console;
                loggingConfig.LogMetrics = true;
                loggingConfig.LogResponses = ResponseLoggingOption.Always;
                loggingConfig.LogResponsesSizeLimit = 4096;
                loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;

                try
                {
                    RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(_regionEndpoint);
                    
                    UnityWebRequest request = UnityWebRequest.Get("https://qyqo9ud4hi.execute-api.us-west-2.amazonaws.com/PartakKey");
                    request.SetRequestHeader("Authorization", "PartakKey218931987391793791392793");
                    await request.SendWebRequest();
                    string encrypted = request.downloadHandler.text;
                    string decrypted = Encryption.DecryptStringAES(encrypted);
                    string[] decryptedSplit = decrypted.Replace("\"", "").Split('-');
                    _accessKeyId = decryptedSplit[0];
                    _secretAccessKey = decryptedSplit[1];
                    
                    _dbClient = new AmazonDynamoDBClient(_accessKeyId, _secretAccessKey, endpoint);
                    Debug.Log($"AmazonDynamoDBClient Success");
                    _table = Table.LoadTable(_dbClient, _tableName);
                    Debug.Log($"{_tableName} Table Success");
                    _s3Client = new AmazonS3Client(_accessKeyId, _secretAccessKey, endpoint);
                    _transferUtility = new TransferUtility(_s3Client);
                    Debug.Log($"{_transferUtility} Success {GetInstanceID()}");
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
        }

        public async Task DownloadLevel(string levelId)
        {
            // await Connect();
            string levelPath = LevelUtility.LevelPath(levelId);
            string imagePath = LevelUtility.LevelImagePath(levelId);

            Debug.Log("Downloading datum  " + levelPath);
            GetItemOperationConfig config = new GetItemOperationConfig();
            Document result = await _table.GetItemAsync(LevelFields.PkLevelValue, levelId, config);

            LocalLevelDatum levelDatum = JsonUtility.FromJson<LocalLevelDatum>(result[LevelFields.LevelDataKey]);
            // Set shared and downloaded to true, because it was downloaded, and obviously was shared.
            levelDatum.Shared = true;
            levelDatum.Downloaded = true;
            levelDatum.LevelID = levelId;
            string json = JsonUtility.ToJson(levelDatum);

            File.WriteAllText(levelPath, json);
            
            Debug.Log("Downloading image  " + imagePath);
            string imageKey = S3LevelImageKey(levelId);
            await _transferUtility.DownloadAsync(imagePath, _s3Bucket, imageKey);
        }
        
        public async Task<Search> QueryLevelsCreatedAt(int pageSize)
        {
            await Connect();
            return QueryLevels(pageSize, _createdAtIndex);
        }

        public async Task<Search> QueryLevelsThumbsUp(int pageSize)
        {
            await Connect();
            return QueryLevels(pageSize, _aggregateThumbIndex);
        }
        
        private Search QueryLevels(int pageSize, string index)
        {
            if (_table == null) return null;

            QueryFilter filter = new QueryFilter(LevelFields.PkKey, QueryOperator.Equal, LevelFields.PkLevelValue);

            QueryOperationConfig config = new QueryOperationConfig()
            {
                Limit = pageSize, 
                Select = SelectValues.SpecificAttributes,
                AttributesToGet = new List<string> { LevelFields.IdKey, LevelFields.ThumbsUpKey, LevelFields.ThumbsDownKey },
                ConsistentRead = false,
                Filter = filter,
                IndexName = index,
                BackwardSearch = true,
            };
            
            return _table.Query(config);
        }
        
        public async Task DownloadLevelPreview(string id, Texture2D image)
        {
            await Connect();
            string key = S3LevelImageKey(id);
            byte[] bytes = await GetImageBytesFromS3(key);
            image.LoadImage(bytes, true);
        }
        
        private async Task<byte[]> GetImageBytesFromS3(string key)
        {
            return await Task.Run( async () =>
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _s3Bucket,
                    Key = key
                };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (Stream responseStream = response.ResponseStream)
                    {
                        await responseStream.CopyToAsync(memoryStream);
                    }

                    return memoryStream.ToArray();
                }
            });
        }

        private async Task<byte[]> GetImageBytesFromS3(string key, CancellationToken ct)
        {
            return await Task.Run( async () =>
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _s3Bucket,
                    Key = key
                };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(request, ct))
                {
                    ct.ThrowIfCancellationRequested();
                    
                    MemoryStream memoryStream = new MemoryStream();
                    using (Stream responseStream = response.ResponseStream)
                    {
                        await responseStream.CopyToAsync(memoryStream);
                    }
                    
                    ct.ThrowIfCancellationRequested();
                    return memoryStream.ToArray();
                }
            }, ct);
        }

        public async Task SaveLevel(string levelId)
        {
            // await Connect();
            
            Debug.Log("Saving level " + levelId);
            string levelPath = LevelUtility.LevelPath(levelId);
            string imagePath = LevelUtility.LevelImagePath(levelId);
            string json = File.ReadAllText(levelPath);

            Document level = new Document
            {
                [LevelFields.PkKey] = "level",
                [LevelFields.IdKey] = levelId,
                [LevelFields.AuthorKey] = SystemInfo.deviceUniqueIdentifier,
                [LevelFields.LevelDataKey] = json,
                [LevelFields.ThumbsUpKey] = 0,
                [LevelFields.ThumbsDownKey] = 0,
                [LevelFields.CreatedAtKey] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                [LevelFields.DownloadedCountKey] = 0,
                [LevelFields.DeleteCountKey] = 0,
                [LevelFields.PlayCountKey] = 0,
                [LevelFields.LastPlayedKey] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            try
            {
                Debug.Log("Saving to Dynamo " + levelId);
                await _table.PutItemAsync(level);
                Debug.Log("Success");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

            try
            {
                Debug.Log("Saving to S3 " + levelId);
                await _transferUtility.UploadAsync(imagePath, _s3Bucket, S3LevelImageKey(levelId));
                Debug.Log("Success");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        public async Task IncrementThumbsUp(string levelID)
        {
            AtomicIncrement(levelID, LevelFields.ThumbsUpKey, true);
            AtomicIncrement(levelID, LevelFields.AggregateThumbKey, true);
        }
        
        public async Task IncrementThumbsDown(string levelID)
        {
            AtomicIncrement(levelID, LevelFields.ThumbsDownKey, true);
            AtomicIncrement(levelID, LevelFields.AggregateThumbKey, false);
        }
        
        public async void UpdateAggregate(string levelId, int value)
        {
#if !UNITY_EDITOR
            return; // I will just run the editor periodically until new build is ubiquitis
#endif
            
            Debug.Log($"Updating aggregate {levelId} {value}" );
            
            var request = new UpdateItemRequest
            {
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "pk", new AttributeValue { S = LevelFields.PkLevelValue } },
                    { "id", new AttributeValue { S = levelId } },
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#F", LevelFields.AggregateThumbKey}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":val",new AttributeValue {N = value.ToString() }}
                },
                UpdateExpression = "SET #F = :val",
                TableName = _tableName
            };

            try
            {
                UpdateItemResponse response = await _dbClient.UpdateItemAsync(request);
                Debug.Log($"Updating aggregate Response: {response}");
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private async void AtomicIncrement(string levelId, string field, bool positive)
        {
            Debug.Log($"Atomically Incrementing {levelId} {field} {positive}");
            
            var request = new UpdateItemRequest
            {
                Key = new Dictionary<string, AttributeValue>()
                {
                    { "pk", new AttributeValue { S = LevelFields.PkLevelValue } },
                    { "id", new AttributeValue { S = levelId } },
                },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#F", field}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":incr",new AttributeValue {N = "1" }}
                },
                UpdateExpression = positive ? "SET #F = #F + :incr" : "SET #F = #F - :incr",
                TableName = _tableName
            };

            try
            {
                UpdateItemResponse response = await _dbClient.UpdateItemAsync(request);
                Debug.Log($"AtomicIncrement Response: {response}");
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private string S3LevelImageKey(string id)
        {
            return $"{_levelImagesFolder}/{id}.png";
        }
        
        [ContextMenu("GenerateIvKey")]
        private void GenerateIvKey()
        {
            try
            {
                using (RijndaelManaged rijndael = new RijndaelManaged())
                {
                    rijndael.Mode = CipherMode.CBC;
                    rijndael.Padding = PaddingMode.PKCS7;
                    rijndael.FeedbackSize = 128;
                    rijndael.GenerateKey();
                    rijndael.GenerateIV();
                    Debug.Log(rijndael.Key.Length);
                    Debug.Log(rijndael.IV.Length);

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("{");
                    Debug.Log("Key");
                    for (int i = 0; i < rijndael.Key.Length; ++i)
                    {
                        stringBuilder.Append(rijndael.Key[i]);
                        if (i != rijndael.Key.Length -1) stringBuilder.Append(",");
                    }
                    stringBuilder.Append("}");
                    Debug.Log(stringBuilder.ToString());
                    Debug.Log(new SoapHexBinary(rijndael.Key).ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append("{");
                    Debug.Log("IV");
                    for (int i = 0; i < rijndael.IV.Length; ++i)
                    {
                        stringBuilder.Append(rijndael.IV[i]);
                        if (i != rijndael.IV.Length -1) stringBuilder.Append(",");
                    }
                    stringBuilder.Append("}");
                    Debug.Log(stringBuilder.ToString());
                    Debug.Log(new SoapHexBinary(rijndael.IV).ToString());
                    
                    // string key = Encoding.UTF8.GetString(rijndael.Key);
                    // string iv = Encoding.UTF8.GetString(rijndael.IV);
                    //
                    // Debug.Log($"key: {ToLiteral(key)}");
                    // Debug.Log($"iv: {ToLiteral(iv)}");
                    // byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    // byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
                    // Debug.Log(keyBytes.Length);
                    // Debug.Log(ivBytes.Length);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        }
        
        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}


public class Encryption {
    public static string DecryptStringAES(string cipherText)
    {
        byte[] keybytes = {233,183,206,63,183,107,201,0,208,76,37,194,66,43,199,70,1,29,155,50,154,21,247,44,229,105,212,88,68,34,146,183};
        byte[] iv = {47,16,90,33,110,27,57,223,77,48,98,236,250,218,217,179};

        var encrypted = Convert.FromBase64String(cipherText);
        var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
        return decriptedFromJavascript;
    }
    private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv) {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0) {
            throw new ArgumentNullException("cipherText");
        }
        if (key == null || key.Length <= 0) {
            throw new ArgumentNullException("key");
        }
        if (iv == null || iv.Length <= 0) {
            throw new ArgumentNullException("key");
        }

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an RijndaelManaged object
        // with the specified key and IV.
        using(var rijAlg = new RijndaelManaged()) {
            //Settings
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            rijAlg.FeedbackSize = 128;

            rijAlg.Key = key;
            rijAlg.IV = iv;

            // Create a decrytor to perform the stream transform.
            var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

            try {
                // Create the streams used for decryption.
                using(var msDecrypt = new MemoryStream(cipherText)) {
                    using(var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {

                        using(var srDecrypt = new StreamReader(csDecrypt)) {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();

                        }

                    }
                }
            } catch {
                plaintext = "keyError";
            }
        }

        return plaintext;
    }

    public static string EncryptStringAES(string plainText) {
        var keybytes = Encoding.UTF8.GetBytes("[�/V)����e�L`��\t�����>g�6�");
        var iv = Encoding.UTF8.GetBytes("{1�\"���{ܐek^�");

        var encryoFromJavascript = EncryptStringToBytes(plainText, keybytes, iv);
        return Convert.ToBase64String(encryoFromJavascript);
    }

    private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv) {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0) {
            throw new ArgumentNullException("plainText");
        }
        if (key == null || key.Length <= 0) {
            throw new ArgumentNullException("key");
        }
        if (iv == null || iv.Length <= 0) {
            throw new ArgumentNullException("key");
        }
        byte[] encrypted;
        // Create a RijndaelManaged object
        // with the specified key and IV.
        using(var rijAlg = new RijndaelManaged()) {
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            rijAlg.FeedbackSize = 128;

            rijAlg.Key = key;
            rijAlg.IV = iv;

            // Create a decrytor to perform the stream transform.
            var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

            // Create the streams used for encryption.
            using(var msEncrypt = new MemoryStream()) {
                using(var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                    using(var swEncrypt = new StreamWriter(csEncrypt)) {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }
        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }
}