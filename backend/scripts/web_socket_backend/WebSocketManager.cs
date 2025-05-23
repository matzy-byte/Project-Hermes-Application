using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using shared;
using Simulation;

namespace Websocket;

public class WebSocketManager
{
    private readonly HttpListener _listener = new HttpListener();

    public WebSocketManager(string url)
    {
        _listener.Prefixes.Add(url); // e.g. "http://localhost:5000/ws/"
    }

    public void Start()
    {
        _listener.Start();
        Task.Run(() => AcceptLoop());
    }


    /// <summary>
    /// Background task to manage connecting clients
    /// </summary>
    private async Task AcceptLoop()
    {
        while (true)
        {
            var context = await _listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                DataLogger.AddLog("Client Connected to WebSocket");

                var wsContext = await context.AcceptWebSocketAsync(null);
                _ = HandleClient(wsContext.WebSocket); // Fire-and-forget
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
    private async Task HandleClient(WebSocket socket)
    {
        var receiveTask = Task.Run(() => HandleIncomingMessages(socket));
        var streamTask = Task.Run(() => StreamLoop(socket));

        await Task.WhenAll(receiveTask, streamTask);
    }


    /// <summary>
    /// Manages incoming messages in a seperate thread
    /// </summary>
    private async Task HandleIncomingMessages(WebSocket socket)
    {

        while (socket.State == WebSocketState.Open)
        {
            //Read the next full message
            WebSocketMessage incomingMessage = await GetFullMessage(socket);

            // Handle the message
            if (IsRequestMessage(incomingMessage.MessageType))
            {
                //Handle a requst Message
                await HandleRequestMessage(socket, incomingMessage);
            }
            else if (IsSetMessage(incomingMessage.MessageType))
            {
                //Handle a set Message
                await HandleSetMessage(incomingMessage);
            }
            else
            {
                throw new Exception("No Handeble message Type");
            }
        }
    }



    /// <summary>
    /// Streams data contiunously to client
    /// </summary>
    private async Task StreamLoop(WebSocket socket)
    {
        while (socket.State == WebSocketState.Open)
        {
            //Check if simulation is running
            if (SimulationManager.SimulationState.SimulationRunning && SimulationManager.SimulationState.SimulationPaused == false)
            {
                try
                {
                    //Send the data
                    await SendMessage(socket, WebSocketMessageGenerator.GetMessageStreamedData(MessageType.TRAINDATA));
                    await Task.Delay(SimulationSettingsGlobal.StreamDelayBetweenMessages);
                    await SendMessage(socket, WebSocketMessageGenerator.GetMessageStreamedData(MessageType.ROBOTDATA));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            //Delay between transmissions
            await Task.Delay(SimulationSettingsGlobal.DataStreamDelay);
        }
    }


    /// <summary>
    /// Reads the websocket messages until a message is finished and returns the message as string
    /// </summary>
    private async Task<WebSocketMessage> GetFullMessage(WebSocket webSocket)
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
        string fullMessageString = Encoding.UTF8.GetString(messageBuffer.ToArray());

        //Convert the string to Message
        if (fullMessageString == null)
        {
            throw new Exception("No Message to Convert");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter(null) }
        };

        WebSocketMessage incomingMessage = JsonSerializer.Deserialize<WebSocketMessage>(fullMessageString, options);

        if (incomingMessage == null)
        {
            throw new Exception("Message could not be Converted");

        }

        return incomingMessage;
    }



    /// <summary>
    /// Generates the response message and sends it to the client
    /// </summary>
    private async Task HandleRequestMessage(WebSocket webSocket, WebSocketMessage incomingMessage)
    {
        DataLogger.AddLog("Websocket Received REQUEST Message MessageId: " + incomingMessage.Id + " Message Type " + incomingMessage.MessageType);

        //Get the response data
        WebSocketMessage resoponse = WebSocketMessageGenerator.GetResponseMessage(incomingMessage);

        //Send the Answer
        await SendMessage(webSocket, resoponse);
        DataLogger.AddLog("Websocket Send Message MessageId: " + resoponse.Id + " Message Type " + resoponse.MessageType);

    }

    /// <summary>
    /// Generates the response message and sends it to the client
    /// </summary>
    private static async Task HandleSetMessage(WebSocketMessage incomingMessage)
    {

        DataLogger.AddLog("Websocket Received SET Message MessageId: " + incomingMessage.Id + " Message Type" + incomingMessage.MessageType);

        //Check the message type
        switch (incomingMessage.MessageType)
        {
            case MessageType.SETTINGS:
                await SimulationSettings.UpdateSettings(incomingMessage.Data.GetRawText());
                return;
            case MessageType.SETSIMULATIONSPEED:
                SimulationSettings.UpdateSimulationSeed(incomingMessage.Data.GetRawText());
                return;
            case MessageType.STARTSIMULATION:
                SimulationManager.StartSimulation();
                return;
            case MessageType.STOPSIMULATION:
                await SimulationManager.StopSimulation();
                return;
            case MessageType.PAUSESIMULATION:
                SimulationManager.PauseSimulation();
                return;
            case MessageType.CONTINUESTIMULATION:
                SimulationManager.ContinueSimulation();
                return;
        }
        throw new Exception("Message not valid");
    }


    private static async Task SendMessage(WebSocket webSocket, WebSocketMessage message)
    {
        //convert answer to json string


        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(null) },
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string answerString = JsonSerializer.Serialize(message, options);

        //Send the message to the client
        // Send the response
        var responseBytes = Encoding.UTF8.GetBytes(answerString);
        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);

    }


    /// <summary>
    /// Checks if a message Type is a request Message
    /// </summary>
    private static bool IsRequestMessage(MessageType messageType)
    {
        return messageType == MessageType.TRAINLINES
        || messageType == MessageType.USEDSTATIONS
        || messageType == MessageType.PACKAGEDATA
        || messageType == MessageType.SIMULATIONSTATE;
    }


    /// <summary>
    /// Checks if a message Type is a set Message
    /// </summary>
    private static bool IsSetMessage(MessageType messageType)
    {
        return messageType == MessageType.SETTINGS
        || messageType == MessageType.SETSIMULATIONSPEED
        || messageType == MessageType.STARTSIMULATION
        || messageType == MessageType.STOPSIMULATION
        || messageType == MessageType.PAUSESIMULATION
        || messageType == MessageType.CONTINUESTIMULATION;
    }
}
