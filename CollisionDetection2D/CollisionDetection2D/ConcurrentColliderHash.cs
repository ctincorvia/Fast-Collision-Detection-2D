using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionDetection2D
{
    public class ConcurrentColliderHash
    {
        ConcurrentDictionary<ICollidable, byte> colliders;
        
        public ConcurrentColliderHash()
        {
            colliders = new ConcurrentDictionary<ICollidable, byte>();
        }

        public void Add(ICollidable collider)
        {
            colliders[collider] = 0;
        }

        public byte Remove(ICollidable collider)
        {
            byte outByte;
            colliders.TryRemove(collider, out outByte);
            return outByte;
        }

        public bool Contains(ICollidable collider)
        {
            return colliders.Keys.Contains(collider);
        }

        public ICollection<ICollidable> Items()
        {
            return colliders.Keys;
        }
    }
}
