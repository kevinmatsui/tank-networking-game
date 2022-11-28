/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: Server.csproj
// FileName: Program.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 12/9/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace TankWars
{
    /// <summary>
    /// This class runs the server program by starting the server controller.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // initialize settings to pass with settings file
            Settings settings = new Settings(@"..\..\..\..\Resources\settings.xml");
            // server object with given settings
            ServerController serv = new ServerController(settings);
            // start the server
            serv.Start();

            Console.Read();
        }
    }
}
