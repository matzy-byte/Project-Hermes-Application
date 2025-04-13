using System.Runtime.CompilerServices;
using TrainLines;
namespace Pathfinding
{
    public class LinePath
    {
        public Line startLine;
        public Line destinationLine { get; }
        public List<Line> path { get; }

        public LinePath(Line startLine, Line destinationLine, List<Line> path)
        {
            this.startLine = startLine;
            this.destinationLine = destinationLine;
            this.path = path;
        }
    }
}