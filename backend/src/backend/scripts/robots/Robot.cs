using Pathfinding;
using Simulation;
using TrainLines;
using Trains;
using Packages;

namespace Robots
{
    public class Robot
    {
        public int index;
        public Train currentTrain;
        public bool onPath;
        public bool onTrain;
        public bool onStation;
        public Station currentStation;
        public Pathfinding.Path path;
        private Station nextExitStation;

        public Dictionary<Station, List<Package>> loadedPackages;
        public Robot(int index, Pathfinding.Path path)
        {
            this.index = index;
            currentStation = path.startStation;

            onPath = true;
            this.path = path;
            onTrain = false;
            onStation = true;
            currentTrain = null;

            loadedPackages = new Dictionary<Station, List<Package>>();
        }

        public Robot(int index, Station startStation)
        {
            this.index = index;
            currentStation = startStation;
            onPath = false;
            onTrain = false;
            onStation = true;
            currentTrain = null;
            loadedPackages = new Dictionary<Station, List<Package>>();
        }

        public void update()
        {
            //All debug to test robots
            //Check if robot has any packages left or is no longer on a path (should always be both true)
            if (onPath == false)
            {
                Console.WriteLine("Path Finished");
                travelToNextLoadingStation();
            }

            if (onPath)
            {
                //when on train travel with the train
                if (onTrain == true)
                {
                    travelWithTrain();
                }
                if (onTrain == false)
                {
                    //When waiting at station
                    if (onStation)
                    {
                        //Check if at final staion
                        if (isAtFinalExit())
                        {
                            onPath = false;
                            onTrain = false;
                            onStation = true;
                            Console.WriteLine("Robot " + index + " at Final Station");
                            return;
                        }

                        //when not on train check if train is entering the current station
                        isTrainAtEnterStation();
                    }
                }
            }

            //Mange the packages
            if (canRemovePackages())
            {
                removePackages();
            }

            if (canLoadPackages())
            {
                addPackagesOnPath();
            }
        }

        /// <summary>
        /// Robot enters a train
        /// </summary>
        private void enterTrain(Train train)
        {
            onTrain = true;
            currentTrain = train;
        }




