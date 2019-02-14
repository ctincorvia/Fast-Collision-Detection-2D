using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionDetection2D
{
    public interface ICollidable
    {
        int X { get; set; }

        int Y { get; set; }

        int SleepTime { get; set; }

        //the maximum speed this object will ever go
        int MaxSpeed { get; set; }

        // The zone this collider is located in
        Zone Zone { get; set; }

        // The distance from the center of this object to it's farthest point
        int CollisionRadius { get; set; }

        // Exact collision detection with another object
        bool PreciseCollides(ICollidable other);
    }
}
