using shared;
using Trains;
using Robots;
using Packages;
using Simulation;
using Helper;
using Json;
using Newtonsoft.Json.Linq;

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



    private static JToken GetMessageData(MessageType messageType)
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



    private static JToken GetPackageData()
    {
        List<Package> packagesInRobot = RobotManager.AllRobots.SelectMany(robot => robot.LoadedPackages.Values)
                                                            .SelectMany(packageList => packageList).ToList();

        List<Package> packagesInStations = PackageManager.WaitingTable.Values.SelectMany(waitingList => waitingList.Values)
                                                                            .SelectMany(packages => packages).ToList();

        List<Package> reservatedPackages = PackageManager.ReservationTable.Values.SelectMany(dict => dict.Values)
                                                                                .SelectMany(packages => packages).ToList();

        //All Packages in the Simulation
        List<Package> allPackages = packagesInRobot;
        allPackages.AddRange(packagesInStations);
        allPackages.AddRange(reservatedPackages);

        //Convert Packages to PackageData
        PackagesListData dataElement = new PackagesListData
        {
            Packages = allPackages.Select(package => (PackageData)package).ToList(),
        };

        return ToJToken(dataElement);
    }

    private static JToken GetSimluationState()
    {
        return ToJToken(SimulationManager.SimulationState);
    }

    private static JToken GetLines()
    {
        List<LineData> lineDatas = lineDatas = TrainManager.AllTrains.Select(train => new LineData
        {
            LineName = train.LineName,
            Stations = train.StationIds,
            TrainId = train.TrainId,
            LineColor = DataManager.AllLines.Find(x => x.Name == train.LineName).Color
        }).ToList();

        return ToJToken(new LinesListData { Lines = lineDatas });
    }


    private static JToken GetStations()
    {
        List<string> usedStationIds = TrainManager.AllStations;

        List<Station> stations = DataManager.AllStations.Where(station => usedStationIds.Contains(station.StationId)).ToList();

        return ToJToken(new StationListData { Stations = stations.Cast<StationData>().ToList() });
    }


    private static JToken GetRobotData()
    {
        List<Robot> robots = RobotManager.AllRobots;
        List<RobotData> robotDatas = robots.Cast<RobotData>().ToList();

        return ToJToken(new RobotListData { Robots = robotDatas });
    }

    private static JToken GetTrainData()
    {
        List<Train> trains = TrainManager.AllTrains;
        List<TrainData> trainDatas = trains.Cast<TrainData>().ToList();

        return ToJToken(new TrainListData { Trains = trainDatas });
    }


    private static JToken ToJToken(object obj)
    {
        return JToken.FromObject(obj);
    }
}