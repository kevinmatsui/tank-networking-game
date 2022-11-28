/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: GameController.csproj
// FileName: Controller.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 11/29/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Newtonsoft.Json;
using NetworkUtil;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TankWars;

namespace GameController {

    /// <summary>
    /// Controller class that contains necessary information to communicate with the server
    /// and inform the view.
    /// </summary>
    public class Controller
    {
        // Controller events that the view can subscribe to
        public delegate void ConnectedHandler();
        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        // creates necessary objects to communicate with server and update world
        private string player; // string for the players name
        
        private int id; // field to set the id
        public int Id { get => id; private set => id = value; } // Id property

        private int worldSize;
        public int WorldSize { get => worldSize; private set => worldSize = value; }

        public World TheWorld { get; private set; }
        private ControlCommands ctrlCmds;
        public Tank userT { get; private set; } // keeps track of the tank object the user is playing as

        // alerts the view an update occurred
        public event Action updateArrived;

        // create booleans to handle player movement commands
        public Stack<string> moveCommands { get; private set; }

        /// <summary>
        /// Default constructor to initialize world and control commands objects
        /// </summary>
        public Controller()
        {
            TheWorld = new World(100);
            ctrlCmds = new ControlCommands();
            // initialize movement commands stack
            moveCommands = new Stack<string>();
        }

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string player, string addr)
        {
            // sets the player name to the name provided by the user
            this.player = player;

            Networking.ConnectToServer(OnConnect, addr, 11000);
            // 11000 big number because other small numbers in use
        }


        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {

            // if error was occured while client and server communicating
            if (state.ErrorOccured)
            {
                // inform the view
                Error("Unable to connect to server!");
                return;
            }
           
            Networking.Send(state.TheSocket, player + '\n'); 
            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveStartupInfo; //ReceiveMessage
            Networking.GetData(state);
        }

        /// <summary>
        /// Method to be used as the OnNetworkAction call back. 
        /// Processes the startup info received from the server.
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveStartupInfo(SocketState state)
        {
            // if error was occured while client and server communicating
            if (state.ErrorOccured)
            {
                // inform the view
                Error("Disconnected from server!"); 
                return;
            }
            // get the data sent from the server and split it into parts at newline characters
            string info = state.GetData();
            string[] parts = Regex.Split(info, @"(?<=[\n])");

            // if there are less than 2 strings sent, or the playername doesn't end with a newline, get more data
            if (parts.Length < 2 || !parts[1].EndsWith("\n")) {
                Networking.GetData(state);
                return;
            }

            // try to parse the id and worldsize sent from the server to ints
            if(int.TryParse(parts[0], out id)&&  int.TryParse(parts[1], out worldSize))
            {
                // remove the data received from the state
                state.RemoveData(0, parts[0].Length + parts[1].Length);
                TheWorld.Size = worldSize;
            }
            else
            {
                return;
            }
            // sets the state's callback method to receiveJson method
            state.OnNetworkAction = ReceiveJson;
            // continues the event loop
            Networking.GetData(state);
        }

        private void ReceiveJson(SocketState state)
        {
            // if error was occured while client and server communicating
            if (state.ErrorOccured)
            {
                // inform the view
                Error("Disconnected from server!"); 
                return;
            }
            // method for Json byte data
            string message = state.GetData();
            string[] parts = Regex.Split(message, @"(?<=[\n])");

            // lock when modifying the world
            lock (TheWorld)
            {
                foreach (string part in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (part.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (part[part.Length - 1] != '\n')
                        break;

                    // tries to find which type to deserialize the json object to
                    // deserializes the Json object to correct type, then adds to 
                    // corresponding dictionary, with given ID.

                    // walls
                    JObject obj = JObject.Parse(part);
                    JToken token = obj["wall"];
                    if (token != null)
                    {
                        Wall w = JsonConvert.DeserializeObject<Wall>(part); 
                        TheWorld.walls[w.Id] = w;
                    }

                    // tanks
                    token = obj["tank"];
                    if(token!= null)
                    {
                        Tank t = JsonConvert.DeserializeObject<Tank>(part);
                        TheWorld.tanks[t.Id] = t;
                        if(t.Id == Id) 
                        {
                            userT = t;
                        }
                    }

                    // projectiles
                    token = obj["proj"];
                    if (token != null)
                    {
                        Projectile p = JsonConvert.DeserializeObject<Projectile>(part);
                        TheWorld.projectiles[p.Id] = p;
                    }

                    // powerups
                    token = obj["power"];
                    if (token != null)
                    {
                        PowerUp p = JsonConvert.DeserializeObject<PowerUp>(part);
                        TheWorld.powerUps[p.Id] = p;
                    }

                    // beams
                    token = obj["beam"];
                    if (token != null)
                    {
                        Beam b = JsonConvert.DeserializeObject<Beam>(part);
                        TheWorld.beams[b.Id] = b;
                    }

                    // remove the data that was received from the state
                    state.RemoveData(0, part.Length);
                }
                
            } // end lock
            // serialize the control commands object to a string ending in new line
            string data = JsonConvert.SerializeObject(ctrlCmds) + '\n';
            // send the control command to the server
            Networking.Send(state.TheSocket, data);
            // reset fire to none if it is alt, so only one beam is fired
            if(ctrlCmds.fire == "alt")
            {
                ctrlCmds.fire = "none";
            }
            // alert the view that an update occurred
            updateArrived?.Invoke();
            // continue the event loop
            Networking.GetData(state); 
        }

        /// <summary>
        /// Sets the moving field of control command object to the direction user input
        /// </summary>
        public void HandleMoveRequest()
        {
            // if the movement Stack does not contain any elements
            if(moveCommands.Count == 0)
            {
                ctrlCmds.moving = "none";
            }
            else
            {
                // set moving to the first element of the movement Stack
                ctrlCmds.moving = moveCommands.Peek();
            }
        }

        /// <summary>
        /// This method is to keep track of the movement Stack when KeyUp event happens
        /// </summary>
        /// <param name="direction"> up/down/right/left </param>
        public void HandleKeyUpRequest(string direction)
        {
            // initializing temporary Stack
            Stack<string> temp = new Stack<string>();
            int size = moveCommands.Count; // size of the original Stack for key pressed
           
            for (int i = 0; i < size; i++)
            {
                // if the current element matches the direction
                if (moveCommands.Peek() == direction)
                {
                    moveCommands.Pop(); // remove the element
                }
                else
                {
                    // store to the temporary Stack
                    temp.Push(moveCommands.Pop());
                }
            }

            int tempSize = temp.Count; // store the size of the temp Stack

            for(int i = 0; i < tempSize; i++)
            {
                // store back to the original Stack for keeping the original order
                moveCommands.Push(temp.Pop());
            }
        }
        /// <summary>
        /// Sets the fire field of control command object to the correct user input
        /// </summary>
        /// <param name="fire"></param>
        public void HandleFireRequest(string fire)
        {
            ctrlCmds.fire = fire;
        }

        /// <summary>
        /// Sets the tdir field of control command object to the direction the user's mouse moved
        /// </summary>
        public void HandleMouseMoveRequest(Vector2D vector)
        {
            ctrlCmds.tdir = vector;
        }
    }
}
