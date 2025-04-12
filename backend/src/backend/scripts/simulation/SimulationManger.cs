using System.Diagnostics;
using json;
using TrainLines;
using System.Threading;
using Trains;

namespace Simulation
{
    public static class SimulationManager
    {

        public static bool isSimulationRunning = false;


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
        /// The loop in wich the main simulation is running
        /// </summary>
        private static void simulationLoop()
        {
            while (isSimulationRunning)
            {
                stopTime();

                //Update train positions
                TrainManager.updateAllTrains();
                TrainManager.allTrains[debugTrainIndex].printTrainInfoDebug();

                sleepTime();
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
    }
}