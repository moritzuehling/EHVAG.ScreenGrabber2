using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Forms = System.Windows.Forms;

namespace MonoGame.ScreenGrabber2
{
    public class ScreenshotGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D ScreenshotTexture;
        Texture2D WhiteTexture;
        IScreenshooter ScreenshotTaker;

        Vector2 SelectStart;
        Vector2 SelectEnd;

        bool IsSelecting;
        bool EscapeWasReleased = false;

        bool isRunning = false;

        float CurrentSpeed = 0;

        float Whiteness = 1;

        IHotkeyInterface HotkeyManager;

        public ScreenshotGame(IScreenshooter screenshooter, IHotkeyInterface hotkeyInterface)
        {
            IsMouseVisible = true;
            Window.IsBorderless = true;
            ScreenshotTaker = screenshooter;
            InactiveSleepTime = System.TimeSpan.Zero;

            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            HotkeyManager = hotkeyInterface;
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
            Sdl.Window.SetSize(Window.Handle, 5, 5);

            Sdl.Rectangle rect;
            Window.Position = new Point(x, y);
            
            Sdl.Display.GetBounds(Sdl.Window.GetDisplayIndex(Window.Handle), out rect);
            Window.Position = new Point(rect.X, rect.Y);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            WhiteTexture = new Texture2D(GraphicsDevice, 32, 32);
            WhiteTexture.SetData<Color>(Enumerable.Repeat(Color.White, 32 * 32).ToArray());
            // GrabScreenshot();
        }

        private void GrabScreenshot()
        {
            System.Console.WriteLine("Grabbing screenshot");

            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.ApplyChanges();

            int width, height;
            byte[] data = ScreenshotTaker.GetScreenshotAsBGRA(Window.Position.X, Window.Position.Y, out width, out height);

            if (ScreenshotTexture != null)
            {
                ScreenshotTexture.Dispose();
                ScreenshotTexture = null;
            }
            
            ScreenshotTexture = new Texture2D(GraphicsDevice, width, height);
            ScreenshotTexture.SetData(data);
        }

        protected override void UnloadContent()
        {
        }
        
        protected override void Update(GameTime gameTime)
        {
            var kbState = Keyboard.GetState();

            if (EscapeWasReleased && kbState.IsKeyDown(Keys.Escape))
                isRunning = false;

            if (kbState.IsKeyUp(Keys.Escape))
                EscapeWasReleased = true;

            if (!isRunning)
            {
                Hide();
                HotkeyManager.WaitFor();
                MoveToMouse();
                GrabScreenshot();
                Show();
                isRunning = true;
                EscapeWasReleased = false;
                Whiteness = 1;

                IsSelecting = false;
                SelectStart = SelectEnd = Vector2.Zero;
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

            Whiteness -= (float)gameTime.ElapsedGameTime.TotalSeconds * 1.2f;

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            if (ScreenshotTexture == null)
            {
                GraphicsDevice.Clear(Color.White);
                base.Draw(gameTime);
                return;
            }

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

            spriteBatch.Draw(WhiteTexture, GraphicsDevice.Viewport.Bounds, new Color(Color.White, Whiteness));

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
