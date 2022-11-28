/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: PowerUp.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 12/9/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Class to make the PowerUp objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerUp
    {
        private static int nextId = 0;
        public const int size = 10;
        // given fields made as JsonProperties
        [JsonProperty(PropertyName = "power")]
        public int Id { get; private set; } = 0;
        [JsonProperty]
        public Vector2D loc { get; private set; } = null;
        [JsonProperty]
        public bool died { get; internal set; } = false;

        /// <summary>
        /// Default constructor for JSON Serialization
        /// </summary>
        public PowerUp()
        {
        }
        /// <summary>
        /// Creates a powerup object with unique id and location
        /// </summary>
        /// <param name="loc"></param>
        public PowerUp(Vector2D loc)
        {
            Id = nextId++;
            this.loc = loc;
            died = false;
        }
        /// <summary>
        /// Overrided toString method to serialize the powerup to json.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

    }
}