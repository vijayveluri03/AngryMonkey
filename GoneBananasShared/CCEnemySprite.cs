using System;
using Box2D.Common;
using Box2D.Dynamics;
using CocosSharp;

namespace GoneBananas
{
	internal class CCEnemySprite : CCSprite
	{
		private GameLayer.ShootBulletDelg mShootBulletCallback = null;
		private bool mDidShootBullet = false;
		private float mTimeDel = 0;
		private float mShootAtTime = 0;

		public CCEnemySprite (CCTexture2D f, CCRect r, float totalAliveTime, GameLayer.ShootBulletDelg shootbullet ) : base (f, r)
		{
			this.mShootBulletCallback = shootbullet;
			this.mDidShootBullet = false;
			this.mShootAtTime = CCRandom.Float_0_1 () * totalAliveTime;

			Schedule (t => { if ( !mDidShootBullet )
				{
					mTimeDel += 0.5f;
					if ( mTimeDel > mShootAtTime )
					{
						mDidShootBullet = true;
						if ( mShootBulletCallback != null )
							mShootBulletCallback ( this );
					}
				}
			}, 0.5f);

		}
	}
}