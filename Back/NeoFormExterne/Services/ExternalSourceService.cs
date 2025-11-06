using Microsoft.Data.SqlClient;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace NeoForm_Externe.Services
{
    public class ExternalSourceService : IExternalSourceService
    {
        private readonly IObjectService _objectService;
        private readonly ILogger<ExternalSourceService> _logger;
        private readonly SemaphoreSlim _concurrencySemaphore = new(10, 10); // Limit concurrent operations

        public ExternalSourceService(IObjectService objectService, ILogger<ExternalSourceService> logger)
        {
            _objectService = objectService;
            _logger = logger;
        }

        public async Task<ExecuteQueryResponse> ExecuteDbqByObjectName(ExecuteQueryRequest req)
        {
            await _concurrencySemaphore.WaitAsync();
            try
            {
                _logger.LogInformation($"Executing database query for object: {req.ObjectName}");

                var queryObjectResult = await _objectService.GetObjectByObjectName(req.ObjectName, false);
                if (queryObjectResult == null)
                {
                    throw new KeyNotFoundException($"Query object not found: {req.ObjectName}");
                }

                var queryObject = queryObjectResult.ToObjectDto();
                var queryConfig = queryObject.ObjectJson?.ObjectConfig?.CollectionQueryConfig;

                if (queryObject.ObjectType != "CDQ")
                {
                    throw new InvalidOperationException($"Object {req.ObjectName} is not a query object (CDQ)");
                }

                if (queryConfig == null)
                {
                    throw new InvalidOperationException($"Query configuration is missing for object: {req.ObjectName}");
                }

                var databaseObjectResult = await _objectService.GetObjectByGuidAsync(queryConfig.DatabaseConfigGuid, false);
                if (databaseObjectResult == null)
                {
                    throw new KeyNotFoundException($"Database object not found for GUID: {queryConfig.DatabaseConfigGuid}");
                }

                var databaseObject = databaseObjectResult.ToObjectDto();
                var databaseConfig = databaseObject.ObjectJson?.ObjectConfig?.CollectionDatabaseConfig;

                if (databaseObject.ObjectType != "CDB")
                {
                    throw new InvalidOperationException($"Referenced object is not a database object (CDB)");
                }

                if (databaseConfig == null)
                {
                    throw new InvalidOperationException("Database configuration is missing");
                }

                return await ExecuteWithParams(databaseConfig, queryConfig, req.Params);
            }
            finally
            {
                _concurrencySemaphore.Release();
            }
        }

        private async Task<ExecuteQueryResponse> ExecuteWithParams(CollectionDatabaseDto databaseConfig, CollectionQueryDto queryConfig, IList<Param> parameters)
        {
            DataTable dataTable;
            switch (databaseConfig.TypeBase)
            {
                case "SQL Server":
                    dataTable = await ExecuteSqlServer(databaseConfig, queryConfig, parameters);
                    break;

                case "MySql":
                    dataTable = await ExecuteMySql(databaseConfig, queryConfig, parameters);
                    break;

                default:
                    throw new ArgumentException("DataBase type non pris en charge");
            }

            ExecuteQueryResponse res = new ExecuteQueryResponse();

            var rootJson = new Dictionary<string, DataTable>{
                            { queryConfig.ReturnRes.Root, dataTable }
                        };

            var rootObject = JObject.FromObject(rootJson);
            var rootResult = new List<Dictionary<string, Dictionary<string, DataTable>>>()
                    {new Dictionary<string, Dictionary<string, DataTable>>
                        {{  queryConfig.ReturnRes.Root, rootJson  }}
                    };
            // json-variable-empty
            if (queryConfig.ReturnRes.Type == "variables")
            {
                JObject obj = new JObject();
                foreach (Param param in queryConfig.ReturnRes.Values)
                {
                    res.Columns.Add(param.Key);
                    obj.Add(param.Key, rootObject.SelectToken(param.Value));
                }
                res.Datas.Add(obj);
                res.Result = JArray.FromObject(rootResult);

                return res;
            }
            else
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    res.Columns.Add(column.ToString());
                }
                foreach (DataRow row in dataTable.Rows)
                {
                    JObject obj = new JObject();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        obj.Add(column.ToString(), Convert.ToString(row[column]));
                    }
                    res.Datas.Add(obj);
                }

                res.Result = JArray.FromObject(rootResult);
                return res;
            }
        }

        private async Task<DataTable> ExecuteSqlServer(CollectionDatabaseDto databaseConfig, CollectionQueryDto queryConfig, IList<Param> parameters)
        {
            using SqlConnection conn = new SqlConnection(databaseConfig.ConnectionString);
            try
            {
                await conn.OpenAsync();
                using SqlDataAdapter da = new SqlDataAdapter(queryConfig.Query, conn);
                da.SelectCommand.CommandTimeout = 30; // Set command timeout

                foreach (var item in parameters)
                {
                    da.SelectCommand.Parameters.Add(new SqlParameter
                    {
                        ParameterName = item.Key,
                        Value = string.IsNullOrEmpty(item.Value) ? DBNull.Value : item.Value
                    });
                }

                DataTable dt = new DataTable();
                da.Fill(dt);
                _logger.LogInformation($"SQL Server query executed successfully, returned {dt.Rows.Count} rows");
                return dt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing SQL Server query for parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        private async Task<DataTable> ExecuteMySql(CollectionDatabaseDto databaseConfig, CollectionQueryDto queryConfig, IList<Param> parameters)
        {
            using MySqlConnection connection = new MySqlConnection(databaseConfig.ConnectionString);
            try
            {
                await connection.OpenAsync();
                using MySqlCommand command = new MySqlCommand(queryConfig.Query, connection)
                {
                    CommandTimeout = 30 // Set command timeout
                };

                foreach (var item in parameters)
                {
                    command.Parameters.Add(new MySqlParameter
                    {
                        ParameterName = item.Key,
                        Value = string.IsNullOrEmpty(item.Value) ? DBNull.Value : item.Value
                    });
                }

                DataTable dataTable = new DataTable();
                using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                dataTable.Load(reader);
                _logger.LogInformation($"MySQL query executed successfully, returned {dataTable.Rows.Count} rows");
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing MySQL query for parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        public async Task<string> ExecuteApiByObjectName(executeApiRequestByObjectName req)
        {
            if (string.IsNullOrEmpty(req.ObjectName))
            {
                throw new ArgumentException("ObjectName is required.");
            }
            try
            {
                // Récupération de la configuration de l'objet
                ObjectModels objectModel = await _objectService.GetObjectByObjectName(req.ObjectName, false);


                ObjectJsonDto? objectJson = JsonConvert.DeserializeObject<ObjectJsonDto>(objectModel.ObjectJson);


                if (objectJson == null)
                {
                    throw new ArgumentException("objectJson is null");
                }

                if (objectJson.ObjectType != ObjectType.ExternalApi)
                {
                    throw new ArgumentException("ObjectType not ExternalApi");
                }

                var baseUrl = objectJson.ObjectConfig.ExternalApiConfig.BaseUrl;
                var method = objectJson.ObjectConfig.ExternalApiConfig.Method.ToString().ToUpper();




                var pathParams = objectJson.ObjectConfig.ExternalApiConfig.Parameters
                    .Where(p => p.Type == "Path Params")
                    .Select(p => new
                    {
                        p.Key,
                        Value = req.Params?.FirstOrDefault(param => param.Key == p.Key)?.Value
                    })
                    .Where(p => p.Value != null)
                    .ToList();

                var queryParams = objectJson.ObjectConfig.ExternalApiConfig.Parameters
                    .Where(p => p.Type == "Query Params")
                    .Select(p => new
                    {
                        p.Key,
                        Value = req.Params?.FirstOrDefault(param => param.Key == p.Key)?.Value
                    })
                    .Where(p => p.Value != null)
                    .ToList();

                var variables = objectJson.ObjectConfig.ExternalApiConfig.Parameters
                    .Where(p => p.Type == "Variable")
                    .Select(p => new KeyValue
                    {
                        Key = p.Key,
                        Value = req.Params?.FirstOrDefault(param => param.Key == p.Key)?.Value
                    })
                    .Where(p => p.Value != null)
                    .ToList();



                // Replace variables in the entire apiConfig object
                objectJson.ObjectConfig.ExternalApiConfig = ReplaceVariablesInObject(objectJson.ObjectConfig.ExternalApiConfig, variables);

                // Récupérer les paramètres de ExternalApiConfig (de objectJson)
                var apiConfigParameters = objectJson.ObjectConfig.ExternalApiConfig.Parameters;

                if (pathParams?.Count > 0)
                {
                    foreach (var param in pathParams)
                    {
                        string placeholder = "{" + param.Key + "}";
                        if (baseUrl.Contains(placeholder))
                        {
                            baseUrl = baseUrl.Replace(placeholder, param.Value);
                        }
                    }
                }
                baseUrl = baseUrl.Replace("{", "").Replace("}", "");



                var client = new RestClient(new RestClientOptions(baseUrl));
                var request = new RestRequest
                {
                    Method = method switch
                    {
                        "GET" => Method.Get,
                        "POST" => Method.Post,
                        "PUT" => Method.Put,
                        "DELETE" => Method.Delete,
                        _ => throw new ArgumentException($"Méthode HTTP non supportée : {method}")
                    }
                };

                if (queryParams?.Count > 0)
                {
                    foreach (var kv in queryParams)
                    {
                        request.AddQueryParameter(kv.Key, kv.Value);
                    }
                }


                if (objectJson.ObjectConfig.ExternalApiConfig.Headers != null)
                {
                    foreach (var kv in objectJson.ObjectConfig.ExternalApiConfig.Headers)
                    {
                        request.AddHeader(kv.Key, kv.Value);
                    }
                }

                // Ajout des headers d'authentification
                var authorization = objectJson.ObjectConfig.ExternalApiConfig.Authorization;
                if (authorization != null)
                {
                    switch (authorization.ApiType)
                    {
                        case "APIKey":
                            request.AddHeader(authorization.key, authorization.value);
                            break;
                        case "BearerToken":
                            request.AddHeader("Authorization", $"Bearer {authorization.bearerToken}");
                            break;
                        case "JWT":
                            string jwtToken = GenerateJWT(authorization);
                            request.AddHeader("Authorization", $"Bearer {jwtToken}");
                            break;
                        case "BasicAuth":
                            string authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authorization.username}:{authorization.password}"));
                            request.AddHeader("Authorization", $"Basic {authValue}");
                            break;
                    }
                }

                // Gestion du corps de la requête
                var bodyRequest = objectJson.ObjectConfig.ExternalApiConfig.BodyRequest;
                var formData = objectJson.ObjectConfig.ExternalApiConfig.FormData;

                if (bodyRequest != null)
                {
                    request.AddJsonBody(bodyRequest.content);
                }
                else if (formData != null)
                {
                    foreach (var kv in formData)
                    {
                        request.AddParameter(kv.Key, kv.Value);
                    }
                }




                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {

                    throw new HttpRequestException($"Erreur API: {response.StatusCode} - {response.ErrorMessage}");
                }


                return response.Content;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        // Function to replace variables in the entire object
        private ExternalApiDto ReplaceVariablesInObject(ExternalApiDto config, List<KeyValue> variables)
        {
            var jsonString = JsonConvert.SerializeObject(config);
            var updatedJsonString = Regex.Replace(jsonString, @"{{(.*?)}}", match =>
            {
                var key = match.Groups[1].Value;
                var param = variables.FirstOrDefault(p => p.Key == key);
                return param != null ? param.Value : match.Value;
            });
            return JsonConvert.DeserializeObject<ExternalApiDto>(updatedJsonString);
        }


        public static string GenerateJWT(Authorization authorization)
        {
            var header = new
            {
                alg = authorization.algorithm,
                typ = "JWT"
            };

            string encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(header)));
            string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(authorization.payload)));

            string token = $"{encodedHeader}.{encodedPayload}";

            string signature = GenerateSignature(token, authorization);

            return $"{token}.{signature}";
        }

        // Method to generate the base64 URL encoding
        private static string Base64UrlEncode(byte[] input)
        {
            string base64 = Convert.ToBase64String(input);
            base64 = base64.Split('=')[0]; // Remove padding
            base64 = base64.Replace('+', '-'); // URL-safe
            base64 = base64.Replace('/', '_'); // URL-safe
            return base64;
        }

        // Signature generation (HMACSHA256)
        private static string GenerateSignature(string token, Authorization authorization)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(authorization.secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Base64UrlEncode(hash);
            }
        }
    }

}
