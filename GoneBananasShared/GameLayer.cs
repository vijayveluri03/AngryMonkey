using System;
using System.Collections.Generic;
using CocosDenshion;
using CocosSharp;
using System.Linq;

using Box2D.Common;
using Box2D.Dynamics;
using Box2D.Collision.Shapes;

namespace GoneBananas
{
    public class GameLayer : CCLayerColor
    {
		public delegate void ShootBulletDelg ( CCSprite fromSprite );

        const float MONKEY_SPEED = 200.0f;
        const float GAME_DURATION = 600.0f; // game ends after 60 seconds or when the monkey hits a ball, whichever comes first
        const int MAX_NUM_BALLS = 10;
		const int MAX_ENEMY_COUNT = 4;
		const int MAX_BANANAS_COUNT = 3;
		const int MAX_LIVES = 3;

		int mPreviousMonkeyTime = 0;
		int mScore = 0;
		const int _EnemyDestroyScore = 10;
		const float _BananaSpeed = 0.5f;
		const float _EnemySpeed = 0.3f;

		float AccelerometerValue = 0;


		int mCurrentLives;

        // point to meter ratio for physics
        const int PTM_RATIO = 32;

        float elapsedTime;
        CCSprite monkey;
        List<CCSprite> visibleBananas;
        List<CCSprite> hitBananas;

		CCLabelTtf mScoreLabel = null;

        // monkey walking animation
        CCAnimation walkAnim;
        CCRepeatForever walkRepeat;
        CCCallFuncN walkAnimStop = new CCCallFuncN (node => node.StopAllActions ());

        // background sprite
        CCSprite grass;

        // particles
        CCParticleSun sun;

        // circle layered behind sun
        CCDrawNode circleNode;

        // parallax node for clouds
        CCParallaxNode parallaxClouds;
            
        // define our banana rotation action
        CCRotateBy rotateBanana = new CCRotateBy (0.8f, 360);

        // define our completion action to remove the banana once it hits the bottom of the screen
        CCCallFuncN moveBananaComplete = new CCCallFuncN (node => node.RemoveFromParent ());

        // physics world
        b2World world;
        
        // balls sprite batch
        CCSpriteBatchNode ballsBatch;
		CCSpriteBatchNode enemyBatch;
		CCSpriteBatchNode bananaBatch;

        CCTexture2D ballTexture;
		CCTexture2D enemyTexture;
		CCSpriteFrame bananasTexture;

		List<CCSprite> mAniLives = new List<CCSprite>();

        public GameLayer ()
        {
            var touchListener = new CCEventListenerTouchAllAtOnce ();
            touchListener.OnTouchesEnded = OnTouchesEnded;

            AddEventListener (touchListener, this);
            Color = new CCColor3B (CCColor4B.White);
            Opacity = 255;

            visibleBananas = new List<CCSprite> ();
            hitBananas = new List<CCSprite> ();

            // batch node for physics balls
            ballsBatch = new CCSpriteBatchNode ("balls", 100);
            ballTexture = ballsBatch.Texture;
            AddChild (ballsBatch, 1, 1);

			var spriteSheet = new CCSpriteSheet ("animations/monkey.plist");
			enemyBatch = new CCSpriteBatchNode ( "Enemy", 10);
			enemyTexture = enemyBatch.Texture;
			AddChild (enemyBatch , 2, 2);

			bananaBatch = new CCSpriteBatchNode ("animations/monkey", 10);
			bananasTexture = spriteSheet.Frames.Find ((x) => x.TextureFilename.StartsWith ("Banana"));
			AddChild (bananaBatch, 1, 3);

			mCurrentLives = MAX_LIVES;



            AddGrass ();
            AddSun ();
            AddMonkey ();



            StartScheduling();

			GoneBananasShared.Accelerometer.AddListener (AccelerometerValueChanaged);

            CCSimpleAudioEngine.SharedEngine.PlayBackgroundMusic ("Sounds/AngryMonkeyBG", true);
        }
		~GameLayer()
		{
			GoneBananasShared.Accelerometer.RemoveListener (AccelerometerValueChanaged);
		}

