using Godot;
using Newtonsoft.Json;
using shared;
using System.Text.Json;

namespace Singletons;

public partial class SessionManager : Node
{
    public static SessionManager Instance { get; set; }

    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();
    private bool connected = false;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        webSocket.Poll();

        if (!connected)
        {
            if (webSocket.GetReadyState() == WebSocketPeer.State.Open)
            {
                connected = true;
            }
            return;
        }

        if (webSocket.GetReadyState() != WebSocketPeer.State.Open)
        {
            connected = false;
            return;
        }

        while (webSocket.GetAvailablePacketCount() > 0)
        {
            var packet = webSocket.GetPacket();
            string packetData = packet.GetStringFromUtf8();

            WebSocketMessage message = JsonConvert.DeserializeObject<WebSocketMessage>(packetData);
            switch (message.Id)
            {
                case 0:
                {
                    break;
                }
                case 1:
                {
                    break;
                }
                case 102:
                {
                    break;
                }
            }
        }
    }

    public void SetConnectionURL(string url)
    {
        connectionString = url;
    }

    public void Request(int id, MessageType type)
    {
        WebSocketMessage message = new(id, type, JsonDocument.Parse("{}").RootElement.Clone());
        string jsonMessage = JsonConvert.SerializeObject(message);
        webSocket.SendText(jsonMessage);
    }

    public void Request(WebSocketMessage message)
    {
        string jsonMessage = JsonConvert.SerializeObject(message);
        webSocket.SendText(jsonMessage);
    }
}
