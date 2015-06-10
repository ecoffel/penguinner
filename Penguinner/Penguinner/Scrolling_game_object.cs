using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//To add a new scrolling object, use Components.Add in Game1.cs

//Example:
// new Scrolling_game_object(this, new Vector2(400, 250), "mini_seal", 5, true, penguinFrog, 2);
//
// this - game object from Game1
// new Vector2(400, 250) - initial position of the object. Y coordinate (250) will not change
//"mini_seal" - name of the sprite used, minus the extension (.png). Must be added to Penguinner content
// 5 - scroll speed
// true - it scrolls from left to right
// penguinFrog - the user controlled character that the object checks if it has collided with
// 2 - amount of damage that the object does to penguinFrog

namespace Penguinner
{
    public class Scrolling_game_object : DrawableGameComponent
    {
        string sprite_string;
        public Vector2 Position { get; set; }
        public int Damage;

        Texture2D sprite;
        SpriteBatch spriteBatch;
        bool scrolls_left_to_right;
        int scroll_speed;
        Penguin_Frog penguin;
        SpriteFont font;
        Collison collision;
        Game mygame;
        int collide_count;
        private bool penguinStateReset = false;

        int MaxX;
        int MinX = 0;
        int MaxY;
        int MinY = 0;

        public Scrolling_game_object(Game g, Vector2 position, string _sprite_string, int _scroll_speed, bool s_left_right, Penguin_Frog _pf, int _damage)
            : base(g)
        {
            Position = position;
            sprite_string = _sprite_string;
            scroll_speed = _scroll_speed;
            scrolls_left_to_right = s_left_right;
            penguin = _pf;
            Damage = _damage;
            mygame = g;
            collide_count = 0;
            collision = new Collison();
        }

        protected override void LoadContent()
        {
            sprite = Game.Content.Load<Texture2D>(sprite_string);
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("Arial");

            MaxX = spriteBatch.GraphicsDevice.Viewport.Width - sprite.Width;
            MaxY = spriteBatch.GraphicsDevice.Viewport.Height - sprite.Height;
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 offset = new Vector2(sprite.Width / 2.0f, sprite.Height / 2.0f);
            spriteBatch.Begin();
            spriteBatch.Draw(sprite, Position, Color.White);

            if (CheckScrollingObjCollisions())
            {
                if ((collide_count == 0) & (!penguin.have_won))
                    penguin.Health -= Damage;

                collide_count += 1;

                penguin.currentFrame = 6;
                penguinStateReset = false;
            }
            else
            {
                collide_count = 0;
                if (!penguinStateReset)
                {
                    penguin.currentFrame = 0;
                    penguinStateReset = true;
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            int leftside = MinX;
            int rightside = MaxX;

            // Choose which direction the object scrolls
            Vector2 dir;
            if (scrolls_left_to_right)
                dir = new Vector2(1, 0);
            else
                dir = new Vector2(-1, 0);

            Position += dir * scroll_speed;

            //If the object goes off screen
            if ((Position.X > rightside) & scrolls_left_to_right)
                Position = new Vector2(leftside, Position.Y);

            if ((Position.X < leftside) & !scrolls_left_to_right)
                Position = new Vector2(rightside, Position.Y);

            base.Update(gameTime);
        }

        private bool CheckScrollingObjCollisions()
        {
            Rectangle penguinRect = new Rectangle((int)(penguin.Position.X + penguin.penguinCollisionOffset.X - penguin.origin.X), (int)(penguin.Position.Y + penguin.penguinCollisionOffset.Y - penguin.origin.Y),
                penguin.penguinCollisionRect.Width, penguin.penguinCollisionRect.Height);

            return collision.IsCollided(penguinRect,
                                        new Rectangle((int) this.Position.X, (int) this.Position.Y, this.sprite.Width, this.sprite.Height));
        }
    }
}
