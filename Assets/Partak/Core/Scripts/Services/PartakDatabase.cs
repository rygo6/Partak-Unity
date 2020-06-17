﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Util;
using GeoTetra.GTPooling;
using GeoTetra.Partak;
using UnityEngine;

namespace GeoTetra.GTBackend
{
    [Serializable]
    public class PartakDatabaseReference : ServiceReferenceT<PartakDatabase>
    {
        public PartakDatabaseReference(string guid) : base(guid)
        { }
    }
    
    public class PartakDatabase : ServiceBehaviour
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
            public const string CreatedAtKey = "created_at";
            public const string DownloadedCountKey = "download_count";
            public const string DeleteCountKey = "delete_count";
            public const string PlayCountKey = "play_count";
            public const string LastPlayedKey = "last_played";
            public const string PkLevelValue = "level";
        } 
        
        private Table _table;
        private AmazonS3Client _s3Client;
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

                try
                {
                    RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(_regionEndpoint);
                    CognitoAWSCredentials credentials = new CognitoAWSCredentials(_identityPoolId, endpoint);
                    ImmutableCredentials credentialsResult = await credentials.GetCredentialsAsync();
                    Debug.Log($"Credential Success: {credentialsResult.Token}");
                    AmazonDynamoDBClient client = new AmazonDynamoDBClient(credentials, endpoint);
                    Debug.Log($"AmazonDynamoDBClient Success");
                    _table = Table.LoadTable(client, _tableName);
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
            string levelPath = LevelUtility.LevelPath(levelId);
            string imagePath = LevelUtility.LevelImagePath(levelId);

            Debug.Log("Downloading datum  " + levelPath);
            GetItemOperationConfig config = new GetItemOperationConfig();
            Document result = await _table.GetItemAsync(LevelFields.PkLevelValue, levelId, config);

            LocalLevelDatum levelDatum = JsonUtility.FromJson<LocalLevelDatum>(result[LevelFields.LevelDataKey]);
            levelDatum.Shared = true;
            levelDatum.Downloaded = true;
            levelDatum.LevelID = levelId; //still needed?
            string json = JsonUtility.ToJson(levelDatum);

            File.WriteAllText(levelPath, json);
            
            Debug.Log("Downloading image  " + imagePath);
            string imageKey = S3LevelImageKey(levelId);
            await _transferUtility.DownloadAsync(imagePath, _s3Bucket, imageKey);
        }
        
        public Search QueryLevelsCreatedAt(int pageSize)
        {
            return QueryLevels(pageSize, _createdAtIndex);
        }

        public Search QueryLevelsThumbsUp(int pageSize)
        {
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

        private string S3LevelImageKey(string id)
        {
            return $"{_levelImagesFolder}/{id}.png";
        }
    }
}