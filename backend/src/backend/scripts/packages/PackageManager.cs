using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using Robots;
using Simulation;
using TrainLines;
using Trains;
//Test
namespace Packages
{
    public static class PackageManager
    {
        /// <summary>
        /// List with all stations that can load a package to the robot
        /// </summary>
        public static List<Station> loadingStations = new List<Station>();

        /// <summary>
        /// List with all package waiting  lists for each loading station
        /// </summary>
        public static List<PackageWaitingList> waitingPackagesLists = new List<PackageWaitingList>();

        private static Random random = new Random();
        /// <summary>
        /// Initializes the Loading Stations and adds some random packages
        /// </summary>
        public static void initialize()
        {
            //Initialize the loading stations
            foreach (string stationID in SimulationSettings.loadingStationIds)
            {
                loadingStations.Add(LineManager.getStationFromId(stationID));
            }

            initializeWaitingList();
            initializePackages();
        }


        /// <summary>
        /// Initializes the packeges waiting at the loading stations
        /// </summary>
        private static void initializePackages()
        {

            foreach (Station station in loadingStations)
            {
                //create the packages
                for (int i = 0; i < SimulationSettings.startPackageCount; i++)
                {
                    Package newPackage = generateNewPackage(station);
                    addPackageToWaitingList(newPackage);
                }
            }
        }

        /// <summary>
        /// Initializes a waiting list for each possible target from each loading station
        /// </summary>
        private static void initializeWaitingList()
        {
            waitingPackagesLists = new List<PackageWaitingList>();

            foreach (Station station in loadingStations)
            {
                waitingPackagesLists.Add(new PackageWaitingList(station));
            }
        }


        /// <summary>
        /// Generates a new package from a starting station with a random target station
        /// </summary>
        private static Package generateNewPackage(Station startStation)
        {
            List<Station> allStations = TrainManager.getAllUsedStations();
            int endStationIndex = random.Next(0, allStations.Count);
            Station endStation = allStations[endStationIndex];
            while (startStation == endStation)
            {
                endStationIndex = random.Next(0, allStations.Count);
                endStation = allStations[endStationIndex];
            }

            return new Package(startStation, endStation, (float)random.NextDouble() * 24.9f + 0.1f);
        }



