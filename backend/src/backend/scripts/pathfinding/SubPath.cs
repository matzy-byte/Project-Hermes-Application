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
    }









}