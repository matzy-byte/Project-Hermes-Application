using shared;
using Packages;
using Pathfinding;
using Trains;
using Simulation;
using Charging;
using Logs;

namespace Robots;

public class Robot : RobotData
{
    public bool OnPath { get; set; }
    public string NextExitStationId { get; set; }

    private NextDestinationType NextDestination;
    private bool ChargeFull;

    /// <summary>
    /// Constructor for Robot, initializes ID and starting station.
    /// </summary>
    public Robot(int robotId, string currentStationId)
    {
        RobotId = robotId;
        CurrentStationId = currentStationId;
        Initialize();
    }

    /// <summary>
    /// Initializes robot properties and resets state.
    /// </summary>
    public void Initialize()
    {
        //Robot Values
        OnTrain = false;
        OnStation = true;
        TrainId = -1;
        TotalPath = [];
        BatteryCapacity = SimulationSettings.SimulationSettingsParameters.TotalRobotBatteryCapacity;
        NextExitStationId = "";
        LoadedPackages = [];
        OnPath = false;

        NextDestination = NextDestinationType.CHARGING;
        ChargeFull = false;
    }

    /// <summary>
    /// Main update loop for robot behavior, including package management, battery, and path handling.
    /// </summary>
    public void Update()
    {
        //Manage the packages
        ManagePackagesOnPath();

        //Manage the Robot Battery
        ManageBattery();

        //Check if new Packages are spawned else return
        if (NoPackagesLeftMode())
        {
            return;
        }

        //When charging nothing else happens
        if (ChargeFull)
        {
            return;
        }

        //All debug to test robots
        //Check if robot has any packages left or is no longer on a path (should always be both true)
        if (!OnPath)
        {
            ManageNewPath();
        }

        if (OnPath)
        {
            //when on train travel with the train
            if (OnTrain)
            {
                TravelWithTrain();
            }

            //When Robot is at Station and not in Train
            if (!OnTrain && OnStation)
            {
                //Robot is at final Exit
                if (CurrentStationId == TotalPath.Last().StationIds.Last())
                {
                    ManageFinalExit();
                    return;
                }

                //when not on train check if train is entering the current station
                IsTrainAtEnterStation();
            }
        }
    }

    /// <summary>
    /// Handles delivery and loading of packages while on station.
    /// </summary>
    private void ManagePackagesOnPath()
    {
        if (OnStation && LoadedPackages.ContainsKey(CurrentStationId))
        {
            DataLogger.AddLog("Robot " + RobotId + " Delivered " + LoadedPackages[CurrentStationId].Count + " Packages At Station " + CurrentStationId);
            Console.WriteLine("Robot " + RobotId + " Delivered " + LoadedPackages[CurrentStationId].Count + " Packages At Station " + CurrentStationId);
            LoadedPackages.Remove(CurrentStationId);
        }

        if (!OnTrain && OnStation && OnPath && NextDestination == NextDestinationType.PACKAGEDELIVERY
            && PackageManager.WaitingTable.ContainsKey(CurrentStationId))
        {
            PackageManager.FillRemainingSpace(this);
        }
    }

