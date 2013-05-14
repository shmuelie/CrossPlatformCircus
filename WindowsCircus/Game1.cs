using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace CrossPlatformCircus
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private Rectangle screenSize;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D trampolineTexture;
        private Vector2 trampolinePosition;

        private Texture2D backgroundTexture;
        private Animation backgroundAnimation;

        private Texture2D clownSpriteSheet;
        private Animation clownAnimation;
        private Rectangle clownArea;
        private Vector2 clownVelocity;

        private Song caliope;

        private SoundEffect bounce;

        private Texture2D balloonTexture;
        private List<Balloon> balloons = new List<Balloon>();
        private int score = 0;
#if !WINRT
        private SoundEffect pop;
#endif

        private SpriteFont scoreFont;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE_7
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if WINRT
            screenSize = new Rectangle(0, 0, 800, 480);
#else
            screenSize = GraphicsDevice.Viewport.Bounds;
#endif

            SetupBalloons();

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

            backgroundTexture = Content.Load<Texture2D>("Background");
            backgroundAnimation = new Animation(backgroundTexture, 500, 640, 480);

            trampolineTexture = Content.Load<Texture2D>("Trampoline");
            trampolinePosition = new Vector2(100.0f, (float)(screenSize.Height - trampolineTexture.Height));

            //caliope = Content.Load<Song>("Caliope");

            clownSpriteSheet = Content.Load<Texture2D>("clownSpriteSheet");
            clownArea = new Rectangle(100, 100, 58, 64);
            clownAnimation = new Animation(clownSpriteSheet, 200, clownArea.Width, clownArea.Height);
            clownVelocity = Vector2.Zero;

            scoreFont = Content.Load<SpriteFont>("scoreFont");

            balloonTexture = Content.Load<Texture2D>("balloon");
#if !WINRT
            pop = Content.Load<SoundEffect>("pop");
#endif

            bounce = Content.Load<SoundEffect>("bounce");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            if (MediaPlayer.State != MediaState.Playing)
            {
                //MediaPlayer.Play(caliope);
            }

#if WINRT
            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                Exit();
            }
#endif

            // Move the sprite by speed, scaled by elapsed time.
            Vector2 newPosition = (new Vector2(clownArea.X, clownArea.Y)) + clownVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            clownArea.X = (int)Math.Round(newPosition.X);
            clownArea.Y = (int)Math.Round(newPosition.Y);

            Point locationPoint = Point.Zero;
            MouseState mouseState = Mouse.GetState();

#if WINRT
            TouchCollection touchPoints = TouchPanel.GetState();
            if (touchPoints.Count > 0)
            {
                locationPoint.X = (int)touchPoints[0].Position.X;
            }
            else
            {
#endif
               locationPoint.X = mouseState.X;
#if WINRT
            }
#endif

            trampolinePosition.X = 
#if WINRT
            ((float)locationPoint.X * ((float)screenSize.Width / (float)GraphicsDevice.Viewport.Width))
#else
            mouseState.X
