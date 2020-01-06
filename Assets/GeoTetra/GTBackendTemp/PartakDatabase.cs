using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Util;
using GeoTetra.Partak;
using UnityEngine;
using Random = System.Random;

namespace GeoTetra.GTBackend
{
    [CreateAssetMenu(menuName = "GeoTetra/Database")]
    public class PartakDatabase : ScriptableObject
    {
        [SerializeField] private string _identityPoolId = "us-west-2:1c228a7e-eb85-433f-a708-d46b063a488f";
        [SerializeField] private string _regionEndpoint  = "us-west-2";
        [SerializeField] private string _tableName = "Partak";
        [SerializeField] private string _s3Bucket = "partak";
        [SerializeField] private string _levelImagesFolder = "LevelImages";
        
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
        } 
        
        private Table _table;
        private TransferUtility _transferUtility;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
#endif
            Connect();
        }

        [ContextMenu("Connect")]
        private async void Connect()
        {
            if (_table == null)
            {
                LoggingConfig loggingConfig = AWSConfigs.LoggingConfig;
                loggingConfig.LogTo = LoggingOptions.Console;
                loggingConfig.LogMetrics = true;
                loggingConfig.LogResponses = ResponseLoggingOption.Always;
                loggingConfig.LogResponsesSizeLimit = 4096;
                loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
                
                
                RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(_regionEndpoint);
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(_identityPoolId, endpoint);
                ImmutableCredentials credentialsResult = await credentials.GetCredentialsAsync();
                Debug.Log($"Credential Success: {credentialsResult.Token}");
                AmazonDynamoDBClient client = new AmazonDynamoDBClient(credentials, endpoint);
                Debug.Log($"AmazonDynamoDBClient Success");
                _table = Table.LoadTable(client, _tableName);
                Debug.Log($"{_tableName} Table Success");
                AmazonS3Client s3Client = new AmazonS3Client(credentials, endpoint);
                _transferUtility = new TransferUtility(s3Client);
                Debug.Log($"{_transferUtility} Success");
            }
        }

        public Search QueryLevels(int pageSize)
        {
            QueryFilter filter = new QueryFilter("pk", QueryOperator.Equal, "level");
            QueryOperationConfig config = new QueryOperationConfig()
            {
                Limit = pageSize, 
                Select = SelectValues.SpecificAttributes,
                AttributesToGet = new List<string> { LevelFields.IdKey },
                ConsistentRead = true,
                Filter = filter
            };
            return _table.Query(config);
        }
        
        public async Task<Texture> DownloadLevelImage(Document document)
        {
            string tempImage = Path.Combine(Application.persistentDataPath, "temp.png");
            await _transferUtility.DownloadAsync(tempImage, _s3Bucket, $"{_levelImagesFolder}/{document[LevelFields.IdKey]}.png");
            byte[] imageBytes = System.IO.File.ReadAllBytes(tempImage);
            Texture2D image = new Texture2D(0,0);
            image.LoadImage(imageBytes, true);
            return image;
        }

        public async Task SaveLevel(int levelIndex)
        {
            Debug.Log("Saving level " + levelIndex);
            string levelPath = LevelUtility.LevelPath(levelIndex);
            string imagePath = LevelUtility.LevelImagePath(levelIndex);
            string json = File.ReadAllText(levelPath);
            string guid = Guid.NewGuid().ToString();
            
            Document level = new Document();
            level[LevelFields.PkKey] = "level";
            level[LevelFields.IdKey] = guid;
            level[LevelFields.AuthorKey] = SystemInfo.deviceUniqueIdentifier;
            level[LevelFields.LevelDataKey] = json;
            level[LevelFields.ThumbsUpKey] = 0;
            level[LevelFields.ThumbsDownKey] = 0;
            level[LevelFields.CreatedAtKey] = DateTime.UtcNow;
            level[LevelFields.DownloadedCountKey] = 1;
            level[LevelFields.DeleteCountKey] = 0;
            level[LevelFields.PlayCountKey] = 0;
            level[LevelFields.LastPlayedKey] = DateTime.UtcNow;

            try
            {
                Debug.Log("Saving to Dynamo " + levelIndex);
                await _table.PutItemAsync(level);
                Debug.Log("Success");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

            try
            {
                Debug.Log("Saving to S3 " + levelIndex);
                await _transferUtility.UploadAsync(imagePath, _s3Bucket, $"{_levelImagesFolder}/{guid}.png");
                Debug.Log("Success");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
