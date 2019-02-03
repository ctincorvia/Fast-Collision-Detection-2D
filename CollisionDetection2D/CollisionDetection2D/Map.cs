using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace CollisionDetection2D
{
    public class Map
    {
        int width;
        int height;
        int maxSpeed;
        bool coarseCollisionActivated;
        bool sleepingObjectsActivated;
        List<Zone> Zones;
        HashSet<ICollidable> CollisionObjects;
        public Map(int width, int height, bool coarseCollision, bool sleepingObjects, int maxSpeed, int xZones = 1, int yZones = 1)
        {
            this.width = width;
            this.height = height;
            this.maxSpeed = maxSpeed;
            coarseCollisionActivated = coarseCollision;
            sleepingObjectsActivated = sleepingObjects;
            CollisionObjects = new HashSet<ICollidable>();
            CreateZones(xZones, yZones);
        }

        public HashSet<Tuple<ICollidable, ICollidable>> DetectCollisions()
        {
            ConcurrentDictionary<ICollidable, HashSet<ICollidable>> CollisionsFound =
                new ConcurrentDictionary<ICollidable, HashSet<ICollidable>>();
            ConcurrentDictionary<ICollidable, HashSet<ICollidable>> CollisionsChecked =
                new ConcurrentDictionary<ICollidable, HashSet<ICollidable>>();

            TickSleepTimers();
            AssignColliderZones();   

            // loop through all of the zones and have each only compare to relevant objects
            Parallel.ForEach(Zones, (zone) =>
            {
                List<ICollidable> CollisionObjectsToCompare = zone.ComputeCollisionObjects();
                int colliderCount = CollisionObjectsToCompare.Count;
                for(int i = 0; i < colliderCount; i++)
                {
                    for(int j = i; j < colliderCount; j++)
                    {
                        ICollidable collider1 = CollisionObjectsToCompare[i];
                        ICollidable collider2 = CollisionObjectsToCompare[j];
                        //if we've already checked this pair of colliders, skip checking them now
                        if (CollisionsChecked[collider1].Contains(collider2))
                            continue;
                        bool collisionFound = PairWiseCollisionCheck(collider1, collider2);
                        if(collisionFound)
                            SafeAddToDictionary(CollisionsFound, collider1, collider2);
                        SafeAddToDictionary(CollisionsChecked, collider1, collider2);
                    }
                }

            });

            return ConstructSetFromDictionary(CollisionsFound);
        }

        public void AddCollider(ICollidable collider)
        {
            CollisionObjects.Add(collider);
            // Upon the addition of a collider object, place it in a zone
            AssignNewZone(collider);
        }

        public void AddColliders(IEnumerable<ICollidable> colliders)
        {
            foreach (var collider in colliders)
                AddCollider(collider);
        }

        public void RemoveCollider(ICollidable collider)
        {
            CollisionObjects.Remove(collider);
        }

        public void RemoveColliders(IEnumerable<ICollidable> colliders)
        {
            foreach (var collider in colliders)
                RemoveCollider(collider);
        }

        //Create all the zones to hold the objects
        //This is run only once, when the Map instance is created.
        private void CreateZones(int xZones, int yZones)
        {
            Zones = new List<Zone>();
            int xInterval = width / xZones;
            int yInterval = height / yZones;
            for( int i = 0; i < xZones; i++)
            {
                for (int j = 0; j < yZones; j++)
                {
                    var minX = i * xInterval;
                    var maxX = (i + 1) * xInterval;
                    var minY = i * yInterval;
                    var maxY = (i + 1) * yInterval;
                    Zones.Add(new Zone(minX, maxX, minY, maxY));
                }
            }
            //For each zone mark all of the zones adjacent to it
            for(var zoneIndex = 0; zoneIndex < Zones.Count; ++zoneIndex)
            {
                int beginning = zoneIndex - xZones - 1;
                int end = beginning + 2;
                for (var i = 0; i < 3; ++i)
                {
                    for(var adjacentZoneIndex = beginning; adjacentZoneIndex < end; ++adjacentZoneIndex)
                    {
                        if(adjacentZoneIndex > 0 && adjacentZoneIndex < Zones.Count)
                        {
                            Zones[zoneIndex].AddAdjacentZone(Zones[adjacentZoneIndex]);
                        }
                    }
                    beginning += xZones;
                    end = beginning + 2;
                }     
            }
        }

        private void AssignColliderZones()
        {
            foreach (var zone in Zones)
                zone.ClearColliders();
            foreach(var collider in CollisionObjects)
                AssignZone(collider);
        }

        //Check a collider's current zone, assign one if it's not correct
        private void AssignZone(ICollidable collider)
        {
            // Asleep colliders are not considered for collision detection
            if (collider.SleepTime > 0)
                return;
            AssignNewZone(collider);
        }

        //Assign a new zone to a collider
        private void AssignNewZone(ICollidable collider)
        {
            foreach (var zone in Zones)
            {
                if (zone.WithinBounds(collider))
                {
                    zone.AddCollider(collider);
                    collider.Zone = zone;
                    return;
                }
            }
        }

        private bool PairWiseCollisionCheck(ICollidable collider1, ICollidable collider2)
        {

            // The square root function is computationally expensive.  Using the sqared value instead improves speed.
            int collideDist = collider1.CollisionRadius + collider2.CollisionRadius;
            int actualDist = Convert.ToInt32(Math.Sqrt(Math.Pow(collider1.X - collider2.X, 2) + Math.Pow(collider1.Y - collider2.Y, 2)));

            if (actualDist <= collideDist)
                return collider1.PreciseCollides(collider2);

            SetSleepTimer(collider1, actualDist - collideDist);
            SetSleepTimer(collider2, actualDist - collideDist);

            return false;
        }

        private void SetSleepTimer(ICollidable collider, int distance)
        {
            double time = distance / maxSpeed;
            time = Math.Floor(time);
            if (time < collider.SleepTime)
                collider.SleepTime = Convert.ToInt32(time);
        }

        private void TickSleepTimers()
        {
            foreach(var collider in CollisionObjects)
            {
                if (collider.SleepTime > 0)
                    --collider.SleepTime;
            }
        }

        //The dictionary has some duplicate data to avoid making redundant comparisons.  Flatten everything in to a hash set here.
        private HashSet<Tuple<ICollidable, ICollidable>> ConstructSetFromDictionary(ConcurrentDictionary<ICollidable, HashSet<ICollidable>> dictionary)
        {
            HashSet<Tuple<ICollidable, ICollidable>> colliderHash = new HashSet<Tuple<ICollidable, ICollidable>>();
            foreach (var colliderKey in dictionary.Keys)
            {
                HashSet<ICollidable> colliderSet = dictionary[colliderKey];
                foreach(var colliderValue in colliderSet)
                {
                    colliderHash.Add(new Tuple<ICollidable, ICollidable>(colliderKey, colliderValue));
                    if(dictionary.ContainsKey(colliderValue))
                        dictionary[colliderValue].Remove(colliderKey);                 
                }
            }
            return null;
        }

        // Maintain a dictionary of all the colliders that have been checked against eachother
        // no matter which collider is used as the key, the other will be in the value 
        private void SafeAddToDictionary(ConcurrentDictionary<ICollidable, HashSet<ICollidable>> dictionary, ICollidable collider1, ICollidable collider2)
        {
            if (dictionary.ContainsKey(collider1))
                dictionary[collider1].Add(collider2);
            else
            {
                dictionary[collider1] = new HashSet<ICollidable>();
                dictionary[collider1].Add(collider2);
            }
            if (dictionary.ContainsKey(collider2))
                dictionary[collider2].Add(collider1);
            else
            {
                dictionary[collider2] = new HashSet<ICollidable>();
                dictionary[collider2].Add(collider1);
            }
        }










    }
}
