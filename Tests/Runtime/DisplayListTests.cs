using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class DisplayListTests
    {
        private TestDisplayList _testList;

        [SetUp]
        public void Setup()
        {
            _testList = new GameObject("Test List", typeof(TestDisplayList)).GetComponent<TestDisplayList>();
        }

        [TearDown]
        public void Teardown()
        {
            GameObject.Destroy(_testList.gameObject);
            _testList = null;
        }

        [Test]
        public void SimplePasses()
        {
            Assert.IsNotNull(_testList);
            Assert.AreEqual(GameObject.FindObjectsOfType<TestDisplayList>().Length, 1);
        }

        [Test]
        public void SimplePassesAgain()
        {
            Assert.IsNotNull(_testList);
            Assert.AreEqual(GameObject.FindObjectsOfType<TestDisplayList>().Length, 1);
        }
    }
}
