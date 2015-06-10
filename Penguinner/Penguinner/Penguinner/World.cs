using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Penguinner
{
    public class World : DrawableGameComponent
    {
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;
        private Texture2D tile;
        private Texture2D waterTile;
        private Texture2D sushiSprite;
        private Vector2 sushiPos;
        // distance from top left of the sprite to the point where it becomes fully opaque
        private  Vector2 waterTileOffset = new Vector2(5, 5);
        private Random rand;
        Penguin_Frog penguin;

        int MaxX;
        int MinX = 0;
        int MaxY;
        int MinY = 0;
       
        List<Scrolling_game_object> game_object_list;

        // contains the state of the ice tiles -- true means it's there, false means melted
        // this list will "move" upward as the penguin moves -- just meaning that the top row will be replaced with new 
        // tiles and all the other rows will move down
        List<List<bool>> worldTiles;

        // these are the locations (x,y) of the holes, and z is the alpha of the tile (so that they can fade in)
        // used to preferentially enlarge the already existing holes
        List<Vector3> holeLocation;

        // tiles per minute that melt -- increases over time
        int meltRate = 200;
        // the probability that a new melt hole will enlarge a pre-existing hole
        float enlargeHoleProb = 0.75f;
        // in ticks
        long nextMeltTime;
        // how many extra rows of tiles to have on each side of the visible screen.
        int extraWorldRows = 2;
        // alpha units / sec
        private float holeFadeRate = 1;

        // rows to scroll before sushi appears
        private int sushiDistance = 150;

        // this is how many scroll lines the penguin has moved up
        int globalTopY = 0;

        Collison collision;
        // is the penguin in the water
        bool waterCollision = false;
        long timeInWater = 0;
        float fadeRate = 2.0f;

        // whether the map is currently scrolling
        private bool scrolling = false;
        // how long (sec) it takes to scroll a tile
        private float scrollRate = 0.15f;
        // this goes from 0 to tile.Height during the scrollRate time period
        private float scrollPosition = 0;

        //sushi collision
        bool sushiCollision = false;
        private float sushiScale = 1f;
        private float sushiEatRate = 1f;
        private Vector2 sushiOrigin;

        // health per second
        private int waterHealthLoss = 20;
        private int timeSinceLastHealthLoss = 0;

        // how many tiles to fill the screen
        int horizontalTiles = 0;
        int verticalTiles = 0;

        SpriteFont font;

        private AttackButters butters;
        Timer timer;

        #region Properties
        public int MapScroll { get { return globalTopY; } }
        public bool CanScroll { get; set; }
        #endregion

        Game g;
        public World(Game game, Penguin_Frog penguinFrog, AttackButters attackButters, List<Scrolling_game_object> g_obj_list, Timer InputTimer)
            : base(game)
        {
            worldTiles = new List<List<bool>>();
            holeLocation = new List<Vector3>();
            rand = new Random();
            collision = new Collison();
            nextMeltTime = 0;
            penguin = penguinFrog;
            game_object_list = g_obj_list;
            butters = attackButters;
            sushiPos = new Vector2(-1, -1);
            g = game;
            timer = InputTimer;
        }

        public override void Initialize()
        {
            device = Game.GraphicsDevice;
            base.Initialize();
        }

        #region Load Content
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(device);
            tile = Game.Content.Load<Texture2D>("World/tile");
            waterTile = Game.Content.Load<Texture2D>("World/water-tile");
            sushiSprite = Game.Content.Load<Texture2D>("sushi");
            font = Game.Content.Load<SpriteFont>("Arial");
            sushiOrigin = new Vector2(sushiSprite.Width/2.0f, sushiSprite.Height/2.0f);

            //setting size of screen
            MaxX = spriteBatch.GraphicsDevice.Viewport.Width - penguin.sprite.Width;
            MaxY = spriteBatch.GraphicsDevice.Viewport.Height - penguin.sprite.Height;

            // number of tiles to fill the screen
            //horizontalTiles = Game.Window.ClientBounds.Width / tile.Width + 1;
            //verticalTiles = Game.Window.ClientBounds.Height / tile.Height + 1;

            horizontalTiles = Game.GraphicsDevice.Viewport.Width / tile.Width + 1;
            verticalTiles = Game.GraphicsDevice.Viewport.Height / tile.Height + 1;

            // start with all tiles present
            for (int i = 0; i < verticalTiles+extraWorldRows; i++)
            {
                List<bool> curRow = new List<bool>();
                for (int j = 0; j < horizontalTiles; j++)
                {
                    curRow.Add(true);
                }
                worldTiles.Add(curRow);
            }
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            if (globalTopY < sushiDistance)
            {
                MoveWorld();
                CanScroll = true;
            }
            else
            {
                CanScroll = false;
            }
            UpdateWorld();
            UpdateTiles();
            add_scroll_line();
            waterCollision = CheckWaterCollisions() != -1;
            sushiCollision = CheckSushiCollision();

            if (sushiCollision)
            {
                if (sushiScale > 0.1)
                {
                    sushiScale -= gameTime.ElapsedGameTime.Milliseconds/1000.0f*sushiEatRate;
                    penguin.Scale += gameTime.ElapsedGameTime.Milliseconds / 1000.0f * sushiEatRate;
                }
            }

            if (scrolling)
            {
                scrollPosition += (float) ((gameTime.ElapsedGameTime.Milliseconds/1000.0)/scrollRate);
                if (scrollPosition >= 1)
                {
                    // the scroll has been completed
                    scrolling = false;
                    scrollPosition = 0;

                    // add new row of tiles
                    List<bool> newRow = new List<bool>();
                    for (int i = 0; i < horizontalTiles; i++)
                        newRow.Add(true);

                    worldTiles.Insert(0, newRow);
                    worldTiles.RemoveAt(worldTiles.Count - 1);

                    // we have scrolled another line
                    globalTopY++;
                    meltRate += 5;

                    // change the positions of all the holes
                    for (int i = 0; i < holeLocation.Count; i++)
                        holeLocation[i] = new Vector3(holeLocation[i].X, holeLocation[i].Y+1, holeLocation[i].Z);
                }
                else
                {
                    // move the positions of the objects while they are scrolling.
                    butters.Position = new Vector2(butters.Position.X, butters.Position.Y + ((float) (gameTime.ElapsedGameTime.Milliseconds/1000.0)/scrollRate)*tile.Height);
                    penguin.Position = new Vector2(penguin.Position.X, penguin.Position.Y + ((float)(gameTime.ElapsedGameTime.Milliseconds / 1000.0)/scrollRate)*tile.Height);
                    foreach (Scrolling_game_object obj in game_object_list)
                        obj.Position = new Vector2(obj.Position.X, obj.Position.Y + ((float)(gameTime.ElapsedGameTime.Milliseconds / 1000.0)/scrollRate)*tile.Height);
                }
            }

            if (waterCollision)
            {
                timeInWater += gameTime.ElapsedGameTime.Milliseconds;
                if (penguin.Alpha > 0.2) penguin.Alpha -= (float)(gameTime.ElapsedGameTime.Milliseconds / 1000.0) / fadeRate;

                timeSinceLastHealthLoss += gameTime.ElapsedGameTime.Milliseconds;
                if ((timeSinceLastHealthLoss > (1.0/waterHealthLoss) * 1000.0) 
                    & (! penguin.have_won) 
                    & (penguin.Alpha <= 0.2))
                {
                    penguin.Health--;
                    timeSinceLastHealthLoss = 0;
                }
            }
            else
            {
                timeInWater = 0;
                timeSinceLastHealthLoss = 0;
                if (penguin.Alpha < 1) penguin.Alpha += 5f * (float) (gameTime.ElapsedGameTime.Milliseconds/1000.0)/fadeRate;
            }

            // update the alpha values of all the water tiles
            for (int i = 0; i < holeLocation.Count; i++)
            {
                if (holeLocation[i].Z < 1)
                    holeLocation[i] = new Vector3(holeLocation[i].X, holeLocation[i].Y, holeLocation[i].Z + (float) (gameTime.ElapsedGameTime.Milliseconds/1000.0)*holeFadeRate);
            }

            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            // draw floor tiles

            MaxX = spriteBatch.GraphicsDevice.Viewport.Width;
            MaxY = spriteBatch.GraphicsDevice.Viewport.Height;

            int screenWidth = MaxX;         //Game.Window.ClientBounds.Width;
            int screenHeight = MaxY;        //Game.Window.ClientBounds.Height;
            
            // first draw ice everywhere
            int rowCount = -1;
            for (int i = 0; i < verticalTiles + extraWorldRows; i++)
            {
                for (int j = 0; j < worldTiles[i].Count; j++)
                {
                    spriteBatch.Draw(tile, new Vector2(j * tile.Width, rowCount * tile.Height + (scrollPosition*tile.Height)), Color.White);
                }
                rowCount++;
            }

            // now draw water tiles at the proper alpha (do this separately and after ice so that water is always on top)
            for (int i = 0; i < holeLocation.Count; i++)
            {
                spriteBatch.Draw(waterTile, new Vector2(holeLocation[i].X * tile.Width, holeLocation[i].Y * tile.Height + (scrollPosition * tile.Height)), null, new Color(new Vector4(1f, 1f, 1f, holeLocation[i].Z)), 0, waterTileOffset, 1f, SpriteEffects.None, 0);
            }

            if (waterCollision)
            {
                spriteBatch.DrawString(font, "Get out of the water!", new Vector2(10, 10), Color.Black);
                penguin.currentFrame = 5;
            }

            KeyboardState k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.Space)){
                spriteBatch.DrawString(font, "Poor Penuginner is hungry, and is out looking for some Sushi.", new Vector2(100, 300), Color.Black);
                spriteBatch.DrawString(font, "He is so hungry that he has forgotten how to swim, and will drown if he stays in the water.", new Vector2(100, 320), Color.Black);
                spriteBatch.DrawString(font, "Watch out for seals, walruses, and whales, and any other creatures he might encounter.", new Vector2(100, 340), Color.Black);
                spriteBatch.DrawString(font, "(Looking for Sushi? Try going up)", new Vector2(100, 360), Color.Black); 
            }

            spriteBatch.DrawString(font, "Press Space for Instructions", new Vector2(10, 450), Color.Black); 

            if (globalTopY >= sushiDistance)
            {
                if ((int) Math.Round(sushiPos.X) == -1)
                    sushiPos = new Vector2(rand.Next(150, 400), 50f);
                spriteBatch.Draw(sushiSprite, sushiPos, null, Color.White, 0f, sushiOrigin, sushiScale, SpriteEffects.None, 0);
            }

            if (sushiCollision)
            {
                spriteBatch.DrawString(font, "You Win!!!!", new Vector2(350, 10), Color.Red, 0, new Vector2(0,0), 1f, SpriteEffects.None, 0);
                penguin.have_won = true;
            }
            
            spriteBatch.End();
        }
        #endregion

        #region Move World
        private void MoveWorld()
        {
            // move the world down a row if the penguin is at top of screen
            if (penguin.Position.Y < 0.4f * Game.GraphicsDevice.Viewport.Height)
            {
                scrolling = true;
                
            }
        }
        #endregion

        #region Add Scroll Line

        int prev_global_top;
        int next_possible_line = 0;
        private void add_scroll_line()
        {
            Random rand = new Random();
            float gen_line_prob = rand.Next(40);
            
            float top_of_window = globalTopY; 

            if (((globalTopY - prev_global_top) > 0 ) && (globalTopY > next_possible_line))
            {
                //distance between lines
                next_possible_line = globalTopY + 20;
 
                bool left_right;
                int lr = rand.Next(2);
                if (lr == 1)
                    left_right = true;
                else
                    left_right = false;
                
                // generates a row of three scrolling objects
                List<object> object_params = random_object_params();
                List<Scrolling_game_object> myobjs = build_scroll_object_row((string)object_params[0], (int)object_params[1], (int) object_params[2], top_of_window - 300, (int) object_params[3], left_right);
                game_object_list.AddRange(myobjs);

                object_params = random_object_params();
                myobjs = build_scroll_object_row((string)object_params[0], (int)object_params[1], (int)object_params[2], top_of_window - 375, (int)object_params[3], !left_right);
                game_object_list.AddRange(myobjs);

                object_params = random_object_params();
                myobjs = build_scroll_object_row((string)object_params[0], (int)object_params[1], (int)object_params[2], top_of_window - 450, (int)object_params[3], left_right);
                game_object_list.AddRange(myobjs);
                
                //make sure the objects draw on top
                foreach (Scrolling_game_object obj in game_object_list)
                {
                    obj.DrawOrder = 2;
                }
            }
            prev_global_top = globalTopY;
        }

        private List<object> random_object_params()
        {
            int max_speed = 4;

            string mysprite = "small_seal";
            int num_obj = rand.Next(max_speed-3) + 3;
            int myspeed = max_speed + 1 - num_obj;
            int mydmg = 5;

            int sprite_type = rand.Next(20);
            if (sprite_type > 10 & sprite_type <= 16)
            {
                mysprite = "small_walrus";
                mydmg = 10;
            }
            if (sprite_type > 16 & sprite_type <= 20)
            {
                mysprite = "small_whale";
                mydmg = 20;
            }

            List<object> mylist = new List<object>();
            mylist.Add(mysprite);
            mylist.Add(num_obj);
            mylist.Add(myspeed);
            mylist.Add(mydmg);
            return mylist;
        }

        public List<Scrolling_game_object> build_scroll_object_row(string sprite_name, int num_obj_in_row, int speed, float y_coordinate, int damage, bool l_to_r)
        {
            decimal window_width = Game.GraphicsDevice.Viewport.Width; //g.Window.ClientBounds.Right - g.Window.ClientBounds.Left - 50;

            int spacing = (int)(window_width / num_obj_in_row);

            Random rand = new Random();
            int pos = rand.Next(50);

            Scrolling_game_object myobj;
            List<Scrolling_game_object> myobjs = new List<Scrolling_game_object>();

            while (pos < g.Window.ClientBounds.Right - 100)
            {
                myobj = new Scrolling_game_object(g, new Vector2(pos, y_coordinate), sprite_name, speed, l_to_r, penguin, damage);
                pos += spacing;
                myobjs.Add(myobj);
                g.Components.Add(myobj);
            }

            return myobjs;
        }

        #endregion    

        #region Water Collisions
        private int CheckWaterCollisions()
        {
            Rectangle penguinRect = new Rectangle((int) (penguin.Position.X + penguin.penguinOffset.X - penguin.origin.X), (int) (penguin.Position.Y + penguin.penguinOffset.Y - penguin.origin.Y),
                penguin.penguinRealSize.Width, penguin.penguinRealSize.Height);

            for (int i = 0; i < holeLocation.Count; i++)
            {
                Rectangle curRect = new Rectangle((int) holeLocation[i].X*tile.Width, (int) holeLocation[i].Y*tile.Height, tile.Width, tile.Height);
                if (collision.IsCollided(penguinRect, curRect))
                    return i;
            }

            return -1;
        }
        #endregion

        #region Sushi Collision
        private bool CheckSushiCollision()
        {
            Rectangle penRect = new Rectangle((int)(penguin.Position.X + penguin.penguinOffset.X - penguin.origin.X), (int)(penguin.Position.Y + penguin.penguinOffset.Y - penguin.origin.Y),
                penguin.penguinRealSize.Width, penguin.penguinRealSize.Height);

            Rectangle sushiRect = new Rectangle((int)(sushiPos.X - sushiOrigin.X), (int)(sushiPos.Y - sushiOrigin.Y),
                sushiSprite.Width, sushiSprite.Height);

            if (collision.IsCollided(penRect, sushiRect))
                return true;
            else
                return false;
        }
        #endregion

        #region Tile Management

        /// <summary>
        /// Runs every frame.
        /// Clears existing tiles and resets them based on current hole positions.
        /// Needed to ensure that holes are destroyed properly when the world scrolls.
        /// </summary>
        private void UpdateWorld()
        {
            ClearHoles();

            // start with all tiles present
            for (int i = 0; i < verticalTiles + extraWorldRows; i++)
            {
                List<bool> curRow = new List<bool>();
                for (int j = 0; j < horizontalTiles; j++)
                {
                    curRow.Add(true);
                }
                worldTiles.Add(curRow);
            }

            foreach (Vector3 hole in holeLocation)
            {
                worldTiles[(int)hole.Y][(int)hole.X] = false;
            }
        }

        /// <summary>
        /// Runs every frame.
        /// Checks the world for holes that are now off screen and deletes them.
        /// </summary>
        private void ClearHoles()
        {
            for (int i = 0; i < holeLocation.Count; i++)
            {
                if (holeLocation[i].Y > verticalTiles + extraWorldRows)
                {
                    holeLocation.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Runs every frame.
        /// Creates new holes in the ice.  Preferentially enlarges existing holes.
        /// </summary>
        private void UpdateTiles()
        {
            // so with meltRate = 2, this is the time 30 seconds ago
            if (nextMeltTime == 0)
                nextMeltTime = DateTime.Now.AddMinutes(1.0 / meltRate).Ticks;

            // if we are (1/meltRate) seconds past the last melt time, melt a new tile
            if (DateTime.Now.Ticks > nextMeltTime)
            {
                // if we should enlarge an existing hole
                if (rand.NextDouble() < enlargeHoleProb && holeLocation.Count > 0)
                {
                    Vector2 holeLoc = new Vector2();

                    // if there are holes to enlarge
                    if (holeLocation.Count > 0)
                    {
                        // pick the hole to enlarge
                        int holeNum = rand.Next(0, holeLocation.Count - 1);

                        // find a random tile on the surface of the hole
                        holeLoc = new Vector2(holeLocation[holeNum].X, holeLocation[holeNum].Y);

                        // while we're still on a hole
                        while (!worldTiles[(int)holeLoc.Y][(int)holeLoc.X])
                        {
                            int moveDir = rand.Next(0, 3);
                            switch (moveDir)
                            {
                                case 0:
                                    if (holeLoc.X + 1 < worldTiles[0].Count) holeLoc.X++;
                                    break;
                                case 1:
                                    if (holeLoc.Y + 1 < worldTiles.Count) holeLoc.Y++;
                                    break;
                                case 2:
                                    if (holeLoc.X - 1 >= 0) holeLoc.X--;
                                    break;
                                case 3:
                                    if (holeLoc.Y - 1 >= 0) holeLoc.Y--;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    // no hole found to enlarge -- so add a new one
                    else
                    {
                        // pick a random location to add a new hole
                        bool melted = false;
                        while (!melted)
                        {
                            int x = rand.Next(0, worldTiles[0].Count - 1);
                            int y = rand.Next(0, verticalTiles + extraWorldRows);

                            // if the tile is not melted, melt it -- if not, continue the loop and pick a new random location
                            if (worldTiles[y][x])
                            {
                                holeLoc = new Vector2(x, y);
                                melted = true;
                                nextMeltTime = DateTime.Now.AddMinutes(1.0 / meltRate).Ticks;
                            }
                        }
                    }

                    // add to the hole
                    holeLocation.Add(new Vector3(holeLoc.X, holeLoc.Y, 0));
                    nextMeltTime = DateTime.Now.AddMinutes(1.0 / meltRate).Ticks;
                }
                else
                {
                    // pick a random location to add a new hole
                    bool melted = false;
                    while (!melted)
                    {
                        int x = rand.Next(0, worldTiles[0].Count - 1);
                        int y = rand.Next(0, verticalTiles+extraWorldRows);

                        // if the tile is not melted, melt it -- if not, continue the loop and pick a new random location
                        if (worldTiles[y][x])
                        {
                            holeLocation.Add(new Vector3(x, y, 0));
                            melted = true;
                            nextMeltTime = DateTime.Now.AddMinutes(1.0 / meltRate).Ticks;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
