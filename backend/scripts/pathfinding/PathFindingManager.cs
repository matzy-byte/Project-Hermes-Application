using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using TrainLines;
using Trains;
using Newtonsoft;
using Newtonsoft.Json;

namespace Pathfinding
{
    public static class PathfindingManager
    {
        private static Dictionary<Line, List<Line>> directConnectionTable = new Dictionary<Line, List<Line>>();

        /// <summary>
        /// Initializes a table with connections between lines
        /// </summary>
        public static void initializePathFinding()
        {
            Console.WriteLine("Start Path finding initialization ...");

            foreach (Line line in LineManager.usableLines)
            {
                directConnectionTable.Add(line, getLinesConnectedToLine(line));
            }

            //   initializeAllConnectionsTable();
            //initializeStationToStationPathTable();

            Console.WriteLine("Path finding initialized");
        }



        /// <summary>
        /// Method to debug if all stations are connected
        /// </summary>
        private static void initializeStationToStationPathTable()
        {

            List<(Station, Station)> stationPairs = new List<(Station, Station)>();

            foreach (Station startStation in TrainManager.getAllUsedStations())
            {
                foreach (Station endStation in TrainManager.getAllUsedStations())
                {
                    if (startStation != endStation)
                    {
                        stationPairs.Add((startStation, endStation));
                    }
                }
            }

            ConcurrentDictionary<(Station, Station), int> stationMap = new ConcurrentDictionary<(Station, Station), int>();


            Parallel.For(0, stationPairs.Count, i =>
            {
                var pair = stationPairs[i];
                int count = getAllTravelPaths(pair.Item1, pair.Item2, 0).Count;
                stationMap[(pair.Item1, pair.Item2)] = count;

                // Optional: log progress every 100 iterations to avoid flooding the console
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Processed {stationMap.Count}/{stationPairs.Count}");
                }
            });

            string jsonString = JsonConvert.SerializeObject(stationMap);
            Console.WriteLine(jsonString);
        }



        /// <summary>
        /// Returns all posible paths between two station.
        /// </summary>
        public static List<Path> getAllTravelPaths(Station startStation, Station endStation, float enterTime)
        {
            List<LinePath> linePaths = getRelavantLinePaths(startStation, endStation);

            List<Path> travelPaths = new List<Path>();
            foreach (LinePath linePath in linePaths)
            {
                Path path = convertLinePathToPath(linePath, startStation, endStation);
                if (path != null)
                {
                    //Create the travel path
                    travelPaths.Add(path);
                    //Calculate time of travel path
                    travelPaths.Last().getTravelTime(enterTime);
                }
            }

            //Order the list by travel time
            return travelPaths.OrderBy(path => path.totalTavelTime).ToList();
        }


        /// <summary>
        /// Get's all possible Linepaths from a start to a end station. 
        /// Without backtracking inside one Linepath
        /// </summary>
        public static List<LinePath> getRelavantLinePaths(Station startStation, Station endStation)
        {
            List<LinePath> relavantPaths = new List<LinePath>();

            List<Line> startLines = LineManager.getLinesWithStation(startStation);
            List<Line> endLines = LineManager.getLinesWithStation(endStation);

            //Get paths with single line connection
            foreach (Line line in startLines)
            {
                if (endLines.Contains(line))
                {
                    relavantPaths.Add(new LinePath(line, line, new List<Line>() { line }));
                }
            }


            int maxLines = 3;

            //Search all not direct connections
            foreach (Line startLine in startLines)
            {
                foreach (Line endLine in endLines)
                {
                    if (startLine == endLine) continue;

                    Queue<List<Line>> queue = new Queue<List<Line>>();
                    queue.Enqueue(new List<Line> { startLine });

                    while (queue.Count > 0)
                    {
                        List<Line> currentPath = queue.Dequeue();
                        Line lastLine = currentPath.Last();

                        if (lastLine == endLine)
                        {
                            relavantPaths.Add(new LinePath(startLine, endLine, new List<Line>(currentPath)));
                            continue;
                        }

                        if (currentPath.Count >= maxLines)
                        {
                            continue; // Don't go deeper than maxLines
                        }

                        if (!directConnectionTable.TryGetValue(lastLine, out var connectedLines)) continue;

                        foreach (var nextLine in connectedLines)
                        {
                            if (!currentPath.Contains(nextLine)) // avoid cycles
                            {
                                var newPath = new List<Line>(currentPath) { nextLine };
                                queue.Enqueue(newPath);
                            }
                        }
                    }
                }
            }


            //Sort path by number of lines used
            return relavantPaths.OrderBy(lp => lp.path.Count).ToList();
        }



