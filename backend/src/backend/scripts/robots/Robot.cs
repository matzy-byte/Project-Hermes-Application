using Pathfinding;
using Simulation;
using TrainLines;
using Trains;

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

        public Robot(int index, Pathfinding.Path path)
        {
            this.index = index;
            currentStation = path.startStation;

            onPath = true;
            this.path = path;
            onTrain = false;
            onStation = true;
            currentTrain = null;
        }


        public void update()
        {
            //All debug to test robots
            //Check if robot is currently on a path
            if (onPath == false)
            {
                //get the next path
                path = RobotManager.getNewPath(currentStation);
                onPath = true;
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
            Train train = path.getTrainFromStartStaion(currentStation);

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
                str += "\"CurrentStaionID\" : " + "\"" + currentStation.triasID + "\"" + ",\n";
            }
            else
            {
                str += "\"CurrentStaionID\" : null, \n";
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
            str += "Final Station: " + path.endStation.name + "\n";
            str += ".\n.\n.\n";


            return str;
        }
    }
}