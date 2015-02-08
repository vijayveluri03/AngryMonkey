using System;
using Android.Hardware;

namespace GoneBananasShared
{
	public delegate int AccelerometerValueChanaged(SensorEvent e);
	public class Accelerometer
	{
		AccelerometerValueChanaged mValueChangedListener = null;
		private static Accelerometer mInstance;

		public static Accelerometer pInstance {
			get {
				if (mInstance == null)
					mInstance = new Accelerometer ();
				return mInstance;
			}
		}

		public static void AddListener(AccelerometerValueChanaged listener)
		{
			mInstance.mValueChangedListener += listener;
		}

		public static void RemoveListener(AccelerometerValueChanaged listener)
		{
			mInstance.mValueChangedListener -= listener;
		}

		public void OnAccelChange(SensorEvent e)
		{
			Console.WriteLine ("accel changed");
			if (mValueChangedListener != null)
				mValueChangedListener (e);
		}
	}
}

