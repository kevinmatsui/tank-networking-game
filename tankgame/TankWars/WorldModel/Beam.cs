/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: Beam.cs
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
    /// Class to make the Beam objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        private static int nextId = 0;

        /// <summary>
        /// Default constructor for JSON Serialization
        /// </summary>
        public Beam() 
        { 
        }
        /// <summary>
        /// Increments the beam's ID and creates a beam object with given owner id, org and dir
        /// </summary>
        /// <param name="org"></param>
        /// <param name="dir"></param>
        /// <param name="ownerId"></param>
        public Beam(Vector2D org, Vector2D dir, int ownerId)
        {
            Id = nextId++;
            this.org = org;
            this.dir = dir;
            OwnerId = ownerId;
            this.loc = org;
            this.Velocity = dir * 50;
        }
        // given fields made as JsonProperties
        [JsonProperty(PropertyName = "beam")]
        public int Id { get; private set; } = 0;
        [JsonProperty]
        public Vector2D org { get; private set; } = null;
        [JsonProperty]
        public Vector2D dir { get; private set; } = null;
        [JsonProperty(PropertyName = "owner")]
        public int OwnerId { get; private set; } = 0;
        public Vector2D loc { get; internal set; } = null;
        public Vector2D Velocity { get; internal set; } = null;
        /// <summary>
        /// Overrided toString method to serialize the beam to json.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + '\n';
        }
    }
}