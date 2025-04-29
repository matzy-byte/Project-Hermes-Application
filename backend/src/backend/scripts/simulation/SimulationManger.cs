using System.Diagnostics;
using json;
using TrainLines;
using System.Threading;
using Trains;
using Robots;
using Packages;
using System.Threading.Tasks;

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

        private static bool stopSimulationAtLoopEnd = false;


        /// <summary>
        /// The loop in wich the main simulation is running
        /// </summary>
        public static void simulationLoop()
        {
            while (true)
            {
                while (isSimulationRunning)
                {
                    if (isSimulationPaused != true)
                    {
                        stopTime();

                        //Update train positions
                        TrainManager.updateAllTrains();
                        RobotManager.updateAllRobots();
                        sleepTime();
                    }
                    if (stopSimulationAtLoopEnd)
                    {
                        clearSimualtion();
                    }
                }

                //Check if simulation must be cleared while no simulation is running
                if (stopSimulationAtLoopEnd)
                {
                    clearSimualtion();
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
            float cycleTime = 1f / SimulationSettingsGlobal.simulationLoopsPerSecond;
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
            //Simulation is already running
            if (isSimulationRunning)
            {
                return;
            }
            //initalize all things that can change through settings
            Console.WriteLine("Initialize Simulaion");
            initializeSimulation();

            //reset time
            totalTime = 0;
            scaledTotalTime = 0;


            Console.WriteLine("Starting Simulation ...");
            isSimulationRunning = true;
        }


        /// <summary>
        /// Initializes everthing that is changeable with settings
        /// </summary>
        private static void initializeSimulation()
        {
            TrainManager.initialize();
            PackageManager.initialize();
            RobotManager.initialize();
        }


        /// <summary>
        /// Stops the simulation after current simulation loop is finished
        /// </summary>
        public static async Task stopSimulation()
        {
            Console.WriteLine("Stoping Simulation ...");
            stopSimulationAtLoopEnd = true;

            //Wait till simulation stopped
            while (isSimulationRunning)
            {
                await Task.Delay(1);
            }
            Console.WriteLine("Simulation Stopped: ");
        }

        /// <summary>
        /// Clears the simulation for a new start
        /// </summary>
        private static void clearSimualtion()
        {
            isSimulationRunning = false;
            stopSimulationAtLoopEnd = false;

            Console.WriteLine("Clearing simulation ...");
            TrainManager.allTrains.Clear();
            RobotManager.allRobots.Clear();
            PackageManager.waitingPackagesLists.Clear();
            PackageManager.loadingStations.Clear();
        }



        /// <summary>
        /// Starts the simulation
        /// </summary>
        public static void pauseSimulation()
        {
            Console.WriteLine("Pause Simulation");
            isSimulationPaused = true;
        }


        /// <summary>
        /// Stops the simulation after current simulation loop is finished
        /// </summary>
        public static void continueSimulation()
        {
            Console.WriteLine("Continue Simulation");
            isSimulationPaused = false;
        }


        /// <summary>
        /// Returns the current settings and state of the simulation
        /// </summary>
        public static string getSimulationStateJSON()
        {
            string str = "{\n";
            str += "\"SimulationState\" : {\n";
            str += "\"SimulationRunning\" : " + isSimulationRunning.ToString().ToLower() + ",\n";
            str += "\"SimulationPaused\" : " + isSimulationPaused.ToString().ToLower() + ",\n";
            str += "\"SimulationTotalTime\" : " + totalTime.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"SimulationTotalTimeScaled\" : " + scaledTotalTime.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"SimulationSpeed\" : " + SimulationSettings.simulationSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"TrainWaitingTimeAtStation\" : " + SimulationSettings.trainWaitingTimeAtStation.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";

            //Loading Stations
            str += "\"LoadingStationIDs\" : [\n";
            foreach (string id in SimulationSettings.loadingStationIds)
            {
                str += "\"" + id + "\"";
                if (id != SimulationSettings.loadingStationIds.Last())
                {
                    str += ",";
                }
                str += "\n";
            }
            str += "],\n";

            //Charging Stations
            str += "\"ChargingStationIDs\" : [\n";
            foreach (string id in SimulationSettings.chargingStationsIds)
            {
                str += "\"" + id + "\"";
                if (id != SimulationSettings.chargingStationsIds.Last())
                {
                    str += ",";
                }
                str += "\n";
            }
            str += "],\n";

            str += "\"StartPackageCount\" : " + SimulationSettings.startPackageCount.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"NumberOfPackagesInRobor\" : " + SimulationSettings.numberOfPackagesInRobot.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"NumberOfRobots\" : " + SimulationSettings.numberOfRobots.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";
            str += "}\n";
            str += "}\n";

            return str;
        }

    }
}