using System;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;

namespace GoneBananas
{
	public class ScoreData
	{
		public string id { get; set; }
		public string time { get; set; }
		public int score { get; set; }
	}

	public class MobileService
	{
		public static List<int> highScores = new List<int>();
		static MobileServiceClient mServiceClient = null;
		static IMobileServiceTable<ScoreData> scoreTable = null;

		public MobileService ()
		{
			CurrentPlatform.Init ();
			mServiceClient = new MobileServiceClient("https://tappo3.azure-mobile.net/","GqawVqVElLYqkkmCLUFOlTXYqoNIiq84" );
			scoreTable = mServiceClient.GetTable<ScoreData>();
		}

		public static async void UpdateScoreToServer( int gameScore)
		{
			var scr = new ScoreData { score = gameScore, time = DateTime.Now.ToString ()  };
			await scoreTable.InsertAsync (scr);  
			scoreTable = mServiceClient.GetTable<ScoreData>();
			MobileServiceCollection<ScoreData, ScoreData> items = await scoreTable.ToCollectionAsync();

			highScores.Clear();
			foreach(ScoreData sd in items)
			{
				highScores.Add(sd.score);
			}
		}

	}

}

