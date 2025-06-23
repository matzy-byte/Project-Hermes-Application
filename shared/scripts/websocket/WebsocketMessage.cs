using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace shared;

public class WebSocketMessage
{
    public int Id { get; set; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public MessageType MessageType { get; set; }
    public JToken Data { get; set; }

    public WebSocketMessage() { }
  
    public WebSocketMessage(int id, MessageType type, JToken data)
    {
        Id = id;
        MessageType = type;
        Data = data;
    }

    public WebSocketMessage Clone()
    {
        return new WebSocketMessage(Id, MessageType, Data);
    }
}

