namespace realvirtual
{
    using NaughtyAttributes;
    using UnityEngine;

    public class TestSensorCount : MonoBehaviour,ITestCheck
    {
        [InfoBox("This is just for internal realvirtual.io Development and Test automation", EInfoBoxType.Warning)]
        public int DueSensorCount;
		public Sensor Sensor;
	
        public int Tolerance = 0;
        public string Check()
        {
			if (Sensor == null)
            {
                Sensor = GetComponent<Sensor>();
            }
            var counter = Sensor.Counter;
            int difference = Mathf.Abs(counter - DueSensorCount);
            if (difference <= Tolerance)
            {
                return "";
            }
            else
            {
                if (Tolerance == 0)
                    return "Sensor count at " + this.name + " is " + counter + " but should be " + DueSensorCount;
                else
                     return "Sensor count at " + this.name + " is " + counter + " but should be within " + Tolerance + " of " + DueSensorCount;
            }
        }
    }
}