        /// <summary>
        /// Adds a package to one of the waiting lists
        /// </summary>
        private static void addPackageToWaitingList(Package package)
        {
            //Add the package to the correct waiting list
            foreach (PackageWaitingList waitingList in waitingPackagesLists)
            {
                if (waitingList.loadingStation == package.sourceStation)
                {
                    bool addedPackage = waitingList.addPackageToList(package);
                    if (addedPackage == false)
                    {
                        throw new Exception("Error when adding the package");
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// Returns a dictionary with <Station, int> with the number of packages waiting at each loading station
        /// </summary>
        public static int getNumberOfPackagesWaiting()
        {
            int totalPackagesWaiting = 0;
            foreach (PackageWaitingList packageWaitingList in waitingPackagesLists)
            {
                totalPackagesWaiting += packageWaitingList.getNumberOfPackagesWaiting();
            }
            return totalPackagesWaiting;
        }


        /// <summary>
        /// Gets a dictionary where the loading station is the key and the value is the number of packages waiting
        /// </summary>
        public static Dictionary<Station, int> getWaitingPackagesPerStation()
        {
            Dictionary<Station, int> waitingPackagesPerStation = new Dictionary<Station, int>();
            foreach (PackageWaitingList packageWaitingList in waitingPackagesLists)
            {
                waitingPackagesPerStation.Add(packageWaitingList.loadingStation, packageWaitingList.getNumberOfPackagesWaiting());
            }
            return waitingPackagesPerStation;
        }

        public static Station getStationWithMostPackagesWaiting()
        {
            if (getNumberOfPackagesWaiting() == 0)
            {
                throw new Exception("No packages are waiting");
            }


            //Robot rides to the loading station with the most packages waiting
            Dictionary<Station, int> waitingPackagesCount = getWaitingPackagesPerStation();

            //Get the station where the most packages are waiting
            Station stationWithMostPackages = waitingPackagesCount.OrderByDescending(kvp => kvp.Value).First().Key;
            return stationWithMostPackages;
        }

        private static PackageWaitingList getPackageWaitingListFromLoadingStation(Station station)
        {
            PackageWaitingList packageWaitingList = null;

            foreach (PackageWaitingList packageWaitingListToCheck in waitingPackagesLists)
            {
                if (packageWaitingListToCheck.loadingStation == station)
                {
                    packageWaitingList = packageWaitingListToCheck;
                    break;
                }
            }

            //Check if actual list is found
            if (packageWaitingList == null)
            {
                throw new Exception("No Matching package waiting list for robots station");
            }

            return packageWaitingList;
        }

        /// <summary>
        /// Returns a list with packages to which the robot should drive
        /// </summary>
        public static Station getNewRobotDestination(Robot robot)
        {
            //Station where the robot is
            Station loadingStation = robot.currentStation;

            //Package Waiting list of the current station
            PackageWaitingList packageWaitingList = getPackageWaitingListFromLoadingStation(loadingStation);


            //Get the target station where most packages must go
            return packageWaitingList.targetStationWithMostPackagesWaiting();
        }


        /// <summary>
        /// Fills a empty robot with packages for the destination
        /// </summary>
        public static void fillEmptyRobot(Robot robot)
        {
            Station endStation = robot.path.endStation;
            Station loadingStation = robot.currentStation;
            PackageWaitingList packageWaitingList = getPackageWaitingListFromLoadingStation(loadingStation);

            //Number of packages for end station
            int waitingPackagesCount = packageWaitingList.waitingPackages[endStation].Count;


            if (waitingPackagesCount > SimulationSettings.numberOfPackagesInRobot)
            {
                //Copy the packages from the waiting list
                List<Package> packagesForRobot = new List<Package>();
                for (int i = 0; i < SimulationSettings.numberOfPackagesInRobot; i++)
                {
                    packagesForRobot.Add(packageWaitingList.waitingPackages[endStation][i]);
                }

                //Copy the packages to the robot
                robot.loadedPackages.Add(endStation, packagesForRobot.ToList());

                //Remove the packages from the waiting list
                packageWaitingList.removePackageRange(packagesForRobot.ToList());
            }
            //All packages fit into the robot
            else
            {
                //Copy the packages to the robot
                robot.loadedPackages.Add(endStation, packageWaitingList.waitingPackages[endStation].ToList());
                //Remove the packages from the waiting list
                packageWaitingList.removePackageRange(packageWaitingList.waitingPackages[endStation].ToList());
            }
        }


        /// <summary>
        /// fills a robot with packages that go to station that are on the path
        /// </summary>
        public static void fillRemainingSpace(Robot robot)
        {
            //Check if the robot still has space 
            int remainingSpace = SimulationSettings.numberOfPackagesInRobot - robot.loadedPackages.Values.Sum(packageList => packageList.Count);
            if (remainingSpace <= 0)
            {
                Console.WriteLine("Robot is already full");
                return;
            }

            Station loadingStation = robot.currentStation;
            PackageWaitingList packageWaitingList = getPackageWaitingListFromLoadingStation(loadingStation);
            List<Station> stationsToPass = robot.path.passedStaions;
            int currentStationIndex = stationsToPass.IndexOf(loadingStation);

            //Remove station the robot already passed on the path
            stationsToPass.RemoveRange(0, currentStationIndex);

            //Get the Lists of packages that are on the path
            Dictionary<Station, List<Package>> packagesForStationsOnPath = new Dictionary<Station, List<Package>>();
            foreach (Station station in stationsToPass)
            {
                if (packageWaitingList.waitingPackages.ContainsKey(station))
                {
                    packagesForStationsOnPath.Add(station, packageWaitingList.waitingPackages[station]);
                }
            }
            //order the dictionary by number of packages waiting
            packagesForStationsOnPath = packagesForStationsOnPath.OrderByDescending(kvp => kvp.Value.Count).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            //Get the packages that can go into the robot
            Dictionary<Station, List<Package>> packagesForRobot = getPackagesThatFitInRobot(packagesForStationsOnPath, remainingSpace);

            //Copy the packages to the robot
            foreach (KeyValuePair<Station, List<Package>> entry in packagesForRobot)
            {
                //Copy to robot
                robot.loadedPackages.Add(entry.Key, entry.Value.ToList());
                //remove from waiting list
                packageWaitingList.removePackageRange(entry.Value.ToList());
            }
        }

        /// <summary>
        /// Returns a dictioary with packages that can go into the robot
        /// </summary>
        private static Dictionary<Station, List<Package>> getPackagesThatFitInRobot(Dictionary<Station, List<Package>> packagesOnPath, int remainingSpace)
        {
            Dictionary<Station, List<Package>> packagesForRobot = new Dictionary<Station, List<Package>>();

            //Loop over the dictionary
            foreach (KeyValuePair<Station, List<Package>> entry in packagesOnPath)
            {
                //Check if robot is full
                if (remainingSpace <= 0)
                {
                    break;
                }

                //Check if all packages fit into the robot
                if (remainingSpace - entry.Value.Count < 0)
                {
                    //Copy the packages from the waiting list
                    List<Package> packagesFitInRobot = new List<Package>();

                    for (int i = 0; i < remainingSpace; i++)
                    {
                        packagesFitInRobot.Add(entry.Value[i]);
                    }

                    //Save only the packages that fit into the robot
                    packagesForRobot.Add(entry.Key, packagesFitInRobot);
                    remainingSpace -= packagesFitInRobot.Count;
                }
                //All packages fit into the robot
                else
                {
                    packagesForRobot.Add(entry.Key, entry.Value);
                    remainingSpace -= entry.Value.Count;
                }
            }

            return packagesForRobot;
        }



        /// <summary>
        /// Returns json string of all packages in the simulation
        /// </summary>
        public static string getPackageDataJSON()
        {
            string str = "{\n";
            str += "\"PackageData\" : {\n";
            str += RobotManager.getRobotPackageJSON();
            str += ",";
            str += getPackageAtSationsJSON();
            str += "\n}\n}";

            return str;
        }

        /// <summary>
        /// gets a json string for packages waiting at a station
        /// </summary>
        private static string getPackageAtSationsJSON()
        {
            string str = "\"Stations\" : [\n";

            foreach (Station station in loadingStations)
            {
                str += "{\n";
                str += "\"StationID\" : " + "\"" + station.triasID + "\",\n";
                str += "\"PackageDestinations\" : [\n";
                str += getPackageWaitingListFromLoadingStation(station).getPackageDestinationListJSON();
                str += "]\n";
                str += "}";

                if (station != loadingStations.Last())
                {
                    str += ",";
                }

                str += "\n";
            }

            str += "]";
            return str;
        }

    }
}