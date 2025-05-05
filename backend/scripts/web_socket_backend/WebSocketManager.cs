using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Packages;
using Robots;
using Simulation;
using Trains;
namespace WS
{
    public class WebSocketManager
    {
        private readonly HttpListener _listener = new HttpListener();

        public WebSocketManager(string url)
        {
            _listener.Prefixes.Add(url); // e.g. "http://localhost:5000/ws/"
        }

        public void start()
        {
            _listener.Start();
            Task.Run(() => acceptLoop());
        }


        /// <summary>
        /// Background task to manage connecting clients
        /// </summary>
        private async Task acceptLoop()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = handleClient(wsContext.WebSocket); // Fire-and-forget
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }


        /// <summary>
        /// When client connects it creates two taks. One for managing
        /// request and one for streaming data
        /// </summary>
        private async Task handleClient(WebSocket socket)
        {
            var receiveTask = Task.Run(() => handleIncomingMessages(socket));
            var streamTask = Task.Run(() => streamLoop(socket));

            await Task.WhenAll(receiveTask, streamTask);
        }


        /// <summary>
        /// Manages incoming messages in a seperate thread
        /// </summary>
        private async Task handleIncomingMessages(WebSocket socket)
        {

            while (socket.State == WebSocketState.Open)
            {
                //Read the next full message
                string incomingMessageString = await getFullMessage(socket);

                //Convert message to a incoming message
                WebSocketMessage incomingMessage = convertIncomingMessage(incomingMessageString);

                // Handle the message
                if (isRequestMessage(incomingMessage.messageType))
                {
                    //Handle a requst Message
                    await handleRequestMessage(socket, incomingMessage);
                }
                else if (isSetMessage(incomingMessage.messageType))
                {
                    //Handle a set Message
                    handleSetMessage(incomingMessage);
                }
                else
                {
                    throw new Exception("No Handleable message Type");
                }
            }
        }



