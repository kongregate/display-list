using UnityEngine;

namespace Tests
{
    public class TestDisplayElement : MonoBehaviour, IDisplayElement<TestData>
    {
        private TestData _data;

        public TestData Data
        {
            get { return _data; }
        }

        public void Populate(TestData data)
        {
            _data = Data;
        }
    }
}
