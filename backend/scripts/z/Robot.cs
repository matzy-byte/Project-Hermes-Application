using Newtonsoft.Json;
using shared;

namespace Z;

public class Robot : RobotData
{
    public bool OnPath { get; set; }
    public string NextExitStationId { get; set; }
    public Dictionary<string, List<Package>> LoadedPackages { get; set; }

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

        if (CanLoadPackages() && OnPath)
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
            AddNewPackageRoute();
            return;
        }

        //When robot is not at a loading station travel to the station where most packages are waiting
        string nextStationId = PackageManager.GetStationWithMostPackagesWaiting();
        //get the next path
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, nextStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];
        OnPath = true;
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
            Console.WriteLine("Robot " + RobotId +  " Delivered " + LoadedPackages[CurrentStationId].Count + " Packages At Station " + CurrentStationId);
            LoadedPackages.Remove(CurrentStationId);
        }
    }

    private bool CanLoadPackages()
    {
        if (!OnTrain && OnStation)
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
        Console.WriteLine("Robot: " + RobotId + " added packages at station: " + CurrentStationId);

        //Get the station that is the final station of the new path
        string targetStationId = PackageManager.GetDestinationStationWithMostPackagesWaiting(CurrentStationId);

        //get the next path
        TotalPath = [.. Pathfinder.GetTransfers(CurrentStationId, targetStationId, SimulationManager.SimulationState.SimulationTotalTimeScaled).Cast<TransferData>()];
        //Fill the empty robot with packages that go to the final station
        PackageManager.FillEmptyRobot(this);

        //fill remaining space with packages that go to station that are on the way
        PackageManager.FillRemainingSpace(this);

        OnPath = true;
    }
}