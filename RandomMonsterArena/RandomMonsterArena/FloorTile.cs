using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public class FloorTile
    {
        public Sprite sprite;
        private int tileNo;
        public bool walkable;
        public Diceman occupyingDiceman;
        public float isoLengthBy2 = 16;

        private Rectangle GetSourceRectForTileNo(int tileNo)
        {
            Rectangle sourceRect = new Rectangle();
            sourceRect.X = (tileNo % 4) * 64;
            sourceRect.Y = (tileNo / 4) * 32;
            sourceRect.Width = 64;
            sourceRect.Height = 32;
            return sourceRect;
        }

        public FloorTile(Texture2D floorTileSet, int tileNo, Vector2 location)
        {
            sprite = new Sprite(floorTileSet, GetSourceRectForTileNo(tileNo), location, Constant.l_floorTile, 0f);
            this.tileNo = tileNo;
            walkable = true;
            occupyingDiceman = null;
        }

        public void HighlightTile()
        {
            sprite.tintColor = Color.CornflowerBlue;
        }

        public void UnhighlightTile()
        {
            sprite.tintColor = Color.White;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }
    }
}
