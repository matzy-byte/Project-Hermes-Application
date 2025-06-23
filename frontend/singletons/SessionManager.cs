using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using shared;
using UI;

namespace Singletons;

public partial class SessionManager : Node
{
    public static SessionManager Instance { get; set; }

    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();
    private bool connected = false;
    private Process _backendProcess;

    public override void _Ready()
    {
        StartBackend();
        Window window = GetWindow();
        window.Mode = Window.ModeEnum.Fullscreen;
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
                Request(101, MessageType.USEDSTATIONS);
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
                case MessageType.LOG:
                    {
                        List<string> data = message.Data.ToObject<List<string>>();
                        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ChatLog.WriteLog(data);
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

    public override void _ExitTree()
    {
        StopBackend();
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

    private void StartBackend()
    {
        string gameDir = Path.GetDirectoryName(OS.GetExecutablePath());
        string backendDir = Path.Combine(gameDir, "..", "backend");
        string dllPath = Path.Combine(backendDir, "backend.dll");
        string exePath = Path.Combine(backendDir, "backend.exe");

        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = backendDir,
            UseShellExecute = true,
            CreateNoWindow = true
        };

        if (File.Exists(exePath))
        {
            startInfo.FileName = exePath;
        }
        else if (File.Exists(dllPath))
        {
            startInfo.FileName = "dotnet";
            startInfo.Arguments = $"\"{dllPath}\"";
        }
        else
        {
            GD.PrintErr("No backend executable found.");
            return;
        }

        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        try
        {
            _backendProcess = Process.Start(startInfo);
            GD.Print("Backend started successfully.");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to start backend: {ex.Message}");
        }
    }

    private void StopBackend()
    {
        if (_backendProcess != null && !_backendProcess.HasExited)
        {
            _backendProcess.Kill();
            _backendProcess.Dispose();
            _backendProcess = null;
        }
    }
}
