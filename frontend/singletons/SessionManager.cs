using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using shared;

namespace Singletons;

public partial class SessionManager : Node
{
    public static SessionManager Instance { get; set; }

    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();
    private bool connected = false;
    private JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public override void _Ready()
    {
        Instance = this;
        ConnectToUrl();
    }

    public override void _PhysicsProcess(double delta)
    {
        webSocket.Poll();

        if (!connected)
        {
            if (webSocket.GetReadyState() == WebSocketPeer.State.Open)
            {
                GD.Print("Connected to WebSocket: " + connectionString);
                Request(102, MessageType.USEDSTATIONS);
                connected = true;
            }
            return;
        }

        if (webSocket.GetReadyState() != WebSocketPeer.State.Open)
        {
            GD.Print("Connected to WebSocket: " + connectionString);
            connected = false;
            GameManagerScript.Instance.Reset();
            return;
        }

        while (webSocket.GetAvailablePacketCount() > 0)
        {
            var packet = webSocket.GetPacket();
            string packetData = packet.GetStringFromUtf8();

            WebSocketMessage message = JsonSerializer.Deserialize<WebSocketMessage>(packetData, options);
            switch (message.MessageType)
            {
                case MessageType.USEDSTATIONS:
                    {
                        StationListData data = message.Data.Deserialize<StationListData>();
                        GameManagerScript.Instance.SpawnStations(data.Stations);
                        break;
                    }
                default:
                    break;
            }
        }
    }

    public void SetConnectionURL(string url)
    {
        connectionString = url;
    }

    public void ConnectToUrl()
    {
        Error error = webSocket.ConnectToUrl("ws://localhost:5000/ws/");
        if (error != Error.Ok)
        {
            GD.Print("Error connecting to WebSocket: " + error);
            //GetTree().CurrentScene.GetNode<HUDScript>("HUD").ShowConnectionDebugInfo(error);
            return;
        }
    }

    public void Request(int id, MessageType type)
    {
        WebSocketMessage message = new(id, type, JsonDocument.Parse("{}").RootElement.Clone());
        string jsonMessage = JsonSerializer.Serialize(message, options);
        webSocket.SendText(jsonMessage);
    }

    public void Request(WebSocketMessage message)
    {
        string jsonMessage = JsonSerializer.Serialize(message, options);
        webSocket.SendText(jsonMessage);
    }
}
