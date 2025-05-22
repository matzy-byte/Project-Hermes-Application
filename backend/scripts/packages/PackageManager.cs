using Trains;
using Robots;
using Simulation;

namespace Packages;

using PackageTable = Dictionary<string, Dictionary<string, List<Package>>>;


public static class PackageManager
{
    public static PackageTable WaitingTable = [];
    private static Random random = new();

    public static void Initialize()
    {
        WaitingTable = [];

        List<string> allStation = TrainManager.AllStations;
        //Initialize the loading stations
        foreach (string stationId in SimulationSettings.SimulationSettingsParameters.LoadingStationIds)
        {
            if (allStation.Contains(stationId) == false)
            {
                throw new Exception("Station to add does not exist in simulation");
            }
            WaitingTable.Add(stationId, []);
        }

        InitializeWaitingList();
        InitializePackages();
    }

    private static void InitializeWaitingList()
    {
        foreach (KeyValuePair<string, Dictionary<string, List<Package>>> entry in WaitingTable)
        {
            TrainManager.AllStations.Where(id => id != entry.Key).ToList().ForEach(id => entry.Value[id] = []);
        }
    }

    private static void InitializePackages()
    {
        foreach (KeyValuePair<string, Dictionary<string, List<Package>>> entry in WaitingTable)
        {
            for (int i = 0; i < SimulationSettings.SimulationSettingsParameters.StartPackagesCount; i++)
            {
                List<string> stations = [.. TrainManager.AllStations.Where(id => id != entry.Key)];
                string destinationId = stations[random.Next(0, stations.Count)];
                Package package = new(destinationId, entry.Key);
                entry.Value[package.DestinationId].Add(package);
            }
        }
    }

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

    public static string GetDestinationStationWithMostPackagesWaiting(string currentStation)
    {
        return WaitingTable[currentStation].OrderByDescending(kvp => kvp.Value.Count).First().Key;
    }

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

    public static void FillEmptyRobot(Robot robot)
    {
        string destinationStationId = robot.TotalPath.Last().StationIds.Last();
        List<Package> waitingPackagesInStation = [.. WaitingTable[robot.CurrentStationId][destinationStationId]];

        if (waitingPackagesInStation.Count > SimulationSettings.SimulationSettingsParameters.NumberOfPackagesInRobot)
        {
            //Copy the packages from the waiting list
            List<Package> packagesForRobot = [];
            for (int i = 0; i < SimulationSettings.SimulationSettingsParameters.NumberOfPackagesInRobot; i++)
            {
                packagesForRobot.Add(waitingPackagesInStation[i]);
            }

            //Update package info
            packagesForRobot.ForEach(x => { x.StationId = ""; x.RobotId = robot.RobotId; });
            //Copy the packages to the robot
            robot.LoadedPackages.Add(destinationStationId, [.. packagesForRobot]);

            //Remove the packages from the waiting list
            foreach (Package package in packagesForRobot)
            {
                RemovePackage(package, robot.CurrentStationId);
            }
        }
        //All packages fit into the robot
        else
        {
            //Update package info
            waitingPackagesInStation.ForEach(x => { x.StationId = ""; x.RobotId = robot.RobotId; });
            //Copy the packages to the robot
            robot.LoadedPackages.Add(destinationStationId, waitingPackagesInStation);
            foreach (Package package in waitingPackagesInStation)
            {
                RemovePackage(package, robot.CurrentStationId);
            }
        }
    }

    private static Dictionary<string, List<Package>> GetPackagesThatFitInRobot(Dictionary<string, List<Package>> packagesOnPath, int remainingSpace)
    {
        Dictionary<string, List<Package>> packagesForRobot = [];

        foreach (KeyValuePair<string, List<Package>> entry in packagesOnPath)
        {
            if (remainingSpace <= 0)
            {
                break;
            }

            if (remainingSpace - entry.Value.Count >= 0)
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

    private static void RemovePackage(Package package, string loadingStationId)
    {
        WaitingTable[loadingStationId][package.DestinationId].Remove(package);
    }
}