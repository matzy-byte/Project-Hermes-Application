using json;
using Microsoft.VisualBasic;
using Pathfinding;
using Simulation;
using TrainLines;
using Trains;

LineManager.initialize();
TrainManager.initialize();

//SimulationManager.startSimulation();
PathfindingManager.initializePathFinding();

Station startStation = LineManager.getStationFromId("de:08212:13");
Station endStation = LineManager.getStationFromId("de:08212:1001");
var path = PathfindingManager.getAllTravelPaths(startStation, endStation);
Console.WriteLine("Stop");