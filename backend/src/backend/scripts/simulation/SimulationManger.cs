using System.Diagnostics;
using json;
using TrainLines;
using System.Threading;
using Trains;
using Robots;
using Packages;

namespace Simulation
{
    public static class SimulationManager
    {

        public static bool isSimulationRunning = false;
        public static bool isSimulationPaused = false;

        //Variables for timings
        public static float actualDeltaTime { get; private set; } = 0f;
        public static float scaledDeltaTime { get; private set; } = 0f;
        public static float scaledTotalTime { get; private set; } = 0f;
        public static float totalTime { get; private set; } = 0f;
        private static Stopwatch stopwatch = new Stopwatch();



        //Variables for debugging
        private static Random random = new Random();
        private static int debugTrainIndex = 5;


        /// <summary>
        /// The loop in wich the main simulation is running
        /// </summary>
        private static void simulationLoop()
        {
            while (isSimulationRunning)
            {
                if (isSimulationPaused != true)
                {
                    stopTime();

                    //Update train positions
                    TrainManager.updateAllTrains();
                    RobotManager.updateAllRobots();

                    //TrainManager.allTrains[debugTrainIndex].printTrainInfoDebug();
                    string packageData = PackageManager.getPackageDataJSON();
                    string robotData = RobotManager.getRobotDataJSON();
                    string trainGeoData = TrainManager.getTrainGeoDataJSON();
                    string trainLines = TrainManager.getTrainLinesJSON();
                    string trainPositions = TrainManager.getTrainPositionsJSON();
                    string trainStationInLine = TrainManager.getTrainStationsJSON();
                    string usedStations = TrainManager.getUsedStationsJSON();
                    RobotManager.debugRobot();
                    sleepTime();
                }
            }
        }


        /// <summary>
        /// Stops the time between two simulation loops to manage delta times   
        /// </summary>
        private static void stopTime()
        {
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            actualDeltaTime = elapsed.Milliseconds / 1000f;
            scaledDeltaTime = actualDeltaTime * SimulationSettings.simulationSpeed;
            totalTime += actualDeltaTime;
            scaledTotalTime += scaledDeltaTime;
            //Console.WriteLine("Delta Time Simulation: " + actualDeltaTime + "\t Delta Time Scaled: " + scaledDeltaTime + "\t Total Time: " + totalTime);
            stopwatch.Restart();
        }

        private static void sleepTime()
        {
            float cycleTime = 1f / SimulationSettings.simulationLoopsPerSecond;
            float remainingTime = cycleTime - actualDeltaTime;
            if (remainingTime > 0)
            {
                Thread.Sleep((int)(remainingTime * 1000)); // Convert seconds to milliseconds
            }
        }


        //Methods for starting/Stopping the simulation


        /// <summary>
        /// Starts the simulation
        /// </summary>
        public static void startSimulation()
        {
            isSimulationRunning = true;
            Console.WriteLine("Starting Simulation");
            simulationLoop();
        }


        /// <summary>
        /// Stops the simulation after current simulation loop is finished
        /// </summary>
        public static void stopSimulation()
        {
            isSimulationRunning = false;
        }



        /// <summary>
        /// Starts the simulation
        /// </summary>
        public static void pauseSimulation()
        {
            isSimulationPaused = true;
        }


        /// <summary>
        /// Stops the simulation after current simulation loop is finished
        /// </summary>
        public static void continueSimulation()
        {
            isSimulationPaused = false;
        }


        /// <summary>
        /// Returns the current settings and state of the simulation
        /// </summary>
        public static string getSimulationStateJSON()
        {
            string str = "{}";
            return str;
        }

    }
}