#endif
            - (trampolineTexture.Width / 2);

            ResolveViewpotCollisions();

            TrampolineCollision();

            clownArea = UpdateBalloons(gameTime, clownArea);

            backgroundAnimation.Update(gameTime);
            clownAnimation.Update(gameTime);

            // drag the clown down by adding to velocity (gravity)
            clownVelocity.Y += 15.0f;

            base.Update(gameTime);
        }

        private void TrampolineCollision()
        {
            Rectangle trampolineRectangle = new Rectangle((int)trampolinePosition.X, (int)trampolinePosition.Y,
                trampolineTexture.Width, trampolineTexture.Height);

            if (trampolineRectangle.Intersects(clownArea))
            {
                // bounce the clown here

                // calculate bounce based on distance from clown center to trampoline center
                int angle = clownArea.Center.X - trampolineRectangle.Center.X + 90;

                float newY = 800 * (float)Math.Sin(MathHelper.ToRadians((float)(angle)));
                float newX = 800 * (float)Math.Cos(MathHelper.ToRadians((float)(angle)));
                clownVelocity.X = newX;
                clownVelocity.Y = newY;

                clownVelocity.Y *= -1;
                clownArea.Y = (int)Math.Round(trampolinePosition.Y - clownArea.Height);

                bounce.Play();
            }
        }

        private Rectangle UpdateBalloons(GameTime gameTime, Rectangle clownRectangle)
        {
            for (int i = 0; i < balloons.Count; i++)
            {
                if (clownRectangle.Intersects(balloons[i].BoundingBox))
                {
                    score += balloons[i].Color * 10 + 10;
                    balloons.RemoveAt(i);
                    clownVelocity.X *= -1;
                    clownVelocity.Y *= -1;

#if !WINRT
                    pop.Play();
#endif

                    // you can only kill off one balloon per frame
                    // this is to prevent problems with our List index
                    // after calling RemoveAt()
                    break;
                }
            }

            Balloon.UpdateAnimation(gameTime);
            return clownRectangle;
        }

        private void ResolveViewpotCollisions()
        {
            // Calculate viewport extents, but subtract width and height of clown texture
            // since we track the clown by its upper left anchor point.
            int MaxX = screenSize.Width - clownArea.Width;
            int MinX = 0;
            int MaxY = screenSize.Height - clownArea.Height;
            int MinY = 0;

            if (clownArea.X > MaxX)
            {
                // We've hit the right side, reverse to create a bounce
                clownVelocity.X *= -1;
                clownArea.X = MaxX;
            }
            else if (clownArea.X < MinX)
            {
                // We've hit the left side, reverse to create a bounce
                clownVelocity.X *= -1;
                clownArea.X = MinX;
            }

            if (clownArea.Y > MaxY)
            {
                // the clown has hit the ground , force a "soft" 90 degree bounce
                float newY = 515 * (float)Math.Sin(MathHelper.ToRadians(90.0f));
                float newX = 515 * (float)Math.Cos(MathHelper.ToRadians(90.0f));
                clownVelocity.X = newX;
                clownVelocity.Y = newY;

                clownVelocity.Y *= -1;
                clownArea.Y = MaxY;

                // And lose 50 points
                score = Math.Max(0, score - 50);
            }
            else if (clownArea.Y < MinY)
            {
                // We've hit the top, reverse to create a bounce
                clownVelocity.Y *= -1;
                clownArea.Y = MinY;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

#if WINRT
            RenderTarget2D tempTarget = new RenderTarget2D(GraphicsDevice, 800, 480);
            GraphicsDevice.SetRenderTarget(tempTarget);
#endif

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, screenSize, backgroundAnimation.CurrentFrame, Color.White);
            spriteBatch.Draw(clownSpriteSheet, new Vector2(clownArea.X, clownArea.Y), clownAnimation.CurrentFrame, Color.White);
            spriteBatch.Draw(trampolineTexture, trampolinePosition, Color.White);
            spriteBatch.DrawString(scoreFont, "Score: " + score, new Vector2(550.0f, 150.0f), Color.DarkBlue);

            foreach (Balloon b in balloons)
            {
                spriteBatch.Draw(balloonTexture,
                    b.Position,
                    new Rectangle(Balloon.State * 32, b.Color * 32, 32, 32),
                    Color.White);
            }
      

            spriteBatch.End();
#if WINRT
            GraphicsDevice.SetRenderTarget(null);
            
            spriteBatch.Begin();

            spriteBatch.Draw(tempTarget, GraphicsDevice.Viewport.Bounds, tempTarget.Bounds, Color.White);

            spriteBatch.End();
#endif

            base.Draw(gameTime);
        }


        private void SetupBalloons()
        {
            balloons.Clear();

            int rows = 3;
            int columns = 21;
            int columnSpacing = 36;
            int rowSpacing = 40;
            int leftMargin = 20;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    balloons.Add(new Balloon { Color = i, Position = new Vector2((j * columnSpacing) + leftMargin, i * rowSpacing) });
                }
            }
        }
      
    }
}