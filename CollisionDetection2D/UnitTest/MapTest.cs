using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CollisionDetection2D;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        public void SimpleCollide()
        {
            Map testMap = new Map(100, 100, 10000, 10, 10);
            DemoRectangle demRect1 = new DemoRectangle(10, 10, 10);
            DemoRectangle demRect2 = new DemoRectangle(11, 11, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 1);
        }

        [TestMethod]
        public void SimpleNonCollision()
        {
            Map testMap = new Map(100, 100, 10000, 10, 10);
            DemoRectangle demRect1 = new DemoRectangle(20, 20, 10);
            DemoRectangle demRect2 = new DemoRectangle(80, 80, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 0);
        }
    }
}
