using AF.AutomationTest.MatchingEngine.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class IdConverter : JsonConverter<Id>
{
    public override Id ReadJson(JsonReader reader, Type objectType, Id existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var id = new Id();
        var token = JToken.Load(reader);

        if (token.Type == JTokenType.Object && token["$oid"] != null)
        {
            id._id = token["$oid"].ToString();
        }
        else if (token.Type == JTokenType.String)
        {
            id._id = token.ToString();
        }
        else
        {
            throw new JsonSerializationException("Unexpected token type for Id");
        }

        return id;
    }

    public override void WriteJson(JsonWriter writer, Id value, JsonSerializer serializer)
    {
        writer.WriteValue(value._id);
    }
}
