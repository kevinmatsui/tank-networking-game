/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: Server.csproj
// FileName: ServerController.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 12/9/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using NetworkUtil;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Diagnostics;

namespace TankWars
{
    /// <summary>
    /// Class to communicate with clients.
    /// </summary>
    class ServerController
    {
        // private fields for ServerController
        private Settings settings { get; }
        private World theWorld;
        private Dictionary<int, SocketState> clients = new Dictionary<int, SocketState>();
        private string startupInfo;

        /// <summary>
        /// This is a constructor that collects the start up info of the world by the given settings.
        /// </summary>
        /// <param name="settings">settings for the world</param>
        public ServerController(Settings settings)
        {
            // initializing fields by given settings argument
            this.settings = settings;

            theWorld = new World(settings);
           
            // store walls received from the settings
            foreach(Wall wall in settings.Walls)
            {
                theWorld.walls[wall.Id] = wall;
            }

            // string builder for startup info
            StringBuilder sb = new StringBuilder();
            // append world size
            sb.Append(theWorld.Size);
            sb.Append("\n");

            // append walls' json strings
            foreach(Wall wall in theWorld.walls.Values)
            {
                sb.Append(wall.ToString());
            }
            // store the startup info to the private variable
            startupInfo = sb.ToString();
        }
      
        /// <summary>
        /// This method starts the server.
        /// </summary>
        internal void Start()
        {
            // start the server with method for connection and port number
            Networking.StartServer(NewClient, 11000);

            // create a new thread to run the update method on the thread
            Thread t = new Thread(Update);
            t.Start();
        }

        /// <summary>
        /// This method receives and stores new client's information.
        /// </summary>
        /// <param name="client"></param>
        private void NewClient(SocketState client)
        {
            //error check
            if (client.ErrorOccured)
            {
                Console.WriteLine("Error occured while connecting the Player (" + client.ID + ").");
                return;
            }

            // change the client handling method
            client.OnNetworkAction = ReceivePlayerName;
            // receive and store the data
            Networking.GetData(client);
        }

        /// <summary>
        /// This method sends start up information for the world.
        /// </summary>
        /// <param name="client"></param>
        private void ReceivePlayerName(SocketState client)
        {
            if (client.ErrorOccured)
            {
                Console.WriteLine("Error occured while connecting the Player (" + client.ID + ").");
                return;
            }

            // get client/player's name
            string name = client.GetData();

            // if data does not ends with new line
            if (!name.EndsWith("\n"))
            {
                // complete getting the data and end the method
                client.GetData(); 
                return;
            }

            client.RemoveData(0, name.Length);
            name = name.Trim(); // remove whitespaces

            // send client's id first
            Networking.Send(client.TheSocket, client.ID + "\n");
            // send client's startup info
            Networking.Send(client.TheSocket, startupInfo);

            lock (theWorld)
            {
                // storing the tank with current client's info
                theWorld.tanks[(int)client.ID] = new Tank((int)client.ID, name, settings.MaxHP);
            }

            lock (clients)
            {
                // add current client to the client dictionary
                clients.Add((int)client.ID, client);
            }

            // change client's handling method
            client.OnNetworkAction = ReceiveControlCommand;
            Networking.GetData(client);
        }

        /// <summary>
        /// This method gets control commands string from the client and stores the information to the control commands dictionary.
        /// </summary>
        /// <param name="client"></param>
        private void ReceiveControlCommand(SocketState client)
        {
            // if client is disconnected
            if(client.ErrorOccured){
                Console.WriteLine("Client " + client.ID + " disconnected.");
                return;
            }
            // get control command json 3strings
            string totalData = client.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            foreach (string p in parts)
            {
                // if current string is empty
                if (p.Length == 0)
                {
                    continue;
                }
                // if current string does not end with a new line
                if (p[p.Length - 1] != '\n')
                    break;

                // deserialize string to control commands object
                ControlCommands ctrlCmd = JsonConvert.DeserializeObject<ControlCommands>(p);

                lock (theWorld)
                {
                    // store the control commands object
                    theWorld.controlCommands[(int)client.ID] = ctrlCmd;
                }
                client.RemoveData(0, p.Length);
            }

            Networking.GetData(client);
        }

        /// <summary>
        /// This method runs on different thread to update the world on every frame.
        /// </summary>
        private void Update()
        {
            // object for delay
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // for running on every frame
            while(true)
            {
                // wait until given msperframe time has passed
                while (watch.ElapsedMilliseconds < settings.MSPerFrame) ;

                watch.Restart(); // reset the watch

                // string builder for store the world information
                StringBuilder sb = new StringBuilder();
                lock (theWorld)
                {
                    theWorld.Update();

                    // append json strings
                    // tanks
                    foreach (Tank tank in theWorld.tanks.Values)
                    {
                        sb.Append(tank.ToString());
                        tank.currFrameShot++; // increment the frame between shots
                        if(tank.Join)
                        {
                            tank.Join = false;
                        }
                    }

                    // projectiles
                    foreach (Projectile proj in theWorld.projectiles.Values)
                    {
                        sb.Append(proj.ToString());
                    }

                    // beams
                    foreach (Beam beam in theWorld.beams.Values)
                    {
                        sb.Append(beam.ToString());
                    }
                    // remove all beam objects
                    theWorld.beams.Clear();

                    // powerups
                    foreach (PowerUp powerUp in theWorld.powerUps.Values)
                    {
                        sb.Append(powerUp.ToString());
                    }
                    // remove disconnected tanks
                    foreach (long id in theWorld.disconnected)
                    {
                        theWorld.tanks.Remove((int)id);
                    }
                    theWorld.disconnected.Clear();
                }

                // information string to send to the client
                string frameInfo = sb.ToString();

                lock (clients)
                {
                    sb.Clear();

                    // clients
                    foreach (SocketState client in clients.Values)
                    {
                        // if sending information to client fails
                        if(!Networking.Send(client.TheSocket, frameInfo))
                        {
                            // add failed client to the disconnected set
                            theWorld.disconnected.Add(client.ID);
                        }   
                    }
                    // remove clients from the dictionary when disconnected
                    foreach(long id in theWorld.disconnected)
                    {
                        clients.Remove((int)id);
                    }

                }

            }
        }           
    }
}
