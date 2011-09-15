using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProveMotoreFisico
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
    }
}