        void StartScheduling()
        {
            Schedule (t => {
                //visibleBananas.Add (AddBanana ());
                elapsedTime += t;
                if (ShouldEndGame ()) {
                    EndGame ();
                }
				AddEnemy ();
                //ShootBullet ();
            }, 1.0f);

            Schedule (t => CheckCollision ());

            Schedule (t => {
                world.Step (t, 8, 1);

                foreach (CCPhysicsSprite sprite in ballsBatch.Children) {
                    if (sprite.Visible && sprite.PhysicsBody.Position.x < 0f || sprite.PhysicsBody.Position.x * PTM_RATIO > ContentSize.Width) { //or should it be Layer.VisibleBoundsWorldspace.Size.Width
                        world.DestroyBody (sprite.PhysicsBody);
                        sprite.Visible = false;
                        sprite.RemoveFromParent ();
                    } else {
                        sprite.UpdateBallTransform();
                    }
                }
            });
        }

		void UpdateLives ( )
		{
			for (int iLivesCount = 0; iLivesCount < MAX_LIVES; iLivesCount++) 
			{
				mAniLives [iLivesCount].Visible = iLivesCount < mCurrentLives;
			}
		}
		bool IsPlayerAlive ()
		{
			return mCurrentLives > 0;
		}
		void PlayerLostLife ()
		{
			mCurrentLives--;
			if (mCurrentLives < 0)
				mCurrentLives = 0;

			UpdateLives ();
		}
		void UpdateScore ( int score )
		{
			mScore += score;

			if (mScoreLabel == null) 
			{
				mScoreLabel = new CCLabelTtf ("Score : " + mScore , "arial", 22) {
					Position = VisibleBoundsWorldspace.Center,
					Color = CCColor3B.Blue,
					HorizontalAlignment = CCTextAlignment.Center,
					VerticalAlignment = CCVerticalTextAlignment.Center,
					AnchorPoint = CCPoint.AnchorMiddle
				};

				AddChild (mScoreLabel, 4);
			}

			mScoreLabel.Text = "Score : " + mScore;
			mScoreLabel.Position = new CCPoint (VisibleBoundsWorldspace.Size.Width / 2, VisibleBoundsWorldspace.Size.Height - 72);

		}
        void AddGrass ()
        {
            grass = new CCSprite ("grass");
			grass.ScaleX = 3;
            AddChild (grass);
        }

		public int AccelerometerValueChanaged(Android.Hardware.SensorEvent e)
		{
			AccelerometerValue = e.Values [1];
			return 0;
		}

        void AddSun ()
        {
            circleNode = new CCDrawNode ();
            circleNode.DrawSolidCircle (CCPoint.Zero, 30.0f, CCColor4B.Yellow);
            AddChild (circleNode);

            sun = new CCParticleSun (CCPoint.Zero);
            sun.StartColor = new CCColor4F (CCColor3B.Red);
            sun.EndColor = new CCColor4F (CCColor3B.Yellow);
            AddChild (sun);
        }

        void AddMonkey ()
        {
            var spriteSheet = new CCSpriteSheet ("animations/monkey.plist");
            var animationFrames = spriteSheet.Frames.FindAll ((x) => x.TextureFilename.StartsWith ("frame"));

            walkAnim = new CCAnimation (animationFrames, 0.1f);
            walkRepeat = new CCRepeatForever (new CCAnimate (walkAnim));
            monkey = new CCSprite (animationFrames.First ()) { Name = "Monkey" };
            monkey.Scale = 0.25f;

            AddChild (monkey);

			Schedule (t => {
				//visibleBananas.Add (AddBanana ());
				UpdateMonkeyMovements (t);
				});
        }

        CCSprite ShootBanana ()
        {
			if (bananaBatch.ChildrenCount < MAX_BANANAS_COUNT) 
			{
				var banana = new CCSprite ( bananasTexture );

				//var p = GetRandomPosition (banana.ContentSize);
				banana.Position = new CCPoint (monkey.Position.X, monkey.Position.Y + (banana.ContentSize.Height / 2));
				banana.Scale = 0.5f;
				bananaBatch.AddChild (banana, 1);

				var moveBanana = new CCMoveTo (1/_BananaSpeed, new CCPoint (banana.Position.X,
					                 VisibleBoundsWorldspace.Size.Height + banana.ContentSize.Height / 2));
				banana.RunActions (moveBanana, moveBananaComplete);
				banana.RepeatForever (rotateBanana);

				return banana;
			}
			return null;
        }

