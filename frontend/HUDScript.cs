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
