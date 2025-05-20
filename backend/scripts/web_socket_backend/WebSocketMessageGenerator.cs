using System.Text.Json;
using shared;
using Trains;
using Robots;
using Packages;
using Simulation;
using Helper;
using Json;

namespace Websocket;

public static class WebSocketMessageGenerator
{
    public static WebSocketMessage GetResponseMessage(WebSocketMessage incomingMessage)
    {
        WebSocketMessage responseMessage = incomingMessage.Clone();
        responseMessage.Data = GetMessageData(responseMessage.MessageType);

        return responseMessage;
    }

    public static WebSocketMessage GetMessageStreamedData(MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.ROBOTDATA:
                return new WebSocketMessage(1, MessageType.ROBOTDATA, GetMessageData(MessageType.ROBOTDATA));
            case MessageType.TRAINDATA:
                return new WebSocketMessage(0, MessageType.TRAINDATA, GetMessageData(MessageType.TRAINDATA));
            default:
                throw new Exception("No Stremed Data for this Message Type: " + messageType);
        }
    }



    private static JsonElement GetMessageData(MessageType messageType)
    {
        switch (messageType)
        {
            //Requestable Data
            case MessageType.PACKAGEDATA:
                return GetPackageData();
            case MessageType.SIMULATIONSTATE:
                return GetSimluationState();
            case MessageType.TRAINLINES:
                return GetLines();
            case MessageType.USEDSTATIONS:
                return GetStations();

            //Streamed Data
            case MessageType.ROBOTDATA:
                return GetRobotData();
            case MessageType.TRAINDATA:
                return GetTrainData();

            default:
                throw new Exception("No Message Type found");
        }
    }



    private static JsonElement GetPackageData()
    {
        List<Package> packagesInRobot = RobotManager.AllRobots.SelectMany(robot => robot.LoadedPackages.Values)
                                                            .SelectMany(packageList => packageList).ToList();

        List<Package> packagesInStations = PackageManager.WaitingTable.Values.SelectMany(waitingList => waitingList.Values)
                                                                            .SelectMany(packages => packages).ToList();

        //All Packages in the Simulation
        List<Package> allPackages = packagesInRobot;
        allPackages.AddRange(packagesInStations);

        //Convert Packages to PackageData
        PackagesListData dataElement = new PackagesListData
        {
            Packages = allPackages.Select(package => (PackageData)package).ToList(),
        };

        return ToJsonElement(dataElement);
    }

    private static JsonElement GetSimluationState()
    {
        return ToJsonElement(SimulationManager.SimulationState);
    }

    private static JsonElement GetLines()
    {
        List<LineData> lineDatas = lineDatas = TrainManager.AllTrains.Select(train => new LineData
        {
            LineName = train.LineName,
            Stations = train.StationIds,
            TrainId = train.TrainId
        }).ToList();

        return ToJsonElement(new LinesListData { Lines = lineDatas });
    }


    private static JsonElement GetStations()
    {
        List<string> usedStationIds = TrainManager.AllStations;

        List<Station> stations = DataManager.AllStations.Where(station => usedStationIds.Contains(station.StationId)).ToList();

        return ToJsonElement(new StationListData { Stations = stations.Cast<StationData>().ToList() });
    }


    private static JsonElement GetRobotData()
    {
        List<Robot> robots = RobotManager.AllRobots;
        List<RobotData> robotDatas = robots.Cast<RobotData>().ToList();

        return ToJsonElement(new RobotListData { Robots = robotDatas });
    }

    private static JsonElement GetTrainData()
    {
        List<Train> trains = TrainManager.AllTrains;
        List<TrainData> trainDatas = trains.Cast<TrainData>().ToList();

        return ToJsonElement(new TrainListData { Trains = trainDatas });
    }


    private static JsonElement ToJsonElement(object obj)
    {
        string json = JsonSerializer.Serialize(obj);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone(); // Clone to make it usable after doc is disposed
    }


}