        CCPoint GetRandomPosition (CCSize spriteSize)
        {
            double rnd = CCRandom.NextDouble ();
            double randomX = (rnd > 0) 
                ? rnd * VisibleBoundsWorldspace.Size.Width - spriteSize.Width / 2 
                : spriteSize.Width / 2;

			return new CCPoint ((float)randomX, VisibleBoundsWorldspace.Size.Height - spriteSize.Height / 2);
        }
		/*CCPoint GetRandomPosition (CCSize spriteSize)
		{
			double rnd = CCRandom.NextDouble ();
			double randomX = (rnd > 0) 
				? rnd * VisibleBoundsWorldspace.Size.Width - spriteSize.Width / 2 
				: spriteSize.Width / 2;

			Random rand = new Random ();
			int randomY = rand.Next ((int)(VisibleBoundsWorldspace.Size.Height / 2), 
				(int)(VisibleBoundsWorldspace.Size.Height-spriteSize.Height / 2));

			return new CCPoint ((float)randomX, randomY);
		}*/

        void AddClouds ()
        {
            float h = VisibleBoundsWorldspace.Size.Height;

            parallaxClouds = new CCParallaxNode {
                Position = new CCPoint (0, h)
            };
             
            AddChild (parallaxClouds);

            var cloud1 = new CCSprite ("cloud");
            var cloud2 = new CCSprite ("cloud");
            var cloud3 = new CCSprite ("cloud");

            float yRatio1 = 1.0f;
            float yRatio2 = 0.15f;
            float yRatio3 = 0.5f;

            parallaxClouds.AddChild (cloud1, 0, new CCPoint (1.0f, yRatio1), new CCPoint (100, -100 + h - (h * yRatio1)));
            parallaxClouds.AddChild (cloud2, 0, new CCPoint (1.0f, yRatio2), new CCPoint (250, -200 + h - (h * yRatio2)));
            parallaxClouds.AddChild (cloud3, 0, new CCPoint (1.0f, yRatio3), new CCPoint (400, -150 + h - (h * yRatio3)));
        }

        void MoveClouds (float dy)
        {
            parallaxClouds.StopAllActions ();
            var moveClouds = new CCMoveBy (1.0f, new CCPoint (0, dy * 0.1f));
            parallaxClouds.RunAction (moveClouds);
        }

        void CheckCollision ()
        {
			bool hit = false;
			for ( int iBananaCount = 0; iBananaCount < bananaBatch.ChildrenCount; iBananaCount++ )
			{
				if (bananaBatch.Children [iBananaCount] is CCSprite) 
				{
					CCSprite banana = (CCSprite)bananaBatch.Children [iBananaCount];
					for ( int iEnemyCount = 0; iEnemyCount < enemyBatch.ChildrenCount; iEnemyCount++ )
					{
						if (enemyBatch.Children [iEnemyCount] is CCSprite) {
							CCSprite enemy = (CCSprite)enemyBatch.Children [iEnemyCount];
							hit = banana.BoundingBoxTransformedToParent.IntersectsRect (enemy.BoundingBoxTransformedToParent);
							if (hit) 
							{
								hitBananas.Add (banana);
								CCSimpleAudioEngine.SharedEngine.PlayEffect ("Sounds/EnemyShoot");
								Explode (banana.Position);
								banana.RemoveFromParent ();
								enemy.RemoveFromParent ();

								UpdateScore (_EnemyDestroyScore);

								break;
							}
						}
						if (hit)
							break;
					}
				}
            }

            foreach (var banana in hitBananas)
            {
                visibleBananas.Remove(banana);
            }

			for ( int iBallCount = 0; iBallCount < ballsBatch.ChildrenCount; iBallCount++ )
			{
				if (ballsBatch.Children [iBallCount] is CCSprite) 
				{
					CCSprite ball = (CCSprite)ballsBatch.Children [iBallCount];

					if (monkey is CCSprite) {
						CCSprite enemy = monkey;
						hit = ball.BoundingBoxTransformedToParent.IntersectsRect (enemy.BoundingBoxTransformedToParent);
						if (hit) 
						{
							CCSimpleAudioEngine.SharedEngine.PlayEffect ("Sounds/LostLife");
							Explode (ball.Position);
							ball.RemoveFromParent ();

							PlayerLostLife ();
							if ( !IsPlayerAlive() )
								EndGame ();

							break;
						}
					}
				}
			}
        }

