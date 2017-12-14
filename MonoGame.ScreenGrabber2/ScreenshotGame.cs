using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Forms = System.Windows.Forms;

namespace MonoGame.ScreenGrabber2
{
    public class ScreenshotGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D ScreenshotTexture;
        IScreenshooter ScreenshotTaker;

        Vector2 SelectStart;
        Vector2 SelectEnd;

        bool IsSelecting;

        float CurrentSpeed = 0;

        public ScreenshotGame(IScreenshooter screenshooter)
        {
            IsMouseVisible = true;
            Window.IsBorderless = true;
            ScreenshotTaker = screenshooter;

            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;

            int width, height;
            byte[] data = ScreenshotTaker.GetScreenshotAsBGRA(Window.Position.X, Window.Position.Y, out width, out height);

            ScreenshotTexture = new Texture2D(GraphicsDevice, width, height);
            ScreenshotTexture.SetData(data);
            
            graphics.ApplyChanges();
        }

        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mstate = Mouse.GetState();

            if (mstate.LeftButton == ButtonState.Pressed)
            {
                if (!IsSelecting)
                {
                    IsSelecting = true;
                    SelectStart = SelectEnd = new Vector2(mstate.X, mstate.Y);
                }
                else
                {
                    var newVec = new Vector2(mstate.X, mstate.Y);
                    CurrentSpeed += (SelectEnd - newVec).Length();
                    SelectEnd = newVec;
                }
            }
            else
            {
                IsSelecting = false;
            }

            CurrentSpeed *= .85f;
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            spriteBatch.Draw(ScreenshotTexture, Vector2.Zero, new Color(Color.White, .5f));

            if ((SelectStart - SelectEnd).LengthSquared() > 2)
            {
                var topLeft = Vector2.Min(SelectStart, SelectEnd);
                var bottomRight = Vector2.Max(SelectStart, SelectEnd);
                var size = bottomRight - topLeft;

                var currentAlpha = MathHelper.Max(.5f, 1 - (CurrentSpeed * .003f));

                spriteBatch.Draw(ScreenshotTexture, topLeft, new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y), Color.CornflowerBlue);
                spriteBatch.Draw(ScreenshotTexture, topLeft, new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y), new Color(Color.White, currentAlpha));
            }


            spriteBatch.End();

            base.Draw(gameTime);

            Program.StartWatch.Stop();
        }
    }
}
