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
            _size = size;
            // Round the collisionRadius up to the nearest int
            CollisionRadius = Convert.ToInt32(Math.Ceiling(Math.Sqrt(Math.Pow(size / 2, 2) * 2)));
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int XSpeed { get; set; }

        public int YSpeed { get; set; }

        public int SleepTime { get; set; }

        public Zone Zone { get; set; }

        public int CollisionRadius { get; set; }

        public void Tick()
        {
            X += XSpeed;
            Y += YSpeed;
        }

        //rote collision detection against another exact square
        public bool PreciseCollides(ICollidable other)
        {
            bool xCollides = false;
            bool yCollides = true;

            var maxX = X + (_size / 2);
            var minX = X - (_size / 2);
            var maxY = Y + (_size / 2);
            var minY = Y - (_size / 2);

            var otherCollider = other as DemoRectangle;

            var maxOX = otherCollider.X + (otherCollider._size / 2);
            var minOX = otherCollider.X - (otherCollider._size / 2);
            var maxOY = otherCollider.Y + (otherCollider._size / 2);
            var minOY = otherCollider.Y - (otherCollider._size / 2);

            xCollides = Between(maxOX, minOX, minX) || Between(maxOX, minOX, maxX);
            yCollides = Between(maxOY, minOY, minY) || Between(maxOY, minOY, maxY);

            return yCollides && xCollides;
        }

        bool Between(float max, float min, float point)
        {
            return (point >= min) && (point <= max);
        }

    }
}
