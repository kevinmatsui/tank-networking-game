/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: Wall.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 12/9/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;


namespace TankWars
{
    /// <summary>
    /// Class to make the Wall objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        // fields in order to offset by the entire wall given the two points
        double top, bottom, left, right;

        private static int nextId = 0;

        /// <summary>
        /// Default constructor for JSON Serialization
        /// </summary>
        public Wall()
        {
        }

        /// <summary>
        /// Creates a wall object with given two points with unique ID.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(Vector2D p1, Vector2D p2)
        {
            Id = nextId++;
            this.p1 = p1;
            this.p2 = p2;

            // find the left, right, top, and bottom most parts of the wall
            left = Math.Min(p1.GetX(), p2.GetX());
            right = Math.Max(p1.GetX(), p2.GetX());
            top = Math.Min(p1.GetY(), p2.GetY());
            bottom = Math.Max(p1.GetY(), p2.GetY());
        }
        // given fields made as JsonProperties
        [JsonProperty(PropertyName = "wall")]
        public int Id { get; private set; } = 0;

        [JsonProperty]
        public Vector2D p1 { get; private set; } = null;

        [JsonProperty]
        public Vector2D p2 { get; private set; } = null;

        /// <summary>
        /// Overrided toString method to serialize the wall to json.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
        /// <summary>
        /// Returns true if the current wall's location collides with a tank or powerup given
        /// an offset of half the size of the tank or powerup
        /// </summary>
        /// <param name="tankLoc"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool CollidesTankAndPowerUp(Vector2D tankLoc, int offset)
        {
            return left - offset < tankLoc.GetX() && tankLoc.GetX() < right + offset
                && top - offset < tankLoc.GetY() && tankLoc.GetY() < bottom + offset;
        }

        /// <summary>
        /// Returns true if the current wall's location collides with a projectile given an
        /// offset of half the size of a wall.
        /// </summary>
        /// <param name="projLoc"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool CollidesProjectile(Vector2D projLoc, int offset)
        {
            return left - offset < projLoc.GetX() && projLoc.GetX() < right + offset
                && top - offset < projLoc.GetY() && projLoc.GetY() < bottom + offset;
        }
    }
}