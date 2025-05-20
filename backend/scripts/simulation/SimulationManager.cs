using System.Diagnostics;
using shared;
using Robots;
using Trains;
using Pathfinding;
using Packages;

namespace Simulation;

public static class SimulationManager
{
    public static SimulationStateData SimulationState { get; set; } = new();

    //Variables for timings
    public static float actualDeltaTime { get; private set; } = 0f;
    public static float scaledDeltaTime { get; private set; } = 0f;
    private static Stopwatch stopwatch = new Stopwatch();

    private static bool stopSimulationAtLoopEnd = false;

    public static void SimulationLoop()
    {
        while (true)
        {
            while (SimulationState.SimulationRunning)
            {
                if (SimulationState.SimulationPaused != true)
                {
                    StopTime();

                    //Update train positions
                    TrainManager.UpdateAllTrains();
                    RobotManager.UpdateAllRobots();
                    SleepTime();
                }
                if (stopSimulationAtLoopEnd)
                {
                    ClearSimualtion();
                }
            }

            //Check if simulation must be cleared while no simulation is running
            if (stopSimulationAtLoopEnd)
            {
                ClearSimualtion();
            }
        }
    }

    private static void StopTime()
    {
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        actualDeltaTime = elapsed.Milliseconds / 1000f;
        scaledDeltaTime = actualDeltaTime * SimulationSettings.SimulationSettingsParameters.SimulationSpeed;
        SimulationState.SimulationTotalTime += actualDeltaTime;
        SimulationState.SimulationTotalTimeScaled += scaledDeltaTime;
        //Console.WriteLine("Delta Time Simulation: " + actualDeltaTime + "\t Delta Time Scaled: " + scaledDeltaTime + "\t Total Time: " + totalTime);
        stopwatch.Restart();
    }

    private static void SleepTime()
    {
        float cycleTime = 1f / SimulationSettingsGlobal.SimulationLoopsPerSecond;
        float remainingTime = cycleTime - actualDeltaTime;
        if (remainingTime > 0)
        {
            Thread.Sleep((int)(remainingTime * 1000)); // Convert seconds to milliseconds
        }
    }

    private static void InitializeSimulation()
    {
        TrainManager.Initialize();
        DataLogger.AddLog("Simulation Initialized Trains");

        Pathfinder.Initialize();
        DataLogger.AddLog("Simulation Initialized Pathfinding");

        PackageManager.Initialize();
        DataLogger.AddLog("Simulation Initialized Packages");

        RobotManager.Initialize();
        DataLogger.AddLog("Simulation Initialized Robots");

        DataLogger.AddLog("Simulation Initialized");

    }

    private static void ClearSimualtion()
    {
        SimulationState.SimulationRunning = false;
        stopSimulationAtLoopEnd = false;

        Console.WriteLine("Clearing simulation ...");
        TrainManager.AllTrains.Clear();
        DataLogger.AddLog("Simulaiton cleared Trains");

        RobotManager.AllRobots.Clear();
        DataLogger.AddLog("Simulaiton cleared Robots");

        PackageManager.WaitingTable.Clear();
        DataLogger.AddLog("Simulaiton cleared Packages");

        DataLogger.AddLog("Simulation Cleared");
    }

    public static void StartSimulation()
    {
        //Simulation is already running
        if (SimulationState.SimulationRunning)
        {
            return;
        }
        //initalize all things that can change through settings
        Console.WriteLine("Initialize Simulation ...");

        InitializeSimulation();

        //reset time
        SimulationState.SimulationTotalTime = 0;
        SimulationState.SimulationTotalTimeScaled = 0;


        Console.WriteLine("Starting Simulation ...");
        SimulationState.SimulationRunning = true;

        DataLogger.AddLog("Simulation Started");
    }

    public static async Task StopSimulation()
    {
        Console.WriteLine("Stoping Simulation ...");
        stopSimulationAtLoopEnd = true;

        //Wait till simulation stopped
        while (SimulationState.SimulationRunning)
        {
            await Task.Delay(1);
        }
        Console.WriteLine("Simulation Stopped: ");

        DataLogger.AddLog("Simulation Stopped");

    }

    public static void PauseSimulation()
    {
        Console.WriteLine("Pause Simulation");
        SimulationState.SimulationPaused = true;
        DataLogger.AddLog("Simulation Paused");

    }

    public static void ContinueSimulation()
    {
        Console.WriteLine("Continue Simulation");
        SimulationState.SimulationPaused = false;
        DataLogger.AddLog("Simulation Contiued");
    }
}