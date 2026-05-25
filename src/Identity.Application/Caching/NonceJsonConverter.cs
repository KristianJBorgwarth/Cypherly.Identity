using System.Text.Json;
using System.Text.Json.Serialization;

namespace Identity.Application.Caching;

public class NonceJsonConverter : JsonConverter<Nonce>
{
    public override Nonce Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        var id = jsonObject.GetProperty("Id").GetGuid();
        var nonceValue = jsonObject.GetProperty("NonceValue").GetString();
        var userId = jsonObject.GetProperty("UserId").GetGuid();
        var deviceId = jsonObject.GetProperty("DeviceId").GetGuid();
        var createdAt = jsonObject.GetProperty("CreatedAt").GetDateTime();
        var expiresAt = jsonObject.GetProperty("ExpiresAt").GetDateTime();

        return Nonce.FromCache(id, nonceValue!, userId, deviceId, createdAt, expiresAt);
    }
    public override void Write(Utf8JsonWriter writer, Nonce value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Id", value.Id);
        writer.WriteString("NonceValue", value.NonceValue);
        writer.WriteString("UserId", value.UserId);
        writer.WriteString("DeviceId", value.DeviceId);
        writer.WriteString("CreatedAt", value.CreatedAt);
        writer.WriteString("ExpiresAt", value.ExpiresAt);
        writer.WriteEndObject();
    }
}
