using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    public static GameManager Instance;
    public string simulationSettings;

    private PackedScene stationScene = ResourceLoader.Load<PackedScene>("res://assets/station/Station.tscn");
    private PackedScene trainScene = ResourceLoader.Load<PackedScene>("res://assets/train/Train.tscn");

    public List<StationScript> stations = [];
    public List<TrainScript> trains = [];

    public override void _Ready()
    {
        Instance = this;
    }

    public void UpdateSimulationSettings()
    {}

    public void StartSimulation()
    {
        SessionManager.Instance.Request("start");
    }

    public void StopSimulation()
    {
        SessionManager.Instance.Request("stop");
    }

    public void PauseSimulation(bool pause)
    {
        if (pause)
        {
            SessionManager.Instance.Request("pause");
            return;
        }
        SessionManager.Instance.Request("resume");
    }

    public void SpawnStations(Station[] stations)
    {
        foreach (Station station in stations)
        {
            StationScript newStation = stationScene.Instantiate<StationScript>();
            newStation.Setup(station.StationID, station.StationName, station.Long, station.Lat);
            GetTree().CurrentScene.AddChild(newStation);
            newStation.Position = GeoMapper.LatLonToGameCoords(station.Lat / 1e13, station.Long / 1e14);
            this.stations.Add(newStation);
        }
    }

    public void SpawnTrains(Train[] trains)
    {
        foreach (Train train in trains)
        {
            TrainScript newTrain = trainScene.Instantiate<TrainScript>();
            GD.Print(train.CurrentStation);
            StationScript currentStation = stations.First(x => train.CurrentStation.Equals(x.StationID));
            StationScript nextStation = stations.First(x => train.NextStation == x.StationID);
            newTrain.Setup(train.TrainID, train.Driving, train.InStation, train.DrivingForward, currentStation, nextStation, train.TravelDistance, train.WaitingTime);
            GetTree().CurrentScene.AddChild(newTrain);
            newTrain.Position = currentStation.Position;
            GD.Print(newTrain.Position);
            this.trains.Add(newTrain);
        }
    }

    public void UpdateTrains(Train[] trains)
    {
        foreach (TrainScript train in this.trains)
        {
            Train data = trains.First(x => train.TrainID == x.TrainID);
            StationScript currentStation = stations.First(x => data.CurrentStation == x.StationID);
            StationScript nextStation = stations.First(x => data.NextStation == x.StationID);
            train.UpdateRoute(data.Driving, data.InStation, data.DrivingForward, currentStation, nextStation, data.TravelDistance, data.WaitingTime);
            
        }
    }
}