        /// <summary>
        /// Converts a line path to a path with travel informations
        /// </summary>
        public static Path convertLinePathToPath(LinePath linePath, Station startStation, Station endStation)
        {
            if (linePath.path.Count == 1)
            {
                SubPath subPath = new SubPath(startStation, endStation, linePath.path[0]);
                return new Path(startStation, endStation, new SubPath[] { subPath });
            }

            List<Line> lines = linePath.path;

            // Build a new list of transfer stations from scratch
            List<Station> transferStations = getTransferStations(lines, startStation, endStation);

            List<SubPath> subPaths = new List<SubPath>();

            for (int i = 0; i < lines.Count; i++)
            {
                Station enter = transferStations[i];
                Station exit = transferStations[i + 1];

                //Remove paths where staions are dublicate (backtravel) or enter and exit are the same
                if (enter == exit || transferStations.Count != transferStations.Distinct().Count())
                {
                    return null;
                }


                Line line = lines[i];

                // Make sure both stations are on the line (optional: for debug)
                if (!line.stations.Contains(enter) || !line.stations.Contains(exit))
                {
                    throw new Exception($"Station mismatch: {enter} or {exit} not on {line}");
                }

                SubPath subPath = new SubPath(enter, exit, line);
                subPaths.Add(subPath);
            }

            return new Path(startStation, endStation, subPaths.ToArray());
        }



        /// <summary>
        /// Finds the best (shortest) transfer station between each pair of lines
        /// </summary>
        private static List<Station> getTransferStations(List<Line> lines, Station startStation, Station endStation)
        {
            List<Station> transferStations = new List<Station> { startStation };

            for (int i = 0; i < lines.Count - 1; i++)
            {
                Line currentLine = lines[i];
                Line nextLine = lines[i + 1];

                Station bestTransfer = null;
                int minCombinedIndex = int.MaxValue;

                foreach (Station station in currentLine.stations)
                {
                    if (nextLine.stations.Contains(station))
                    {

                        int index1 = Array.IndexOf(currentLine.stations, station);
                        int index2 = Array.IndexOf(nextLine.stations, station);
                        int totalIndex = index1 + index2;

                        if (totalIndex < minCombinedIndex)
                        {
                            minCombinedIndex = totalIndex;
                            bestTransfer = station;
                        }
                    }
                }

                if (bestTransfer == null)
                {
                    throw new Exception($"No transfer station between {currentLine} and {nextLine}");
                }

                transferStations.Add(bestTransfer);
            }

            transferStations.Add(endStation);
            return transferStations;
        }


        /// <summary>
        /// Gets all the lines that are directly connected to one line 
        /// </summary>
        public static List<Line> getLinesConnectedToLine(Line line)
        {
            List<Line> connectedLines = new List<Line>();
            //All stations in the line
            Station[] stations = line.stations;

            //Loop over all stations
            foreach (Station station in stations)
            {
                //Find lines that contain also the station
                foreach (Line lineToCheck in LineManager.usableLines)
                {
                    //Dont check the origin line or dublicates
                    if (lineToCheck == line || connectedLines.Contains(lineToCheck))
                    {
                        continue;
                    }
                    if (lineToCheck.stations.Contains(station))
                    {
                        connectedLines.Add(lineToCheck);
                    }
                }
            }

            return connectedLines;
        }
    }
}