    /// <summary>
    /// Handles actions when arriving at the final station in the current path.
    /// </summary>
    private void ManageFinalExit()
    {
        //Robot no longer on Path
        OnPath = false;
        Console.WriteLine("Robot " + RobotId + " at Final Station");
        DataLogger.AddLog("Robot " + RobotId + " at Final Station");

        switch (NextDestination)
        {
            case NextDestinationType.CHARGING:
                ChargeFull = true;
                DataLogger.AddLog("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                Console.WriteLine("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                break;

            case NextDestinationType.LOADING:
                ChargeFull = true;
                DataLogger.AddLog("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                Console.WriteLine("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                break;

            case NextDestinationType.PACKAGEDELIVERY:
                DataLogger.AddLog("Robot " + RobotId + " deliverd last Package at Station" + CurrentStationId);
                Console.WriteLine("Robot " + RobotId + " deliverd last Package at Station" + CurrentStationId);
                break;
        }
    }

    /// <summary>
    /// Manages battery consumption and charging behavior.
    /// </summary>
    private void ManageBattery()
    {
        IsCharging = ChargeFull || (!OnTrain && OnStation && ChargingManager.ChargingStations.Contains(CurrentStationId)) || NextDestination == NextDestinationType.NOPACKAGESLEFT;

        if (IsCharging)
        {
            ChargingManager.ChargeRobot(this);
        }
        else
        {
            float deltaTimeMinute = SimulationManager.scaledDeltaTime / 60f;
            BatteryCapacity -= deltaTimeMinute * SimulationSettings.SimulationSettingsParameters.RobotIdleBatteryConsumption;
            return;
        }

        if (ChargeFull)
        {
            ChargeFull = BatteryCapacity < SimulationSettings.SimulationSettingsParameters.TotalRobotBatteryCapacity;
            if (!ChargeFull)
            {
                Console.WriteLine("Robot " + RobotId + " Fully Charged at Station " + CurrentStationId);
                DataLogger.AddLog("Robot " + RobotId + " Fully Charged at Station " + CurrentStationId);
            }
        }
    }

    /// <summary>
    /// Checks if the train has arrived at the station and allows the robot to enter.
    /// </summary>
    private void IsTrainAtEnterStation()
    {
        Transfer transfer = (Transfer)TotalPath.Find(x => x.StationIds.First() == CurrentStationId);
        string exitStation = transfer.StationIds.Last();
        Train train = TrainManager.AllTrains[transfer.TrainId];

        int enterStationIndex = train.StationIds.IndexOf(CurrentStationId);
        int exitStationIndex = train.StationIds.IndexOf(exitStation);
        bool drivingForward = enterStationIndex < exitStationIndex;

        if (train.DrivingForward == drivingForward)
        {
            if (train.InStation && train.CurrentStationId == CurrentStationId)
            {
                EnterTrain(train.TrainId);
                NextExitStationId = exitStation;
            }
        }
    }

    /// <summary>
    /// Allows robot to enter the specified train.
    /// </summary>
    private void EnterTrain(int trainId)
    {
        OnTrain = true;
        TrainId = trainId;
        BatteryCapacity -= SimulationSettings.SimulationSettingsParameters.RobotActionBatteryConsumption;

        DataLogger.AddLog("Robot " + RobotId + " entered Train " + TrainId + " at Station " + CurrentStationId);
        Console.WriteLine("Robot " + RobotId + " entered Train " + TrainId + " at Station " + CurrentStationId);
    }

    /// <summary>
    /// Updates robot's state while riding the train.
    /// </summary>
    private void TravelWithTrain()
    {
        Train train = TrainManager.AllTrains[TrainId];
        if (!train.InStation)
        {
            OnStation = false;
            return;
        }
        CurrentStationId = train.CurrentStationId;
        OnStation = true;

        if (CurrentStationId == NextExitStationId)
        {
            OnTrain = false;
            BatteryCapacity -= SimulationSettings.SimulationSettingsParameters.RobotActionBatteryConsumption;

            DataLogger.AddLog("Robot " + RobotId + " exit Train " + TrainId + " at Station " + CurrentStationId);
            Console.WriteLine("Robot " + RobotId + " exit Train " + TrainId + " at Station " + CurrentStationId);
            TrainId = -1;
        }
    }

    /// <summary>
    /// Handles switching robot to next destination type.
    /// </summary>
    private void ManageNewPath()
    {
        switch (NextDestination)
        {
            case NextDestinationType.CHARGING:
                NextDestination = NextDestinationType.LOADING;
                TravelToNextLoadingStation();
                break;

            case NextDestinationType.LOADING:
                NextDestination = NextDestinationType.PACKAGEDELIVERY;
                AddNewPackageRoute();
                break;

            case NextDestinationType.PACKAGEDELIVERY:
                NextDestination = NextDestinationType.CHARGING;
                AddChargingRoute();
                break;

            case NextDestinationType.NOPACKAGESLEFT:
                break;
        }
    }

    /// <summary>
    /// Builds route to deliver packages after loading.
    /// </summary>
    private void AddNewPackageRoute()
    {
        if (!PackageManager.ReservationTable.ContainsKey(Tuple.Create(RobotId, CurrentStationId)) && PackageManager.WaitingTable.ContainsKey(CurrentStationId))
        {
            PackageManager.ReservatePackages(RobotId, CurrentStationId);
        }

        string targetStationId = PackageManager.ReservationTable[Tuple.Create(RobotId, CurrentStationId)].Keys.First();

        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, targetStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];

        DataLogger.AddLog("Robot " + RobotId + " has new Delivery Route to Station " + TotalPath.Last().StationIds.Last());
        Console.WriteLine("Robot " + RobotId + " has new Delivery Route to Station " + TotalPath.Last().StationIds.Last());

        PackageManager.FillEmptyRobot(this);
        PackageManager.FillRemainingSpace(this);
        OnPath = true;
    }

    /// <summary>
    /// Calculates route to next loading station.
    /// </summary>
    public void TravelToNextLoadingStation()
    {
        if (LoadedPackages.Count != 0)
        {
            throw new Exception("Robot is not empty");
        }

        if (!PackageManager.HasPackagesToLoad())
        {
            NextDestination = NextDestinationType.NOPACKAGESLEFT;

            DataLogger.AddLog("Robot " + RobotId + " cant add new Packages because none are Left in the Simulation");
            Console.WriteLine("Robot " + RobotId + " cant add new Packages because none are Left in the Simulation");
            return;
        }

        if (PackageManager.WaitingTable.ContainsKey(CurrentStationId))
        {
            if (PackageManager.HasPackageToLoadAtStation(CurrentStationId))
            {
                DataLogger.AddLog("Robot " + RobotId + " Charged at Station " + CurrentStationId + " where it picks up Packages");
                Console.WriteLine("Robot " + RobotId + " Charged at Station " + CurrentStationId + " where it picks up Packages");
                PackageManager.ReservatePackages(RobotId, CurrentStationId);
                return;
            }
        }

        string nextStationId = PackageManager.GetStationWithMostPackagesWaiting();
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, nextStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];

        DataLogger.AddLog("Robot " + RobotId + " has new Path to Loading Station " + TotalPath.Last().StationIds.Last());
        Console.WriteLine("Robot " + RobotId + " has new Path to Loading Station " + TotalPath.Last().StationIds.Last());
        PackageManager.ReservatePackages(RobotId, TotalPath.Last().StationIds.Last());
        OnPath = true;
    }

