using shared;
using Packages;
using Pathfinding;
using Trains;
using Simulation;

namespace Robots;

public class Robot : RobotData
{
    public bool OnPath { get; set; }
    public string NextExitStationId { get; set; }
    public Dictionary<string, List<Package>> LoadedPackages { get; set; }

    private bool OnPackagePath;

    public Robot(int robotId, string currentStationId)
    {
        RobotId = robotId;
        CurrentStationId = currentStationId;
        Initialize();
    }

    public void Initialize()
    {
        OnTrain = false;
        OnStation = true;
        TrainId = -1;
        TotalPath = [];

        OnPath = false;
        OnPackagePath = false;
        NextExitStationId = "";
        LoadedPackages = [];
    }

    public void Update()
    {
        //Manage the packages
        if (CanRemovePackages())
        {
            RemovePackages();
        }

        if (CanLoadPackages())
        {
            AddPackagesOnPath();
        }

        //All debug to test robots
        //Check if robot has any packages left or is no longer on a path (should always be both true)
        if (!OnPath)
        {
            TravelToNextLoadingStation();
        }

        if (OnPath)
        {
            //when on train travel with the train
            if (OnTrain)
            {
                TravelWithTrain();
            }
            if (!OnTrain)
            {
                //When waiting at station
                if (OnStation)
                {
                    //Check if at final staion
                    if (CurrentStationId == TotalPath.Last().StationIds.Last())
                    {
                        OnPath = false;
                        OnTrain = false;
                        OnPackagePath = false;
                        OnStation = true;
                        Console.WriteLine("Robot " + RobotId + " at Final Station");
                        return;
                    }

                    //when not on train check if train is entering the current station
                    IsTrainAtEnterStation();
                }
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

        DataLogger.AddLog("Robot  " + RobotId + " enterd Train " + TrainId + " at Station " + CurrentStationId);
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

            DataLogger.AddLog("Robot  " + RobotId + " exit Train " + TrainId + " at Station " + CurrentStationId);

            TrainId = -1;
        }
    }

    public void TravelToNextLoadingStation()
    {
        //Check if robot is actually empty
        if (LoadedPackages.Count != 0)
        {
            throw new Exception("Robot is not empty");
        }

        //When robot is at loading station without a path 
        if (PackageManager.WaitingTable.ContainsKey(CurrentStationId))
        {
            DataLogger.AddLog("Robot " + RobotId + " At Loading Station " + CurrentStationId);
            AddNewPackageRoute();
            return;
        }

        //Check if there are any Packages Left
        if (!PackageManager.HasPackagesToLoad())
        {
            return;
        }
        //When robot is not at a loading station travel to the station where most packages are waiting
        string nextStationId = PackageManager.GetStationWithMostPackagesWaiting();
        //get the next path
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, nextStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];
        OnPath = true;

        DataLogger.AddLog("Robot " + RobotId + " At Destination. New Loading Station " + TotalPath.Last().StationIds.Last());

        //Reservate packages
        PackageManager.ReservatePackages(RobotId, TotalPath.Last().StationIds.Last());
    }

    private bool CanRemovePackages()
    {
        if (OnStation)
        {
            if (LoadedPackages.ContainsKey(CurrentStationId))
            {
                return true;
            }
        }

        return false;
    }

    private void RemovePackages()
    {
        if (LoadedPackages.ContainsKey(CurrentStationId))
        {
            DataLogger.AddLog("Robot " + RobotId + " Delivered " + LoadedPackages[CurrentStationId].Count + " Packages At Station " + CurrentStationId);
            Console.WriteLine("Robot " + RobotId + " Delivered " + LoadedPackages[CurrentStationId].Count + " Packages At Station " + CurrentStationId);
            LoadedPackages.Remove(CurrentStationId);
        }
    }

    private bool CanLoadPackages()
    {
        if (!OnTrain && OnStation && OnPath && OnPackagePath)
        {
            if (PackageManager.WaitingTable.ContainsKey(CurrentStationId))
            {
                return true;
            }
        }
        return false;
    }

    private void AddPackagesOnPath()
    {
        //fill remaining space with packages that go to station that are on the way
        PackageManager.FillRemainingSpace(this);
        //Console.WriteLine("Adds packages at station" + currentStation.name);
    }

    private void AddNewPackageRoute()
    {
        //Edge case when robot spawns at Loading Station
        if (!PackageManager.ReservationTable.ContainsKey(Tuple.Create(RobotId, CurrentStationId)))
        {
            PackageManager.ReservatePackages(RobotId, CurrentStationId);
        }

        //Get the Target Station of the Reservated Packages
        string targetStationId = PackageManager.ReservationTable[Tuple.Create(RobotId, CurrentStationId)].Keys.First();

        //get the next path
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, targetStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];
        DataLogger.AddLog("Robot " + RobotId + " new Destiantion from Loading Station " + TotalPath.Last().StationIds.Last());

        //Fill the empty robot with packages that go to the final station
        PackageManager.FillEmptyRobot(this);

        //fill remaining space with packages that go to station that are on the way
        PackageManager.FillRemainingSpace(this);

        OnPath = true;
        OnPackagePath = true;
    }
}