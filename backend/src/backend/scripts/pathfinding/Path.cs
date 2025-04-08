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


        public void getTravelTime(float enterTime)
        {
            totalTavelTime = 0;

            for (int i = 0; i < subPaths.Length; i++)
            {
                if (i == 0)
                {
                    SubPath subPath = subPaths[i];
                    subPath.calculateTravelTime(enterTime);
                    totalTavelTime += subPath.travelTime;
                }
                else
                {
                    SubPath subPath = subPaths[i];
                    subPath.calculateTravelTime(subPaths[i - 1].exitTime);
                    totalTavelTime += subPath.travelTime;
                }
            }
        }
    }
}