using shared;
using Trains;
using Robots;
using Packages;
using Simulation;
using Helper;
using Json;
using Newtonsoft.Json.Linq;
using Logs;

namespace Websocket;

public static class WebSocketMessageGenerator
{
    /// <summary>
    /// Generates the response message by cloning the incoming message and filling its data.
    /// </summary>
    public static WebSocketMessage GetResponseMessage(WebSocketMessage incomingMessage)
    {
        WebSocketMessage responseMessage = incomingMessage.Clone();
        responseMessage.Data = GetMessageData(responseMessage.MessageType);

        return responseMessage;
    }

    /// <summary>
    /// Generates a new streamed data message based on message type.
    /// </summary>
    public static WebSocketMessage GetMessageStreamedData(MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.ROBOTDATA:
                return new WebSocketMessage(1, MessageType.ROBOTDATA, GetMessageData(MessageType.ROBOTDATA));
            case MessageType.TRAINDATA:
                return new WebSocketMessage(0, MessageType.TRAINDATA, GetMessageData(MessageType.TRAINDATA));
            case MessageType.LOG:
                return new WebSocketMessage(2, MessageType.LOG, GetMessageData(MessageType.LOG));
            default:
                throw new Exception("No Stremed Data for this Message Type: " + messageType);
        }
    }

    /// <summary>
    /// Retrieves the message data for a specific message type.
    /// </summary>
    public static JToken GetMessageData(MessageType messageType)
    {
        switch (messageType)
        {
            //Requestable Data
            case MessageType.PACKAGEDATA:
                return GetPackageData();
            case MessageType.SIMULATIONSTATE:
                return GetSimulationState();
            case MessageType.TRAINLINES:
                return GetLines();
            case MessageType.USEDSTATIONS:
                return GetStations();

            //Streamed Data
            case MessageType.ROBOTDATA:
                return GetRobotData();
            case MessageType.TRAINDATA:
                return GetTrainData();
            case MessageType.LOG:
                return GetLog();

            default:
                throw new Exception("No Message Type found");
        }
    }

    /// <summary>
    /// Collects and returns all package data from robots, stations and reservations.
    /// </summary>
    private static JToken GetPackageData()
    {
        List<PackageData> packagesInRobot = RobotManager.AllRobots.SelectMany(robot => robot.LoadedPackages.Values)
                                                            .SelectMany(packageList => packageList).ToList();

        List<PackageData> packagesInStations = PackageManager.WaitingTable.Values.SelectMany(waitingList => waitingList.Values)
                                                                            .SelectMany(packages => packages).ToList();

        List<PackageData> reservatedPackages = PackageManager.ReservationTable.Values.SelectMany(dict => dict.Values)
                                                                                .SelectMany(packages => packages).ToList();

        //All Packages in the Simulation
        List<PackageData> allPackages = packagesInRobot;
        allPackages.AddRange(packagesInStations);
        allPackages.AddRange(reservatedPackages);

        //Convert Packages to PackageData
        PackagesListData dataElement = new PackagesListData
        {
            Packages = allPackages.Select(package => (PackageData)package).ToList(),
        };

        return ToJToken(dataElement);
    }

    /// <summary>
    /// Returns current simulation state data.
    /// </summary>
    public static JToken GetSimulationState()
    {
        return ToJToken(SimulationManager.SimulationState);
    }

    /// <summary>
    /// Collects and returns train line data.
    /// </summary>
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

    /// <summary>
    /// Collects and returns used station data.
    /// </summary>
    private static JToken GetStations()
    {
        List<string> usedStationIds = TrainManager.AllStations;

        List<Station> stations = DataManager.AllStations.Where(station => usedStationIds.Contains(station.StationId)).ToList();

        return ToJToken(new StationListData { Stations = stations.Cast<StationData>().ToList() });
    }

    /// <summary>
    /// Collects and returns robot data.
    /// </summary>
    private static JToken GetRobotData()
    {
        List<Robot> robots = RobotManager.AllRobots;
        List<RobotData> robotDatas = robots.Cast<RobotData>().ToList();
        return ToJToken(new RobotListData { Robots = robotDatas });
    }

    /// <summary>
    /// Collects and returns train data.
    /// </summary>
    public static JToken GetTrainData()
    {
        List<Train> trains = TrainManager.AllTrains;
        List<TrainData> trainDatas = trains.Cast<TrainData>().ToList();

        return ToJToken(new TrainListData { Trains = trainDatas });
    }

    public static JToken GetLog()
    {
        return ToJToken(DataLogger.CollectLog());
    }

    /// <summary>
    /// Converts any object to JToken.
    /// </summary>
    private static JToken ToJToken(object obj)
    {
        return JToken.FromObject(obj);
    }
}
