using System.Collections.Generic;
using System.Linq;

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
        // Adjacent Zones, relevant because they need to be considered for collisions
        HashSet<Zone> AdjacentZones;
        public Zone(int minX, int maxX, int minY, int maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            ZoneObjects = new HashSet<ICollidable>();
            AdjacentZones = new HashSet<Zone>();
        }

        public void AddCollider(ICollidable collider)
        {
            ZoneObjects.Add(collider);
        }

        public void ClearColliders()
        {
            ZoneObjects.Clear();
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

        /// <summary>
        /// Find all the objects in this zone and adjacent zones
        /// </summary>
        /// <returns> All objects that need to be considered for collisions </returns>
        public List<ICollidable> ComputeCollisionObjects()
        {
            HashSet<ICollidable> CollisionObjects = new HashSet<ICollidable>();
            CollisionObjects.UnionWith(ZoneObjects);
            foreach (var zone in AdjacentZones)
                CollisionObjects.UnionWith(zone.ZoneObjects);
            return CollisionObjects.ToList();
        }
    }
}
