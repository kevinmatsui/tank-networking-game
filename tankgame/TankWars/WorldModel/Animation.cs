/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: Animation.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 11/29/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Creates animation object to be used for the tank explosions and beams
    /// </summary>
    public class Animation
    {
        // the x and y coordinates in world space of the animation
        private double x;
        private double y;

        // keeps track of how many times the animation has played
        private double limit;

        /// <summary>
        /// Two param constructor given x and y location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Animation(double x, double y)
        {
            this.x = x;
            this.y = y;
            //sets initial limit to 5
            this.limit = 5;
        }

        /// <summary>
        /// This method gets the limit
        /// </summary>
        /// <returns></returns>
        public double GetLimit()
        {
            return this.limit;
        }
       
        /// <summary>
        /// This method sets the limit
        /// </summary>
        /// <param name="limit"></param>
        public void SetLimit(double limit)
        {
            this.limit = limit;
        }
        /// <summary>
        /// Returns x coordinate of animation
        /// </summary>
        /// <returns></returns>
        public double GetX()
        {
            return this.x;
        }
        /// <summary>
        /// Returns y coordinate of animation
        /// </summary>
        /// <returns></returns>
        public double GetY()
        {
            return this.y;
        }

    }
}
