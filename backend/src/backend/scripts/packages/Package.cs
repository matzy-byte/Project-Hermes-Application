using TrainLines;
//Test
namespace Packages
{
    public class Package
    {
        public Station targetStation;
        public Station sourceStation;
        public float weight;

        public Package(Station sourceStation, Station targetStation, float weight)
        {
            this.targetStation = targetStation;
            this.sourceStation = sourceStation;
            this.weight = weight;
        }
    }
}