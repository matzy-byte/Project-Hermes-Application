using System.Diagnostics;
using json;
using TrainLines;
using System.Threading;

namespace Simulation
{
    public static class SimulationManager
    {

        public static bool isSimulationRunning = false;


        //Variables for timings
        public static float actualDeltaTime { get; private set; } = 0f;
        public static float scaledDeltaTime { get; private set; } = 0f;
        public static float totalTime { get; private set; } = 0f;
        private static Stopwatch stopwatch = new Stopwatch();



        //Variables for debugging
        private static Random random = new Random();


        /// <summary>
        /// Starts the simulation
        /// </summary>
        public static void startSimulation()
        {
            isSimulationRunning = true;
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


                //Simulate Work
                System.Threading.Thread.Sleep(random.Next(10, 100));
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
            Console.WriteLine("Delta Time Simulation: " + actualDeltaTime + "\t Delta Time Scaled: " + scaledDeltaTime + "\t Total Time: " + totalTime);
            stopwatch.Reset();
            stopwatch.Start();
        }
    }
}