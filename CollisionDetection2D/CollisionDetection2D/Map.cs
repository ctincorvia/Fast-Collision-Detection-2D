using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        List<Zone> Zones;
        HashSet<ICollidable> CollisionObjects;
        public Map(int width, int height, int maxSpeed, int xZones = 1, int yZones = 1)
        {
            this.width = width;
            this.height = height;
            this.maxSpeed = maxSpeed;
            CollisionObjects = new HashSet<ICollidable>();
            CreateZones(xZones, yZones);
        }

        public HashSet<Tuple<ICollidable, ICollidable>> DetectCollisions()
        {
            ConcurrentDictionary<ICollidable, ConcurrentColliderHash> CollisionsFound =
                new ConcurrentDictionary<ICollidable, ConcurrentColliderHash>();

            TickSleepTimers();
            AssignColliderZones();

            var sw = Stopwatch.StartNew();
            // loop through all of the zones and have each only compare to relevant objects
            //foreach(var zone in Zones)
            Parallel.ForEach(Zones, (zone) =>            
            {
                var swInner = Stopwatch.StartNew();
                List<ICollidable> CollisionObjectsToCompare = zone.ComputeCollisionObjects();
                swInner.Stop();
                int colliderCount = CollisionObjectsToCompare.Count;
                var swInner2 = Stopwatch.StartNew();
                for (int i = 0; i < colliderCount; i++)
                {
                    for (int j = i + 1; j < colliderCount; j++)
                    {
                        ICollidable collider1 = CollisionObjectsToCompare[i];
                        ICollidable collider2 = CollisionObjectsToCompare[j];
                        bool collisionFound = PairWiseCollisionCheck(collider1, collider2);
                        if (collisionFound)
                            SafeAddToDictionary(CollisionsFound, collider1, collider2);
                    }
                }
                swInner2.Stop();
            }
            );
            sw.Stop();

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
                    var minY = j * yInterval;
                    var maxY = (j + 1) * yInterval;
                    Zones.Add(new Zone(minX, maxX, minY, maxY));
                }
            }
            //For each zone mark all of the zones adjacent to it
            for(var zoneIndex = 0; zoneIndex < Zones.Count; ++zoneIndex)
            {
                int beginning = zoneIndex - xZones - 1;
                int end = beginning + 2;
                //beginning and ending tweaked so edge cases don't get wonky adjacent zones
                if (zoneIndex % xZones == 0)
                    beginning += 1;
                if (zoneIndex % xZones == xZones - 1)
                    end -= 1;
                for (var i = 0; i < 3; ++i)
                {
                    for(var adjacentZoneIndex = beginning; adjacentZoneIndex <= end; ++adjacentZoneIndex)
                    {
                        if(adjacentZoneIndex >= 0 && adjacentZoneIndex < Zones.Count)
                        {
                            if (zoneIndex != adjacentZoneIndex)
                                Zones[zoneIndex].AddAdjacentZone(Zones[adjacentZoneIndex]);
                        }
                    }
                    beginning += xZones;
                    end += xZones;
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
            int collideDist = collider1.CollisionRadius + collider2.CollisionRadius;
            int collideDistSquared = collideDist * collideDist;
            int actualDistSquared = (collider1.X - collider2.X) * (collider1.X - collider2.X)
                            +(collider1.Y - collider2.Y) * (collider1.Y - collider2.Y);

            if (actualDistSquared <= collideDistSquared)
            {
                collider1.SleepTime = -1;
                collider2.SleepTime = -1;
                return collider1.PreciseCollides(collider2);
            }
            int actualDist = Convert.ToInt32(Math.Ceiling(Math.Sqrt(actualDistSquared)));
            SetSleepTimer(collider1, actualDist - collideDist);
            SetSleepTimer(collider2, actualDist - collideDist);

            return false;
        }

        private void SetSleepTimer(ICollidable collider, int distance)
        {
            double time = distance / maxSpeed;
            time = Math.Floor(time);
            if (time == 0)
                collider.SleepTime = -1;
            if (time < collider.SleepTime || collider.SleepTime == 0)
                collider.SleepTime = Convert.ToInt32(time);
        }

        private void TickSleepTimers()
        {
            foreach(var collider in CollisionObjects)
            {
                if (collider.SleepTime > 0)
                    --collider.SleepTime;
                if (collider.SleepTime == -1)
                    collider.SleepTime = 0;
            }
        }

        //The dictionary has some duplicate data to avoid making redundant comparisons.  Flatten everything in to a hash set here.
        private HashSet<Tuple<ICollidable, ICollidable>> ConstructSetFromDictionary(ConcurrentDictionary<ICollidable, ConcurrentColliderHash> dictionary)
        {
            HashSet<Tuple<ICollidable, ICollidable>> colliderHash = new HashSet<Tuple<ICollidable, ICollidable>>();
            foreach (var colliderKey in dictionary.Keys)
            {
                ConcurrentColliderHash colliderSet = dictionary[colliderKey];
                foreach(var colliderValue in colliderSet.Items())
                {
                    colliderHash.Add(new Tuple<ICollidable, ICollidable>(colliderKey, colliderValue));
                    if(dictionary.ContainsKey(colliderValue))
                        dictionary[colliderValue].Remove(colliderKey);                 
                }
            }
            return colliderHash;
        }

        // Maintain a dictionary of all the colliders that have been checked against eachother
        // no matter which collider is used as the key, the other will be in the value 
        private void SafeAddToDictionary(ConcurrentDictionary<ICollidable, ConcurrentColliderHash> dictionary, ICollidable collider1, ICollidable collider2)
        {
            if (dictionary.ContainsKey(collider1))
                dictionary[collider1].Add(collider2);
            else
            {
                dictionary[collider1] = new ConcurrentColliderHash();
                dictionary[collider1].Add(collider2);
            }
            if (dictionary.ContainsKey(collider2))
                dictionary[collider2].Add(collider1);
            else
            {
                dictionary[collider2] = new ConcurrentColliderHash();
                dictionary[collider2].Add(collider1);
            }
        }
    }
}
