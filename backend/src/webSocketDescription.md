# JSON Files
To see how the data that is requested or sended must look like check the folder backend/jsondata/WebSocketData. The ID of the messsage can be picked at random and the server responses with the same id. But TrainPositions and RobotData is alsways the same ID (0 & 1).

# Get Requests
Look at jsonData/WebSocketData/Requestable folder for templates for requests and asnwers.
## Train Lines
Requests the Train Lines that are used in the simulation. The JSON contains the TrainID of the  Line and it's name. 
* Request: requestTrainLines.json
* Answer: TrainLines.json
## Train Stations in Train Line
Request the Station IDs that are in each Train Line. The JSON contains the TrainID and the StationIDs that are in each train line.
* Request: requestStationsInLine.json
* Answer: TrainStationsInLine.json
## Used Stations
Requests all Stations that are used in the Simulation. The JSON contains all StationIDs, Station Name, and Position(long, lat) that are used.
* Request: requestUsedStations.json
* Answer: UsedStations.json
## Train Line Geo Data
Requests the Geo Data of each Train Line. The JSON contains all TrainIDs and the GeoData (long, lat) for each train line.
* Request: requestTrainGeoData.json
* Answer: TrainGeoData.json
## Package Data
Request all packages that are currently in the Simulation. The JSON contains where the packages are currently and what the destination station is. 
* Request: requestPackageData.json
* Answer: PackageData.json

## Simulation State
Request the current Simulation State. 
* Request: requestSimulationState.json
* Answer: SimulationState.json


# Set Requests
Look at jsonData/WebSocketData/Setable 
## Set Settings
Resets the and Starts the simulation with new settings
* Request: setStimulationSettings.json
## Set simulation Speed
Sets the simulation speed to a new speed. 
* Request: changeSimulationSpeed.json
## Start Simulation
Starts the simulation from beginning
* Request: startSimulation.json
## Stop Simulation
Stops the simulation and resets the Robots, Trains and Packages
* Request: stopSimulation.json
## Pause Simulation
Pauses the simulation
* Request: pauseSimulation.json
## Continue Simulation
Continues the simulation
* Request: continueSimulation.json


# Streamed Data
Look at jsonData/WebSocketData/Streamed folder for template.
## Train Data
Simulation streames the Position and other Data 
Server sends data in this format: TrainPosition.json
* Message ID is always 0

## Robot Data
Simulation streames the Position and other Data 
Server sends data in this format: RobotData.json
* Message ID is always 1
