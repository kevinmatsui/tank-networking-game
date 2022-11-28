/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: World.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 12/9/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Class to create the world object that contains all of the objects in the game
    /// </summary>
    public class World
    {
        // Holds all of the dictionaries for the necessary objects in the world
        public Dictionary<int, Wall> walls { get; } = new Dictionary<int, Wall>();

        public Dictionary<int, Tank> tanks { get; } = new Dictionary<int, Tank>();

        public Dictionary<int, Beam> beams { get; } = new Dictionary<int, Beam>();

        public Dictionary<int, Projectile> projectiles { get; } = new Dictionary<int, Projectile>();

        public Dictionary<int, PowerUp> powerUps { get; } = new Dictionary<int, PowerUp>();

        public Dictionary<int, ControlCommands> controlCommands { get; } = new Dictionary<int, ControlCommands>();

        // fields for the world, that do not get serialized 
        public int Size;
        public int framesPerShot;
        public int respawnRate;
        public HashSet<long> disconnected = new HashSet<long>();
        public HashSet<int> deadProj = new HashSet<int>();
        public HashSet<int> deadPowerUp = new HashSet<int>();
        public int maxPowerDelay = 1650;
        public int maxPowerUps = 2;
        public int powerDelay = 0;
        public int currPowerFrames = 0;
        private Random rnd;

        private Settings settings;

        /// <summary>
        /// This constructor initializes the world by the given size.
        /// </summary>
        /// <param name="size"></param>
        public World(int size)
        {
            Size = size;
            rnd = new Random();
        }

        /// <summary>
        /// This is a constructor that initializes the world by the given settings.
        /// </summary>
        /// <param name="settings"></param>
        public World(Settings settings)
        {
            rnd = new Random();
            // initializing world settings
            this.settings = settings;
            Size = settings.UniverseSize;
            framesPerShot = settings.FramesPerShot;
            respawnRate = settings.RespawnRate;
            maxPowerUps = settings.MaxPowerUps;
            maxPowerDelay = settings.MaxPowerUpDelay;
            powerDelay = rnd.Next(maxPowerDelay);
        }

        /// <summary>
        /// This is a helper method to generate random location for the object.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Vector2D RandomLocation(int offset)
        {
            // intializing variables for random location
            int x = 0;
            int y = 0;
            bool noCollision = true;

            while (true)
            {
                noCollision = true;
                // generate random location for the object
                x = rnd.Next(-Size / 2, Size / 2);
                y = rnd.Next(-Size / 2, Size / 2);

                foreach (Wall wall in walls.Values)
                {
                    // if the object collides with the wall
                    if (wall.CollidesTankAndPowerUp(new Vector2D(x, y), offset))
                    {
                        noCollision = false;
                    }
                }

                // break out of the loop if there was no collision
                if (noCollision)
                {
                    break;
                }
            }
            return new Vector2D(x, y);
        }

        /// <summary>
        /// This method updates the state of the world.
        /// </summary>
        public void Update()
        {
            // loop through the control command dictionary
            foreach (KeyValuePair<int, ControlCommands> ctrlCmd in controlCommands)
            {
                Vector2D dir = null;
                // getting the tank that sent current control command
                Tank tank = tanks[ctrlCmd.Key];

                if(tank.Hp == 0)
                {
                    continue;
                }

                // updating the tanks velocity and direction given the moving command
                switch (ctrlCmd.Value.moving)
                {
                    case "up":
                        dir = new Vector2D(0, -1);
                        tank.Velocity = dir;
                        tank.Bdir = dir;
                        break;
                    case "right":
                        dir = new Vector2D(1, 0);
                        tank.Velocity = dir;
                        tank.Bdir = dir;
                        break;
                    case "down":
                        dir = new Vector2D(0, 1);
                        tank.Velocity = dir;
                        tank.Bdir = dir;
                        break;
                    case "left":
                        dir = new Vector2D(-1, 0);
                        tank.Velocity = dir;
                        tank.Bdir = dir;
                        break;
                    default:
                        tank.Velocity = new Vector2D(0, 0);
                        break;
                }

                // handles control commands for firing
                switch (ctrlCmd.Value.fire)
                {
                    case "main":
                        // if enough frames have passed since tanks last shot, create new projectile
                        if (tank.currFrameShot >= framesPerShot)
                        {
                            Projectile proj = new Projectile(tank.Loc, tank.Tdir, ctrlCmd.Key);
                            proj.Velocity *= settings.ProjectileSpeed;

                            projectiles.Add(proj.Id, proj);
                            // reset frame counter for tank
                            tank.currFrameShot = 0;
                        }
                        break;
                    case "alt":
                        if (tank.power > 0)
                        {
                            Beam beam = new Beam(tank.Loc, tank.Tdir, ctrlCmd.Key);
                            beams.Add(beam.Id, beam);
                            // decrement tank's power ups
                            tank.power--; 
                        }
                        break;
                    default:
                        break;
                }
                // change the tank's turret direction to the control command
                tank.Tdir = ctrlCmd.Value.tdir;
                // set tank velocity to the settings
                tank.Velocity *= settings.EngineStrength;

            } // end switch

            controlCommands.Clear();

            // if the current frame reaches the given power up frame counter
            // and the dictionary contains less than max power up values
            if (currPowerFrames > powerDelay && powerUps.Count < maxPowerUps)
            {
                // create power up object with random location
                PowerUp newPowerUp = new PowerUp(RandomLocation(PowerUp.size / 2 + settings.WallSize / 2 + 1));
                powerUps.Add(newPowerUp.Id, newPowerUp);

                // ranodmly generate delay for power ups less than the max delay
                powerDelay = rnd.Next(maxPowerDelay);
                currPowerFrames = 0; // reset current frame counter
            }
            else
            {
                currPowerFrames++; 
            }

            // remove dead power ups from the dictionary
            foreach (int id in deadPowerUp)
            {
                powerUps.Remove(id);
            }
            deadPowerUp.Clear();

            // update the world by
            // powerups
            foreach (PowerUp powerUp in powerUps.Values)
            {
                foreach (Tank tank in tanks.Values)
                {
                    // if alive tank gets power up
                    if (tank.Hp != 0 && tank.CollidesProjectileAndPowerUp(powerUp.loc, settings.TankSize/2))
                    {
                        tank.power++; // increment tank's power up values
                        // clean up dead power up
                        powerUp.died = true;
                        deadPowerUp.Add(powerUp.Id);
                    }
                }
            }
            // tanks
            foreach (Tank tank in tanks.Values)
            {
                // randomly generate tank's location it joins
                if (tank.Join)
                {
                    tank.Loc = RandomLocation(settings.TankSize / 2 + settings.WallSize / 2 + 1);
                }
                // if tank died on previous frame
                if (tank.Hp == 0 && tank.Died)
                {
                    tank.Died = false;
                }
                if (tank.Hp == 0)
                {
                    // increment the frames until respawn for the dead tank
                    tank.respawnFrames++;
                    // if enough frames have passed since the tank died, respawn the tank
                    if (tank.respawnFrames > respawnRate)
                    {
                        // sets the tanks location to the random location
                        tank.Loc = RandomLocation(settings.TankSize / 2 + settings.WallSize / 2 + 1);
                        tank.Hp = tank.MaxHP; // reset hp
                        // reset the frame counter to 0
                        tank.respawnFrames = 0;
                    }
                }
                // if tank is not moving, don't check for collisions
                if (tank.Velocity.Length() == 0)
                {
                    continue;
                }
                // updated tank location by velocity 
                Vector2D newLoc = tank.Loc + tank.Velocity;
                bool collision = false;

                //wrap around
                if (newLoc.GetY() < -Size / 2) // top
                {
                    newLoc = new Vector2D(newLoc.GetX(), Size / 2);
                }
                else if (newLoc.GetY() > Size / 2) // bottom
                {
                    newLoc = new Vector2D(newLoc.GetX(), -Size / 2);
                }
                else if (newLoc.GetX() > Size / 2) // right
                {
                    newLoc = new Vector2D(-Size / 2, newLoc.GetY());
                }
                else if (newLoc.GetX() < -Size / 2) // left
                {
                    newLoc = new Vector2D(Size / 2, newLoc.GetY());
                }

                // loop through the walls and check for collisions
                foreach (Wall wall in walls.Values)
                {
                    if (wall.CollidesTankAndPowerUp(newLoc, settings.WallSize / 2 + settings.TankSize / 2 + 1))
                    {
                        collision = true;
                        tank.Velocity = new Vector2D(0, 0);
                        break;
                    }
                }
                // if there are no collisions set the tanks location to the new location
                if (!collision)
                {
                    tank.Loc = newLoc;
                }

            } 

            // remove dead projectiles from the dictionary
            foreach (int id in deadProj)
            {
                projectiles.Remove(id);
            }
            deadProj.Clear();

            // projectiles
            foreach (Projectile proj in projectiles.Values)
            {
                // update pojectile location by velocity
                Vector2D newLoc = proj.loc + proj.Velocity;
                bool collision = false;

                // if projectile goes outside of the world 
                if (newLoc.GetY() < -Size / 2 || newLoc.GetY() > Size / 2 || newLoc.GetX() > Size / 2 || newLoc.GetX() < -Size / 2) 
                {
                    collision = true;
                }
               
                foreach (Wall wall in walls.Values)
                {
                    // if projectile hits the wall
                    if (wall.CollidesProjectile(newLoc, settings.WallSize/2))
                    {
                        collision = true;
                        break;
                    }
                }
                // checks for collisions between projectiles and tanks 
                foreach (Tank tank in tanks.Values)
                {
                    // if projectile hits other tank that is alive
                    if (tank.Hp != 0 && tank.CollidesProjectileAndPowerUp(newLoc, settings.TankSize/2) && tank.Id != proj.OwnerId)
                    {
                        collision = true;
                        // decrement the tanks hp
                        tank.Hp -= 1;

                        // if too many projectiles hit the tank at once
                        if(tank.Hp < 0)
                        {
                            tank.Hp = 0;
                        }
                        // if the tanks hp reached 0
                        if (tank.Hp == 0)
                        {
                            tank.Velocity = new Vector2D(0, 0);
                            tank.Died = true;
                            // increment the score of the tank that fired the projectile
                            tanks[proj.OwnerId].Score++;
                        }
                        break;
                    }
                }

                // if there was no collision, update the projectiles location
                if (!collision)
                {
                    proj.loc = newLoc;
                } 
                else
                {
                    // clean up dead projectile
                    proj.died = true;
                    deadProj.Add(proj.Id);
                }
            }

            // beams
            foreach (Beam beam in beams.Values)
            {
                foreach (Tank tank in tanks.Values)
                {
                    // if beam hits the tank that is alive
                    if (tank.Hp != 0 && Tank.Intersects(beam.loc, beam.dir, tank.Loc, 30))
                    {
                        // increment the score of the tank that fired beam
                        tanks[beam.OwnerId].Score++;
                        // update for dead tank
                        tank.Died = true;
                        tank.Hp = 0;
                    }
                }
            }

            // if current tank's client is disconnected
            foreach (long id in disconnected)
            {
                // update tanks properties
                tanks[(int)id].Dc = true;
                tanks[(int)id].Hp = 0;
                tanks[(int)id].Died = true;
            }

        }

    }
}
