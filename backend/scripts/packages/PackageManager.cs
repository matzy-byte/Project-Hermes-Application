using Trains;
using Robots;
using Simulation;
using shared;
using Newtonsoft;
using Newtonsoft.Json;

namespace Packages;

using PackageTable = Dictionary<string, Dictionary<string, List<Package>>>;


public static class PackageManager
{
    public static PackageTable WaitingTable = [];

    public static Dictionary<Tuple<int, string>, Dictionary<string, List<Package>>> ReservationTable = [];
    private static Random random = new();

    /// <summary>
    /// Initializes the waiting and reservation tables and populates starting packages.
    /// </summary>
    public static void Initialize()
    {
        WaitingTable = [];
        //Initialize the loading stations
        foreach (string stationId in SimulationSettings.SimulationSettingsParameters.LoadingStationIds)
        {
            if (TrainManager.AllStations.Contains(stationId))
            {
                WaitingTable.Add(stationId, []);
            }
        }

        InitializeWaitingList();
        InitializePackages();
    }

    /// <summary>
    /// Initializes the waiting list structure for all stations.
    /// </summary>
    private static void InitializeWaitingList()
    {
        foreach (KeyValuePair<string, Dictionary<string, List<Package>>> entry in WaitingTable)
        {
            TrainManager.AllStations.Where(id => id != entry.Key).ToList().ForEach(id => entry.Value[id] = []);
        }
    }

    /// <summary>
    /// Populates the waiting table with initial packages at each loading station.
    /// </summary>
    private static void InitializePackages()
    {
        foreach (KeyValuePair<string, Dictionary<string, List<Package>>> entry in WaitingTable)
        {
            for (int i = 0; i < SimulationSettings.SimulationSettingsParameters.StartPackagesCount; i++)
            {
                List<string> stations = [.. TrainManager.AllStations.Where(id => !WaitingTable.ContainsKey(id))];
                string destinationId = stations[random.Next(0, stations.Count)];
                Package package = new(destinationId, entry.Key);
                entry.Value[package.DestinationId].Add(package);
            }

            //Add log File
            DataLogger.AddLog("Added " + SimulationSettings.SimulationSettingsParameters.StartPackagesCount + "Packages to Station: " + entry.Key);
        }
    }

    /// <summary>
    /// Finds the station with the highest number of packages waiting.
    /// </summary>
    public static string GetStationWithMostPackagesWaiting()
    {
        int numberOfPackages = WaitingTable.SelectMany(x => x.Value.Values).Sum(packageList => packageList.Count);
        if (numberOfPackages == 0)
        {
            throw new Exception("No packages are waiting");
        }

        string stationId = WaitingTable.OrderByDescending(kvp => kvp.Value.Values.Sum(packages => packages.Count)).First().Key;
        return stationId;
    }

    /// <summary>
    /// Finds the destination station with the most packages waiting from a given station.
    /// </summary>
    public static string GetDestinationStationWithMostPackagesWaiting(string currentStation)
    {
        return WaitingTable[currentStation].OrderByDescending(kvp => kvp.Value.Count).First().Key;
    }

    /// <summary>
    /// Fills the remaining space in a robot with packages from stations along its path.
    /// </summary>
    public static void FillRemainingSpace(Robot robot)
    {
        int remainingSpace = SimulationSettings.SimulationSettingsParameters.NumberOfPackagesInRobot - robot.LoadedPackages.Values.Sum(packageList => packageList.Count);
        if (remainingSpace <= 0)
        {
            return;
        }

        Dictionary<string, List<Package>> waitingPackagesInStation = WaitingTable[robot.CurrentStationId];
        List<string> stationIdsToPass = [.. robot.TotalPath.SelectMany(x => x.StationIds).Distinct()];
        int currentStationIndex = stationIdsToPass.IndexOf(robot.CurrentStationId);
        stationIdsToPass.RemoveRange(0, currentStationIndex);

        Dictionary<string, List<Package>> packagesOnPath = [];
        foreach (KeyValuePair<string, List<Package>> entry in waitingPackagesInStation)
        {
            if (stationIdsToPass.Contains(entry.Key) && entry.Value.Count > 0)
            {
                packagesOnPath.Add(entry.Key, entry.Value);
            }
        }

        packagesOnPath = packagesOnPath.OrderByDescending(kvp => kvp.Value.Count).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        Dictionary<string, List<Package>> packagesForRobot = GetPackagesThatFitInRobot(packagesOnPath, remainingSpace);

        //Updae Package Info
        packagesForRobot.Values.SelectMany(list => list).ToList().ForEach(x => { x.StationId = ""; x.RobotId = robot.RobotId; });

        int packageSum = packagesForRobot.Sum(kvp => kvp.Value.Count);
        if (packageSum > 0)
        {
            DataLogger.AddLog("Robot " + robot.RobotId + " Added " + robot.LoadedPackages.Sum(kvp => kvp.Value.Count) + " packages to not empty Robot");
            Console.WriteLine("Robot " + robot.RobotId + " Added " + robot.LoadedPackages.Sum(kvp => kvp.Value.Count) + " packages to not empty Robot");
        }

        //Copy the packages to the robot
        foreach (KeyValuePair<string, List<Package>> entry in packagesForRobot)
        {
            //Copy to robot
            if (robot.LoadedPackages.ContainsKey(entry.Key))
            {
                robot.LoadedPackages[entry.Key].Concat([.. entry.Value]);
            }
            else
            {
                robot.LoadedPackages.Add(entry.Key, [.. entry.Value]);
            }

            //remove from waiting list
            foreach (Package package in entry.Value.ToList())
            {
                RemovePackage(package, robot.CurrentStationId);
            }
        }
    }

