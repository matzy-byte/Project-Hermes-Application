using TrainLines;
using Trains;
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


        /// <summary>
        /// Returns which train must be taken from a station
        /// </summary>
        public Train getTrainFromStartStaion(Station startStation)
        {
            foreach (SubPath subPath in subPaths)
            {
                if (subPath.enterStation == startStation)
                {
                    return TrainManager.getTrainFromLine(subPath.line);
                }
            }

            throw new Exception("Cant find station in path");
        }



        /// <summary>
        /// returns the next exit station on the path
        /// </summary>
        public Station getNextExit(Station startStation)
        {
            foreach (SubPath subPath in subPaths)
            {
                if (subPath.enterStation == startStation)
                {
                    return subPath.exitStation;
                }
            }
            throw new Exception("Cant find station in path");
        }


        /// <summary>
        /// Generates the json string for the path
        /// </summary>
        public string getPathJSON()
        {
            string str = "{\n";
            str += "\"StartStationID\" : " + "\"" + startStation.triasID + "\"" + ",\n";
            str += "\"EndStationID\" : " + "\"" + endStation.triasID + "\"" + ",\n";
            str += "\"SubPaths\" : [\n";
            foreach (SubPath subPath in subPaths)
            {
                str += subPath.getSubPathJSON();
                if (subPath != subPaths.Last())
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