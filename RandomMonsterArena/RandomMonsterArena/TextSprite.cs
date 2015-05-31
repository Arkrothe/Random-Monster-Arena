using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public class TextSprite
    {
        #region Data members
        private SpriteFont spriteFont;
        private Vector2 location = Vector2.Zero;
        public string text;
        private Color tintColor;
        private float scalingFactor;
        private float layerDepth;

        #endregion

        #region Methods
        public TextSprite(SpriteFont spriteFont, Vector2 location, string text, Color color, float size, float layer)
        {
            this.spriteFont = spriteFont;
            this.location = location;
            this.text = text;
            tintColor = color;
            scalingFactor = size;
            layerDepth = layer;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(spriteFont, text, location, tintColor, 0f, Vector2.Zero, scalingFactor, SpriteEffects.None, layerDepth);
        }
        #endregion
    }
}
