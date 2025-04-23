using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SessionManager : Node
{
    public static SessionManager Instance;
    private string connectionString = "ws://localhost:5000/ws/";
    private WebSocketPeer webSocket = new();

    public override void _Ready()
    {
        Instance = this;
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
                string message = packet.GetStringFromUtf8();

                if (message.Contains("TrainPositions"))
                {
                    Dictionary<string, JsonElement> data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message);
                    Train[] trains = data["TrainPositions"].Deserialize<Train[]>();
                    if (GameManager.Instance.trains.Count > 0)
                    {
                        GameManager.Instance.UpdateTrains(trains);
                    }
                    else if (GameManager.Instance.stations.Count > 0)
                    {
                        GameManager.Instance.SpawnTrains(trains);
                    }
                    //Streamed Train positions
                    //GD.Print("Streamed Train Positons \t Message Length: " + message.Length);
                }
                else if (message.Contains("TrainLines"))
                {
                    //train line Request
                    //GD.Print("Requested TrainLine \t Message Length: " + message.Length);
                }
                else if (message.Contains("StationsInLine"))
                {
                    //Stations in line
                    //GD.Print("Requested StationInLine \t Message Length: " + message.Length);
                }
                else if (message.Contains("TrainGeoData"))
                {
                    //Geo Data
                    //GD.Print("Requested TrainGeoData \t Message Length: " + message.Length);
                }
                else if (message.Contains("RobotData"))
                {
                    //Geo Data
                    //GD.Print("Requested RobotData \t Message Length: " + message.Length);
                }
                else if (message.Contains("UsedStations"))
                {
                    Dictionary<string, JsonElement> data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message);
                    Station[] stations = data["UsedStations"].Deserialize<Station[]>();
                    GameManager.Instance.SpawnStations(stations);
                }
            }
        }
    }

    public void Request(string message)
    {
        webSocket.SendText(message);
    }
}


