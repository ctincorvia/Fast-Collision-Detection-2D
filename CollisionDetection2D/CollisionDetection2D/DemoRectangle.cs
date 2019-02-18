using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionDetection2D
{
    public class DemoRectangle : ICollidable
    {
        int _size;

        public DemoRectangle(int x, int y, int xSpeed, int ySpeed, int size)
        {
            X = x;
            Y = y;
            XSpeed = xSpeed;
            YSpeed = ySpeed;
            MaxSpeed = 2;
            _size = size;
            // Round the collisionRadius up to the nearest int
            CollisionRadius = Convert.ToInt32(Math.Ceiling(Math.Sqrt(Math.Pow(((double)size) / 2, 2) * 2)));
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int XSpeed { get; set; }

        public int YSpeed { get; set; }

        public int SleepTime { get; set; }

        public Zone Zone { get; set; }

        public int CollisionRadius { get; set; }

        public int MaxSpeed { get; set; }

        public void Tick()
        {
            if(X > 0 && X < 1000)
                X += XSpeed;
            if( Y > 0 && Y < 1000)
                Y += YSpeed;
        }

        //rote collision detection against another exact square
        public bool PreciseCollides(ICollidable other)
        {
            bool xCollides = false;
            bool yCollides = true;
            float size = (float)_size;
            var otherCollider = other as DemoRectangle;
            float otherSize = (float)otherCollider._size;

            var maxX = X + (size / 2);
            var minX = X - (size / 2);
            var maxY = Y + (size / 2);
            var minY = Y - (size / 2);           

            var maxOX = otherCollider.X + (otherSize / 2);
            var minOX = otherCollider.X - (otherSize / 2);
            var maxOY = otherCollider.Y + (otherSize / 2);
            var minOY = otherCollider.Y - (otherSize / 2);

            xCollides = Between(maxOX, minOX, minX) || Between(maxOX, minOX, maxX);
            yCollides = Between(maxOY, minOY, minY) || Between(maxOY, minOY, maxY);

            xCollides = xCollides || (Between(maxX, minX, minOX) || Between(maxX, minX, maxOX));
            yCollides = yCollides || (Between(maxY, minY, minOY) || Between(maxY, minY, maxOY)); 

            return yCollides && xCollides;
        }

        bool Between(float max, float min, float point)
        {
            return (point >= min) && (point <= max);
        }

    }
}
