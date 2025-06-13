using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using shared;

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

            WebSocketMessage message = JsonConvert.DeserializeObject<WebSocketMessage>(packetData);
            switch (message.MessageType)
            {
                case MessageType.TRAINDATA:
                    {
                        if (GameManagerScript.Instance.Stations.Count == 0)
                        {
                            return;
                        }
                        TrainListData data = message.Data.ToObject<TrainListData>();
                        if (GameManagerScript.Instance.Trains.Count == 0)
                        {
                            GameManagerScript.Instance.SpawnTrains(data.Trains);
                            break;
                        }
                        GameManagerScript.Instance.UpdateTrains(data.Trains);
                        break;
                    }
                case MessageType.ROBOTDATA:
                    {
                        if (GameManagerScript.Instance.Stations.Count == 0)
                        {
                            return;
                        }
                        RobotListData data = message.Data.ToObject<RobotListData>();
                        if (GameManagerScript.Instance.Robots.Count == 0)
                        {
                            GameManagerScript.Instance.SpawnRobots(data.Robots);
                            break;
                        }
                        GameManagerScript.Instance.UpdateRobots(data.Robots);
                        break;
                    }
                case MessageType.USEDSTATIONS:
                    {
                        StationListData data = message.Data.ToObject<StationListData>();
                        GameManagerScript.Instance.SpawnStations(data.Stations);
                        break;
                    }
                case MessageType.TRAINLINES:
                    {
                        LinesListData data = message.Data.ToObject<LinesListData>();
                        GameManagerScript.Instance.DrawLinePaths(data.Lines);
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
        WebSocketMessage message = new(id, type, new JObject());
        string jsonMessage = JsonConvert.SerializeObject(message);
        webSocket.SendText(jsonMessage);
    }

    public void Request(WebSocketMessage message)
    {
        string jsonMessage = JsonConvert.SerializeObject(message);
        webSocket.SendText(jsonMessage);
    }
}
