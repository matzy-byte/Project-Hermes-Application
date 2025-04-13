using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using TrainLines;

namespace Pathfinding
{
    public static class PathfindingManager
    {
        private static Dictionary<Line, List<Line>> directConnectionTable = new Dictionary<Line, List<Line>>();
        private static List<LinePath> allConnectionsTable = new List<LinePath>();
        private static ConcurrentDictionary<(Station, Station), List<Path>> stationToStationPathTable = new ConcurrentDictionary<(Station, Station), List<Path>>();

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

            initializeAllConnectionsTable();
            //initializeStationToStationPathTable();

            Console.WriteLine("Path finding initialized");
        }

        private static void initializeStationToStationPathTable()
        {

            List<(Station, Station)> stationPairs = new List<(Station, Station)>();

            foreach (Line startLine in LineManager.usableLines)
            {
                foreach (Line endLine in LineManager.usableLines)
                {
                    foreach (Station startStation in startLine.stations)
                    {
                        foreach (Station endStation in endLine.stations)
                        {
                            if (startStation != endStation)
                            {
                                stationPairs.Add((startStation, endStation));
                            }
                        }
                    }
                }
            }

            //Generate Path infomrations
            Parallel.ForEach(stationPairs, pair =>
            {
               // stationToStationPathTable.TryAdd((pair.Item1, pair.Item2), getAllTravelPaths(pair.Item1, pair.Item2));
            });
        }


        /// <summary>
        /// initializs lookup of possible line paths for each line every other line 
        /// </summary>
        public static void initializeAllConnectionsTable()
        {
            allConnectionsTable.Clear();

            foreach (Line startLine in LineManager.usableLines)
            {
                foreach (Line endLine in LineManager.usableLines)
                {
                    if (startLine != endLine)
                    {
                        allConnectionsTable.AddRange(getAllLinePaths(startLine, endLine));
                    }
                }
            }
        }

        /// <summary>
        /// Returns all possible Line paths from a start to a end line
        /// </summary>
        private static List<LinePath> getAllLinePaths(Line startLine, Line endLine)
        {
            // List to store all the possible paths from startLine to endLine
            List<LinePath> allPaths = new List<LinePath>();

            // Queue for BFS that will hold paths and their associated visited lines
            Queue<List<Line>> pathQueue = new Queue<List<Line>>();
            pathQueue.Enqueue(new List<Line> { startLine });

            // Set to track all visited lines across all paths
            HashSet<Line> visitedLinesAcrossPaths = new HashSet<Line>();

            // Start BFS
            while (pathQueue.Count > 0)
            {
                // Get the current path from the queue
                List<Line> currentPath = pathQueue.Dequeue();
                Line currentLine = currentPath.Last();  // Last line in the current path

                // If we reached the endLine, save the path
                if (currentLine == endLine)
                {
                    allPaths.Add(new LinePath(startLine, endLine, new List<Line>(currentPath)));
                    continue; // Continue with other potential paths
                }

                // Mark the current line as visited for this path
                visitedLinesAcrossPaths.Add(currentLine);

                // Explore all connected lines (direct connections)
                if (directConnectionTable.ContainsKey(currentLine))
                {
                    foreach (var nextLine in directConnectionTable[currentLine])
                    {
                        // If the next line is not visited in any previous path, we can explore it
                        if (!visitedLinesAcrossPaths.Contains(nextLine))
                        {
                            // Copy the current path to continue exploration
                            List<Line> newPath = new List<Line>(currentPath) { nextLine };
                            pathQueue.Enqueue(newPath);
                        }
                    }
                }
            }

            // Return the list of all found paths
            return allPaths;
        }


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
        /// Get's all possible Linepaths from start to end station
        /// </summary>
        public static List<LinePath> getRelavantLinePaths(Station startStation, Station endStation)
        {
            List<LinePath> relavantPaths = new List<LinePath>();

            List<Line> startLines = LineManager.getLinesWithStation(startStation);
            List<Line> endLines = LineManager.getLinesWithStation(endStation);

            //Get paths with same line
            foreach (Line line in startLines)
            {
                if (endLines.Contains(line))
                {
                    relavantPaths.Add(new LinePath(line, line, new List<Line>() { line }));
                }
            }

            //Search all not direct connections
            foreach (Line startLine in startLines)
            {
                foreach (Line endLine in endLines)
                {
                    foreach (LinePath linePath in allConnectionsTable)
                    {
                        if (linePath.startLine == startLine && linePath.destinationLine == endLine)
                        {
                            relavantPaths.Add(linePath);
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