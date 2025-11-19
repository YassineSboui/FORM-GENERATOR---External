using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NeoForm_Externe.Models.Exceptions;

namespace NeoForm_Externe.Services
{
    public class ObjectService : IObjectService
    {
        private readonly ExternalNeoFormContext _context;
        private readonly IEncryptionService _encryption;
        private readonly ILogger<ObjectService> _logger;

        public ObjectService(ExternalNeoFormContext context, IEncryptionService encryption, ILogger<ObjectService> logger)
        {
            _context = context;
            _encryption = encryption;
            _logger = logger;
        }

        public async Task<List<ObjectModels>> GetObjectsByTypeAsync(string objectType)
        {
            var objects = await _context.Objects
                .Where(o => o.ObjectType == objectType)
                .ToListAsync();

            foreach (var o in objects)
            {
                if (o.IsEncrypted)
                {
                    o.ObjectJson = decyptObject(o.ObjectJson);
                }
            }

            return objects;
        }

        public async Task<ObjectModels?> GetObjectByGuidAsync(string guid, bool nullifySensitiveFields = true)
        {
            var o = await _context.Objects
                .FirstOrDefaultAsync(obj => obj.Guid == guid);

            if (o != null && o.IsEncrypted)
            {
                o.ObjectJson = decyptObject(o.ObjectJson, nullifySensitiveFields);
            }

            return o;
        }



        public async Task<ObjectModels> GetObjectByObjectName(string name, bool nullifySensitiveFields = true)
        {
            ObjectModels? o = await _context.Objects.FirstOrDefaultAsync(s => s.ObjectName == name);
            if (o == null) throw new RecordNotFoundException("Object not found");

            if (o.IsEncrypted)
            {
                o.ObjectJson = decyptObject(o.ObjectJson, nullifySensitiveFields);
            }

            return o;
        }
        private string decyptObject(string objectJson, bool nullifySensitiveFields = true)
        {
            EncryptedObjectJsonDto? encryptedObject = JsonConvert.DeserializeObject<EncryptedObjectJsonDto>(objectJson);
            if (encryptedObject == null) { throw new InvalidOperationException(); }
            string objectConfigJSON = _encryption.Decrypt(encryptedObject.ObjectConfig);
            JObject objectConfig = JObject.Parse(objectJson);
            objectConfig["objectConfig"] = JObject.Parse(objectConfigJSON);

            // Only nullify sensitive fields if requested (for API responses)
            if (nullifySensitiveFields)
            {
                // Set sensitive SMTP fields to null
                if (objectConfig["objectConfig"]?["SMTPConfig"] != null)
                {
                    objectConfig["objectConfig"]["SMTPConfig"]["username"] = null;
                    objectConfig["objectConfig"]["SMTPConfig"]["password"] = null;
                }
                // Set sensitive CollectionDatabaseConfig fields to null
                if (objectConfig["objectConfig"]?["CollectionDatabaseConfig"] != null)
                {
                    objectConfig["objectConfig"]["CollectionDatabaseConfig"]["connectionString"] = null;
                }
                // Set sensitive ExternalApiConfig Authorization fields to null
                if (objectConfig["objectConfig"]?["externalApiConfig"]?["authorization"] != null)
                {
                    var auth = objectConfig["objectConfig"]["externalApiConfig"]["authorization"];
                    auth["apiType"] = null;
                    auth["api_type"] = null;
                    auth["key"] = null;
                    auth["value"] = null;
                    auth["bearerToken"] = null;
                    auth["algorithm"] = null;
                    auth["secret"] = null;
                    auth["secret_encoded"] = null;
                    auth["payload"] = null;
                    auth["username"] = null;
                    auth["password"] = null;
                }
            }

            return JsonConvert.SerializeObject(objectConfig, Formatting.None);
        }

        public async Task<ObjectModels> PublishObject(JObject obj)
        {
            var guid = obj["guid"]?.ToString();

            if (string.IsNullOrWhiteSpace(guid) || !Guid.TryParse(guid, out Guid parsedGuid))
            {
                _logger.LogError("Invalid or missing GUID in object: {Object}", obj.ToString(Formatting.None));
                throw new ArgumentException("Invalid or missing GUID in object");
            }

            _logger.LogInformation("Processing object with GUID: {Guid}", guid);

            // Handle encryption if isEncrypted flag is true
            if (obj.TryGetValue("isEncrypted", out var isEncrypted) && isEncrypted.Value<bool>())
            {
                _logger.LogInformation("Encrypting object configuration for GUID: {Guid}", guid);
                var objectConfig = JsonConvert.SerializeObject(obj.GetValue("objectConfig"), Formatting.None);
                obj["objectConfig"] = _encryption.Encrypt(objectConfig);
            }

            var objectJson = obj.ToString(Formatting.None);

            var existing = await _context.Objects.FirstOrDefaultAsync(o => o.Guid == guid);

            if (existing != null)
            {
                _logger.LogInformation("Updating existing object with GUID: {Guid}", guid);
                existing.ObjectJson = objectJson;
                _context.Objects.Update(existing);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated object with GUID: {Guid}", guid);
                return existing;
            }
            else
            {
                _logger.LogInformation("Creating new object with GUID: {Guid}", guid);
                var model = new ObjectModels
                {
                    Guid = guid,
                    ObjectJson = objectJson
                };
                _context.Objects.Add(model);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created new object with GUID: {Guid}", guid);
                return model;
            }
        }






    }
}
