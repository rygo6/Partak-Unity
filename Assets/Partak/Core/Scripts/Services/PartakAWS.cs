using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Util;
using GeoTetra.GTPooling;
using GeoTetra.Partak;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        [SerializeField] private string _identityPoolId = "us-west-2:1c228a7e-eb85-433f-a708-d46b063a488f";
        [SerializeField] private string _regionEndpoint  = "us-west-2";
        [SerializeField] private string _tableName = "Partak";
        [SerializeField] private string _s3Bucket = "partak";
        [SerializeField] private string _levelImagesFolder = "LevelImages";
        [SerializeField] private string _createdAtIndex = "pk-created_at-index";
        [SerializeField] private string _thumbsUpIndex = "pk-thumbs_up-index";
        
        public static class LevelFields
        {
            public const string PkKey = "pk";
            public const string IdKey = "id";
            public const string AuthorKey = "author";
            public const string LevelDataKey = "level_data";
            public const string ThumbsUpKey = "thumbs_up";
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
                LoggingConfig loggingConfig = AWSConfigs.LoggingConfig;
                loggingConfig.LogTo = LoggingOptions.Console;
                loggingConfig.LogMetrics = true;
                loggingConfig.LogResponses = ResponseLoggingOption.Always;
                loggingConfig.LogResponsesSizeLimit = 4096;
                loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;

                try
                {
                    RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(_regionEndpoint);
                    CognitoAWSCredentials credentials = new CognitoAWSCredentials(_identityPoolId, endpoint);
                    ImmutableCredentials credentialsResult = await credentials.GetCredentialsAsync();
                    Debug.Log($"Credential Success: {credentialsResult.Token}");
                    _dbClient = new AmazonDynamoDBClient(credentials, endpoint);
                    Debug.Log($"AmazonDynamoDBClient Success");
                    _table = Table.LoadTable(_dbClient, _tableName);
                    Debug.Log($"{_tableName} Table Success");
                    _s3Client = new AmazonS3Client(credentials, endpoint);
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
            await Connect();
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
            return QueryLevels(pageSize, _thumbsUpIndex);
        }
        
        private Search QueryLevels(int pageSize, string index)
        {
            QueryFilter filter = new QueryFilter(LevelFields.PkKey, QueryOperator.Equal, LevelFields.PkLevelValue);

            QueryOperationConfig config = new QueryOperationConfig()
            {
                Limit = pageSize, 
                Select = SelectValues.SpecificAttributes,
                AttributesToGet = new List<string> { LevelFields.IdKey, LevelFields.ThumbsUpKey },
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
                        responseStream.CopyTo(memoryStream);
                    }

                    return memoryStream.ToArray();
                }
            });
        }

        private async Task<byte[]> GetImageBytesFromS3(string key, CancellationToken cancellationToken)
        {
            return await Task.Run( async () =>
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _s3Bucket,
                    Key = key
                };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(request, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) return null;
                    
                    MemoryStream memoryStream = new MemoryStream();
                    using (Stream responseStream = response.ResponseStream)
                    {
                        responseStream.CopyTo(memoryStream);
                    }

                    return cancellationToken.IsCancellationRequested ? null : memoryStream.ToArray();
                }
            }, cancellationToken);
        }

        public async Task SaveLevel(string levelId)
        {
            await Connect();
            
            Debug.Log("Saving level " + levelId);
            string levelPath = LevelUtility.LevelPath(levelId);
            string imagePath = LevelUtility.LevelImagePath(levelId);
            string json = File.ReadAllText(levelPath);

            Document level = new Document();
            level[LevelFields.PkKey] = "level";
            level[LevelFields.IdKey] = levelId;
            level[LevelFields.AuthorKey] = SystemInfo.deviceUniqueIdentifier;
            level[LevelFields.LevelDataKey] = json;
            level[LevelFields.ThumbsUpKey] = 0;
            level[LevelFields.ThumbsDownKey] = 0;
            level[LevelFields.CreatedAtKey] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            level[LevelFields.DownloadedCountKey] = 0;
            level[LevelFields.DeleteCountKey] = 0;
            level[LevelFields.PlayCountKey] = 0;
            level[LevelFields.LastPlayedKey] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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

        public async Task IncrementThumbsUp(string levelID, bool positive)
        {
            await Connect();
            AtomicIncrement(levelID, LevelFields.ThumbsUpKey, positive);
        }
        
        public async Task IncrementThumbsDown(string levelID, bool positive)
        {
            await Connect();
            AtomicIncrement(levelID, LevelFields.ThumbsDownKey, positive);
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
    }
}
