using System;
using System.Collections.Generic;
using CocosSharp;

namespace GoneBananas
{
    public class GameOverLayer : CCLayerColor
    {

        string scoreMessage = string.Empty;

        public GameOverLayer (int score)
        {

            var touchListener = new CCEventListenerTouchAllAtOnce ();
            touchListener.OnTouchesEnded = (touches, ccevent) => Window.DefaultDirector.ReplaceScene (GameLayer.GameScene (Window));

            AddEventListener (touchListener, this);

            scoreMessage = String.Format ("Game Over. Your Score : {0} !", score);

            Color = new CCColor3B (CCColor4B.Black);

            Opacity = 255;
			MobileService.UpdateScoreToServer (score);
        }

        public void AddMonkey ()
        {
            var spriteSheet = new CCSpriteSheet ("animations/monkey.plist");
            var frame = spriteSheet.Frames.Find ((x) => x.TextureFilename.StartsWith ("frame"));
           
            var monkey = new CCSprite (frame) {
                Position = new CCPoint (VisibleBoundsWorldspace.Size.Center.X + 20, VisibleBoundsWorldspace.Size.Center.Y + 300),
                Scale = 0.5f
            };

            AddChild (monkey);
        }

        protected override void AddedToScene ()
        {
            base.AddedToScene ();

            Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.ShowAll;

            var scoreLabel = new CCLabelTtf (scoreMessage, "arial", 24) {
                Position = new CCPoint (VisibleBoundsWorldspace.Size.Center.X, VisibleBoundsWorldspace.Size.Center.Y + 50),
                Color = new CCColor3B (CCColor4B.Yellow),
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle
            };

            AddChild (scoreLabel);

			var playAgainLabel = new CCLabelTtf ("Tap to Play Again and give your monkey an another chance :)", "arial", 24) {
                Position = VisibleBoundsWorldspace.Size.Center,
                Color = new CCColor3B (CCColor4B.Green),
                HorizontalAlignment = CCTextAlignment.Center,
                VerticalAlignment = CCVerticalTextAlignment.Center,
                AnchorPoint = CCPoint.AnchorMiddle,
            };

            AddChild (playAgainLabel);

            AddMonkey ();
        }

		public static CCScene SceneWithScore (CCWindow mainWindow, int score)
        {
            var scene = new CCScene (mainWindow);
            var layer = new GameOverLayer (score);

            scene.AddChild (layer);

            return scene;
        }
    }
}