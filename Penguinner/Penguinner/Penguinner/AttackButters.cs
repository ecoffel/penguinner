using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penguinner
{
    public class AttackButters : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private Texture2D sprite;
        private Texture2D bulletSprite;
        private Vector2 bulletOrigin;
        private Vector2 bulletPosition;
        private float bulletAngle = 0;
        private Vector2 pos;
        private Vector2 vel;
        private float scale = 0.75f;

        // how long between butters appearances (in ms).  reset to a random value after each apperance
        private int interval;
        // how many ms since the last butters attack
        private int timeSinceLastApperance = 0;
        // initial speed that butters moves at -- also random (px/second)
        private int buttersSpeed = 225;
        // how quickly butters slows down -- random, and determines how far on screen he comes (px/sec/sec)
        private int buttersSpeedDecay = 4;
        public bool attacking = false;
        private bool attackReset = false;
        private bool bulletFired = false;

        // in px/sec
        private float bulletSpeed = 300.0f;

        private Random rand;
        private Collison collision;
        public Penguin_Frog Penguin;

        #region Properties
        public Vector2 Position { get { return pos; } set { pos = value; } }
        public Vector2 Velocity { get { return vel; } set { vel = value; } }
        #endregion

        public AttackButters(Game game) : base(game)
        {
            rand = new Random();
            collision = new Collison();

            // this is the position of butter's right eye relative to the top left of the image
            bulletOrigin = new Vector2(215f, 30f);

            interval = rand.Next(7000, 10000);
            buttersSpeed = rand.Next(200, 350);
            buttersSpeedDecay = rand.Next(3, 5);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            sprite = Game.Content.Load<Texture2D>("Characters/butters");
            bulletSprite = Game.Content.Load<Texture2D>("Characters/butters-bullet");
         
            // start butters invisible on left side of screen
            pos = new Vector2(-sprite.Width, 100);
            vel = new Vector2(0, 0);

            base.LoadContent();
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            timeSinceLastApperance += gameTime.ElapsedGameTime.Milliseconds;

            if (timeSinceLastApperance > interval && !attacking)
            {
                attacking = true;
                attackReset = false;
                vel = new Vector2(buttersSpeed, 0);
            }

            if (attacking)
            {
                vel = new Vector2(vel.X - buttersSpeedDecay, 0);

                if (vel.X <= 0)
                {
                    if (!bulletFired)
                    {
                        bulletPosition = bulletOrigin + pos;
                        bulletAngle = (float) Math.Atan((bulletPosition.Y-Penguin.Position.Y)/(bulletPosition.X-Penguin.Position.X));
                        bulletFired = true;
                    }
                    attacking = false;
                    timeSinceLastApperance = 0;
                    buttersSpeed = rand.Next(200, 350);
                    buttersSpeedDecay = rand.Next(3, 5);
                    interval = rand.Next(7000, 10000);
                }
            }
            else
            {
                if (pos.X > -sprite.Width)
                {
                    if (vel.X < 2)
                        vel = new Vector2(vel.X - buttersSpeedDecay, vel.Y);
                }
                else
                {
                    if (!attackReset)
                    {
                        pos = new Vector2(pos.X, rand.Next(75, 400));
                        vel = new Vector2(0, 0);
                        attackReset = true;
                    }
                }
            }

            if (bulletFired)
            {
                bulletPosition = new Vector2((float)(bulletPosition.X + Math.Cos(bulletAngle) * bulletSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000.0)), (float)(bulletPosition.Y + Math.Sin(bulletAngle) * bulletSpeed * (gameTime.ElapsedGameTime.Milliseconds / 1000.0)));

                if (bulletPosition.X > Game.GraphicsDevice.Viewport.Width || bulletPosition.Y < 0 || bulletPosition.Y > Game.GraphicsDevice.Viewport.Height)
                    bulletFired = false;

                if (collision.IsCollided(new Rectangle((int) Penguin.Position.X, (int) Penguin.Position.Y, Penguin.Size.Width, Penguin.Size.Height),
                                         new Rectangle((int)bulletPosition.X, (int)bulletPosition.Y, bulletSprite.Width, bulletSprite.Height)) &
                    (Penguin.have_won == false))
                {
                    Penguin.Health -= 20;
                    bulletFired = false;
                }
            }

            pos = new Vector2((float) (pos.X + vel.X * (gameTime.ElapsedGameTime.Milliseconds / 1000.0)), 
                              (float) (pos.Y + vel.Y*(gameTime.ElapsedGameTime.Milliseconds/1000.0)));

            base.Update(gameTime);
        }
        #endregion

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(sprite, pos, null, Color.White, 0f, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            if (bulletFired)
                spriteBatch.Draw(bulletSprite, bulletPosition, null, Color.White, bulletAngle, new Vector2(0,0), 1.0f, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
