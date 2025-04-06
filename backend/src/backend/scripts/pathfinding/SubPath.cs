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

        public SubPath(Station enterStation, Station exitStation, Line line)
        {
            this.enterStation = enterStation;
            this.exitStation = exitStation;
            this.line = line;
        }

        public void calculateTravelTime()
        {
            travelTime = getDrivingTime();
        }

        /// <summary>
        /// Gets the time inside train
        /// </summary>
        private float getDrivingTime()
        {
            int enterStationIndex = Array.IndexOf(line.stations, enterStation);
            int exitStationIndex = Array.IndexOf(line.stations, exitStation);
            bool drivingForward = enterStationIndex < exitStationIndex;

            //Get the time between stops 
            Train train = TrainManager.getTrainFromLine(line);
            float timeBetweenStations = train.calculateTimeBetweenStations(drivingForward);

            //Stops on the track without the enter and exit stop
            int segments = Math.Abs(exitStationIndex - enterStationIndex);
            int intermediateStops = segments - 1;

            //Time while driving
            float drivingTime = segments * timeBetweenStations;
            //Time waiting at stops inside of the train
            float waitingTime = intermediateStops * SimulationSettings.trainWaitingTimeAtStation;

            return drivingTime + waitingTime;
        }
    }









}