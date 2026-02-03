using System.Diagnostics;
using Godot;
using Newtonsoft.Json;
using shared;

namespace Singletons;

public partial class SessionControlManager : Node
{
    public static SessionControlManager Instance { get; set; }

    private string connectionString = "ws://localhost:5001/ws/";
    private WebSocketPeer webSocket = new();
    private bool connected = false;
    private Process _backendProcess;

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
                connected = true;
            }
            return;
        }

        if (webSocket.GetReadyState() != WebSocketPeer.State.Open)
        {
            GD.Print("Connected to WebSocket: " + connectionString);
            connected = false;
            GameManagerScript.Instance.Reset(true);
            return;
        }

        while (webSocket.GetAvailablePacketCount() > 0)
        {
            var packet = webSocket.GetPacket();
            string packetData = packet.GetStringFromUtf8();

            WebSocketMessage message = JsonConvert.DeserializeObject<WebSocketMessage>(packetData);
            switch (message.MessageType)
            {
                case MessageType.CONTROLHOVER:
                    {
                        var img = Image.LoadFromFile("res://cursor_transparent.png");
                        var tex = ImageTexture.CreateFromImage(img);

                        Input.SetCustomMouseCursor(
                            tex,
                            Input.CursorShape.Arrow,
                            Vector2.Zero
                        );
                        ControlHoverData data = message.Data.ToObject<ControlHoverData>();
                        Input.MouseMode = Input.MouseModeEnum.Confined;
	                    Input.WarpMouse(new Vector2(data.X, data.Y));
                        break;
                    }
                case MessageType.SETSIMULATIONSPEED:
                    {
                        ControlSpeedData data = message.Data.ToObject<ControlSpeedData>();
                        WebSocketMessage speedMessage = new(
                            201,
                            MessageType.SETSIMULATIONSPEED,
                            JsonConvert.SerializeObject(new SimulationSpeedWrapper() { SimulationSpeed = (float)data.SimulationSpeed })
                        );
                        SessionManager.Instance.Request(speedMessage);
                        break;
                    }
                case MessageType.SETTINGS:
                    {
                        SimulationSettingsData data = message.Data.ToObject<SimulationSettingsData>();
                        WebSocketMessage settingsMessage = new(204, MessageType.SETTINGS, JsonConvert.SerializeObject(data));
                        SessionManager.Instance.Request(settingsMessage);
                        break;
                    }
                case MessageType.STARTSIMULATION:
                    {
                        SessionManager.Instance.Request(200, MessageType.STARTSIMULATION);
                        break;
                    }
                case MessageType.STOPSIMULATION:
                    {
                        SessionManager.Instance.Request(205, MessageType.STOPSIMULATION);
                        break;
                    }
                case MessageType.PAUSESIMULATION:
                    {
                        SessionManager.Instance.Request(202, MessageType.PAUSESIMULATION);
                        break;
                    }
                case MessageType.CONTINUESTIMULATION:
                    {
                        SessionManager.Instance.Request(203, MessageType.CONTINUESTIMULATION);
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
        Error error = webSocket.ConnectToUrl("ws://localhost:5001/ws/");
        if (error != Error.Ok)
        {
            GD.Print("Error connecting to WebSocket: " + error);
            return;
        }
    }
}
