using BloogBot;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBotTests
{
    [TestClass]
    public class NavigationTests
    {
        const int KALIMDOR_MAP_ID = 1;

        // this test actually uses the Navigation library to build a path between two points. this kind of test is helpful for troubleshooting
        // specific navigation issues based on the movemaps generated from the WoW client's static geometry.
        // for example, if the bot is struggling to move up a hill, you can debug that here.
        // see CalculatePath_1.png and CalculatePath_2.png
        [TestMethod]
        public void CalculatePathTest()
        {
            var playerPosition = new Position(-2942f, -314, 56f);
            var targetPosition = new Position(-2996f, -535f, 37f);

            var path = Navigation.CalculatePath(KALIMDOR_MAP_ID, playerPosition, targetPosition, false);

            // if the Navigation library fails to build a path, it will just return the start and end points in an array of length 2.
            // here we assert that the result path length is greater than 2 which indicates pathfinding was successful.
            Assert.IsTrue(path.Length > 2);
        }

        // this test mocks the result of Navigation.CalculatePath by manually building an array of Positions. this can be used, for example, to
        // experiment with targeting logic. you can place foes around the 2d coordinate plane close to your mocked navigation path, and assert
        // which mobs should be excluded as potential targets, and which one should be selected, based on the targeting logic you specify.
        // in this case, we have Target1 that is technically closer, which should make it a better target, but it's nearby a level 60 elite mob.
        // therefore, we should exclude Target1, and instead choose Target2 as a target, even though it's farther away.
        // see TargetingLogicTest_1
        [TestMethod]
        public void TargetingLogicTest()
        {
            var pathToTarget1 = new[]
            {
                new Position(20f, 20f, 0),
                new Position(30f, 20f, 0),
                new Position(40f, 20f, 0),
                new Position(50f, 20f, 0),
                new Position(60f, 20f, 0),
                new Position(70f, 20f, 0)
            };

            var pathToTarget2 = new[]
            {
                new Position(20f, 20f, 0),
                new Position(20f, 30f, 0),
                new Position(20f, 40f, 0),
                new Position(20f, 50f, 0),
                new Position(20f, 60f, 0),
                new Position(20f, 70f, 0),
                new Position(20f, 80f, 0),
                new Position(20f, 90f, 0)
            };

            var target1 = new Mock<WoWUnit>();
            target1.SetupGet(t1 => t1.Guid).Returns(0);
            target1.SetupGet(t1 => t1.Position).Returns(new Position(70f, 20f, 0));
            target1.SetupGet(t1 => t1.Level).Returns(20);
            target1.SetupGet(t1 => t1.CreatureRank).Returns(CreatureRank.Normal);

            var eliteMob = new Mock<WoWUnit>();
            eliteMob.SetupGet(em => em.Guid).Returns(1);
            eliteMob.SetupGet(em => em.Position).Returns(new Position(60f, 20f, 0));
            eliteMob.SetupGet(em => em.Level).Returns(60);
            eliteMob.SetupGet(em => em.CreatureRank).Returns(CreatureRank.Elite);

            var target2 = new Mock<WoWUnit>();
            target2.SetupGet(t2 => t2.Guid).Returns(2);
            target2.SetupGet(t2 => t2.Position).Returns(new Position(90f, 20f, 0));
            target2.SetupGet(t2 => t2.Level).Returns(20);
            target2.SetupGet(t2 => t2.CreatureRank).Returns(CreatureRank.Normal);

            // used as an analog for ObjectManager.Units
            var units = new[] { target1.Object, eliteMob.Object, target2.Object };

            bool additionalTargetingCriteria(WoWUnit u, Position[] path) =>
                !units.Any(o =>
                    o.Guid != u.Guid &&
                    o.CreatureRank == CreatureRank.Elite &&
                    path.Any(p => p.DistanceTo(o.Position) < 30)
                );

            var dict = new Dictionary<WoWUnit, Position[]>
            {
                { target1.Object, pathToTarget1 },
                { target2.Object, pathToTarget2 },
            };

            var targetResult = dict
                .Where(d => additionalTargetingCriteria(d.Key, d.Value))
                .FirstOrDefault();

            // assert that the selected target is Target2. even though Target2 is farther from the player, Target1 is too close to an elite mob
            // and is therefore too dangerous to fight. we choose Target2 instead.
            Assert.AreEqual(target2.Object, targetResult.Key);
        }
    }
}
