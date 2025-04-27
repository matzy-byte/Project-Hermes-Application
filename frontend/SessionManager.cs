using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class SessionManager : Node
{
    public static SessionManager Instance;
    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();
    private bool connected = false;

    public override void _Ready()
    {
        Instance = this; 
    }

    public override void _Process(double delta)
    {
        webSocket.Poll();
        if (!connected)
        {
            
            if (webSocket.GetReadyState() == WebSocketPeer.State.Open)
            {
                GD.Print("Connected to WebSocket: " + connectionString);
                GetTree().CurrentScene.GetNode<HUDScript>("HUD").ShowSimmulationSettings();
                Request(102, MessageType.USEDSTATIONS);
                connected = true;
            }
            return;
        }

        if (webSocket.GetReadyState() == WebSocketPeer.State.Open)
        {
            while (webSocket.GetAvailablePacketCount() > 0)
            {
                var packet = webSocket.GetPacket();
                string packetData = packet.GetStringFromUtf8();

                WebSocketMessage message = JsonSerializer.Deserialize<WebSocketMessage>(packetData, new JsonSerializerOptions{Converters = { new JsonStringEnumConverter() }});
                Dictionary<string, JsonElement> data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message.data);
                switch (message.id)
                {
                    case 0:
                    {
                        if (GameManager.Instance.stations.Count < 185) break;
                        Train[] trains = data["TrainPositions"].Deserialize<Train[]>();
                        if (GameManager.Instance.trains.Count <= 0)
                        {
                            GameManager.Instance.SpawnTrains(trains);
                        }
                        else {
                            GameManager.Instance.UpdateTrains(trains);
                        }
                        break;
                    }
                    case 1:
                    {
                        if (GameManager.Instance.trains.Count < 16) break;
                        Robot[] robots = data["RobotData"].Deserialize<Robot[]>();
                        if (GameManager.Instance.robots.Count <= 0)
                        {
                            GameManager.Instance.SpawnRobots(robots);
                        }
                        else
                        {
                            GameManager.Instance.UpdateRobots(robots);
                        }
                        break;
                    }
                    case 102:
                    {
                        Station[] stations = data["UsedStations"].Deserialize<Station[]>();
                        GameManager.Instance.SpawnStations(stations);
                        break;
                    }
                }
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
        }
    }
    
    public void Request(int id, MessageType type)
    {
        WebSocketMessage message = new()
        {
            id = id,
            messageType = type,
            data = JsonDocument.Parse("{}").RootElement.Clone()
        };
        string jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });
        webSocket.SendText(jsonMessage);
    }
}