        /// <summary>
        /// Streams data contiunously to client
        /// </summary>
        private async Task streamLoop(WebSocket socket)
        {

            while (socket.State == WebSocketState.Open)
            {
                //Check if simulation is running
                if (SimulationManager.isSimulationRunning && SimulationManager.isSimulationPaused == false)
                {
                    try
                    {
                        //Send the data
                        await sendTrainData(socket);
                        await Task.Delay(SimulationSettingsGlobal.streamDelayBetweenMessages);
                        await sendRobotData(socket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                //Delay between transmissions
                await Task.Delay(SimulationSettingsGlobal.dataStreamDelay);
            }
        }


        /// <summary>
        /// Reads the websocket messages until a message is finished and returns the message as string
        /// </summary>
        private async Task<string> getFullMessage(WebSocket webSocket)
        {
            List<byte> messageBuffer = new List<byte>(); // Dynamic buffer

            WebSocketReceiveResult result;
            do
            {
                byte[] buffer = new byte[1024 * 4];

                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch
                {
                    throw new Exception("Cant read Message");
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    throw new Exception("Websocket closed");
                }

                // Add received bytes to the dynamic message buffer
                messageBuffer.AddRange(buffer.Take(result.Count));

            } while (!result.EndOfMessage);

            // Now we have the full message!
            return Encoding.UTF8.GetString(messageBuffer.ToArray());
        }



        /// <summary>
        /// Generates the response message and sends it to the client
        /// </summary>
        private async Task handleRequestMessage(WebSocket webSocket, WebSocketMessage incomingMessage)
        {
            //Get the response data
            string resposeData = getResponseDataString(incomingMessage);

            //Generate the answer message
            WebSocketMessage answerMessage = generateWebsocketMessage(incomingMessage, resposeData);

            //convert answer to json string
            string answerString = convertResponseToJSON(answerMessage);

            //Send the message to the client
            // Send the response
            var responseBytes = Encoding.UTF8.GetBytes(answerString);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Generates the response message and sends it to the client
        /// </summary>
        private static void handleSetMessage(WebSocketMessage incomingMessage)
        {
            //Check the message type
            switch (incomingMessage.messageType)
            {
                case MessageType.SETTINGS:
                    SimulationSettings.updateSettings(incomingMessage.data.GetRawText());
                    return;
                case MessageType.SETSIMULATIONSPEED:
                    SimulationSettings.updateSimulationSeed(incomingMessage.data.GetRawText());
                    return;
                case MessageType.STARTSIMULATION:
                    SimulationManager.startSimulation();
                    return;
                case MessageType.STOPSIMULATION:
                    SimulationManager.stopSimulation();
                    return;
                case MessageType.PAUSESIMULATION:
                    SimulationManager.pauseSimulation();
                    return;
                case MessageType.CONTINUESIMULATION:
                    SimulationManager.continueSimulation();
                    return;
            }
            throw new Exception("Message not valid");
        }


        /// <summary>
        /// Generates the response data string
        /// </summary>
        private string getResponseDataString(WebSocketMessage incomingMessage)
        {
            switch (incomingMessage.messageType)
            {
                //Request Data
                case MessageType.TRAINLINES:
                    return TrainManager.getTrainLinesJSON();
                case MessageType.TRAINSTATIONSINLINE:
                    return TrainManager.getTrainStationsJSON();
                case MessageType.USEDSTATIONS:
                    return TrainManager.getUsedStationsJSON();
                case MessageType.TRAINGEODATA:
                    return TrainManager.getTrainGeoDataJSON();
                case MessageType.PACKAGEDATA:
                    return PackageManager.getPackageDataJSON();
                case MessageType.SIMULATIONSTATE:
                    return SimulationManager.getSimulationStateJSON();
            }

            throw new Exception("Message not valid");
        }


        /// <summary>
        /// Sends the train data message
        /// </summary>
        private async Task sendTrainData(WebSocket webSocket)
        {
            WebSocketMessage message = new WebSocketMessage
            {
                id = 0,
                messageType = MessageType.TRAINDATA,
                data = JsonDocument.Parse("{}").RootElement.Clone()
            };

            //Get the data string
            string trainData = TrainManager.getTrainPositionsJSON();

            //Generate the full message
            message = generateWebsocketMessage(message, trainData);

            //Send the message
            //convert answer to json string
            string messageString = convertResponseToJSON(message);

            //Send the message to the client
            // Send the response
            var responseBytes = Encoding.UTF8.GetBytes(messageString);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        /// <summary>
        /// Sends a Robot data message
        /// </summary>
        private async Task sendRobotData(WebSocket webSocket)
        {
            WebSocketMessage message = new WebSocketMessage
            {
                id = 1,
                messageType = MessageType.ROBOTDATA,
                data = JsonDocument.Parse("{}").RootElement.Clone()
            };

            //Get the data string
            string robotData = RobotManager.getRobotDataJSON();

            //Generate the full message
            message = generateWebsocketMessage(message, robotData);

            //Send the message
            //convert answer to json string
            string messageString = convertResponseToJSON(message);

            //Send the message to the client
            // Send the response
            var responseBytes = Encoding.UTF8.GetBytes(messageString);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        /// <summary>
        /// Converts a message string to a message object
        /// </summary>
        private static WebSocketMessage convertIncomingMessage(string message)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<WebSocketMessage>(message, options);
        }



        /// <summary>
        /// Generates a response message with incoming mesagge and a data string
        /// </summary>
        private static WebSocketMessage generateWebsocketMessage(WebSocketMessage incomingMessage, string dataString)
        {
            int id = incomingMessage.id;
            MessageType messageType = incomingMessage.messageType;
            JsonDocument jsonDocument = JsonDocument.Parse(dataString);
            JsonElement jsonElement = jsonDocument.RootElement.Clone();

            return new WebSocketMessage
            {
                id = id,
                messageType = messageType,
                data = jsonElement
            };
        }


        /// <summary>
        /// Converts a WebSocketMessage to a json string
        /// </summary>
        private string convertResponseToJSON(WebSocketMessage responseMessage)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Serialize(responseMessage, options);
        }



        /// <summary>
        /// Checks if a message Type is a request Message
        /// </summary>
        private static bool isRequestMessage(MessageType messageType)
        {
            return messageType == MessageType.TRAINLINES
            || messageType == MessageType.TRAINSTATIONSINLINE
            || messageType == MessageType.USEDSTATIONS
            || messageType == MessageType.TRAINGEODATA
            || messageType == MessageType.PACKAGEDATA
            || messageType == MessageType.SIMULATIONSTATE;
        }


        /// <summary>
        /// Checks if a message Type is a set Message
        /// </summary>
        private static bool isSetMessage(MessageType messageType)
        {
            return messageType == MessageType.SETTINGS
            || messageType == MessageType.SETSIMULATIONSPEED
            || messageType == MessageType.STARTSIMULATION
            || messageType == MessageType.STOPSIMULATION
            || messageType == MessageType.PAUSESIMULATION
            || messageType == MessageType.CONTINUESIMULATION;
        }

        public enum MessageType
        {
            //Streamed Data
            ROBOTDATA,
            TRAINDATA,

            //Requested Data
            TRAINLINES,
            TRAINSTATIONSINLINE,
            USEDSTATIONS,
            TRAINGEODATA,
            PACKAGEDATA,
            SIMULATIONSTATE,

            //Set Data
            SETTINGS,
            SETSIMULATIONSPEED,
            STARTSIMULATION,
            STOPSIMULATION,
            PAUSESIMULATION,
            CONTINUESIMULATION
        }

        private class WebSocketMessage
        {
            public int id { get; set; }
            public MessageType messageType { get; set; }
            public JsonElement data { get; set; }
        }
    }
}