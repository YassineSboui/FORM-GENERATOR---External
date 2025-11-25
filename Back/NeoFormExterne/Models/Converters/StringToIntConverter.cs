using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoForm_Externe.Models
{
    /// <summary>
    /// Custom JSON converter to handle expires_in field that can be either string or integer
    /// depending on the OIDC provider (Azure AD returns string, Auth0 returns integer)
    /// </summary>
    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (int.TryParse(stringValue, out int value))
                {
                    return value;
                }
                throw new JsonException($"Cannot convert string '{stringValue}' to int");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            throw new JsonException($"Cannot convert {reader.TokenType} to int");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
