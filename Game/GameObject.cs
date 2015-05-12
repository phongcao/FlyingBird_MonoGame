using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlyingBird_MonoGame
{
    public class GameObject
    {
        private Texture2D m_pTexture;
        private Rectangle m_sourceRect;
        public Rectangle m_frameInfo;
        public Vector2 m_pos;
        public bool m_isActive;
        public int m_iCurrentFrame;
        public int m_iTotalFrame;

        public GameObject(Texture2D texture, int frameX = 0, int frameY = 0, int frameWidth = 0, int frameHeight = 0, bool active = false)
        {
            m_pTexture = texture;
            m_frameInfo = new Rectangle(frameX, frameY, frameWidth, frameHeight);

            if (frameWidth == 0 || frameHeight == 0)
            {
                m_frameInfo.Width = texture.Width;
                m_frameInfo.Height = texture.Height;
            }

            m_sourceRect = m_frameInfo;
            m_iTotalFrame = texture.Width / m_frameInfo.Width;
            m_iCurrentFrame = 0;
            m_pos = new Vector2(0, 0);
            m_isActive = active;
        }

        public void SetPosition(int x, int y)
        {
            m_pos.X = x;
            m_pos.Y = y;
        }

        public void Render(SpriteBatch spriteBatch, int scrollX = 0)
        {
            if (!m_isActive)
            {
                return;
            }

            Vector2 pos = new Vector2(m_pos.X - scrollX, m_pos.Y);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            m_sourceRect.X = (int)(m_iCurrentFrame * m_frameInfo.Width);
            spriteBatch.Draw(m_pTexture, pos, m_sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
    }
}
