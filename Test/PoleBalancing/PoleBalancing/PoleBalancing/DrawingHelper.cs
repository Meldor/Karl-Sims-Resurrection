using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PoleBalancing
{
    class DrawingHelper
    {
        public static void DrawLine(SpriteBatch spriteBatch, Texture2D rectTexture, Vector2 start, Vector2 end, Color color)
        {
            Vector2 delta = end - start;
            float rotation = (float)Math.Atan2(delta.Y, delta.X);
            float lenght = delta.Length();
            spriteBatch.Draw(rectTexture, start, new Rectangle(0, 0, 1, 1), color, rotation, Vector2.Zero, new Vector2(lenght, 1.0f), SpriteEffects.None, 1);
        }

        public static void DrawPoint(SpriteBatch spriteBatch, Texture2D rectTexture, Vector2 position, int size, Color color)
        {
            spriteBatch.Draw(rectTexture, position, new Rectangle(0, 0, size, size), color, 0f, new Vector2(size / 2, size / 2), 1f, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Genera una texture rettangolare
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice (da graphics.GraphicsDevice)</param>
        /// <param name="width">Larghezza della texture</param>
        /// <param name="height">ALtezza della texture</param>
        /// <param name="color">Colore della texture</param>
        /// <returns>Texture</returns>
        public static Texture2D RectangularTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            int totPixel = width * height;
            Texture2D texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
            Color[] colorArray = new Color[totPixel];

            for (int i = 0; i < totPixel; i++)
                colorArray[i] = color;
            texture.SetData(colorArray);

            return texture;
        }
    }
}
