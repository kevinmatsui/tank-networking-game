/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: Tank.cs
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
    /// Class to make the Tank objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        // Fields for tank that don't get serialized
        public int MaxHP { get; private set; } = 3;
        public int currFrameShot = 0;

        public int respawnFrames = 0;
        public int power = 0;
        public Vector2D Velocity { get; internal set; }

        /// <summary>
        /// Default constructor for JSON Serialization
        /// </summary>
        public Tank()
        {
        }
        /// <summary>
        /// Creates tank object with given id, name, and maxHp
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="MaxHP"></param>
        public Tank(int id, string name, int MaxHP)
        {
            Id = id;
            Loc = new Vector2D(0, 0);
            Bdir = new Vector2D(0, -1); // faces upwards
            Tdir = Bdir;
            Name = name;
            Hp = MaxHP;
            this.MaxHP = MaxHP;
            Score = 0;
            Died = false;
            Dc = false;
            Join = true;
            Velocity = new Vector2D(0, 0);
        }

        // given fields made as JsonProperties
        [JsonProperty(PropertyName = "tank")]
        public int Id { get; private set; } = 0;
        [JsonProperty]
        public string Name { get; private set; } = null;
        [JsonProperty]
        public Vector2D Loc { get; internal set; } = null;
        [JsonProperty]
        public Vector2D Bdir { get; internal set; } = null;
        [JsonProperty]
        public Vector2D Tdir { get; internal set; } = null;
        [JsonProperty]
        public int Score { get; internal set; } = 0;
        [JsonProperty]
        public int Hp { get; internal set; } = 3;
        [JsonProperty]
        public bool Died { get; internal set; } = false;
        [JsonProperty]
        public bool Dc { get; internal set; } = false;
        [JsonProperty]
        public bool Join { get; set; } = false;

        /// <summary>
        /// Overrided toString method to serialize the tank to json.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        /// <summary>
        /// Returns true if the current tank's location collides with a projectile or
        /// powerup with the given offset of half the tanks size
        /// </summary>
        /// <param name="projLoc"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool CollidesProjectileAndPowerUp(Vector2D projLoc, int offset)
        {

            return Loc.GetX() + offset > projLoc.GetX() && projLoc.GetX() > Loc.GetX() - offset
                && Loc.GetY() + offset > projLoc.GetY() && projLoc.GetY() > Loc.GetY() - offset;
        }

        /// <summary>
        /// Determines if a beam intersects the tank
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }
    }
}
