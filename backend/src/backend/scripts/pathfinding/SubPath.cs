using Simulation;
using TrainLines;
using Trains;

namespace Pathfinding
{
    public class SubPath
    {
        public Station enterStation;
        public Station exitStation;
        public Line line;
        public float travelTime;

        public float exitTime;

        public SubPath(Station enterStation, Station exitStation, Line line)
        {
            this.enterStation = enterStation;
            this.exitStation = exitStation;
            this.line = line;
        }


        /// <summary>
        /// Calculates the travel time for a sub path based on the enter time at the station
        /// </summary>
        public void calculateTravelTime(float enterTime)
        {
            int enterStationIndex = Array.IndexOf(line.stations, enterStation);
            int exitStationIndex = Array.IndexOf(line.stations, exitStation);
            bool drivingForward = enterStationIndex < exitStationIndex;
            Train train = TrainManager.getTrainFromLine(line);

            float timeWaitingForTrain = train.nextPickupTime(enterStation, drivingForward, enterTime);
            float timeDrivingTrain = train.getTravelTime(enterStation, exitStation, drivingForward);

            travelTime = timeWaitingForTrain + timeDrivingTrain;
            exitTime = enterTime + travelTime;
        }


        public string getSubPathJSON()
        {

            List<Station> stations = LineManager.getBetweenStation(enterStation, exitStation, line);

            string str = "{\n";
            str += "\"TrainID\" : " + TrainManager.getTrainFromLine(line).id + ",\n";
            str += "\"Stations\" : [\n";

            //Loop over the stations used for this sub path
            foreach (Station station in stations)
            {
                str += "\"" + station.triasID + "\"";
                if (station != stations.Last())
                {
                    str += ",";
                }
                str += "\n";
            }

            str += "]\n";
            str += "}";

            return str;
        }



    }









}