        void EndGame ()
        {
            // Stop scheduled events as we transition to game over scene
            UnscheduleAll();

			var gameOverScene = GameOverLayer.SceneWithScore (Window, mScore);
            var transitionToGameOver = new CCTransitionMoveInR (0.3f, gameOverScene);

            Director.ReplaceScene (transitionToGameOver);
        }

        void Explode (CCPoint pt)
        {
            var explosion = new CCParticleExplosion (pt); //TODO: manage "better" for performance when "many" particles
            explosion.TotalParticles = 10;
            explosion.AutoRemoveOnFinish = true;
            AddChild (explosion);
        }

        bool ShouldEndGame ()
        {
            return elapsedTime > GAME_DURATION;
        }

		void UpdateMonkeyMovements ( float dt )
		{
			if (Math.Abs (AccelerometerValue) > 0.5f) {
				CCPoint pos = monkey.Position;
				pos += new CCPoint (MONKEY_SPEED * AccelerometerValue * dt, 0);

				if (pos.X > VisibleBoundsWorldspace.Size.Width)
					pos.X = VisibleBoundsWorldspace.Size.Width;
				else if (pos.X < 0)
					pos.X = 0;

				monkey.Position = pos;
			}
			else
				AccelerometerValue = 0;

			if (Math.Abs (AccelerometerValue) < 0.5f)
				monkey.StopAllActions ();
			else 
			{
				if ( monkey.NumberOfRunningActions < 1 )
					monkey.RunAction (walkRepeat);
			}
		}

        void OnTouchesEnded (List<CCTouch> touches, CCEvent touchEvent)
        {
            /*monkey.StopAllActions ();

            var location = touches [0].LocationOnScreen;
            location = WorldToScreenspace (location);  //Layer.WorldToScreenspace(location); 
            float ds = CCPoint.Distance (monkey.Position, location);

            var dt = ds / MONKEY_SPEED;

            var moveMonkey = new CCMoveTo (dt, location);

            //BUG: calling walkRepeat separately as it doesn't run when called in RunActions or CCSpawn
            monkey.RunAction (walkRepeat);
            monkey.RunActions (moveMonkey, walkAnimStop);*/

            // move the clouds relative to the monkey's movement
            //MoveClouds (location.Y - monkey.Position.Y);


			visibleBananas.Add (ShootBanana ());
        }

        protected override void AddedToScene ()
        {
            base.AddedToScene ();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.NoBorder;

            grass.Position = VisibleBoundsWorldspace.Center;
			monkey.Position = VisibleBoundsWorldspace.Center - new CCPoint ( 0, VisibleBoundsWorldspace.Size.Height/4 );

            var b = VisibleBoundsWorldspace;
			sun.Position = new CCPoint (72,	VisibleBoundsWorldspace.Size.Height - 72);

            circleNode.Position = sun.Position;

            //AddClouds ();
			UpdateScore (0);

			for (int iLivesCount = 0; iLivesCount < MAX_LIVES; iLivesCount++) 
			{
				CCSprite lifeSprite = new CCSprite ("Life.png");
				lifeSprite.Scale = 0.036f;
				lifeSprite.Visible = false;
				AddChild (lifeSprite, 3);

				lifeSprite.Position =  new CCPoint (VisibleBoundsWorldspace.Size.Width - ((iLivesCount +1) * 72),
					VisibleBoundsWorldspace.Size.Height - 72);

				mAniLives.Add (lifeSprite);
			}

			UpdateLives ();
        }

