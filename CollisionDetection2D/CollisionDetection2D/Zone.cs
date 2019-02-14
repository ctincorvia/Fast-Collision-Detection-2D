using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CollisionDetection2D
{
    public class Zone
    {
        int minX;
        int maxX;
        int minY;
        int maxY;
        // all objects within this zone
        public HashSet<ICollidable> ZoneObjects;
        // all objects that are in the zone but extend over the edge of the zone
        HashSet<ICollidable> FringeObjects;
        // Adjacent Zones, relevant because they need to be considered for collisions
        HashSet<Zone> AdjacentZones;
        public Zone(int minX, int maxX, int minY, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            ZoneObjects = new HashSet<ICollidable>();
            FringeObjects = new HashSet<ICollidable>();
            AdjacentZones = new HashSet<Zone>();
        }

        public void AddCollider(ICollidable collider)
        {
            ZoneObjects.Add(collider);
            if (ExtendsOutside(collider))
                FringeObjects.Add(collider);
        }

        public void ClearColliders()
        {
            ZoneObjects.Clear();
            FringeObjects.Clear();
        }

        public void AddAdjacentZone(Zone zone)
        {
            AdjacentZones.Add(zone);
        }

        public bool WithinBounds(ICollidable collider)
        {
            if (collider.X > maxX)
                return false;
            if (collider.X < minX)
                return false;
            if (collider.Y > maxY)
                return false;
            if (collider.Y < minY)
                return false;
            return true;
        }

        public bool WithinBounds(int x, int y)
        {
            if (x > maxX)
                return false;
            if (x < minX)
                return false;
            if (y > maxY)
                return false;
            if (y < minY)
                return false;
            return true;
        }

        public bool ExtendsOutside(ICollidable collider)
        {
            var superX = collider.X + collider.CollisionRadius;
            var superY = collider.Y + collider.CollisionRadius;
            var subX = collider.X - collider.CollisionRadius;
            var subY = collider.Y - collider.CollisionRadius;

            if (!WithinBounds(superX, collider.Y))
                return true;
            if (!WithinBounds(subX, collider.Y))
                return true;
            if (!WithinBounds(collider.X, superY))
                return true;
            if (!WithinBounds(collider.X, superY))
                return true;
            return false;
        }

        /// <summary>
        /// Find all the objects in this zone and adjacent zones
        /// </summary>
        /// <returns> All objects that need to be considered for collisions </returns>
        public List<ICollidable> ComputeCollisionObjects()
        {
            var sw = Stopwatch.StartNew();
            HashSet<ICollidable> CollisionObjects = new HashSet<ICollidable>();
            CollisionObjects.UnionWith(ZoneObjects);
            foreach (var zone in AdjacentZones)
                CollisionObjects.UnionWith(zone.FringeObjects);
            sw.Stop();
            return CollisionObjects.ToList();
        }
    }
}
