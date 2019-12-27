using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Util;
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

        private void OnEnable()
        {
            Debug.Log("PartakDatabase OnEnable");
            
            LoggingConfig loggingConfig = AWSConfigs.LoggingConfig;
            loggingConfig.LogTo = LoggingOptions.Console;
            loggingConfig.LogMetrics = true;
            loggingConfig.LogResponses = ResponseLoggingOption.Always;
            loggingConfig.LogResponsesSizeLimit = 4096;
            loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
            
            Connect();
        }

        [ContextMenu("TestPopulate")]
        private async void TestPopulate()
        {
            for (int i = 0; i < 10; i++)
            {
                await SaveLevel(UnityEngine.Random.Range(0, 1000).ToString());
            }
        }

        [ContextMenu("Connect")]
        private async void Connect()
        {
            if (_table == null)
            {
                RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(_regionEndpoint);
                CognitoAWSCredentials credentials = new CognitoAWSCredentials(_identityPoolId, endpoint);
                ImmutableCredentials credentialsResult = await credentials.GetCredentialsAsync();
                Debug.Log($"Credential Success: {credentialsResult.Token}");
                AmazonDynamoDBClient client = new AmazonDynamoDBClient(credentials, endpoint);
                Debug.Log($"AmazonDynamoDBClient Success: {credentialsResult.Token}");
                _table = Table.LoadTable(client, _tableName);
                Debug.Log($"{_tableName} Table Success");
            }
        }

        public Search QueryLevels(int pageSize)
        {
            
            
            QueryFilter filter = new QueryFilter("pk", QueryOperator.Equal, "level");
            QueryOperationConfig config = new QueryOperationConfig()
            {
                Limit = pageSize, 
                Select = SelectValues.SpecificAttributes,
                AttributesToGet = new List<string> { "author", "level_data"},
                ConsistentRead = true,
                Filter = filter
            };
            Search search = _table.Query(config);
            
//            List<Document> documentList = new List<Document>();
//            do
//            {
//                Debug.Log("Populating documentlsit");
//                documentList = search.GetNextSet();
//                foreach (var document in documentList)
//                {
//                    Debug.Log(document["level_data"]);
//                }
//            } while (!search.IsDone);

            return search;
        }

        public async Task SaveLevel(string levelData)
        {
            Document level = new Document();
            level[LevelFields.PkKey] = "level";
            level[LevelFields.IdKey] = Guid.NewGuid().ToString();
            level[LevelFields.AuthorKey] = SystemInfo.deviceUniqueIdentifier;
            level[LevelFields.LevelDataKey] = levelData;
            level[LevelFields.ThumbsUpKey] = 0;
            level[LevelFields.ThumbsDownKey] = 0;
            level[LevelFields.CreatedAtKey] = DateTime.UtcNow;
            level[LevelFields.DownloadedCountKey] = 1;
            level[LevelFields.DeleteCountKey] = 0;
            level[LevelFields.PlayCountKey] = 0;
            level[LevelFields.LastPlayedKey] = DateTime.UtcNow;
            
            await _table.PutItemAsync(level);
        }
    }
}
