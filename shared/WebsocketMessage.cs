using System.Text.Json;

namespace shared;

public class WebSocketMessage
{
    public int Id { get; set; }
    public MessageType MessageType { get; set; }
    public JsonElement Data { get; set; }
}