        void InitPhysics ()
        {
            CCSize s = Layer.VisibleBoundsWorldspace.Size;

            var gravity = new b2Vec2 (0.0f, -10.0f);
            world = new b2World (gravity);

            world.SetAllowSleeping (true);
            world.SetContinuousPhysics (true);

            var def = new b2BodyDef ();
            def.allowSleep = true;
            def.position = b2Vec2.Zero;
            def.type = b2BodyType.b2_staticBody;
            b2Body groundBody = world.CreateBody (def);
            groundBody.SetActive (true);

            b2EdgeShape groundBox = new b2EdgeShape ();
            groundBox.Set (b2Vec2.Zero, new b2Vec2 (s.Width / PTM_RATIO, 0));
            b2FixtureDef fd = new b2FixtureDef ();
            fd.shape = groundBox;
            groundBody.CreateFixture (fd);
        }

		void ShootBullet ( CCSprite enemySprite )
        {
            if (ballsBatch.ChildrenCount < MAX_NUM_BALLS) {
                int idx = (CCRandom.Float_0_1 () > .5 ? 0 : 1);
                int idy = (CCRandom.Float_0_1 () > .5 ? 0 : 1);
                var sprite = new CCPhysicsSprite (ballTexture, new CCRect (32 * idx, 32 * idy, 32, 32), PTM_RATIO);

                ballsBatch.AddChild (sprite);

                //CCPoint p = GetRandomPosition (sprite.ContentSize);

				sprite.Position = enemySprite.Position;// new CCPoint (p.X, p.Y);

                var def = new b2BodyDef ();
				def.position = new b2Vec2 (sprite.Position.X / PTM_RATIO, sprite.Position.Y / PTM_RATIO);
				Random rand = new Random ();
				/*if(rand.Next (100) < 50)
                	def.linearVelocity = new b2Vec2 (-10.0f, 0.0f);
				else
					def.linearVelocity = new b2Vec2 (10.0f, 0.0f);*/
				def.linearVelocity = new b2Vec2 (0.0f, -1.0f);
                def.type = b2BodyType.b2_dynamicBody;
                b2Body body = world.CreateBody (def);

                var circle = new b2CircleShape ();
                circle.Radius = 0.5f;

                var fd = new b2FixtureDef ();
                fd.shape = circle;
                fd.density = 1f;
                fd.restitution = 0.85f;
                fd.friction = 0f;
                body.CreateFixture (fd);

                sprite.PhysicsBody = body;
            }
        }
		void AddEnemy ()
		{
			if ( enemyBatch.ChildrenCount < MAX_ENEMY_COUNT) {
				float idx = (CCRandom.Float_0_1 () > .5 ? 0 : 1);
				float idxOp = 1 - idx;
				float idy = (float) (0.6 + (CCRandom.Float_0_1 () *  0.2));
				CCEnemySprite enemySprite = new CCEnemySprite (enemyBatch.Texture, new CCRect (0,0, enemyBatch.Texture.PixelsWide, enemyBatch.Texture.PixelsHigh ), 5.0f, ShootBullet );
				enemySprite.Scale = 0.3f;
				enemyBatch.AddChild (enemySprite);

				enemySprite.Position = new CCPoint ( idx * VisibleBoundsWorldspace.Size.Width, idy * VisibleBoundsWorldspace.Size.Height );
				//enemySprite.ZOrder  =

				// sending the enemy to the opposite direction
				var moveEnemy = new CCMoveTo (1/_EnemySpeed, new CCPoint (idxOp * VisibleBoundsWorldspace.Size.Width,
												enemySprite.Position.Y));
				enemySprite.RunActions (moveEnemy, moveBananaComplete);
				//enemySprite.RepeatForever (rotateBanana);

			}
		}

        public override void OnEnter ()
        {
            base.OnEnter ();

            InitPhysics ();
        }

        public static CCScene GameScene (CCWindow mainWindow)
        {
            var scene = new CCScene (mainWindow);
            var layer = new GameLayer ();
			
            scene.AddChild (layer);

            return scene;
        }
    }
}