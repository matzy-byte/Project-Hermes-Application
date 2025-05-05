using Pathfinding;
using Simulation;
using TrainLines;
using Trains;


LineManager.initialize();
TrainManager.initialize();
PathfindingManager.initializePathFinding();
Console.WriteLine("Simulation is Ready...");

// Create an instance of the WebSocketManager
WS.WebSocketManager webSocketManager = new WS.WebSocketManager(SimulationSettingsGlobal.webSocketURL);
// Start the WebSocket server in a separate thread (asynchronous task)
Task.Run(() => webSocketManager.start());
Console.WriteLine("WebSocket server started on " + SimulationSettingsGlobal.webSocketURL);

//Sets the simulation in the ready state
//SimulationManager.startSimulation();

//Starts the actual simulation
SimulationManager.simulationLoop();
