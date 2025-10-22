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

        public ObjectService(ExternalNeoFormContext context, IEncryptionService encryption , ILogger<ObjectService> logger)
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
            }

            return JsonConvert.SerializeObject(objectConfig, Formatting.None);
        }

        public async Task<ObjectModels> PublishObject(JObject obj)
        {
            var guid = obj["guid"]?.ToString();

            if (string.IsNullOrWhiteSpace(guid) || !Guid.TryParse(guid, out Guid parsedGuid))
                throw new ArgumentException("Invalid or missing GUID in object");

            var existing = await _context.Objects.FirstOrDefaultAsync(o => o.Guid == guid);

            var objectJson = obj.ToString(Formatting.None);

            if (existing != null)
            {
                existing.ObjectJson = objectJson;
                _context.Objects.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }
            else
            {
                var model = new ObjectModels
                {
                    Guid = guid,
                    ObjectJson = objectJson
                };
                _context.Objects.Add(model);
                await _context.SaveChangesAsync();
                return model;
            }
        }






    }
}
