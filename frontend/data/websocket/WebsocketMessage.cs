using System.Text.Json;

public class WebSocketMessage
{
    public int id { get; set; }
    public MessageType messageType { get; set; }
    public JsonElement data { get; set; }

    public override string ToString()
    {
        return $"{id}, {messageType}, {data}";
    }
}