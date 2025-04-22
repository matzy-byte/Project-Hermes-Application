using System.Security.Cryptography.X509Certificates;
using Helper;
using Simulation;
using TrainLines;

namespace Trains
{
    public class Train
    {
        public int id;
        public Line line;

        public bool inStation;
        public bool drivingForward;
        public Station currentStation;
        public Station nextStation;
        public float distanceBetweenStationsMoved;

        //Timer to manage timings of driving and standing
        private float inStationTimer = 0f;
        private float drivingTimer = 0f;
        private float timeBetweenStations;


        //Dictionaries with times when the train will reach the diffrent stations
        private Dictionary<Station, List<float>> timeTableForward = new Dictionary<Station, List<float>>();
        private Dictionary<Station, List<float>> timeTableBackwards = new Dictionary<Station, List<float>>();


        /// <summary>
        /// Create Instance of train assignt to a train line
        /// </summary>
        public Train(Line line, int id)
        {
            this.id = id;
            this.line = line;
            initializeTrain();
        }


        /// <summary>
        /// Start initialization of a train at the beginning of the simulation
        /// </summary>
        public void initializeTrain()
        {
            inStation = true;
            drivingForward = true;
            distanceBetweenStationsMoved = 0f;
            currentStation = LineManager.findStationWithIdInLine(line.transitInfo.startStationId, line);
            nextStation = findNextStation();
            timeBetweenStations = getTimeBetweenStations();
            generateTimeTables();
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

        /// <summary>
        /// Generates the JSON string with train Position
        /// </summary>
        /// <returns></returns>
        public string getTrainPostionJson()
        {
            string str = "{\n";
            str += "\"TrainID\" : " + id.ToString() + ",\n";
            if (inStation)
            {
                str += "\"Driving\" : false,\n";
                str += "\"InStation\" : true,\n";
            }
            else
            {
                str += "\"Driving\" : true,\n";
                str += "\"InStation\" : false,\n";
            }
            str += "\"DrivingForward\" : " + drivingForward.ToString().ToLower() + ",\n";
            str += "\"CurrentStation\" : " + "\"" + currentStation.triasID + "\"" + ",\n";
            str += "\"NextStation\" : " + "\"" + nextStation.triasID + "\"" + ",\n";
            str += "\"TravelDistance\" : " + distanceBetweenStationsMoved.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"WaitingTime\" : " + (inStationTimer / SimulationSettings.trainWaitingTimeAtStation).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";
            str += "}";
            return str;
        }


        /// <summary>
        /// Generates json string with all stations inside the train line
        /// </summary>
        public string getTrainStationsJSON()
        {
            string str = "{\n";
            str += "\"TrainID\" : " + id.ToString() + ",\n";
            str += "\"Stations\" : [\n";
            foreach (Station station in line.stations)
            {
                str += "\"" + station.triasID + "\"";
                if (station != line.stations.Last())
                {
                    str += ",";
                }
                str += "\n";
            }
            str += "]\n";
            str += "}";
            return str;
        }


        public string getTrainGeoDataJSON()
        {
            string str = "{\n";
            str += "\"TrainID\" : " + id.ToString() + ",\n";
            str += line.geoData.geoDatJSON();
            str += "}";
            return str;
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


        public float calculateTimeBetweenStations(bool drivingForward)
        {
            if (drivingForward)
            {
                return line.transitInfo.travelTime * 60 / line.stations.Length;
            }

            return line.transitInfo.travelTimeReverse * 60 / line.stations.Length;
        }


        private void generateTimeTables()
        {
            // Clear and initialize dictionaries
            timeTableForward.Clear();
            timeTableBackwards.Clear();
            foreach (Station station in line.stations)
            {
                timeTableForward[station] = new List<float>();
                timeTableBackwards[station] = new List<float>();
            }
            bool goingForward = true;

            Station[] stations = line.stations;
            Station startStation = stations.First();
            Station endStation = stations.Last();

            for (int i = 0; i < SimulationSettings.preComputedStopTimes * 2; i++)
            {
                float timeBetweenStations = calculateTimeBetweenStations(goingForward);

                for (int j = 0; j < stations.Length; j++)
                {
                    //Get the correct station
                    Station station;
                    if (goingForward)
                    {
                        station = stations[j];
                    }
                    else
                    {
                        station = stations[stations.Length - 1 - j];
                    }

                    // Add the current time as a departure time
                    if (goingForward)
                    {
                        //Skip station that cant travel forward
                        if (station == endStation)
                        {
                            continue;
                        }
                        //Very first entry is hardcoded
                        if (j == 0 && i == 0)
                        {
                            timeTableForward[station].Add(SimulationSettings.trainWaitingTimeAtStation);
                        }
                        //When at first station it needs time from backwards table
                        else if (station == startStation)
                        {
                            float previousStationExitTime = timeTableBackwards[stations[1]].Last();
                            float entryTime = previousStationExitTime + timeBetweenStations;
                            float exitTime = entryTime + SimulationSettings.trainWaitingTimeAtStation;
                            timeTableForward[station].Add(exitTime);
                        }
                        else
                        {
                            float previousStationExitTime = timeTableForward[stations[j - 1]].Last();
                            float entryTime = previousStationExitTime + timeBetweenStations;
                            float exitTime = entryTime + SimulationSettings.trainWaitingTimeAtStation;
                            timeTableForward[station].Add(exitTime);
                        }
                    }
                    else
                    {
                        //Skip station that cant go backwards
                        if (station == startStation)
                        {
                            continue;
                        }

                        //if at end station time is from forward table
                        if (station == endStation)
                        {
                            //Get exit time from last station that can move forward direction
                            float previousStationExitTime = timeTableForward[stations[stations.Length - 2]].Last();
                            float entryTime = previousStationExitTime + timeBetweenStations;
                            float exitTime = entryTime + SimulationSettings.trainWaitingTimeAtStation;
                            timeTableBackwards[station].Add(exitTime);
                        }
                        else
                        {
                            //Get exit time from previous station
                            int index = stations.Length - 1 - (j - 1);
                            float previousStationExitTime = timeTableBackwards[stations[index]].Last();
                            float entryTime = previousStationExitTime + timeBetweenStations;
                            float exitTime = entryTime + SimulationSettings.trainWaitingTimeAtStation;
                            timeTableBackwards[station].Add(exitTime);
                        }
                    }
                }

                // Switch direction
                goingForward = !goingForward;
            }
        }


        public float nextPickupTime(Station station, bool drivingForward, float time)
        {
            if (drivingForward)
            {
                foreach (float pickupTime in timeTableForward[station])
                {
                    if (pickupTime >= time)
                    {
                        return pickupTime;
                    }
                }
            }

            else
            {
                foreach (float pickupTime in timeTableBackwards[station])
                {
                    if (pickupTime >= time)
                    {
                        return pickupTime;
                    }
                }
            }

            return float.PositiveInfinity;
        }


        public float getTravelTime(Station enterStation, Station exitStation, bool drivingForward)
        {
            float exitTime;
            float enterTime;
            if (drivingForward)
            {
                //Edge cases where exit station is last station
                if (exitStation == line.stations.Last())
                {
                    exitTime = timeTableBackwards[exitStation].First();
                }
                else
                {
                    exitTime = timeTableForward[exitStation].First();
                }
                enterTime = timeTableForward[enterStation].First();
            }
            else
            {
                //edge case where exit station is first station
                if (exitStation == line.stations.First())
                {
                    exitTime = timeTableForward[exitStation][1];
                }
                else
                {
                    exitTime = timeTableBackwards[exitStation].First();
                }
                enterTime = timeTableBackwards[enterStation].First();
            }
            return exitTime - enterTime - SimulationSettings.trainWaitingTimeAtStation;

        }
    }
}