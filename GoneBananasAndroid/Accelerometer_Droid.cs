using System;
using Android.Hardware;
using System.Text;
using GoneBananasShared;

namespace GoneBananasAndroid
{
	public class Accelerometer_Droid: Java.Lang.Object, ISensorEventListener
	{
		SensorManager mSensorManager;
		public Accelerometer_Droid (SensorManager sensorManager)
		{
			mSensorManager = sensorManager;
		}

		public void OnAccuracyChanged (Sensor sensor, SensorStatus accuracy)
		{
		}

		public void OnSensorChanged (SensorEvent e)
		{
//			var text = new StringBuilder("x = ")
//				.Append(e.Values[0])
//				.Append(", y=")
//				.Append(e.Values[1])
//				.Append(", z=")
//				.Append(e.Values[2]);

			//Console.WriteLine (text);
			Accelerometer.pInstance.OnAccelChange (e);
		}

		public void OnGamePause()
		{
			mSensorManager.UnregisterListener (this);
		}

		public void OnGameResume()
		{
			mSensorManager.RegisterListener(this, mSensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
		}
	}
}

