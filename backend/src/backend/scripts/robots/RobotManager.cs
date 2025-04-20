using Simulation;
using Trains;
using TrainLines;
using Pathfinding;
using Packages;
namespace Robots
{
    public static class RobotManager
    {
        public static List<Robot> allRobots = new List<Robot>();

        /// <summary>
        /// Initializes all robots at a random start station
        /// </summary>
        public static void initialize()
        {
            allRobots = new List<Robot>();
            for (int i = 0; i < SimulationSettings.numberOfRobots; i++)
            {
                //allRobots.Add(new Robot(i, getNewPath(null)));
                allRobots.Add(new Robot(i, PackageManager.loadingStations.First()));
            }
            Console.WriteLine("Number of Robots initialized: " + allRobots.Count);
        }

        /// <summary>
        /// Updates all robots
        /// </summary>
        public static void updateAllRobots()
        {
            foreach (Robot robot in allRobots)
            {
                robot.update();
            }
        }

        public static void debugRobot()
        {
            Console.WriteLine(allRobots[0].debugRobotString());
        }

        /// <summary>
        /// Generates the json string for all robots
        /// </summary>
        /// <returns></returns>
        public static string getRobotDataJSON()
        {
            string str = "{\n";
            str += "\"RobotData\" : [\n";
            foreach (Robot robot in allRobots)
            {
                str += robot.getRobotJSON();
                if (robot != allRobots.Last())
                {
                    str += ",";
                }
                str += "\n";
            }

            str += "]\n";
            str += "}";

            return str;
        }

        /// <summary>
        /// Debug method to generate new Paths
        /// </summary>
        public static Pathfinding.Path getNewPath(Station lastEndStation)
        {
            if (lastEndStation == null)
            {
                List<Station> allStations = TrainManager.getAllUsedStations();
                Random random = new Random();
                List<Pathfinding.Path> allPaths = new List<Pathfinding.Path>();
                Station startStation;
                Station endStation;

                do
                {
                    startStation = allStations[random.Next(0, allStations.Count)];
                    endStation = allStations[random.Next(0, allStations.Count)];

                    if (startStation != endStation)
                    {
                        allPaths = PathfindingManager.getAllTravelPaths(startStation, endStation, SimulationManager.scaledTotalTime);
                    }
                }
                while (allPaths.Count < 1);

                return allPaths.First();
            }
            else
            {
                List<Station> allStations = TrainManager.getAllUsedStations();
                Random random = new Random();
                List<Pathfinding.Path> allPaths = new List<Pathfinding.Path>();
                Station endStation;

                do
                {
                    endStation = allStations[random.Next(0, allStations.Count)];

                    allPaths = PathfindingManager.getAllTravelPaths(lastEndStation, endStation, SimulationManager.scaledTotalTime);

                }
                while (allPaths.Count < 1 && lastEndStation != endStation);

                return allPaths.First();
            }
        }
    }
}