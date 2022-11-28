/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: Projectile.cs
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
    /// Class to make the Projectile objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        private static int nextId = 0;
        public Vector2D Velocity { get; internal set; }

        /// <summary>
        /// Default constructor for JSON Serialization
        /// </summary>
        public Projectile()
        {
        }

        /// <summary>
        /// Creates a projectile object with unique id, with given location direction and ownerid.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="dir"></param>
        /// <param name="ownerId"></param>
        public Projectile(Vector2D loc, Vector2D dir, int ownerId)
        {
            Id = nextId++;
            this.loc = loc;
            this.dir = dir;
            died = false;
            OwnerId = ownerId;
            Velocity = dir;
        }
        // given fields made as JsonProperties
        [JsonProperty(PropertyName = "proj")]
        public int Id { get; private set; } = 0;
        [JsonProperty]
        public Vector2D loc { get; internal set; } = null;
        [JsonProperty]
        public Vector2D dir { get; private set; } = null;
        [JsonProperty]
        public bool died { get; internal set; } = false;
        [JsonProperty(PropertyName = "owner")]
        public int OwnerId { get; private set; } = 0;


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