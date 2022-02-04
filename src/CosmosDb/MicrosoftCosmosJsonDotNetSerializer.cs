using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Data.Cosmos
{
  [ExcludeFromCodeCoverage]
  public class MicrosoftCosmosJsonDotNetSerializer : CosmosSerializer
  {
    private static readonly Encoding DefaultEncoding = (Encoding) new UTF8Encoding(false, true);
    private readonly JsonSerializer _serializer;

    public MicrosoftCosmosJsonDotNetSerializer(JsonSerializerSettings settings) => this._serializer = JsonSerializer.Create(settings);

    public override T FromStream<T>(Stream stream)
    {
      using (stream)
      {
        if (typeof (Stream).IsAssignableFrom(typeof (T)))
          return (T) stream;
        using (StreamReader reader1 = new StreamReader(stream))
        {
          using (JsonTextReader reader2 = new JsonTextReader((TextReader) reader1))
            return this._serializer.Deserialize<T>((JsonReader) reader2);
        }
      }
    }

    public override Stream ToStream<T>(T input)
    {
      MemoryStream stream = new MemoryStream();
      using (StreamWriter streamWriter = new StreamWriter((Stream) stream, MicrosoftCosmosJsonDotNetSerializer.DefaultEncoding, 1024, true))
      {
        using (JsonWriter jsonWriter = (JsonWriter) new JsonTextWriter((TextWriter) streamWriter))
        {
          jsonWriter.Formatting = Formatting.None;
          this._serializer.Serialize(jsonWriter, (object) input);
          jsonWriter.Flush();
          streamWriter.Flush();
        }
      }
      stream.Position = 0L;
      return (Stream) stream;
    }
  }
}