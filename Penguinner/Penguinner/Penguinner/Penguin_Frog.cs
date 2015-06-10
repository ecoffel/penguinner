using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Penguinner
{
    public class Penguin_Frog : DrawableGameComponent
    {
        public Vector2 Position { get; set; }
        public Rectangle Size { get; set; }
        public Texture2D sprite;
        SpriteBatch spriteBatch;
        public int currentFrame { get; set; }
        public bool have_won = false;
        
        // width of the sprite box -- larger than the penguin
        int spriteWidth = 48;
        int spriteHeight = 44;

        // how far to move from the top left of the sprite corner to get to the actual penguin rectangle
        public Vector2 penguinOffset = new Vector2(8, 2);
        // size of the actual penguin image from the penguinOffset point
        public Rectangle penguinRealSize = new Rectangle(0, 0, 29, 38);

        public Vector2 penguinCollisionOffset = new Vector2(8, 2);
        public Rectangle penguinCollisionRect = new Rectangle(0, 0, 29, 38);

        public Vector2 origin;
        public int Health { get; set; }
        SpriteFont font;
        string err_string;
        float alpha;
        public float Scale { get; set; }
        Texture2D health_sprite;

        int MaxX;
        int MinX = 0;
        int MaxY;
        int MinY = 0;

        public float Alpha { get { return alpha; } set { alpha = value; } }
        public World world { get; set; }

        public Penguin_Frog(Game g, Vector2 position)
            : base(g)
        {
            Position = position;
            currentFrame = 0;
            Health = 100;
            alpha = 1f;
            Scale = 1f;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            sprite = Game.Content.Load<Texture2D>("penguinsheet");
            font = Game.Content.Load<SpriteFont>("Arial");
            health_sprite = Game.Content.Load<Texture2D>("health_bar");
            //setting size of screen
            MaxX = spriteBatch.GraphicsDevice.Viewport.Width - sprite.Width;
            MaxY = spriteBatch.GraphicsDevice.Viewport.Height - sprite.Height;
        }

        public override void Update(GameTime gameTime)
        {
            Size = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);
            origin = new Vector2(Size.Width / 2, Size.Height / 2);
            //origin = new Vector2((Scale-1f)*spriteWidth/2, (Scale-1f)*spriteHeight/2);

            err_string = "";
            if (this.Health <= 0)
            {
                err_string = "You died. Sorry.";
            }

            KeyboardState k = Keyboard.GetState();

            Vector2 right = new Vector2(1, 0);
            Vector2 left = new Vector2(-1, 0);
            Vector2 up = new Vector2(0, -1);
            Vector2 down = new Vector2(0, 1);

            if (k.IsKeyDown(Keys.Up))
            {
                Position += 5 * up;
                if (currentFrame == 0)
                    currentFrame = 1;
                else if (currentFrame == 1)
                    currentFrame = 2;
                else if (currentFrame == 2)
                    currentFrame = 3;
                else
                    currentFrame = 0;
            }
            if (k.IsKeyDown(Keys.Down))
            {
                Position += 5 * down;
                if (currentFrame == 0)
                    currentFrame = 1;
                else if (currentFrame == 1)
                    currentFrame = 2;
                else if (currentFrame == 2)
                    currentFrame = 3;
                else
                    currentFrame = 0;
            }
            if (k.IsKeyDown(Keys.Left))
            {
                Position += 5 * left;
                if (currentFrame == 0)
                    currentFrame = 1;
                else if (currentFrame == 1)
                    currentFrame = 2;
                else if (currentFrame == 2)
                    currentFrame = 3;
                else
                    currentFrame = 0;
            }
            if (k.IsKeyDown(Keys.Right))
            {
                Position += 5 * right;
                if (currentFrame == 0)
                    currentFrame = 1;
                else if (currentFrame == 1)
                    currentFrame = 2;
                else if (currentFrame == 2)
                    currentFrame = 3;
                else
                    currentFrame = 0;
            }

            if (currentFrame == 6)
            {
                penguinOffset = new Vector2(3, 14);
                penguinRealSize = new Rectangle(0, 0, 36, 23);
            }
            else
            {
                penguinOffset = new Vector2(8, 2);
                penguinRealSize = new Rectangle(0, 0, 29, 38);
            }

            int leftside = 0; //Game.Window.ClientBounds.Left - 275; //minx
            int rightside = Game.GraphicsDevice.Viewport.Width; //Game.Window.ClientBounds.Right - 275; //maxx
            int bottom = Game.GraphicsDevice.Viewport.Height; //Game.Window.ClientBounds.Height - 15; //miny
            int top = 0; //maxy

            if (Position.X > rightside) {
                Position = new Vector2(rightside, Position.Y);
            }
            else  if (Position.X < leftside) {
                Position = new Vector2(leftside, Position.Y);
            }
            if (Position.Y > bottom)
                Position = new Vector2(Position.X, bottom);
            if (Position.Y <= top)
                Position = new Vector2(Position.X, top);
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            spriteBatch.Draw(sprite, Position, Size, new Color(new Vector4(1f, 1f, 1f, alpha)), 0f, origin, Scale, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, err_string, new Vector2(10, 10), Color.Black);
            

            Rectangle health_bar = new Rectangle(600, 10, Health, 15);
            Color mycolor = Color.Green;
            if (Health > 50)
                mycolor = Color.Green;
            if (Health <= 50 && Health > 25)
                mycolor = Color.Orange;
            if (Health <= 25)
                mycolor = Color.Red;
            
            spriteBatch.Draw(health_sprite, health_bar, mycolor);
            string mystring = "Health: " + this.Health;
            Vector2 FontOrigin = font.MeasureString(mystring) / 2;
            spriteBatch.DrawString(font, mystring, new Vector2(600, 30), Color.Black,
                0, new Vector2(0 , 0), 1f, SpriteEffects.None, 1.0f); 
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
