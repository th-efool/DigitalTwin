namespace realvirtual
{
    using NaughtyAttributes;
    using UnityEngine;

    public class TestSensorSink: MonoBehaviour,ITestCheck
    {
        [InfoBox("This is just for internal realvirtual.io Development and Test automation", EInfoBoxType.Warning)]
        public int DueSinkCount;

        public int DueSinkRange;
        public string Check()
        {
            var counter = GetComponent<Sink>().SumDestroyed;
            // add an allowed range
            if (counter >= DueSinkCount - DueSinkRange && counter <= DueSinkCount + DueSinkRange)
            {
                return "";
            }
            else
            {
                return "Sink count at " + this.name + " is " + counter + " but should be " + DueSinkCount + " with a range of " + DueSinkRange;
            }
          
        }
    }

}