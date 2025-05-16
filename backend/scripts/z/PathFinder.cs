namespace Z;

public static class Pathfinder
{
    private static Dictionary<Line, List<Line>> directConnectionTable = [];

    public static void Initialize()
    {
        directConnectionTable = [];

        foreach (Line line in DataManager.AllLines)
        {
            directConnectionTable.Add(line, GetConnectedLines(line));
        }
    }

    public static List<Transfer> GetPath(Transit transit)
    {
        List<Transfer> transfers = [];

        return transfers;
    }

    private static List<Line> GetConnectedLines(Line line)
    {
        List<Line> connectedLines = [];

        foreach (string id in line.Stations)
        {
            foreach (Line lineToCheck in DataManager.AllLines)
            {
                if (lineToCheck == line || connectedLines.Contains(lineToCheck))
                    continue;

                if (lineToCheck.Stations.Contains(id))
                    connectedLines.Add(lineToCheck);
            }
        }

        return connectedLines;
    }
}