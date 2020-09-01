﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridCreation
{
    public enum CreateMode
    {
        /// <summary>
        /// Create grids with selected lines/arcs
        /// </summary>
        Select,
        /// <summary>
        /// Create orthogonal grids
        /// </summary>
        Orthogonal,
        /// <summary>
        /// Create radial and arc grids
        /// </summary>
        RadialAndArc
    }

    public enum BubbleLocation
    {
        /// <summary>
        /// Place bubble at the start point
        /// </summary>
        StartPoint,
        /// <summary>
        /// Place bubble at the end point
        /// </summary>
        EndPoint
    }

    /// <summary>
    /// Class contains common const values
    /// </summary>
    static class Values
    {
        public const double PI = 3.1415926535897900;
        // ratio from degree to radian
        public const double DEGTORAD = PI / 180;
    }
}