    /// <summary>
    /// Loads packages into an empty robot from the reservation table.
    /// </summary>
    public static void FillEmptyRobot(Robot robot)
    {
        //Copy values to Robot
        robot.LoadedPackages = ReservationTable[Tuple.Create(robot.RobotId, robot.CurrentStationId)].ToDictionary();

        //Remove the packages from the Reservation Table
        ReservationTable.Remove(Tuple.Create(robot.RobotId, robot.CurrentStationId));

        DataLogger.AddLog("Robot " + robot.RobotId + " Added " + robot.LoadedPackages.Sum(kvp => kvp.Value.Count) + " packages to empty Robot");
        Console.WriteLine("Robot " + robot.RobotId + " Added " + robot.LoadedPackages.Sum(kvp => kvp.Value.Count) + " packages to empty Robot");
    }

    /// <summary>
    /// Selects packages that fit within the robot's remaining space.
    /// </summary>
    private static Dictionary<string, List<Package>> GetPackagesThatFitInRobot(Dictionary<string, List<Package>> packagesOnPath, int remainingSpace)
    {
        Dictionary<string, List<Package>> packagesForRobot = [];

        foreach (KeyValuePair<string, List<Package>> entry in packagesOnPath)
        {
            if (entry.Value.Count <= 0)
            {
                continue;
            }

            if (remainingSpace <= 0)
            {
                break;
            }

            if (remainingSpace - entry.Value.Count >= 0 && entry.Value.Count > 0)
            {
                packagesForRobot.Add(entry.Key, entry.Value);
                remainingSpace -= entry.Value.Count;
                continue;
            }

            List<Package> lastPackages = [];

            for (int i = 0; i < remainingSpace; i++)
            {
                lastPackages.Add(entry.Value[i]);
            }

            packagesForRobot.Add(entry.Key, lastPackages);
            remainingSpace -= lastPackages.Count;
            break;
        }

        return packagesForRobot;
    }

    /// <summary>
    /// Removes a package from the waiting list at a specified station.
    /// </summary>
    private static void RemovePackage(Package package, string loadingStationId)
    {
        WaitingTable[loadingStationId][package.DestinationId].Remove(package);
    }

    /// <summary>
    /// Reserves packages for a robot at a given loading station.
    /// </summary>
    public static void ReservatePackages(int robotId, string loadingStationId)
    {
        Tuple<int, string> robotStation = Tuple.Create(robotId, loadingStationId);
        //Check if  Reservation Table has entry for robot
        if (!ReservationTable.ContainsKey(robotStation))
        {
            ReservationTable.Add(robotStation, []);
        }

        //Get the Station that the robot is taking after picking up the packages
        string destinationWithMostPackages = GetDestinationStationWithMostPackagesWaiting(loadingStationId);
        List<Package> allPackagesForRobot = WaitingTable[loadingStationId][destinationWithMostPackages];

        //only save the packages that are fitting into the robot
        List<Package> packagesForRobot = allPackagesForRobot.Take(SimulationSettings.SimulationSettingsParameters.NumberOfPackagesInRobot).ToList();

        //Save the dictionary in the Reservation table
        ReservationTable[robotStation] = new Dictionary<string, List<Package>> { { destinationWithMostPackages, packagesForRobot } };

        Console.WriteLine("Robot " + robotId + " Reserved " + packagesForRobot.Count + " Packages at Station " + loadingStationId);
        DataLogger.AddLog("Robot " + robotId + " Reserved " + packagesForRobot.Count + " Packages at Station " + loadingStationId);

        //Remove the packages from the waiting list
        foreach (Package package in packagesForRobot)
        {
            RemovePackage(package, loadingStationId);
        }
    }

    /// <summary>
    /// Checks if there are any packages waiting to be loaded.
    /// </summary>
    public static bool HasPackagesToLoad()
    {
        int packageWaitingCount = WaitingTable.Values.SelectMany(innerDict => innerDict.Values)
                                                        .Sum(packageList => packageList.Count);

        return packageWaitingCount > 0;
    }

    /// <summary>
    /// Checks if there are packages waiting at a specific station.
    /// </summary>
    public static bool HasPackageToLoadAtStation(string stationId)
    {
        int packagesWatingCount = WaitingTable[stationId].Values.Sum(packageList => packageList.Count);
        return packagesWatingCount > 0;
    }

    /// <summary>
    /// Adds packages to the waiting table by deserializing JSON data.
    /// </summary>
    public static void AddPackages(string data)
    {
        //Try to deserialize the packages
        try
        {
            PackagesListData newPackages = JsonConvert.DeserializeObject<PackagesListData>(data);

            //Remove Packages where Destination Station is not in Simulation
            List<PackageData> validPackages = newPackages.Packages.Where(p => TrainManager.AllStations.Contains(p.DestinationId)).ToList();

            //Remove Packages where Loading Station are valid
            validPackages = validPackages.Where(p => WaitingTable.ContainsKey(p.StationId)).ToList();

            //Remove Packages where Destination is alo a loading station
            validPackages = validPackages.Where(p => !WaitingTable.ContainsKey(p.DestinationId)).ToList();

            //Make the Robot ID at every Packages to -1
            validPackages = validPackages.Select(p => { p.RobotId = -1; return p; }).ToList();

            //Add the Packages to the Waiting Table
            validPackages.ForEach(p => WaitingTable[p.StationId][p.DestinationId].Add(new Package(p.DestinationId, p.StationId)));

            Console.WriteLine("Packagemanager Added " + validPackages.Count + " Packages");
            DataLogger.AddLog("Packagemanager Added " + validPackages.Count + " Packages");
        }
        catch (Exception e)
        {
            return;
        }
    }
}