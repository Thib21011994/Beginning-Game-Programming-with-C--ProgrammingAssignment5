using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TeddyMineExplosion;

namespace ProgrammingAssignment5
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // window resolution
        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // teddy velocity boundaries
        const double minVelocity = -0.5;
        const double maxVelocity = 0.5;

        // mine support
        Texture2D mineSprite;
        List<Mine> mines = new List<Mine>();

        // teddy support
        Texture2D teddySprite;
        List<TeddyBear> teddies = new List<TeddyBear>();

        // explosion support
        Texture2D explosionSprite;
        List<Explosion> explosions = new List<Explosion>();

        // click processing
        bool leftClickStarted = false;
        bool leftButtonReleased = true;
        
        // timer support
        int TotalSpawnDelayMilliseconds;
        int elapsedSpawnDelayMilliseconds = 0;

        // random number generator
        Random rand = new Random();
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and make mouse visible
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            // TODO: use this.Content to load your game content here

            // load sprites
            mineSprite = Content.Load<Texture2D>(@"bin\Windows\Graphics\mine");
            teddySprite = Content.Load<Texture2D>(@"bin\Windows\Graphics\teddybear");
            explosionSprite = Content.Load<Texture2D>(@"bin\Windows\Graphics\explosion");

            // set initial spawn delay
            TotalSpawnDelayMilliseconds = rand.Next(1000, 3000);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // get current mouse state
            MouseState mouse = Mouse.GetState();

            // check for left click started
            if (mouse.LeftButton == ButtonState.Pressed && leftButtonReleased)
            {
                leftClickStarted = true;
                leftButtonReleased = false;
            }
            else if (mouse.LeftButton == ButtonState.Released)
            {
                leftButtonReleased = true;

                // if left click finished, add new mine to list of mines
                if (leftClickStarted)
                {
                    leftClickStarted = false;

                    // add a new mine to the end of the list of mines
                    Vector2 mineLocation;
                    mineLocation.X = mouse.X;
                    mineLocation.Y = mouse.Y;
                    Mine newMine = new Mine(mineSprite, (int)mineLocation.X, (int)mineLocation.Y);
                    mines.Add(newMine);
                }
            }

            // spawn teddies at random time
            elapsedSpawnDelayMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
            if (elapsedSpawnDelayMilliseconds >= TotalSpawnDelayMilliseconds)
            {
                elapsedSpawnDelayMilliseconds = 0;
                TotalSpawnDelayMilliseconds = rand.Next(1000, 3000);

                // add new teddy to the list of teddies
                Vector2 randomVelocity;
                randomVelocity.X = (float)(rand.NextDouble() * (maxVelocity - minVelocity) + minVelocity);                
                randomVelocity.Y = (float)(rand.NextDouble() * (maxVelocity - minVelocity) + minVelocity);
                TeddyBear newTeddy = new TeddyBear(teddySprite, randomVelocity, WindowWidth, WindowHeight);
                teddies.Add(newTeddy);
            }

            // update each teddy bear in the list of teddies
            foreach (TeddyBear teddy in teddies)
            {
                teddy.Update(gameTime);

                // check for collisions between teddies and mines
                foreach (Mine mine in mines)
                {
                    if (teddy.CollisionRectangle.Intersects(mine.CollisionRectangle))
                    {
                        teddy.Active = false;
                        mine.Active = false;
                        explosions.Add(new Explosion(explosionSprite, 
                            teddy.CollisionRectangle.Center.X, 
                            teddy.CollisionRectangle.Center.Y));
                    }
                }                                
            }

            // update each explosion in the list of explosions
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // remove inactive mines
            for (int i = mines.Count - 1; i >= 0; i--)
            {
                if (!mines[i].Active)
                {
                    mines.RemoveAt(i);
                }
            }

            // remove inactive teddies
            for (int i = teddies.Count - 1; i >= 0; i--)
            {
                if (!teddies[i].Active)
                {
                    teddies.RemoveAt(i);
                }
            }

            // remove inactive explosions
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                if (!explosions[i].Playing)
                {
                    explosions.RemoveAt(i);
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin();
            foreach (Mine mine in mines)
            {
                mine.Draw(spriteBatch);
            }
            foreach (TeddyBear teddy in teddies)
            {
                teddy.Draw(spriteBatch);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
