using Godot;
using System;

public partial class SessionManager : Node
{
    public static SessionManager Instance;
    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();

    public override void _Ready()
    {
        Instance = this; 
    }

    public override void _Process(double delta)
    {
        webSocket.Poll();
        if (webSocket.GetReadyState() == WebSocketPeer.State.Open)
        {
            while (webSocket.GetAvailablePacketCount() > 0)
            {
                var packet = webSocket.GetPacket();
                GD.Print("Received packet: " + packet.ToString());
                StreamPeerBuffer buffer = new()
                {
                    DataArray = packet
                };
                byte packetId = buffer.GetU8();
                int value = buffer.Get32();
                string message = buffer.GetUtf8String(packet.Length - buffer.GetPosition());

                GD.Print($"ID: {packetId}, Int: {value}, Msg: {message}");
            }
        }
    }

    public void SetConnectionURL(string url)
    {
        connectionString = url;
    }

    public void ConnectToUrl()
    {
        Error error = webSocket.ConnectToUrl(connectionString);
        if (error != Error.Ok)
        {
            GD.Print("Error connecting to WebSocket: " + error);
            GetTree().CurrentScene.GetNode<HUDScript>("HUD").ShowConnectionDebugInfo(error);
            return;
        } else
        {
            GD.Print("Connected to WebSocket: " + connectionString);
            GetTree().CurrentScene.GetNode<HUDScript>("HUD").ShowSimmulationSettings();
        }  
    }
}


