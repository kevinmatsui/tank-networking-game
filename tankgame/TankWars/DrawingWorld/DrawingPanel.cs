/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: DrawingWorld.csproj
// FileName: DrawingPanel.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 11/29/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using GameController;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace DrawingWorld
{
    public class DrawingPanel : Panel
    {
        // private fields
        private World theWorld;
        private Controller controller;

        // for animations
        private Dictionary<int, Animation> circles;
        private Dictionary<Beam, Animation> beamAnimations;

        // for image set dictionary
        private Dictionary<string, Image> images;

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        public DrawingPanel(World w, Controller controller)
        {
            // initializing fields
            DoubleBuffered = true;
            theWorld = w;
            this.controller = controller;
            circles = new Dictionary<int, Animation>();
            beamAnimations = new Dictionary<Beam, Animation>();

            // storing images that will be used
            images = new Dictionary<string, Image>();
            images.Add("background", Image.FromFile("..\\..\\..\\Resources\\Images\\Background.png"));
            images.Add("wall", Image.FromFile("..\\..\\..\\Resources\\Images\\WallSprite.png"));

            images.Add("blueProj", Image.FromFile("..\\..\\..\\Resources\\Images\\BlueProjectile.png"));
            images.Add("blueTank", Image.FromFile("..\\..\\..\\Resources\\Images\\BlueTank.png"));
            images.Add("blueTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\BlueTurret.png"));

            images.Add("darkProj", Image.FromFile("..\\..\\..\\Resources\\Images\\DarkProjectile.png"));
            images.Add("darkTank", Image.FromFile("..\\..\\..\\Resources\\Images\\DarkTank.png"));
            images.Add("darkTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\DarkTurret.png"));

            images.Add("greenProj", Image.FromFile("..\\..\\..\\Resources\\Images\\GreenProjectile.png"));
            images.Add("greenTank", Image.FromFile("..\\..\\..\\Resources\\Images\\GreenTank.png"));
            images.Add("greenTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\GreenTurret.png"));

            images.Add("lightGreenProj", Image.FromFile("..\\..\\..\\Resources\\Images\\LightGreenProjectile.png"));
            images.Add("lightGreenTank", Image.FromFile("..\\..\\..\\Resources\\Images\\LightGreenTank.png"));
            images.Add("lightGreenTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\LightGreenTurret.png"));

            images.Add("orangeProj", Image.FromFile("..\\..\\..\\Resources\\Images\\OrangeProjectile.png"));
            images.Add("orangeTank", Image.FromFile("..\\..\\..\\Resources\\Images\\OrangeTank.png"));
            images.Add("orangeTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\OrangeTurret.png"));

            images.Add("purpleProj", Image.FromFile("..\\..\\..\\Resources\\Images\\PurpleProjectile.png"));
            images.Add("purpleTank", Image.FromFile("..\\..\\..\\Resources\\Images\\PurpleTank.png"));
            images.Add("purpleTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\PurpleTurret.png"));

            images.Add("redProj", Image.FromFile("..\\..\\..\\Resources\\Images\\RedProjectile.png"));
            images.Add("redTank", Image.FromFile("..\\..\\..\\Resources\\Images\\RedTank.png"));
            images.Add("redTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\RedTurret.png"));

            images.Add("yellowProj", Image.FromFile("..\\..\\..\\Resources\\Images\\YellowProjectile.png"));
            images.Add("yellowTank", Image.FromFile("..\\..\\..\\Resources\\Images\\YellowTank.png"));
            images.Add("yellowTurr", Image.FromFile("..\\..\\..\\Resources\\Images\\YellowTurret.png"));

        }

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();
            // changes panel origin and angle
            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e); // draw on the panel by calling the delegate

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// This method is to draw the tank.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            int id = t.Id;
            // helper method to draw different color tanks by its id
            ColorChanger(id, "Tank", e);
        }

        /// <summary>
        /// This method is to draw the turret.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            int id = t.Id;
            // helper method to draw different color turrets by its id
            ColorChanger(id, "Turr", e);
        }

        /// <summary>
        /// This method is to draw the projectile.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile proj = o as Projectile;
            int id = proj.OwnerId;
            // helper method to draw different color projectiles by its id
            ColorChanger(id, "Proj", e);
        }

        /// <summary>
        /// This is a helper method to draw tank/turret/projectile with different color by the player's unique id
        /// </summary>
        /// <param name="id">player id</param>
        /// <param name="obj">string for tank/turret/projectile</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void ColorChanger(int id, string obj, PaintEventArgs e)
        {
            // to draw 8 different color tanks with tank's id
            if (id % 8 == 0)
            {
                // helper method to draw blue color tank / turret / projectile by finding from images dictionary
                ObjectChanger(obj, images["blue" + obj], e);
            }
            else if (id % 8 == 1)
            {
                // dark
                ObjectChanger(obj, images["dark" + obj], e);
            }
            else if (id % 8 == 2)
            {
                // green
                ObjectChanger(obj, images["green" + obj], e);
            }
            else if (id % 8 == 3)
            {
                // light green
                ObjectChanger(obj, images["lightGreen" + obj], e);
            }
            else if (id % 8 == 4)
            {
                // orange
                ObjectChanger(obj, images["orange" + obj], e);
            }
            else if (id % 8 == 5)
            {
                // purple
                ObjectChanger(obj, images["purple" + obj], e);
            }
            else if (id % 8 == 6)
            {
                // red
                ObjectChanger(obj, images["red" + obj], e);
            }
            else if (id % 8 == 7)
            {
                // yellow
                ObjectChanger(obj, images["yellow" + obj], e);
            }
        }

        /// <summary>
        /// This is a helper method to draw tank/turret/projectile with different size
        /// </summary>
        /// <param name="obj">string for tank/turret/projectile</param>
        /// <param name="im">image object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void ObjectChanger(string obj, Image im, PaintEventArgs e)
        {
            // if the passed object string is for Tank
            if (obj == "Tank")
            {
                // draw with tank size
                e.Graphics.DrawImage(im, -30, -30, 60, 60);
            }
            // is for Turret
            else if (obj == "Turr")
            {
                // draw with turret size
                e.Graphics.DrawImage(im, -25, -25, 50, 50);
            }
            // is for Projectile
            else if (obj == "Proj")
            {
                // draw with projectile size
                e.Graphics.DrawImage(im, -15, -15, 30, 30);
            }
        }

        /// <summary>
        /// This method is to draw the player name and score.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        private void TankInfoDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            // for centering the player name/score textbox
            Rectangle rect = new Rectangle(-125, 40, 250, 25);
            StringFormat sForm = new StringFormat();
            sForm.Alignment = StringAlignment.Center;
            sForm.LineAlignment = StringAlignment.Center;

            // for player name/score font
            GraphicsUnit units = GraphicsUnit.Pixel;
            Font helvetica = new Font("Helvetica", 15, FontStyle.Bold, units);

            // draw player name with its score
            e.Graphics.DrawString(t.Name + ": " + t.Score, helvetica, Brushes.White, rect, sForm);

            // draw player healthbar
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            {
                Rectangle healthR = new Rectangle(-22, -30, 45, 6); // size of the healthbar rectangle
                if (t.Hp == 3)
                {
                    e.Graphics.FillRectangle(greenBrush, healthR); // green healthbar
                }
                else if (t.Hp == 2)
                {
                    healthR.Width = 33; // decrease the healthbar width
                    e.Graphics.FillRectangle(yellowBrush, healthR); // yellow healthbar
                }
                else if (t.Hp == 1)
                {
                    healthR.Width = 16; // decrease the healthbar width
                    e.Graphics.FillRectangle(redBrush, healthR); // red healthbar
                }
            }
        }

        /// <summary>
        /// This method is to draw the background.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void BackgroundDrawer(PaintEventArgs e)
        {

            // starting point of background in world coordinate
            float start = -controller.WorldSize / 2;

            // draw by finding background image from the dictionary
            e.Graphics.DrawImage(images["background"], start, start, controller.WorldSize, controller.WorldSize);
        }

        /// <summary>
        /// This method is to draw the wall.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;
            Vector2D point1 = w.p1;
            Vector2D point2 = w.p2;

            // for calculating total bricks
            int numBricks = 0;
            Vector2D difference = point1 - point2;

            // starting point of the wall image
            float x = (float)point1.GetX() - 25;
            float y = (float)point1.GetY() - 25;

            // if bricks are horizontal
            if (difference.GetY() == 0)
            {
                // get total number of bricks for the wall
                numBricks = Math.Abs((int)difference.GetX() / 50) + 1;

                // if second point is before the first point
                if (point1.GetX() > point2.GetX())
                {
                    // loop total number of bricks times
                    for (int i = 0; i < numBricks; i++)
                    {
                        e.Graphics.DrawImage(images["wall"], x, y, 50, 50); // draw wall
                        x -= 50; // decrement x position
                    }
                }
                // if first point is before the first point
                else if (point1.GetX() < point2.GetX())
                {
                    for (int i = 0; i < numBricks; i++)
                    {
                        e.Graphics.DrawImage(images["wall"], x, y, 50, 50); // draw wall
                        x += 50; // increment x poisiton
                    }
                }
            }
            // if bricks are vertical
            else if (difference.GetX() == 0)
            {
                // get total number of bricks for the wall
                numBricks = Math.Abs((int)difference.GetY() / 50) + 1;
                // if second point is above the first point
                if (point1.GetY() > point2.GetY())
                {
                    // loop total number of bricks times
                    for (int i = 0; i < numBricks; i++)
                    {
                        e.Graphics.DrawImage(images["wall"], x, y, 50, 50); // draw wall
                        y -= 50; // decrement y
                    }
                }
                // if first point is above the first point
                else if (point1.GetY() < point2.GetY())
                {
                    for (int i = 0; i < numBricks; i++)
                    {
                        e.Graphics.DrawImage(images["wall"], x, y, 50, 50); // draw wall
                        y += 50; // increment y
                    }
                }
            }
        }

        /// <summary>
        /// This method is to draw the powerup.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void PowerUpDrawer(object o, PaintEventArgs e)
        {
            PowerUp p = o as PowerUp;

            RectangleF srcRect = new RectangleF(0, 0, 10, 10); // size of the powerup

            // draw power up circle
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            {
                e.Graphics.FillEllipse(yellowBrush, srcRect);
            }
        }

        /// <summary>
        /// This method is to draw the tank explosion animation.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void TankExplosion(object o, PaintEventArgs e)
        {
            RectangleF srcRect = new RectangleF(0, 0, 5, 5); // size of the explosion circle

            // draw explosion circle
            using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                e.Graphics.FillEllipse(blackBrush, srcRect);
            }
        }

        /// <summary>
        /// This method is to draw the beam animation.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Animation beam = o as Animation;

            double limit = beam.GetLimit();
            int yIncrement = 0;
            float xMid = -(float)(8 - limit / 5) / 2; // center point for the beam

            // decrease the beam size with the given limit value
            RectangleF srcRect = new RectangleF(xMid, 20, (float)(8 - limit / 5), 1300);
            // initialization for circle animation around the beam
            RectangleF cirRect = new RectangleF(0, 0, 0, 0);

            using (System.Drawing.SolidBrush blueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue))
            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            {
                e.Graphics.FillRectangle(whiteBrush, srcRect); // draw the beam

                // loop to draw many circles around the beam
                for (int i = 0; i < 15; i++)
                {
                    // draw circles above the beam
                    cirRect = new RectangleF((float)limit, 0 + yIncrement, 3, 3);
                    e.Graphics.FillEllipse(blueBrush, cirRect);

                    // draw circles below the beam
                    cirRect = new RectangleF(-(float)limit, 0 + yIncrement, 3, 3);
                    e.Graphics.FillEllipse(blueBrush, cirRect);

                    // y increment to draw circles in different location
                    yIncrement += 60;
                }
            }
        }

        /// <summary>
        /// This is a method that runs on every frame to draw the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (theWorld.tanks.Count > 0)
            {
                // lock for multiple threads
                lock (theWorld)
                {
                    // to center the view on the middle of the world
                    float worldX = (float)((900 / 2) - controller.userT.Loc.GetX());
                    float worldY = (float)((900 / 2) - controller.userT.Loc.GetY());
                    e.Graphics.TranslateTransform(worldX, worldY);

                    // if the size of the world is not 0
                    if (controller.WorldSize != 0)
                    {
                        BackgroundDrawer(e); // draw background 
                    }

                    // for each walls in the dictionary
                    foreach (Wall wall in theWorld.walls.Values)
                    {
                        DrawObjectWithTransform(e, wall, 0, 0, 0, WallDrawer); // draw wall
                    }

                    // initializing the set to remove the tank explosion that is done
                    HashSet<int> cDied = new HashSet<int>();

                    // loop animation circles dictionary
                    foreach (int key in circles.Keys)
                    {
                        Animation c = circles[key];
                        double limit = c.GetLimit();

                        // if the circle limit did not exceed
                        if (limit <= 100)
                        {
                            // draw animation circles with different x and y values by using the limit
                            // cross
                            DrawObjectWithTransform(e, c, c.GetX() - limit, c.GetY(), 0, TankExplosion);
                            DrawObjectWithTransform(e, c, c.GetX() + limit, c.GetY(), 0, TankExplosion);
                            DrawObjectWithTransform(e, c, c.GetX(), c.GetY() - limit, 0, TankExplosion);
                            DrawObjectWithTransform(e, c, c.GetX(), c.GetY() + limit, 0, TankExplosion);
                            // diagonal
                            DrawObjectWithTransform(e, c, c.GetX() - limit, c.GetY() - limit, 0, TankExplosion);
                            DrawObjectWithTransform(e, c, c.GetX() + limit, c.GetY() + limit, 0, TankExplosion);
                            DrawObjectWithTransform(e, c, c.GetX() + limit, c.GetY() - limit, 0, TankExplosion);
                            DrawObjectWithTransform(e, c, c.GetX() - limit, c.GetY() + limit, 0, TankExplosion);

                            limit += 5;
                            c.SetLimit(limit);
                        }
                        else
                        {
                            cDied.Add(key); // store to the dead circles set if the limit exceeds
                        }
                    }

                    // loop dead circles set
                    foreach (int id in cDied)
                    {
                        // if the dead tank respawns or has been removed from tank dictionary
                        if (!theWorld.tanks.ContainsKey(id) || theWorld.tanks[id].Hp != 0)
                        {
                            // remove the dead circle animation
                            circles.Remove(id);
                        }
                    }

                    // initializing set to remove disconnected tanks
                    HashSet<int> dcTank = new HashSet<int>();
                    // loop tanks dictionary
                    foreach (Tank tank in theWorld.tanks.Values)
                    {
                        if(tank.Dc)
                        {
                            dcTank.Add(tank.Id); // add disconnected tank to set
                        }
                        // if the tank died
                        // and the dead tank is not already in animation circles dictionary
                        if (!circles.ContainsKey(tank.Id) && (tank.Died || tank.Hp == 0))
                        {
                            // store to the animation circles dictionary with tank info
                            circles.Add(tank.Id, new Animation(tank.Loc.GetX(), tank.Loc.GetY()));
                        }
                        else if (tank.Hp != 0) // if tank is not dead
                        {
                            // draw tank
                            DrawObjectWithTransform(e, tank, tank.Loc.GetX(), tank.Loc.GetY(), tank.Bdir.ToAngle(), TankDrawer);
                            // draw turret
                            DrawObjectWithTransform(e, tank, tank.Loc.GetX(), tank.Loc.GetY(), tank.Tdir.ToAngle(), TurretDrawer);
                            // draw player name and healthbar
                            DrawObjectWithTransform(e, tank, tank.Loc.GetX(), tank.Loc.GetY() - 10, 0, TankInfoDrawer);
                        }

                    }
                    // remove the disconnected tanks from the tank dictionary
                    foreach(int id in dcTank)
                    {
                        theWorld.tanks.Remove(id);
                    }
                    // initializing the set for removing dead powerups
                    HashSet<int>powerDied = new HashSet<int>();
                    // loop powerups dictionary 
                    foreach (PowerUp powerUp in theWorld.powerUps.Values)
                    {
                        if (powerUp.died)
                        {
                            powerDied.Add(powerUp.Id); // add to the powerup died set
                        }
                        else
                        {
                            // draw power ups
                            DrawObjectWithTransform(e, powerUp, powerUp.loc.GetX(), powerUp.loc.GetY(), 0, PowerUpDrawer);
                        }
                    }

                    foreach (int id in powerDied)
                    {
                        // remove died power up from the dictionary
                        theWorld.powerUps.Remove(id);
                    }

                    // initializing the set for removing dead projectiles
                    HashSet<int> projDied = new HashSet<int>();
                    // loop projectiles dictionary
                    foreach (Projectile proj in theWorld.projectiles.Values)
                    {
                        if (proj.died)
                        {
                            projDied.Add(proj.Id); // add to the projectile died set
                        }
                        else
                        {
                            // draw projectiles
                            DrawObjectWithTransform(e, proj, proj.loc.GetX(), proj.loc.GetY(), proj.dir.ToAngle(), ProjectileDrawer);
                        }
                    }

                    foreach (int id in projDied)
                    {
                        // remove died projectiles from the dictionary
                        theWorld.projectiles.Remove(id);
                    }

                    // loop beam dictionary
                    foreach (Beam beam in theWorld.beams.Values)
                    {
                        // add to the beam animation dictionary
                        beamAnimations.Add(beam, new Animation(beam.org.GetX(), beam.org.GetY()));
                    }

                    // initializing the set for removing dead beams
                    HashSet<Beam>beamDied = new HashSet<Beam>();
                    // loop beam animation dictionary
                    foreach (Beam beam in beamAnimations.Keys)
                    {
                        // remove the dead beam from the beam dictionary
                        theWorld.beams.Remove(beam.Id);

                        Animation b = beamAnimations[beam];
                        double limit = b.GetLimit();
                        // if the beam limit did not exceed
                        if (limit <= 40)
                        {
                            // draw the beam
                            DrawObjectWithTransform(e, b, beam.org.GetX(), beam.org.GetY(), beam.dir.ToAngle() + 180, BeamDrawer);
                            // increment the limit

                            limit++;
                            b.SetLimit(limit);
                        }
                        else
                        {
                            // store to the beam died set if the limit exceeds
                            beamDied.Add(beam);
                        }
                    }

                    foreach (Beam bDied in beamDied)
                    {
                        // remove from the beam animation dictionary
                        beamAnimations.Remove(bDied);
                    }

                    // Do anything that Panel (from which we inherit) needs to do
                    base.OnPaint(e);

                } // end lock
            } // end if

        }
    }
}
