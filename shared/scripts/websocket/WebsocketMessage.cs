using System.Text.Json;

namespace shared;

public class WebSocketMessage
{
    public int Id { get; set; }
    public MessageType MessageType { get; set; }
    public JsonElement Data { get; set; }
    public WebSocketMessage() { }
    public WebSocketMessage(int id, MessageType type, JsonElement data)
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

