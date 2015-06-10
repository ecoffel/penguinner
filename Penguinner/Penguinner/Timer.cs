using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Penguinner
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Timer : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        double StartTime;
        double CurrentTime;
        SpriteFont font;
        public Timer(Game game)
            : base(game)
        {
            StartTime = -1;
            CurrentTime = 0;
        }

        public double Reset(){
            double temp = CurrentTime - StartTime;
            StartTime = -1;
            CurrentTime = 0;
            return temp;
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("Arial");
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if (StartTime == -1) {
                StartTime = gameTime.TotalGameTime.TotalSeconds;}
            CurrentTime = gameTime.TotalGameTime.TotalSeconds;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            string output = Math.Round(CurrentTime - StartTime).ToString();
            spriteBatch.DrawString(font,"Time: " + output,new Vector2(600, 50), Color.Black);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
