using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Penguinner
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Penguin_Frog penguinFrog;
        World world;
        AttackButters butters;
        Timer timer;

        SpriteFont font;

        public int level;
        private int resetWaitTime = 0;
        private int resetMsgTime = 2000;
        private bool won = false;
        private int wonWaitTime = 1500;
        private int wonElapsedTime = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // I'm not sure if this is the right way to do this -- i want the World class to know the penguin's position, so 
            // i just passed the penguin into it...
            penguinFrog = new Penguin_Frog(this, new Vector2(400, 400));
            penguinFrog.DrawOrder = 2;

            List<Scrolling_game_object> scr_obj_list = new List<Scrolling_game_object>();
            scr_obj_list = build_scroll_object_row("small_seal", 3, 2, 75, 5, true);
            scr_obj_list.AddRange(build_scroll_object_row("small_seal", 5, 3, 150, 10, false));
            scr_obj_list.AddRange(build_scroll_object_row("small_seal", 2, 4, 225, 10, false));
            foreach (Scrolling_game_object obj in scr_obj_list)
            {
                obj.DrawOrder = 2;
                Components.Add(obj);
            }

            timer = new Timer(this);
            timer.DrawOrder = 2;

            butters = new AttackButters(this);
            butters.DrawOrder = 2;
            butters.Penguin = penguinFrog;

            world = new World(this, penguinFrog, butters, scr_obj_list, timer);
            world.DrawOrder = 1;

            penguinFrog.world = world;

            Components.Add(penguinFrog);
            Components.Add(world);
            Components.Add(butters);
            Components.Add(timer);
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = this.Content.Load<SpriteFont>("Arial24");
            Song song = Content.Load<Song>("music");  // Put the name of your song in instead of "song_title"
            try
            {
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        bool reset = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (penguinFrog.Health <= 0)
            {
                Components.Clear();
                reset = true;
            }

            if (penguinFrog.have_won)
            {
                wonElapsedTime += gameTime.ElapsedGameTime.Milliseconds;

                if (wonElapsedTime >= wonWaitTime)
                {
                    Components.Clear();
                    won = true;
                    reset = true;
                    wonElapsedTime = 0;
                }
            }

            if (reset)
            {
                resetWaitTime += gameTime.ElapsedGameTime.Milliseconds;
                if (resetWaitTime > resetMsgTime)
                {
                    reset = false;
                    won = false;
                    this.Initialize();
                    resetWaitTime = 0;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            if (reset)
            {
                string mystring = "";

                if (won)
                    mystring = "You won!!! Starting from beginning.";
                else 
                    mystring = "You died. Starting from beginning.";

                spriteBatch.DrawString(font, mystring, new Vector2(170, 20), Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private List<Scrolling_game_object> build_scroll_object_row(string sprite_name, int num_obj_in_row, int speed, float y_coordinate, int damage, bool l_to_r)
        {
            decimal window_width = GraphicsDevice.Viewport.Width;

            int spacing = (int)(window_width / num_obj_in_row);

            Random rand = new Random();
            int pos = 10;

            Scrolling_game_object myobj;
            List<Scrolling_game_object> myobjs = new List<Scrolling_game_object>();
            
            while (pos < this.Window.ClientBounds.Right - 100) {
                myobj = new Scrolling_game_object(this, new Vector2(pos, y_coordinate), sprite_name, speed, l_to_r, penguinFrog, damage);
                pos += spacing;
                myobjs.Add(myobj);
            }
            
            return myobjs;
        }
    }
}
