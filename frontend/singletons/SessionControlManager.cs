using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using shared;
using UI;
using Camera;

namespace Singletons;

public partial class SessionControlManager : Node
{
    public static SessionControlManager Instance { get; set; }

    private string connectionString = "ws://localhost:5001/ws/";
    private WebSocketPeer webSocket = new();
    private bool connected = false;
    private bool resetBlocked = false;
    private float simulationSpeed = 0.0f;
    private int currentCamera = 0;
    private int? _activeViewId = null;
    private readonly HashSet<int> _viewControlIds = [11, 12, 13, 21, 22, 23];

    public override void _Ready()
    {
        Instance = this;
        ConnectToUrl();
    }

    public override void _PhysicsProcess(double delta)
    {
        webSocket.Poll();

        var state = webSocket.GetReadyState();

        if (!connected)
        {
            if (state == WebSocketPeer.State.Open)
            {
                GD.Print("Connected to WebSocket: " + connectionString);
                connected = true;
            }
            return;
        }

        if (state != WebSocketPeer.State.Open)
        {
            GD.Print("Connection lost to WebSocket.");
            connected = false;
            GameManagerScript.Instance.Reset(true);
            return;
        }

        while (webSocket.GetAvailablePacketCount() > 0)
        {
            var packet = webSocket.GetPacket();
            string packetData = packet.GetStringFromUtf8();

            Markers payload = JsonConvert.DeserializeObject<Markers>(packetData);
            if (payload?.markers == null) continue;

            HandleImmediateActions(payload.markers);
            ProcessViewOwnership(payload.markers);
        }
    }

    private void HandleImmediateActions(List<WebSocketMessage> markers)
    {
        foreach (var marker in markers)
        {
            switch (marker.Id)
            {
                case 32:
                    if (GameManagerScript.Instance.Paused)
                    {
                        GD.Print("32: Fortsetzen");
                        GameManagerScript.PauseSimulation(false);
                    }
                    break;

                case 31:
                    if (!GameManagerScript.Instance.Paused)
                    {
                        GD.Print("31: Pause");
                        GameManagerScript.PauseSimulation(true);
                    }
                    break;

                case 16: SetSimulationSpeed(20.0f, "16: Geschwindigkeit 1"); break;
                case 17: SetSimulationSpeed(75.0f, "17: Geschwindigkeit 2"); break;
                case 18: SetSimulationSpeed(150.0f, "18: Geschwindigkeit 3"); break;

                case 33:
                    if (!resetBlocked)
                    {
                        ExecuteResetSequence();
                    }
                    break;
            }
        }
    }

    private void ProcessViewOwnership(List<WebSocketMessage> markers)
    {
        var detectedViewIds = markers
            .Select(m => m.Id)
            .Where(_viewControlIds.Contains)
            .ToList();

        if (_activeViewId.HasValue && !detectedViewIds.Contains(_activeViewId.Value))
        {
            _activeViewId = null;
        }

        if (!_activeViewId.HasValue && detectedViewIds.Count > 0)
        {
            int newId = detectedViewIds.First();
            _activeViewId = newId;
            ExecuteViewAction(newId);
        }
    }

    private void ExecuteViewAction(int id)
    {
        switch (id)
        {
            case 11:
                GD.Print("11: Robot 0 Selected");
                SelectRobot(0);
                break;
            case 12:
                GD.Print("12: Robot 1 Selected");
                SelectRobot(1);
                break;
            case 13:
                GD.Print("13: Robot 2 Selected");
                SelectRobot(2);
                break;
            case 21:
                SetStaticCamera();
                break;
            case 22:
                SetBirdseyeCamera();
                break;
            case 23:
                GD.Print("23: Random Train Selected");
                SelectTrain();
                break;
        }
    }

    #region Helper Methods

    private void SetSimulationSpeed(float speed, string log)
    {
        if (simulationSpeed == speed) return;
        simulationSpeed = speed;
        GD.Print(log);

        WebSocketMessage speedMessage = new(
            201,
            MessageType.SETSIMULATIONSPEED,
            JsonConvert.SerializeObject(new SimulationSpeedWrapper() { SimulationSpeed = simulationSpeed })
        );
        SessionManager.Instance.Request(speedMessage);
    }

    private void SetStaticCamera()
    {
        if (currentCamera == 0) return;
        currentCamera = 0;
        GD.Print("21: Kamera statisch");
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraStatic").Current = true;
        UpdateSpriteVisuals(175, 100);
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
    }

    private void SetBirdseyeCamera()
    {
        if (currentCamera == 1) return;
        currentCamera = 1;
        GD.Print("22: Kamera Vogelperspektive");
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraMovable").Current = true;
        UpdateSpriteVisuals(150, 75);
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
    }

    private void UpdateSpriteVisuals(float scale, float radius)
    {
        var sprites = GetTree().GetNodesInGroup("Sprite");
        foreach (var node in sprites)
        {
            if (node is Sprite3D s) s.Scale = new Vector3(scale, scale, scale);
        }

        var colliders = GetTree().GetNodesInGroup("SpriteCollider");
        foreach (var node in colliders)
        {
            if (node is CollisionShape3D c && c.Shape is SphereShape3D sphere)
            {
                sphere.Radius = radius;
            }
        }
    }

    private void ExecuteResetSequence()
    {
        resetBlocked = true;
        GD.Print("33: Reset triggered");
        GameManagerScript.Instance.StopSimulation();

        GetTree().CurrentScene.GetNode<HUDScript>("HUD").NewSimulation();

        currentCamera = -1;
        SetStaticCamera();

        GameManagerScript.Instance.StartSimulation();
        StartResetTimer();
    }

    public void SetConnectionURL(string url) => connectionString = url;

    public void ConnectToUrl()
    {
        Error error = webSocket.ConnectToUrl(connectionString);
        if (error != Error.Ok) GD.Print("Error connecting to WebSocket: " + error);
    }

    private void SelectRobot(int index)
    {
        var robot = GameManagerScript.Instance.Robots.ElementAtOrDefault(index);
        if (robot == null) return;
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();

        Node3D obj = robot.Select();
        FollowCameraScript followCamera = GetTree().CurrentScene.GetNode("Cameras").GetNode<FollowCameraScript>("CameraFollow");
        followCamera.SetTarget(obj);
        followCamera.Camera.Current = true;
        currentCamera = 2;
    }

    private void SelectTrain()
    {
        if (GameManagerScript.Instance.Trains.Count == 0) return;
        Random random = new();
        var train = GameManagerScript.Instance.Trains[random.Next(0, GameManagerScript.Instance.Trains.Count)];
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();

        Node3D obj = train.Select();
        FollowCameraScript followCamera = GetTree().CurrentScene.GetNode("Cameras").GetNode<FollowCameraScript>("CameraFollow");
        followCamera.SetTarget(obj);
        followCamera.Camera.Current = true;
        currentCamera = 2;
    }

    private async void StartResetTimer()
    {
        await ToSignal(GetTree().CreateTimer(5.0), SceneTreeTimer.SignalName.Timeout);
        resetBlocked = false;
    }

    #endregion

    #region Data Classes
    private class Markers { public List<WebSocketMessage> markers; }
    private class SimulationSpeedWrapper { public float SimulationSpeed { get; set; } }
    #endregion
}