/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: Server.csproj
// FileName: Settings.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 12/9/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Class to create settings object that contains the settings and the walls for the tankwars game
    /// from reading in from an xml file
    /// </summary>
    public class Settings
    {
        // settings variables for the game
        public int UniverseSize { get; } = 2000;
        public int MSPerFrame { get; } = 17;
        public int FramesPerShot { get; } = 80;
        public int RespawnRate { get; } = 300;
        public int MaxHP { get; } = 3;
        public int ProjectileSpeed { get; } = 25;
        public double EngineStrength { get; } = 3;
        public int TankSize { get; } = 60;
        public int WallSize { get; } = 50;
        public int MaxPowerUps { get; } = 2;
        public int MaxPowerUpDelay { get; } = 1650;

        public HashSet<Wall> Walls { get; } = new HashSet<Wall>();
        /// <summary>
        /// Creates a Settings object by reading in a settings XML file
        /// </summary>
        /// <param name="fileName"></param>
        public Settings(string fileName)
        {
            try
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(fileName, readerSettings))
                {
                    Vector2D p1 = null;
                    Vector2D p2 = null;
                    int num = 0;
                    double dNum = 0;
                    // keep reading if not at end
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                // for initial settings, read until the setting title, then parse
                                // the value to the correct setting variable using helper method
                                case "UniverseSize":
                                    UniverseSize = ReadSettings(reader, num);
                                    break;
                                case "MSPerFrame":
                                    MSPerFrame = ReadSettings(reader, num);
                                    break;
                                case "FramesPerShot":
                                    FramesPerShot = ReadSettings(reader, num);
                                    break;
                                case "RespawnRate":
                                    RespawnRate = ReadSettings(reader, num);
                                    break;
                                case "MaxHP":
                                    MaxHP = ReadSettings(reader, num);
                                    break;
                                case "ProjectileSpeed":
                                    ProjectileSpeed = ReadSettings(reader, num);
                                    break;
                                case "EngineStrength":
                                    reader.Read();
                                    double.TryParse(reader.Value, out dNum);
                                    EngineStrength = dNum;
                                    break;
                                case "TankSize":
                                    TankSize = ReadSettings(reader, num);
                                    break;
                                case "WallSize":
                                    WallSize = ReadSettings(reader, num);
                                    break;
                                case "MaxPowerUps":
                                    MaxPowerUps = ReadSettings(reader, num);
                                    break;
                                case "MaxPowerUpDelay":
                                    MaxPowerUpDelay = ReadSettings(reader, num);
                                    break;
                                // if it is a wall, use helper method to read in points, then add the
                                // wall to the dictionary
                                case "Wall":
                                    reader.Read();
                                    p1 = ReadPoint(reader);
                                    p2 = ReadPoint(reader);
                                    Walls.Add(new Wall(p1, p2));
                                    break;
                                default:
                                    break;

                            }
                        } // end if                       
                    } // end while
                }
            }

            catch (Exception)
            {

            }
        }
        /// <summary>
        /// Private helper method that reads in generic settings at the top of the file
        /// and returns the parsed number
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private int ReadSettings(XmlReader reader, int num)
        {
            reader.Read();
            int.TryParse(reader.Value, out num);
            return num;
        }
        /// <summary>
        /// Helper method to read in the points of the walls, returns
        /// a vector2d object for the location of the point
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Vector2D ReadPoint(XmlReader reader)
        {
            int x = 0;
            int y = 0;
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    // read to x and parse value
                    if (reader.Name == "x")
                    {
                        reader.Read();
                        x = int.Parse(reader.Value);
                    }
                    // read to y and parse value
                    else if (reader.Name == "y")
                    {
                        reader.Read();
                        y = int.Parse(reader.Value);
                        break;
                    }
                }
            }
            return new Vector2D(x, y);
        }
    }
}
