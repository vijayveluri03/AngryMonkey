package gonebananasandroid;


public class Accelerometer_Droid
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.hardware.SensorEventListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onAccuracyChanged:(Landroid/hardware/Sensor;I)V:GetOnAccuracyChanged_Landroid_hardware_Sensor_IHandler:Android.Hardware.ISensorEventListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onSensorChanged:(Landroid/hardware/SensorEvent;)V:GetOnSensorChanged_Landroid_hardware_SensorEvent_Handler:Android.Hardware.ISensorEventListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("GoneBananasAndroid.Accelerometer_Droid, GoneBananasAndroid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Accelerometer_Droid.class, __md_methods);
	}


	public Accelerometer_Droid () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Accelerometer_Droid.class)
			mono.android.TypeManager.Activate ("GoneBananasAndroid.Accelerometer_Droid, GoneBananasAndroid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public Accelerometer_Droid (android.hardware.SensorManager p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == Accelerometer_Droid.class)
			mono.android.TypeManager.Activate ("GoneBananasAndroid.Accelerometer_Droid, GoneBananasAndroid, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Hardware.SensorManager, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
	}


	public void onAccuracyChanged (android.hardware.Sensor p0, int p1)
	{
		n_onAccuracyChanged (p0, p1);
	}

	private native void n_onAccuracyChanged (android.hardware.Sensor p0, int p1);


	public void onSensorChanged (android.hardware.SensorEvent p0)
	{
		n_onSensorChanged (p0);
	}

	private native void n_onSensorChanged (android.hardware.SensorEvent p0);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
