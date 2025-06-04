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
            return await _context.Objects
                .Where(o => o.ObjectType == objectType)
                .ToListAsync();
        }

        public async Task<ObjectModels?> GetObjectByGuidAsync(string guid)
        {
            return await _context.Objects
                .FirstOrDefaultAsync(o => o.Guid == guid);
        }

   
        public async Task<ObjectModels> GetObjectByObjectName(string name)
        {
            ObjectModels? o = await _context.Objects.FirstOrDefaultAsync(s => s.ObjectName == name);
            if (o == null) throw new RecordNotFoundException("Object not found");

            if (o.IsEncrypted)
            {
                o.ObjectJson = decyptObject(o.ObjectJson);
            }

            return o;
        }
        private string decyptObject(string objectJson)
        {
            EncryptedObjectJsonDto? encryptedObject = JsonConvert.DeserializeObject<EncryptedObjectJsonDto>(objectJson);
            if (encryptedObject == null) { throw new InvalidOperationException(); }
            string objectConfigJSON = _encryption.Decrypt(encryptedObject.ObjectConfig);
            JObject objectConfig = JObject.Parse(objectJson);
            objectConfig["objectConfig"] = JObject.Parse(objectConfigJSON);
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
