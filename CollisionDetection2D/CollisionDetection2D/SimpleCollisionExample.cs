using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace CollisionDetection2D
{   
    // A simple example of the easiest implementation of collision detection
    // every object is compared to every other object in the space every detection 
    // used in testing the accuracy of the optimized detection engine
    public class SimpleCollisionExample
    {
        public List<ICollidable> CollisionObjects;
        public SimpleCollisionExample()
        {
            CollisionObjects = new List<ICollidable>();
        }

        public HashSet<Tuple<ICollidable, ICollidable>> DetectCollisions()
        {
            HashSet <Tuple<ICollidable, ICollidable>> collisions = new HashSet<Tuple<ICollidable, ICollidable>>();
            int colliderCount = CollisionObjects.Count;
            for (int i = 0; i < colliderCount; i++)
            {
                for (int j = i + 1; j < colliderCount; j++)
                {
                    ICollidable collider1 = CollisionObjects[i];
                    ICollidable collider2 = CollisionObjects[j];
                    bool collisionFound = PairWiseCollisionCheck(collider1, collider2);
                    if (collisionFound)
                        collisions.Add(new Tuple<ICollidable, ICollidable>(collider1, collider2));
                }
            }
            return collisions;
        }

        private bool PairWiseCollisionCheck(ICollidable collider1, ICollidable collider2)
        {
            return collider1.PreciseCollides(collider2);
        }        
    }
}
