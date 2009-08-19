﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;

using NUnit.Framework;

namespace OpenSim.Region.Framework.Tests
{
    [TestFixture]
    public class BorderTests
    {

        [Test]
        public void TestCross()
        {
            List<Border> testborders = new List<Border>();

            Border NorthBorder = new Border();
            NorthBorder.BorderLine = new Vector3(0, 256, 256);  //<---
            NorthBorder.CrossDirection = Cardinals.N;
            testborders.Add(NorthBorder);

            Border SouthBorder = new Border();
            SouthBorder.BorderLine = new Vector3(0, 256, 0);    //--->
            SouthBorder.CrossDirection = Cardinals.S;
            testborders.Add(SouthBorder);

            Border EastBorder = new Border();
            EastBorder.BorderLine = new Vector3(0, 256, 256);   //<---
            EastBorder.CrossDirection = Cardinals.E;
            testborders.Add(EastBorder);

            Border WestBorder = new Border();
            WestBorder.BorderLine = new Vector3(0, 256, 0);     //--->
            WestBorder.CrossDirection = Cardinals.W;
            testborders.Add(WestBorder);

            Vector3 position = new Vector3(200,200,21);
            
            foreach (Border b in testborders)
            {
                Assert.That(!b.TestCross(position));

            }

            position = new Vector3(200,280,21);
            Assert.That(NorthBorder.TestCross(position));



            // Test automatic border crossing
            // by setting the border crossing aabb to be the whole region
            position = new Vector3(25,25,21); // safely within one 256m region

            // The Z value of the BorderLine is reversed, making all positions within the region 
            // trigger bordercross

            SouthBorder.BorderLine = new Vector3(0,256,256); // automatic border cross in the region
            Assert.That(SouthBorder.TestCross(position));

            NorthBorder.BorderLine = new Vector3(0, 256, 0); // automatic border cross in the region
            Assert.That(NorthBorder.TestCross(position));
            
            EastBorder.BorderLine = new Vector3(0, 256, 0); // automatic border cross in the region
            Assert.That(EastBorder.TestCross(position));

            WestBorder.BorderLine = new Vector3(0, 256, 255); // automatic border cross in the region
            Assert.That(WestBorder.TestCross(position));

        }

        [Test]
        public void TestCrossSquare512()
        {
            List<Border> testborders = new List<Border>();

            Border NorthBorder = new Border();
            NorthBorder.BorderLine = new Vector3(0, 512, 512);
            NorthBorder.CrossDirection = Cardinals.N;
            testborders.Add(NorthBorder);

            Border SouthBorder = new Border();
            SouthBorder.BorderLine = new Vector3(0, 512, 0);
            SouthBorder.CrossDirection = Cardinals.S;
            testborders.Add(SouthBorder);

            Border EastBorder = new Border();
            EastBorder.BorderLine = new Vector3(0, 512, 512);
            EastBorder.CrossDirection = Cardinals.E;
            testborders.Add(EastBorder);

            Border WestBorder = new Border();
            WestBorder.BorderLine = new Vector3(0, 512, 0);
            WestBorder.CrossDirection = Cardinals.W;
            testborders.Add(WestBorder);

            Vector3 position = new Vector3(450,220,21);

            foreach (Border b in testborders)
            {
                Assert.That(!b.TestCross(position));

            }

            //Trigger east border
            position = new Vector3(513,220,21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.E)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

            //Trigger west border
            position = new Vector3(-1, 220, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.W)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

            //Trigger north border
            position = new Vector3(220, 513, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.N)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

            //Trigger south border
            position = new Vector3(220, -1, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.S)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

        }

        [Test]
        public void TestCrossRectangle512x256()
        {
            List<Border> testborders = new List<Border>();

            Border NorthBorder = new Border();
            NorthBorder.BorderLine = new Vector3(0, 512, 256);
            NorthBorder.CrossDirection = Cardinals.N;
            testborders.Add(NorthBorder);

            Border SouthBorder = new Border();
            SouthBorder.BorderLine = new Vector3(0, 512, 0);
            SouthBorder.CrossDirection = Cardinals.S;
            testborders.Add(SouthBorder);

            Border EastBorder = new Border();
            EastBorder.BorderLine = new Vector3(0, 256, 512);
            EastBorder.CrossDirection = Cardinals.E;
            testborders.Add(EastBorder);

            Border WestBorder = new Border();
            WestBorder.BorderLine = new Vector3(0, 256, 0);
            WestBorder.CrossDirection = Cardinals.W;
            testborders.Add(WestBorder);

            Vector3 position = new Vector3(450, 220, 21);

            foreach (Border b in testborders)
            {
                Assert.That(!b.TestCross(position));

            }

            //Trigger east border
            position = new Vector3(513, 220, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.E)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

            //Trigger west border
            position = new Vector3(-1, 220, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.W)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

            //Trigger north border
            position = new Vector3(220, 257, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.N)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));

            }

            //Trigger south border
            position = new Vector3(220, -1, 21);
            foreach (Border b in testborders)
            {
                if (b.CrossDirection == Cardinals.S)
                    Assert.That(b.TestCross(position));
                else
                    Assert.That(!b.TestCross(position));
                
            }
        }

        [Test]
        public void TestCrossOdd512x512w256hole()
        {
            List<Border> testborders = new List<Border>();
            //   512____
            //      |  |
            // 256__|  |___
            //      |      |
            //      |______|
            //     0   |    512
            //        256

            // Compound North border since the hole is at the top
            Border NorthBorder1 = new Border();
            NorthBorder1.BorderLine = new Vector3(0, 256, 512);
            NorthBorder1.CrossDirection = Cardinals.N;
            testborders.Add(NorthBorder1);

            Border NorthBorder2 = new Border();
            NorthBorder2.BorderLine = new Vector3(256, 512, 256);
            NorthBorder2.CrossDirection = Cardinals.N;
            testborders.Add(NorthBorder2);

            Border SouthBorder = new Border();
            SouthBorder.BorderLine = new Vector3(0, 512, 0);
            SouthBorder.CrossDirection = Cardinals.S;
            testborders.Add(SouthBorder);

            //Compound East border
            Border EastBorder1 = new Border();
            EastBorder1.BorderLine = new Vector3(0, 256, 512);
            EastBorder1.CrossDirection = Cardinals.E;
            testborders.Add(EastBorder1);

            Border EastBorder2 = new Border();
            EastBorder2.BorderLine = new Vector3(257, 512, 256);
            EastBorder2.CrossDirection = Cardinals.E;
            testborders.Add(EastBorder2);



            Border WestBorder = new Border();
            WestBorder.BorderLine = new Vector3(0, 512, 0);
            WestBorder.CrossDirection = Cardinals.W;
            testborders.Add(WestBorder);

            Vector3 position = new Vector3(450, 220, 21);

            foreach (Border b in testborders)
            {
                Assert.That(!b.TestCross(position));

            }

            position = new Vector3(220, 450, 21);

            foreach (Border b in testborders)
            {
                Assert.That(!b.TestCross(position));

            }

            bool result = false;
            int bordersTriggered = 0;

            position = new Vector3(450, 450, 21);

            foreach (Border b in testborders)
            {
                if (b.TestCross(position))
                {
                    bordersTriggered++;
                    result = true;
                }
            }

            Assert.That(result);
            Assert.That(bordersTriggered == 2);

        }
    }
}