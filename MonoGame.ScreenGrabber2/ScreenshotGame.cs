using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading;
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
        bool EscapeWasReleased = true;

        float CurrentSpeed = 0;

        public ScreenshotGame(IScreenshooter screenshooter)
        {
            System.Console.WriteLine(this.Window.GetType().FullName);

            System.Environment.Exit(0);

            IsMouseVisible = true;
            Window.IsBorderless = true;
            ScreenshotTaker = screenshooter;
            InactiveSleepTime = System.TimeSpan.Zero;

            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }
        
        public void Hide()
        {
            Sdl.Window.Hide(Window.Handle);
        }

        public void Show()
        {
            Sdl.Window.Show(Window.Handle);
        }

        public void MoveToMouse()
        {
            int x, y;
            Sdl.Mouse.GetGlobalState(out x, out y);
            Sdl.Window.SetSize(Window.Handle, 50, 50);

            var s = Forms.Screen.FromPoint(new System.Drawing.Point(x, y));
            this.Window.Position = new Point(x, y);
            
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GrabScreenshot();
        }

        private void GrabScreenshot()
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.ApplyChanges();

            int width, height;
            byte[] data = ScreenshotTaker.GetScreenshotAsBGRA(Window.Position.X, Window.Position.Y, out width, out height);

            if (ScreenshotTexture != null)
                ScreenshotTexture.Dispose();
            
            ScreenshotTexture = new Texture2D(GraphicsDevice, width, height);
            ScreenshotTexture.SetData(data);
        }

        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (EscapeWasReleased)
                {
                    Hide();
                    Thread.Sleep(2000);
                    MoveToMouse();
                    Show();
                    GrabScreenshot();
                }
                EscapeWasReleased = false;

            }
            else if (!EscapeWasReleased)
            {
                EscapeWasReleased = true;
            }

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


            while (Mouse.GetState().RightButton == ButtonState.Pressed)
                Thread.Sleep(100);
            Program.StartWatch.Stop();
        }
    }
}
