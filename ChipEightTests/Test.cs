using NUnit.Framework;
using System.Diagnostics;

namespace ChipEightTestSuite
{
    [SetUpFixture]
    public class RootFixtureSetup
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestContext.Progress.WriteLine("RootFixtureSetup:OneTimeSetUp");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestContext.Progress.WriteLine("RootFixtureSetup:OneTimeTearDown");
        }
    }

    namespace TestLifeCycle
    {
        [SetUpFixture]
        public class FixtureSetup
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                TestContext.Progress.WriteLine("FixtureSetup:OneTimeSetUp");
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                TestContext.Progress.WriteLine("FixtureSetup:OneTimeTearDown");
            }
        }

        [TestFixture]
        public class Tests
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                TestContext.Progress.WriteLine("Tests:OneTimeSetUp");
            }

            [SetUp]
            public void Setup()
            {
                TestContext.Progress.WriteLine("Tests:SetUp");
            }


            public Tests()
            {
                TestContext.Progress.WriteLine("Tests:Constructor");
            }

            [Test]
            public void Test1()
            {
                TestContext.Progress.WriteLine("Tests:Test1");
            }

            [Test]
            public void Test2()
            {
                TestContext.Progress.WriteLine("Tests:Test2");
            }

            [TearDown]
            public void TearDown()
            {
                TestContext.Progress.WriteLine("Tests:TearDown");
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                TestContext.Progress.WriteLine("Tests:OneTimeTearDown");
            }
        }
    }
}
