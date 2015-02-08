using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CocosSharp;
using Microsoft.Xna.Framework;
using GoneBananas;
using Android.Hardware;

namespace GoneBananasAndroid
{
    [Activity(
        Label = "GoneBananas",
        AlwaysRetainTaskState = true,
        Icon = "@drawable/ic_launcher",
        Theme = "@android:style/Theme.NoTitleBar",
		ScreenOrientation = ScreenOrientation.Landscape,
        LaunchMode = LaunchMode.SingleInstance,
        MainLauncher = true,
        ConfigurationChanges =  ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)
    ]
    public class MainActivity : AndroidGameActivity
    {
		SensorManager mSensorManager = null;
		Accelerometer_Droid mAccelerometer = null;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var application = new CCApplication();
            application.ApplicationDelegate = new GoneBananasApplicationDelegate();
            SetContentView(application.AndroidContentView);
            application.StartGame();

			if (mSensorManager == null)
				mSensorManager = (SensorManager)GetSystemService (Activity.SensorService);

			if (mAccelerometer == null)
				mAccelerometer = new Accelerometer_Droid (mSensorManager);
        }

		protected override void OnPause ()
		{
			base.OnPause ();
			mAccelerometer.OnGamePause ();
		}
		protected override void OnResume ()
		{
			base.OnResume ();
			mAccelerometer.OnGameResume ();
		}
    }
}