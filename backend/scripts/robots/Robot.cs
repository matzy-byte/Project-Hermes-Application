using shared;
using Packages;
using Pathfinding;
using Trains;
using Simulation;
using Charging;

namespace Robots;

public class Robot : RobotData
{
    public bool OnPath { get; set; }
    public string NextExitStationId { get; set; }
    public Dictionary<string, List<Package>> LoadedPackages { get; set; }

    private NextDestinationType NextDestination;

    public Robot(int robotId, string currentStationId)
    {
        RobotId = robotId;
        CurrentStationId = currentStationId;
        Initialize();
    }

    public void Initialize()
    {
        //Robot Values
        OnTrain = false;
        OnStation = true;
        TrainId = -1;
        TotalPath = [];
        BatteryCapacaty = SimulationSettings.SimulationSettingsParameters.TotalRobotBatteryCapacity;
        NextExitStationId = "";
        LoadedPackages = [];
        OnPath = false;

        NextDestination = NextDestinationType.CHARGING;
    }

    public void Update()
    {
        //Manage the packages
        ManagePackagesOnPath();

        //Manage the Robot Battery
        ManageBattery();

        //When charging nothing else happens
        if (IsCharging)
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


    private void ManageFinalExit()
    {
        //Robot no longer on Path
        OnPath = false;
        Console.WriteLine("Robot " + RobotId + " at Final Station");
        DataLogger.AddLog("Robot " + RobotId + " at Final Station");

        switch (NextDestination)
        {
            case NextDestinationType.CHARGING:
                //When Robot is at charging station it Charges Full
                IsCharging = true;
                DataLogger.AddLog("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                Console.WriteLine("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                break;

            case NextDestinationType.LOADING:
                //When Robot is at Loaing Station -> Robot first Charges to 100%
                IsCharging = true;
                DataLogger.AddLog("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                Console.WriteLine("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
                break;
            case NextDestinationType.PACKAGEDELIVERY:
                //Robot is now on Path to a charging station
                DataLogger.AddLog("Robot " + RobotId + " deliverd last Package at Station" + CurrentStationId);
                Console.WriteLine("Robot " + RobotId + " deliverd last Package at Station" + CurrentStationId);
                break;
        }
    }


    private void ManageBattery()
    {
        bool removeBattery = OnTrain || (OnStation && !ChargingManager.ChargingStations.Contains(CurrentStationId));

        //Remove Battery when robot is on idle
        if (removeBattery)
        {
            //Delta Time in Minutes
            float deltaTimeMinute = SimulationManager.scaledDeltaTime / 60f;
            //Remove Battery Capacaty
            BatteryCapacaty -= deltaTimeMinute * SimulationSettings.SimulationSettingsParameters.RobotIdleBatteryConsumption;
            return;
        }

        //Charge Robot when robot is on Path but staying at charging station
        if (!OnTrain && OnStation && ChargingManager.ChargingStations.Contains(CurrentStationId) && !IsCharging)
        {
            ChargingManager.ChargeRobot(this);
            return;
        }

        //Robot is at a charging for charging, Charge Robot to 100%
        if (IsCharging)
        {
            ChargingManager.ChargeRobot(this);
            //When robot is fully charged make it no longer charge
            IsCharging = BatteryCapacaty < SimulationSettings.SimulationSettingsParameters.TotalRobotBatteryCapacity;

            if (!IsCharging)
            {
                Console.WriteLine("Robot " + RobotId + " Fully Charged at Station " + CurrentStationId);
                DataLogger.AddLog("Robot " + RobotId + " Fully Charged at Station " + CurrentStationId);
            }
        }
    }


    private void IsTrainAtEnterStation()
    {
        Transfer transfer = (Transfer)TotalPath.Find(x => x.StationIds.First() == CurrentStationId);
        string exitStation = transfer.StationIds.Last();
        Train train = TrainManager.AllTrains[transfer.TrainId];

        //check if train is traveling at the right direciton
        int enterStationIndex = train.StationIds.IndexOf(CurrentStationId);
        int exitStationIndex = train.StationIds.IndexOf(exitStation);
        bool drivingForward = enterStationIndex < exitStationIndex;

        if (train.DrivingForward == drivingForward)
        {
            //check if train is at the right staion
            if (train.InStation && train.CurrentStationId == CurrentStationId)
            {
                //Robot enters train
                EnterTrain(train.TrainId);

                //Save the exit station
                NextExitStationId = exitStation;
            }
        }
    }

    private void EnterTrain(int trainId)
    {
        OnTrain = true;
        TrainId = trainId;

        //Remove Battery Capacaty at train enter
        BatteryCapacaty -= SimulationSettings.SimulationSettingsParameters.RobotActionBatteryConsumption;

        DataLogger.AddLog("Robot " + RobotId + " entered Train " + TrainId + " at Station " + CurrentStationId);
        Console.WriteLine("Robot " + RobotId + " entered Train " + TrainId + " at Station " + CurrentStationId);
    }

    private void TravelWithTrain()
    {
        Train train = TrainManager.AllTrains[TrainId];
        //Update the current station when train is waiting at station
        if (!train.InStation)
        {
            OnStation = false;
            return;
        }
        CurrentStationId = train.CurrentStationId;
        OnStation = true;

        //When train is at the exit station
        if (CurrentStationId == NextExitStationId)
        {
            //robot leaves train
            OnTrain = false;

            //Remove Battery Capacaty at train exit
            BatteryCapacaty -= SimulationSettings.SimulationSettingsParameters.RobotActionBatteryConsumption;

            DataLogger.AddLog("Robot " + RobotId + " exit Train " + TrainId + " at Station " + CurrentStationId);
            Console.WriteLine("Robot " + RobotId + " exit Train " + TrainId + " at Station " + CurrentStationId);
            TrainId = -1;
        }
    }

    private void ManageNewPath()
    {
        //Check where robot should travel next
        switch (NextDestination)
        {
            case NextDestinationType.CHARGING:

                //Set the Robots next destination to loading Packages
                NextDestination = NextDestinationType.LOADING;

                //Get the path to the next Loading Station
                TravelToNextLoadingStation();
                break;

            case NextDestinationType.LOADING:
                //Set the Robots next destination to loading Packages
                NextDestination = NextDestinationType.PACKAGEDELIVERY;

                //Get the new Packages and Route to Deliver
                AddNewPackageRoute();
                break;

            case NextDestinationType.PACKAGEDELIVERY:
                //After Package Delivery Robot goes to Charging
                NextDestination = NextDestinationType.CHARGING;

                AddChargingRoute();
                //Add the Path to the next Charging Station
                break;
        }
    }

    private void AddNewPackageRoute()
    {
        //Edge case when robot spawns at Loading Station
        if (!PackageManager.ReservationTable.ContainsKey(Tuple.Create(RobotId, CurrentStationId)) && PackageManager.WaitingTable.ContainsKey(CurrentStationId))
        {
            PackageManager.ReservatePackages(RobotId, CurrentStationId);
        }

        //Get the Target Station of the Reservated Packages
        string targetStationId = PackageManager.ReservationTable[Tuple.Create(RobotId, CurrentStationId)].Keys.First();

        //get the next path
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, targetStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];

        DataLogger.AddLog("Robot " + RobotId + " has new Delivery Route to Station " + TotalPath.Last().StationIds.Last());
        Console.WriteLine("Robot " + RobotId + " has new Delivery Route to Station " + TotalPath.Last().StationIds.Last());

        //Fill the empty robot with packages that go to the final station
        PackageManager.FillEmptyRobot(this);

        //fill remaining space with packages that go to station that are on the way
        PackageManager.FillRemainingSpace(this);

        //Robot is now on a Path
        OnPath = true;
    }

    public void TravelToNextLoadingStation()
    {
        //Check if robot is actually empty
        if (LoadedPackages.Count != 0)
        {
            throw new Exception("Robot is not empty");
        }

        //Check if any Packages are left in the simulation
        if (!PackageManager.HasPackagesToLoad())
        {
            //Set robot in Charging mode
            IsCharging = true;

            //If any Packages are spwaned Robot Picks them up
            return;
        }

        //Case where Robot Charged at a loading station
        if (PackageManager.WaitingTable.ContainsKey(CurrentStationId))
        {
            //Check if the Station where the robot is has a package to Deliver
            if (PackageManager.HasPackageToLoadAtStation(CurrentStationId))
            {
                DataLogger.AddLog("Robot " + RobotId + " Charged at Station " + CurrentStationId + " where it picks up Packages");
                Console.WriteLine("Robot " + RobotId + " Charged at Station " + CurrentStationId + " where it picks up Packages");
                //Robot reservates Packages to pick up next simulation loop
                PackageManager.ReservatePackages(RobotId, CurrentStationId);
                return;
            }
        }

        //When robot is not at a loading station travel to the station where most packages are waiting
        string nextStationId = PackageManager.GetStationWithMostPackagesWaiting();
        //get the next path
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, nextStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];

        DataLogger.AddLog("Robot " + RobotId + " has new Path to Loading Station " + TotalPath.Last().StationIds.Last());
        Console.WriteLine("Robot " + RobotId + " has new Path to Loading Station " + TotalPath.Last().StationIds.Last());
        //Reservate packages
        PackageManager.ReservatePackages(RobotId, TotalPath.Last().StationIds.Last());

        //Set Robot on Path
        OnPath = true;
    }

    public void AddChargingRoute()
    {
        //If Robot is at Charging Station set it in Charging mode
        if (ChargingManager.ChargingStations.Contains(CurrentStationId))
        {
            IsCharging = true;
            DataLogger.AddLog("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
            Console.WriteLine("Robot " + RobotId + " is Charging at Station " + CurrentStationId);
            return;
        }

        //Find the path to the next Charging station
        TotalPath = [.. Pathfinder.GetTransfersToChargingStation(CurrentStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];
        DataLogger.AddLog("Robot " + RobotId + " has new route to Charging Station " + TotalPath.Last().StationIds.Last());
        Console.WriteLine("Robot " + RobotId + " has new route to Charging Station " + TotalPath.Last().StationIds.Last());
        OnPath = true;
    }


    private enum NextDestinationType
    {
        PACKAGEDELIVERY,
        CHARGING,
        LOADING
    }
}