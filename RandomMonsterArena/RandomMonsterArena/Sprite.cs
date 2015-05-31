using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RandomMonsterArena
{
    public class Sprite
    {
        #region Data members

        private Texture2D texture;
        
        private Rectangle               sourceRect;
        private float                   scalingFactor = 1.0f;               
        private float                   rotation = 0.0f;
        private float                   layerDepth;
        public bool                     isIsometric = true;
        public Color                    tintColor = Color.White;        

        private Vector2 location = Vector2.Zero;
        private float zOffset;

        #endregion

        #region Properties

        private int width
        {
            get { return (int)(sourceRect.Width); }
        }

        private int height
        {
            get { return (int)(sourceRect.Height); }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public float ScalingFactor
        {
            get { return scalingFactor; }
            set { scalingFactor = value; }
        }

        /*/// <summary>
        /// Actual corner location that is offsetted by the scaling factor. Do not use for assigning to a variable.
        /// </summary>
        private Vector2 OffsetLocation
        {
            get
            {
                return new Vector2(
                    location.X + (width * (1 - scalingFactor) / 2),
                    location.Y + (height * (1 - scalingFactor) / 2));
            }
        }*/

        /// <summary>
        /// Center of sprite in relative terms, remains the same regardless of the scaling factor or X-Y plane.
        /// </summary>
        private Vector2 RelativeCenter
        {
            get { return new Vector2(width / 2, height / 2); }
        }
        
        /// <summary>
        /// Origin location of the sprite.
        /// </summary>
        public Vector2 Location
        {
            get
            {
                return location + RelativeCenter;
                //return new Vector2(location.X + (width * (1 - scalingFactor) / 2),
                //  location.Y + (height * (1 - scalingFactor) / 2));
            }
            set
            {
                location = value - RelativeCenter;
            }
        }

        /// <summary>
        /// Gives the isometric location of the sprite.
        /// </summary>
        public Vector2 IsometricLocation
        {
            get
            {
                return new Vector2(
                    Location.X - Location.Y,
                    ((Location.X + Location.Y) / 2));
            }
        }

        /*/// <summary>
        /// Rectangle where the actual sprite will be present in the world. Do not use for assigning to a variable.
        /// </summary>
        private Rectangle ScaledDestRect
        {
            get
            {
                return new Rectangle(
                    (int)location.X + (int)(width * (1 - scalingFactor) / 2),
                    (int)location.Y + (int)(height * (1 - scalingFactor) / 2),
                    (int)(width * scalingFactor),
                    (int)(height * scalingFactor));
            }
        }*/

        /// <summary>
        /// Scaled Isometric Destination Rectangle
        /// </summary>
        public Rectangle ScaledIsoDestRec
        {
            get
            {
                return new Rectangle(
                    (int)IsometricLocation.X - (int)((width * scalingFactor) / 2),
                    (int)IsometricLocation.Y - (int)((height * scalingFactor) / 2),
                    (int)(width * scalingFactor),
                    (int)(height * scalingFactor));
            }
        }
        #endregion

        #region public methods

        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="texture">Texture to be used by the sprite</param>
        /// <param name="location">World location of the unscaled sprite</param>
        /// <param name="velocity">Velocity of the sprite</param>
        /// <param name="sourceRect">Rectangle of the source frame in the texture for the sprite</param>
        /// <param name="layer">Layer drawing depth for the sprite</param>
        /// <param name="scaling">Scaling factor for the sprite</param>
        public Sprite(Texture2D texture, Rectangle sourceRect, Vector2 location, float layer, float zOffset)
        {
            this.texture = texture;
            this.sourceRect = sourceRect;
            this.layerDepth = layer;
            this.zOffset = zOffset;
            this.Location = location;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="sprite"></param>
        public Sprite(Sprite sprite)
        {
            this.texture = sprite.texture;
            this.sourceRect = sprite.sourceRect;
            this.layerDepth = sprite.layerDepth;
            this.zOffset = sprite.zOffset;
            this.Location = sprite.Location;
        }
        
        /// <summary>
        /// Drawing function responsible for drawing the sprite.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            /*this overload of draw takes the dimensions of the original frame (w,h) and goes to location + relative center,  
             * and refers that to the relative center of the frame's image to draw the scaled image. 
             * So the scaled image vs the actual image when drawn on the same location, will have same centers but different 
             * "location" offsets. To get the offset to location where the scaled down image will be drawn use formula
             * offset = ( w * ( 1 - s ) / 2 ) where w is width/height, s is the scaling.
             */
            Vector2 drawLoc = Location;
            if (isIsometric)
            {
                drawLoc = new Vector2(IsometricLocation.X, IsometricLocation.Y - zOffset);
            }
            spriteBatch.Draw(
                texture,
                drawLoc,
                sourceRect,
                tintColor,
                rotation,
                RelativeCenter,
                scalingFactor,
                SpriteEffects.None,
                layerDepth);
        }
        #endregion
    }
}
