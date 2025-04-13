using Godot;
using System;



// class for connecting to server websocket
public partial class SessionManager : Node
{
    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();

    public override void _Ready()

    {
        Error error = webSocket.ConnectToUrl(connectionString);
        if (error != Error.Ok)
        {
            GD.Print("Error connecting to WebSocket: " + error);
            return;
        } else
        {
            GD.Print("Connected to WebSocket: " + connectionString);

        }   
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
                var buffer = new Godot.StreamPeerBuffer();
                buffer.DataArray = packet;

                byte packetId = buffer.GetU8();
                int value = buffer.Get32();
                string message = buffer.GetUtf8String(packet.Length - buffer.GetPosition());

                GD.Print($"ID: {packetId}, Int: {value}, Msg: {message}");
            }
        }
    }
}


