using TrainLines;
namespace Pathfinding
{
    public class Path
    {
        public Station startStation;
        public Station endStation;

        public SubPath[] subPaths;
        public float totalTavelTime;

        public Path(Station startStation, Station endStation, SubPath[] subPaths)
        {
            this.startStation = startStation;
            this.endStation = endStation;
            this.subPaths = subPaths;
        }


        public void getTravelTime()
        {
            totalTavelTime = 0;
            foreach (SubPath subPath in subPaths)
            {
                subPath.calculateTravelTime();
                totalTavelTime += subPath.travelTime;
            }
        }
    }
}