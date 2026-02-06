using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Camera;
using CommandLine;
using Godot;
using Interface;
using Newtonsoft.Json;
using shared;
using UI;

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

            Markers markers = JsonConvert.DeserializeObject<Markers>(packetData);
            foreach (WebSocketMessage marker in markers.markers)
            {
                switch (marker.Id)
                {
                    case 31:
                        {
                            GameManagerScript.PauseSimulation(false);
                            break;
                        }
                    case 32:
                        {
                            GameManagerScript.PauseSimulation(true);
                            break;
                        }
                    case 16:
                        {
                            WebSocketMessage speedMessage = new(
                                201,
                                MessageType.SETSIMULATIONSPEED,
                                JsonConvert.SerializeObject(new SimulationSpeedWrapper() { SimulationSpeed = 20.0f })
                            );
                            SessionManager.Instance.Request(speedMessage);
                            break;
                        }
                    case 17:
                        {
                            WebSocketMessage speedMessage = new(
                                201,
                                MessageType.SETSIMULATIONSPEED,
                                JsonConvert.SerializeObject(new SimulationSpeedWrapper() { SimulationSpeed = 75.0f })
                            );
                            SessionManager.Instance.Request(speedMessage);
                            break;
                        }
                    case 18:
                        {
                            WebSocketMessage speedMessage = new(
                                201,
                                MessageType.SETSIMULATIONSPEED,
                                JsonConvert.SerializeObject(new SimulationSpeedWrapper() { SimulationSpeed = 150.0f })
                            );
                            SessionManager.Instance.Request(speedMessage);
                            break;
                        }
                    case 21:
                        {
                            GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraStatic").Current = true;
                            GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(175, 175, 175));
                            GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 100);
                            ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
                            break;
                        }
                    case 22:
                        {
                            GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraMovable").Current = true;
                            GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(150, 150, 150));
                            GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 75);
                            ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
                            break;
                        }
                    case 23:
                        {
                            Vector2 mousePos = GetViewport().GetMousePosition();
                            Camera3D camera = GetViewport().GetCamera3D();
                            Vector3 from = camera.ProjectRayOrigin(mousePos);
                            Vector3 to = from + camera.ProjectRayNormal(mousePos) * 10000f;

                            var spaceState = camera.GetWorld3D().DirectSpaceState;
                            var result = spaceState.IntersectRay(new PhysicsRayQueryParameters3D
                            {
                                From = from,
                                To = to,
                                CollisionMask = 1,
                            });

                            if (result.TryGetValue("collider", out var colliderObj))
                            {
                                var colliderNode = (Node)colliderObj;
                                if (colliderNode != null)
                                {
                                    if (colliderNode is IInteractable interactable)
                                    {
                                        Node3D obj = interactable.Select();
                                        FollowCameraScript followCamera = GetTree().CurrentScene.GetNode("Cameras").GetNode<FollowCameraScript>("CameraFollow");
                                        followCamera.SetTarget(obj);
                                        followCamera.Camera.Current = true;
                                    }
                                }
                            }
                            break;
                        }
                    case 33:
                        {
                            GameManagerScript.Instance.StopSimulation();
                            GetTree().CurrentScene.GetNode<HUDScript>("HUD").NewSimulation();
                            GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraStatic").Current = true;
                            GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(175, 175, 175));
                            GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 100);
                            ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
                            GameManagerScript.Instance.StartSimulation();
                            break;
                        }
                }
            }

            /*switch (message.MessageType)
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
            }*/
        }
    }

    public void SetConnectionURL(string url)
    {
        connectionString = url;
    }

    public void ConnectToUrl()
    {
        Godot.Error error = webSocket.ConnectToUrl("ws://localhost:5001/ws/");
        if (error != Godot.Error.Ok)
        {
            GD.Print("Error connecting to WebSocket: " + error);
            return;
        }
    }

    private class Markers
    {
        public List<WebSocketMessage> markers;
    }

    private class CubeData
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
