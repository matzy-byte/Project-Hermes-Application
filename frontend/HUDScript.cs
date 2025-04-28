using Godot;
using System;
using System.Collections.Generic;

public partial class HUDScript : Control
{
    private PackedScene loadingStationEntryScene = ResourceLoader.Load<PackedScene>("res://ui/LoadingStationEntry.tscn");
    public List<LoadingStationEntryScript> loadingStationEntries = [];

    public override void _Ready()
    {
        GetNode<Button>("SimulationSettings/LoadingStationsAdd").Pressed += LoadingStationsAdd;
        GetNode<Button>("ConnectBox/ConnectButton").Pressed += ConnectToUrl;
        GetNode<Button>("SimulationControl/StartButton").Pressed += StartSimulation;
        GetNode<Button>("SimulationControl/PauseButton").Pressed += PauseSimulation;
        GetNode<Button>("SimulationControl/ResumeButton").Pressed += ContinueSimulation;
        GetNode<Button>("SimulationControl/StopButton").Pressed += StopSimulation;
        GetNode<Button>("SimulationControl/Camera1").Pressed += OnCameraOneButtonPressed;
        GetNode<Button>("SimulationControl/Camera2").Pressed += OnCameraTwoButtonPressed;
        GetNode<Button>("SimulationControl/Camera3").Pressed += OnCameraThreeButtonPressed;
    }

    public void SetConnectionURL(string url)
    {
        SessionManager.Instance.SetConnectionURL(url);
    }

    public void ConnectToUrl()
    {
        SessionManager.Instance.ConnectToUrl();
    }

    public void ShowConnectionDebugInfo(Error error)
    {
        GetNode<Label>("ErrorInfoLabel").Text = error.ToString();
        GetNode<Label>("ErrorInfoLabel").Visible = true;
    }

    public void ShowSimmulationSettings()
    {
        GetNode<VBoxContainer>("SimulationSettings").Visible = true;
        GetNode<VBoxContainer>("SimulationControl").Visible = true;
        GetNode<VBoxContainer>("ConnectBox").Visible = false;
    }

    public void StartSimulation()
    {
        GetNode<VBoxContainer>("SimulationSettings").Visible = false;
        GetNode<VBoxContainer>("SimulationControl").Position = new();
        SessionManager.Instance.Request(3, MessageType.STARTSIMULATION);
    }

    public void PauseSimulation()
    {
        SessionManager.Instance.Request(3, MessageType.PAUSESIMULATION);
    }

    public void ContinueSimulation()
    {
        GetNode<VBoxContainer>("SimulationSettings").Visible = false;
        GetNode<VBoxContainer>("SimulationControl").Position = new();
        SessionManager.Instance.Request(3, MessageType.CONTINUESIMULATION);
    }

    public void StopSimulation()
    {
        SessionManager.Instance.Request(3, MessageType.STOPSIMULATION);
    }

    public void OnCameraOneButtonPressed()
    {
        GetParent().GetNode<Camera3D>("Cameras/StaticCam").Current = true;
    }

    public void OnCameraTwoButtonPressed()
    {
        GetParent().GetNode<Camera3D>("Cameras/MoveableCam").Current = true;
    }

    public void OnCameraThreeButtonPressed()
    {
        GetParent().GetNode<Camera3D>("Cameras/FreeCam").Current = true;
    }

    public void RemoveLoadingStationEntry(LoadingStationEntryScript entry)
    {
        loadingStationEntries.Remove(entry);
        entry.QueueFree();
    }

    private void LoadingStationsAdd()
    {
        LoadingStationEntryScript loadingStationEntry = loadingStationEntryScene.Instantiate<LoadingStationEntryScript>();
        GetNode<VBoxContainer>("SimulationSettings/LoadingStations").AddChild(loadingStationEntry);

        string entryID = $"# {Time.GetTicksUsec()}";
        loadingStationEntry.SetID(entryID);
        loadingStationEntries.Add(loadingStationEntry);
    }
}
