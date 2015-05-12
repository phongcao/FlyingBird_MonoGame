using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.IO;

namespace FlyingBird_MonoGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        // Defines
        const int k_iBirdStartX         = 50;
        const int k_iBirdStartY         = 100;
        const int k_iBirdAnim           = 10;
        const int k_iMapScrollSpeed     = 10;
        const int k_iNumActivePipes     = 20;
        const int k_iGravity            = 5;
        const int k_iLiftForce          = k_iGravity << 1;

        // Global
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Random rnd = new Random(DateTime.Now.Millisecond);
        long m_lFrameCounter;
        bool m_isGameOver;

        // Textures
        Texture2D m_birdTexture;
        Texture2D m_pipeTexture;
        Texture2D m_skylineTexture;
        Texture2D m_groundTexture;

        // Sprites
        GameObject m_sprBird;
        GameObject m_sprSkyline;
        GameObject m_sprGround;
        GameObject[] m_sprPipe = new GameObject[k_iNumActivePipes];

        // Map
        int m_iMapScrollX;
        int m_iLastPipeX;

        public Game()
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
            // TODO: Add your initialization logic here
            m_lFrameCounter = 0;
            m_iMapScrollX = 0;
            m_iLastPipeX = GraphicsDevice.Viewport.Width;
            m_isGameOver = false;

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
            m_birdTexture = LoadTexture2D(Content.RootDirectory + "/bird.png");
            m_pipeTexture = LoadTexture2D(Content.RootDirectory + "/pipe.png");
            m_skylineTexture = LoadTexture2D(Content.RootDirectory + "/skyline.png");
            m_groundTexture = LoadTexture2D(Content.RootDirectory + "/ground.png");

            m_sprBird = new GameObject(m_birdTexture, 0, 0, m_birdTexture.Width / 2, m_birdTexture.Height, true);
            m_sprBird.SetPosition(k_iBirdStartX, k_iBirdStartY);
            m_sprSkyline = new GameObject(m_skylineTexture, 0, 0, 0, 0, true);
            m_sprGround = new GameObject(m_groundTexture, 0, 0, 0, 0, true);
            InitPipes();
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
            // TODO: Add your update logic here

            // Check for game over
            if (m_isGameOver)
            {
                // Update gravity
                m_sprBird.m_pos.Y += k_iGravity << 1;

                // Replay
                if (m_sprBird.m_pos.Y >= GraphicsDevice.Viewport.Height)
                {
                    m_iMapScrollX = 0;
                    m_iLastPipeX = GraphicsDevice.Viewport.Width;
                    m_sprBird.SetPosition(k_iBirdStartX, k_iBirdStartY);
                    InitPipes();
                    m_isGameOver = false;
                }
            }
            else
            {
                // Update Map
                ScrollMap();

                // Update pipes
                UpdatePipes(m_iMapScrollX);

                // Update gravity
                m_sprBird.m_pos.Y += k_iGravity;

                // Handle player's input
                ProcessInput();

                // Handle collisions
                CheckCollisions();

                // Update bird's anim
                m_sprBird.m_iCurrentFrame = (m_lFrameCounter % k_iMapScrollSpeed < (k_iMapScrollSpeed >> 1)) ? 0 : 1;
            }

            // Frame counter
            m_lFrameCounter++;

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
            RenderMap(spriteBatch, m_iMapScrollX);
            RenderPipes(spriteBatch, m_iMapScrollX);
            m_sprBird.Render(spriteBatch);

            base.Draw(gameTime);
        }

        protected Texture2D LoadTexture2D(string file)
        {
            try
            {
                Stream fs = TitleContainer.OpenStream(file);
                Texture2D texture = Texture2D.FromStream(GraphicsDevice, fs);
                fs.Dispose();
                return texture;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected void RenderMap(SpriteBatch spriteBatch, int scrollX)
        {
            int windowsHeight = GraphicsDevice.Viewport.Height;
            int windowsWidth = GraphicsDevice.Viewport.Width;

            // Render ground
            int groundX = -scrollX;
            int groundY = windowsHeight - m_groundTexture.Height;

            do
            {
                if (groundX + m_groundTexture.Width > 0)
                {
                    m_sprGround.SetPosition(groundX, groundY);
                    m_sprGround.Render(spriteBatch);
                }

                groundX += m_groundTexture.Width;
            }
            while (groundX < windowsWidth);

            // Render skyline
            int skylineX = -scrollX;
            int skylineY = windowsHeight - m_groundTexture.Height - m_skylineTexture.Height;

            do
            {
                if (skylineX + m_skylineTexture.Width > 0)
                {
                    m_sprSkyline.SetPosition(skylineX, skylineY);
                    m_sprSkyline.Render(spriteBatch);
                }

                skylineX += m_skylineTexture.Width;
            }
            while (skylineX < windowsWidth);
        }

        protected void ScrollMap()
        {
            m_iMapScrollX += k_iMapScrollSpeed;
        }

        protected GameObject GeneratePipe(int startX)
        {
            // Random pipe type
            int frameId = rnd.Next(2);

            // Random pipe length
            int height = m_pipeTexture.Height - rnd.Next(m_pipeTexture.Height >> 1);
            height = (height > (GraphicsDevice.Viewport.Height - m_groundTexture.Height - (m_birdTexture.Height << 1)) ? (m_pipeTexture.Height >> 1) : height);

            // Random distance between two nearby pipes
            int x = startX + rnd.Next(1, k_iNumActivePipes >> 2) * m_pipeTexture.Width;

            GameObject pipe = new GameObject(m_pipeTexture, 0, (frameId == 0) ? 0 : (m_pipeTexture.Height - height), m_pipeTexture.Width >> 1, height, true);
            pipe.m_iCurrentFrame = frameId;
            pipe.SetPosition(x, 0);

            return pipe;
        }

        protected void InitPipes()
        {
            for (int i = 0; i < k_iNumActivePipes; i++)
            {
                m_sprPipe[i] = GeneratePipe(m_iLastPipeX);
                m_iLastPipeX = (int)(m_sprPipe[i].m_pos.X + m_pipeTexture.Width);
            }
        }

        protected void UpdatePipes(int scrollX)
        {
            for (int i = 0; i < k_iNumActivePipes; i++)
            {
                if (!m_sprPipe[i].m_isActive)
                {
                    m_sprPipe[i] = GeneratePipe(m_iLastPipeX);
                    m_iLastPipeX = (int)(m_sprPipe[i].m_pos.X + m_pipeTexture.Width);
                }
                else
                {
                    int x = (int)m_sprPipe[i].m_pos.X;
                    int y = (m_sprPipe[i].m_iCurrentFrame == 0) ? (GraphicsDevice.Viewport.Height - m_groundTexture.Height - m_sprPipe[i].m_frameInfo.Height) : 0;

                    if (x + m_sprPipe[i].m_frameInfo.Width - scrollX <= 0)
                    {
                        m_sprPipe[i].m_isActive = false;
                    }
                    else
                    {
                        m_sprPipe[i].SetPosition(x, y);
                    }
                }
            }
        }

        protected void RenderPipes(SpriteBatch spriteBatch, int scrollX)
        {
            for (int i = 0; i < k_iNumActivePipes; i++)
            {
                m_sprPipe[i].Render(spriteBatch, scrollX);
            }
        }

        protected void ProcessInput()
        {
            // Handle touch input
            TouchCollection touchCollection = TouchPanel.GetState();

            foreach (TouchLocation tl in touchCollection)
            {
                if (tl.State == TouchLocationState.Moved)
                {
                    m_sprBird.m_pos.Y -= k_iLiftForce;
                    return;
                }
            }

        #if WINDOWS_UAP
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                m_sprBird.m_pos.Y -= k_iLiftForce;
            }
        #endif // WINDOWS_UAP
        }

        protected void CheckCollisions()
        {
            Rectangle birdRect = new Rectangle((int)m_sprBird.m_pos.X, (int)m_sprBird.m_pos.Y, m_sprBird.m_frameInfo.Width, m_sprBird.m_frameInfo.Height);

            // Check ground collision
            if (birdRect.Y + birdRect.Height >= GraphicsDevice.Viewport.Height - m_groundTexture.Height)
            {
                m_isGameOver = true;
                return;
            }

            // Check pipes collision
            for (int i = 0; i < k_iNumActivePipes; i++)
            {
                if (m_sprPipe[i].m_isActive)
                {
                    Rectangle pipeRect = new Rectangle((int)m_sprPipe[i].m_pos.X - m_iMapScrollX, (int)m_sprPipe[i].m_pos.Y, m_sprPipe[i].m_frameInfo.Width, m_sprPipe[i].m_frameInfo.Height);                 

                    // Collision occurs
                    if (pipeRect.Intersects(birdRect))
                    {
                        m_isGameOver = true;
                    }
                }
            }
        }
    }
}