    /// <summary>
    /// Adds charging route for robot.
    /// </summary>
    public void AddChargingRoute()
    {
        if (ChargingManager.ChargingStations.Contains(CurrentStationId))
        {
            ChargeFull = true;
            DataLogger.AddLog("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
            Console.WriteLine("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
            return;
        }

        TotalPath = [.. Pathfinder.GetTransfersToChargingStation(CurrentStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];
        DataLogger.AddLog("Robot " + RobotId + " has new route to Charging Station " + TotalPath.Last().StationIds.Last());
        Console.WriteLine("Robot " + RobotId + " has new route to Charging Station " + TotalPath.Last().StationIds.Last());
        OnPath = true;
    }

    /// <summary>
    /// Handles mode when no more packages are left in simulation.
    /// </summary>
    private bool NoPackagesLeftMode()
    {
        if (NextDestination != NextDestinationType.NOPACKAGESLEFT)
        {
            return false;
        }

        if (NextDestination == NextDestinationType.NOPACKAGESLEFT && PackageManager.HasPackagesToLoad())
        {
            NextDestination = NextDestinationType.CHARGING;

            DataLogger.AddLog("Robot " + RobotId + " Can load newly Added Packages");
            Console.WriteLine("Robot " + RobotId + " Can load newly Added Packages");

            return false;
        }
        return true;
    }

    private enum NextDestinationType
    {
        PACKAGEDELIVERY,
        CHARGING,
        LOADING,
        NOPACKAGESLEFT
    }
}