using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CollisionDetection2D;
using System.Linq;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        public void SimpleCollide()
        {
            Map testMap = new Map(100, 100, 10, 10);
            DemoRectangle demRect1 = new DemoRectangle(5, 5, 0, 0, 10);
            DemoRectangle demRect2 = new DemoRectangle(6, 6, 0, 0, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 1);
        }

        [TestMethod]
        public void SimpleNonCollision()
        {
            Map testMap = new Map(100, 100, 10, 10);
            DemoRectangle demRect1 = new DemoRectangle(82, 82, 0, 0, 2);
            DemoRectangle demRect2 = new DemoRectangle(87, 87, 0, 0, 2);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 0);
        }

        [TestMethod]
        public void LargeMapCollision()
        {
            Map testMap = new Map(100, 100, 10, 10);
            DemoRectangle demRect1 = new DemoRectangle(172, 750, 0, 0, 10);
            DemoRectangle demRect2 = new DemoRectangle(186, 760, 0, 0, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 0);
        }

        [TestMethod]
        public void CollisionAndNonCollision()
        {
            Map testMap = new Map(100, 100, 10, 10);

            DemoRectangle demRect1 = new DemoRectangle(30, 30, 0, 0, 10);
            DemoRectangle demRect2 = new DemoRectangle(80, 80, 0, 0, 10);
            DemoRectangle demRect3 = new DemoRectangle(10, 10, 0, 0, 10);
            DemoRectangle demRect4 = new DemoRectangle(11, 11, 0, 0, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);
            testMap.AddCollider(demRect3);
            testMap.AddCollider(demRect4);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 1);
        }

        [TestMethod]
        public void MultipleCollisions()
        {
            Map testMap = new Map(100, 100, 10, 10);

            DemoRectangle demRect1 = new DemoRectangle(30, 30, 0, 0, 10);
            DemoRectangle demRect2 = new DemoRectangle(31, 31, 0, 0, 10);
            DemoRectangle demRect3 = new DemoRectangle(10, 10, 0, 0, 10);
            DemoRectangle demRect4 = new DemoRectangle(11, 11, 0, 0, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);
            testMap.AddCollider(demRect3);
            testMap.AddCollider(demRect4);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 2);
        }

        [TestMethod]
        public void OverlappingCollisions()
        {
            Map testMap = new Map(100, 100, 10, 10);

            DemoRectangle demRect1 = new DemoRectangle(30, 30, 0, 0, 10);
            DemoRectangle demRect2 = new DemoRectangle(31, 31, 0, 0, 10);
            DemoRectangle demRect3 = new DemoRectangle(32, 32, 0, 0, 10);
            DemoRectangle demRect4 = new DemoRectangle(33, 33, 0, 0, 10);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);
            testMap.AddCollider(demRect3);
            testMap.AddCollider(demRect4);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 6);
        }

        [TestMethod]
        public void CollisionAcrossMultipleZones()
        {
            Map testMap = new Map(100, 100, 10, 10);

            DemoRectangle demRect1 = new DemoRectangle(5, 5, 0, 0, 100);
            DemoRectangle demRect2 = new DemoRectangle(15, 15, 0, 0, 100);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);

            var detectedCollisions = testMap.DetectCollisions();
            Assert.IsTrue(detectedCollisions.Count() == 1);
        }

        [TestMethod]
        public void MovingObjectsCollide()
        {
            Map testMap = new Map(100, 100, 1, 1);
            SimpleCollisionExample simpleCollides = new SimpleCollisionExample();

            DemoRectangle demRect1 = new DemoRectangle(25, 25, 1, 0, 5);
            DemoRectangle demRect2 = new DemoRectangle(35, 25, 0, 0, 5);

            testMap.AddCollider(demRect1);
            testMap.AddCollider(demRect2);
            simpleCollides.CollisionObjects.Add(demRect1);
            simpleCollides.CollisionObjects.Add(demRect2);

            for(int i = 0; i < 10; i++)
            {
                var detectedCollisions = testMap.DetectCollisions();
                var simpleDetectedCollisions = testMap.DetectCollisions();
                Assert.IsTrue(detectedCollisions.Count() == simpleDetectedCollisions.Count());
                demRect1.Tick();
                demRect2.Tick();
            }
        }



        [TestMethod]
        public void ManyRandomCollisions()
        {
            List<DemoRectangle> rectList = new List<DemoRectangle>();
            Map testMap = new Map(1000, 1000, 8, 8);
            SimpleCollisionExample simpleCollides = new SimpleCollisionExample();
            for(var i = 0; i < 500; ++i)
            {
                var randRect = GenerateRandomRectangle(1000, 1000);
                testMap.AddCollider(randRect);
                simpleCollides.CollisionObjects.Add(randRect);
                rectList.Add(randRect);                
            }
            
            for(int i = 0; i < 5; i++)
            {
                var mapSW1 = new Stopwatch();
                var simpleSW1 = new Stopwatch();

                var firstPassCollisions = testMap.DetectCollisions().Count();

                var simpleFirstPassCollisions = simpleCollides.DetectCollisions().Count();

                Assert.IsTrue(firstPassCollisions == simpleFirstPassCollisions);
                foreach (var rect in rectList)
                {
                    rect.Tick();
                }
            }
            var mapSW = new Stopwatch();
            var simpleSW = new Stopwatch();
            mapSW.Start();
            var mapCollisions = testMap.DetectCollisions();
            mapSW.Stop();

            simpleSW.Start();
            var simpleCollisions = simpleCollides.DetectCollisions();
            simpleSW.Stop();
            //Assert.IsTrue(simpleSW.ElapsedTicks > mapSW.ElapsedTicks);
            Assert.IsTrue(simpleCollisions.Count() == mapCollisions.Count());
        }

        private static readonly Random rand = new Random();
        private DemoRectangle GenerateRandomRectangle(int maxX, int maxY)
        {
            int x = rand.Next(1, maxX);
            int y = rand.Next(1, maxY);
            int w = rand.Next(5, 11);
            int xs = rand.Next(-1, 2);
            int ys = rand.Next(-1, 2);

            return new DemoRectangle(x, y, xs, ys, w);
        }
    }
}