        /// <summary>
        /// Returns true when robot is at a station and has packages for that station
        /// </summary>
        private bool canRemovePackages()
        {
            if (onStation)
            {
                if (loadedPackages.ContainsKey(currentStation))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Remove the packages that can be dropped of at station
        /// </summary>
        private void removePackages()
        {
            if (loadedPackages.ContainsKey(currentStation))
            {
                Console.WriteLine("Removed " + loadedPackages[currentStation].Count + " packages at station " + currentStation.name);
                loadedPackages.Remove(currentStation);
            }
        }


        /// <summary>
        /// Checks if train is at a loading station (must be on the station and not just on the train)
        /// </summary>
        private bool canLoadPackages()
        {
            if (onTrain == false && onStation == true)
            {
                if (PackageManager.loadingStations.Contains(currentStation))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add packages when robot is at a loading station and not on the train while still being on a path
        /// Just to add more packages for the current path
        /// </summary>
        private void addPackagesOnPath()
        {
            //fill remaining space with packages that go to station that are on the way
            PackageManager.fillRemainingSpace(this);
            Console.WriteLine("Adds packages at station" + currentStation.name);
        }

        /// <summary>
        /// Robot enters a train
        /// </summary>
        private void exitTrain()
        {
            onTrain = false;
            currentTrain = null;
        }


        /// <summary>
        /// Checks if the train that the robot is taking on the station and driving in the right direciton
        /// </summary>
        private void isTrainAtEnterStation()
        {
            Station startStation = currentStation;
            Station endStation = path.getNextExit(startStation);
            Train train = path.getTrainFromStartStation(currentStation);

            //check if train is traveling at the right direciton
            int enterStationIndex = Array.IndexOf(train.line.stations, startStation);
            int exitStationIndex = Array.IndexOf(train.line.stations, endStation);
            bool drivingForward = enterStationIndex < exitStationIndex;

            if (train.drivingForward == drivingForward)
            {
                //check if train is at the right staion
                if (train.inStation && train.currentStation == startStation)
                {
                    //Robot enters train
                    enterTrain(train);

                    //Save the exit station
                    nextExitStation = endStation;
                }
            }
        }


        /// <summary>
        /// Manages robot when traveling on a train
        /// </summary>
        private void travelWithTrain()
        {
            //Update the current station when train is waiting at station
            if (currentTrain.inStation)
            {
                currentStation = currentTrain.currentStation;
                onStation = true;

                //Check if robot is at the exit station
                isTrainAtExitStation();
            }
            else
            {
                onStation = false;
            }
        }


        /// <summary>
        /// Checks if the train is at exitStation and leaves the train when true
        /// </summary>
        private void isTrainAtExitStation()
        {
            if (currentTrain.inStation)
            {
                //When train is at the exit station
                if (currentTrain.currentStation == nextExitStation)
                {
                    //robot leaves train
                    exitTrain();
                }
            }
        }

        /// <summary>
        /// Checks if the robot is at it's path end station
        /// </summary>
        private bool isAtFinalExit()
        {
            return currentStation == path.endStation;
        }


        /// <summary>
        /// Generates the json string for a robot
        /// </summary>
        public string getRobotJSON()
        {
            string str = "{\n";
            str += "\"RobotID\" : " + index + ",\n";
            str += "\"OnPath\" : " + onPath.ToString().ToLower() + ",\n";
            str += "\"OnTrain\" : " + onTrain.ToString().ToLower() + ",\n";
            str += "\"OnStation\" : " + onStation.ToString().ToLower() + ",\n";
            if (onTrain && currentTrain != null)
            {
                str += "\"TrainID\" : " + currentTrain.id + ",\n";
            }
            else
            {
                str += "\"TrainID\" : null,\n";
            }

            if (onTrain == false || (onTrain && currentTrain != null && currentTrain.inStation))
            {
                str += "\"CurrentStationID\" : " + "\"" + currentStation.triasID + "\"" + ",\n";
            }
            else
            {
                str += "\"CurrentStationID\" : null, \n";
            }

            if (onPath)
            {
                str += "\"TotalPath\" : \n";
                str += path.getPathJSON();
                str += "\n";
            }
            else
            {
                str += "\"TotalPath\" : null\n";
            }

            str += "}\n";

            return str;
        }


        public string debugRobotString()
        {
            string str = "Index: " + index + "\n";
            str += "In Station: " + onStation + "\n";
            str += "On Train: " + onTrain + "\n";
            str += "On Path:" + onPath + "\n";
            str += "Current Station: " + currentStation.name + "\n";
            if (currentTrain != null)
            {
                str += "Current Train: " + currentTrain.line.name + "\n";
            }
            if (onPath)
            {
                str += "Final Station: " + path.endStation.name + "\n";
            }
            else
            {
                str += "Final Station: null\n";

            }
            str += ".\n.\n.\n";


            return str;
        }



        /// <summary>
        /// Sets to robot so it travels to the next loading station
        /// </summary>
        public void travelToNextLoadingStation()
        {
            //Check if robot is actually empty
            if (loadedPackages.Count != 0)
            {
                throw new Exception("Robot is not empty");
            }

            //When robot is at loading station without a path 
            if (PackageManager.loadingStations.Contains(currentStation))
            {
                addNewPackageRoute();
                return;
            }

            //When robot is not at a loading station travel to the station where most packages are waiting
            Station nextStation = PackageManager.getStationWithMostPackagesWaiting();
            //get the next path
            List<Pathfinding.Path> possiblePaths = PathfindingManager.getAllTravelPaths(currentStation, nextStation, SimulationManager.scaledTotalTime);
            path = possiblePaths.First();
            onPath = true;
        }


        /// <summary>
        /// Adds a completly new package route to the robot
        /// </summary>
        public void addNewPackageRoute()
        {
            Console.WriteLine("Add packages at station: " + currentStation.name);

            //Get the station that is the final station of the new path
            Station targetStation = PackageManager.getNewRobotDestination(this);

            //get the next path
            List<Pathfinding.Path> possiblePaths = PathfindingManager.getAllTravelPaths(currentStation, targetStation, SimulationManager.scaledTotalTime);
            path = possiblePaths.First();
            //Fill the empty robot with packages that go to the final station
            PackageManager.fillEmptyRobot(this);

            //fill remaining space with packages that go to station that are on the way
            PackageManager.fillRemainingSpace(this);

            onPath = true;
        }
    }
}