using Helper;
using Simulation;
using TrainLines;

namespace Trains
{
    public class Train
    {
        public Line line;

        public bool inStation;
        public bool drivingForward;
        public Station currentStation;
        public Station nextStation;
        public float distanceBetweenStationsMoved;
        private int startStationIndex;
        private int endStationIndex;


        //Timer to manage timings of driving and standing
        private float inStationTimer = 0f;
        private float drivingTimer = 0f;
        private float timeBetweenStations;


        /// <summary>
        /// Create Instance of train assignt to a train line
        /// </summary>
        public Train(Line line)
        {
            this.line = line;
            initializeTrain();
        }


        /// <summary>
        /// Start initialization of a train at the beginning of the simulation
        /// </summary>
        public void initializeTrain()
        {
            getStationRange();

            //Check if the train starts in the back
            if (startStationIndex > endStationIndex)
            {
                line.flipLine();
                getStationRange();
            }

            inStation = true;
            drivingForward = true;
            distanceBetweenStationsMoved = 0f;
            currentStation = LineManager.findStationWithIdInLine(line.transitInfo.startStationId, line);
            nextStation = findNextStation();
            timeBetweenStations = getTimeBetweenStations();
        }


        /// <summary>
        /// Method to show that a train entered a station
        /// </summary>
        public void enterStation()
        {
            inStation = true;
            distanceBetweenStationsMoved = 0f;
            currentStation = nextStation;

            //Flip driving direction if station is at end
            if (drivingForward && isCurrentStationEndStation())
            {
                drivingForward = false;
            }
            //Flip driving direction if station is at Start
            else if (drivingForward == false && isCurrentStationStartStation())
            {
                drivingForward = true;
            }

            nextStation = findNextStation();
            timeBetweenStations = getTimeBetweenStations();
            inStationTimer = 0;
        }


        /// <summary>
        /// Method to show that a train is leaving it's station
        /// </summary>
        public void exitStation()
        {
            inStation = false;
            drivingTimer = 0;
        }


        /// <summary>
        /// Checks if the current station is the destionStation
        /// </summary>
        private bool isCurrentStationEndStation()
        {
            string endStationId = line.transitInfo.destinationStartionId;

            return currentStation.triasID == endStationId;
        }

        /// <summary>
        /// Checks if the current station is the start Station
        /// </summary>
        private bool isCurrentStationStartStation()
        {
            string startStationId = line.transitInfo.startStationId;

            return currentStation.triasID == startStationId;
        }

        /// <summary>
        /// Finds the next station in line based on the driving direction and the current station
        /// </summary>
        private Station findNextStation()
        {
            if (drivingForward)
            {
                if (isCurrentStationEndStation())
                {
                    throw new Exception("Cant find station --> Train is at end station with wrong driving direction TrainLine:" + line.name);
                }

                int index = Array.IndexOf(line.stations, currentStation);
                return line.stations[index + 1];
            }
            else
            {
                if (isCurrentStationStartStation())
                {
                    throw new Exception("Cant find station --> Train is at end station with wrong driving direction TrainLine:" + line.name);
                }
                int index = Array.IndexOf(line.stations, currentStation);
                return line.stations[index - 1];
            }
        }


        /// <summary>
        /// Finds the start end end Index of the station in the station line
        /// </summary>
        private void getStationRange()
        {
            Station startStation = LineManager.findStationWithIdInLine(line.transitInfo.startStationId, line);
            Station endStation = LineManager.findStationWithIdInLine(line.transitInfo.destinationStartionId, line);

            startStationIndex = Array.IndexOf(line.stations, startStation);
            endStationIndex = Array.IndexOf(line.stations, endStation);
        }


        /// <summary>
        /// gets the time a train needs between two stations
        /// </summary>
        private float getTimeBetweenStations()
        {
            float totalTime;
            if (drivingForward)
            {
                totalTime = line.transitInfo.travelTime;
            }
            else
            {
                totalTime = line.transitInfo.travelTimeReverse;
            }

            //Get's travel time between stations in seconds (all stations have the same travel time)
            return (totalTime * 60) / line.stations.Length;
        }



        /// <summary>
        /// Updates the train in driving mode
        /// </summary>
        private void updateDriving()
        {
            drivingTimer += SimulationManager.scaledDeltaTime;
            distanceBetweenStationsMoved = drivingTimer / timeBetweenStations;

            //Update when station is reached
            if (distanceBetweenStationsMoved >= 1)
            {
                enterStation();
            }
        }


        /// <summary>
        /// Updates the train when in station
        /// </summary>
        private void updateInStation()
        {
            inStationTimer += SimulationManager.scaledDeltaTime;
            float stationTime01 = inStationTimer / SimulationSettings.trainWaitingTimeAtStation;

            //Update when time is over
            if (stationTime01 >= 1)
            {
                exitStation();
            }
        }


        /// <summary>
        /// Update function to manage Train movement
        /// </summary>
        public void trainUpdate()
        {
            if (inStation == false)
            {
                updateDriving();
            }
            else
            {
                updateInStation();
            }
        }


        public void printTrainInfoDebug()
        {
            string str = line.name + " :";
            str += "\nIn Station: " + inStation + "\n Driving Forward: " + drivingForward;
            str += "\nCurrent Station: " + currentStation.name + "\nNext Station: " + nextStation.name;
            str += "\nDistance to next Station: " + distanceBetweenStationsMoved + "\n Time in station: " + inStationTimer / SimulationSettings.trainWaitingTimeAtStation;
            str += "\nNumber of Stations: " + line.stations.Length;
            str += "\nIndex of CurrentStation: " + Array.IndexOf(line.stations, currentStation);
            Console.WriteLine(str);
        }
    }
}