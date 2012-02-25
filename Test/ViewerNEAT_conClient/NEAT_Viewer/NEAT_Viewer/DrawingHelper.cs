using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NEAT_Viewer
{
    class DrawingHelper
    {
        #region DrawLine
        public static float DrawLine(SpriteBatch spriteBatch, Texture2D rectTexture, Vector2 start, Vector2 end, Color color)
        {
            return DrawLine(spriteBatch, rectTexture, start, end, color, 0);
        }

        public static float DrawLine(SpriteBatch spriteBatch, Texture2D rectTexture, Vector2 start, Vector2 end, Color color, float layerDepth)
        {
            return DrawLine(spriteBatch, rectTexture, start, end, color, 1.0f, 1.0f);
        }

        public static float DrawLine(SpriteBatch spriteBatch, Texture2D rectTexture, Vector2 start, Vector2 end, Color color, float layerDepth, float thickness)
        {
            Vector2 delta = end - start;
            float rotation = (float)Math.Atan2(delta.Y, delta.X);
            float lenght = delta.Length();
            spriteBatch.Draw(rectTexture, start, new Rectangle(0, 0, 1, 1), color, rotation, Vector2.Zero, new Vector2(lenght, thickness), SpriteEffects.None, layerDepth);
            return rotation;
        }
        #endregion

        public static void DrawPoint(SpriteBatch spriteBatch, Texture2D rectTexture, Vector2 position, int size, Color color)
        {
            spriteBatch.Draw(rectTexture, position, new Rectangle(0, 0, size, size), color, 0f, new Vector2(size / 2, size / 2), 1f, SpriteEffects.None, 0);
        }

        #region DrawSpline
        public static void DrawSpline(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, int numPoints, float layerDepth)
        {
            DrawSpline(spriteBatch, rectTexture, color, start, startTangent, end, endTangent, numPoints, layerDepth, 1.0f);
        }

        public static void DrawSpline(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, int numPoints, float layerDepth, float thickness)
        {
            Vector2 p1, p2;

            p1 = Vector2.Hermite(start, startTangent, end, endTangent, 0);
            for (int i = 1; i < numPoints; i++)
            {
                p2 = Vector2.Hermite(start, startTangent, end, endTangent, (float)i / (numPoints - 1));
                DrawingHelper.DrawLine(spriteBatch, rectTexture, p1, p2, color, layerDepth, thickness);
                p1 = p2;
            }
            return;
        }
        #endregion

        #region DrawSplineArrow

        public static void DrawSplineArrow(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, int numPoints, float aperture, float lenght_head, float layerDepth)
        {
            DrawSplineArrow(spriteBatch, rectTexture, color, start, startTangent, end, endTangent, numPoints, aperture, lenght_head, layerDepth, 1.0f);
        }

        public static void DrawSplineArrow(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, int numPoints, float aperture, float lenght_head, float layerDepth, float thickness)
        {
            DrawSpline(spriteBatch, rectTexture, color, start, startTangent, end, endTangent, numPoints, layerDepth, thickness);
            float rotation = (float)Math.Atan2(endTangent.Y, endTangent.X);
            aperture *= (float)(Math.PI / 180);
            DrawLine(spriteBatch, rectTexture, -lenght_head * (new Vector2((float)Math.Cos(rotation - aperture), (float)Math.Sin(rotation - aperture))) + end, end, color, layerDepth);
            DrawLine(spriteBatch, rectTexture, -lenght_head * (new Vector2((float)Math.Cos(rotation + aperture), (float)Math.Sin(rotation + aperture))) + end, end, color, layerDepth);
        }

        #endregion

        #region DrawArrow
        public static void DrawArrow(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 end, float aperture, float lenght_head)
        {
            DrawArrow(spriteBatch, rectTexture, color, start, end, aperture, lenght_head, 0);
        }

        public static void DrawArrow(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 end, float aperture, float lenght_head, float layerDepth)
        {
            DrawArrow(spriteBatch, rectTexture, color, start, end, aperture, lenght_head, layerDepth, 1.0f);
        }

        public static void DrawArrow(SpriteBatch spriteBatch, Texture2D rectTexture, Color color, Vector2 start, Vector2 end, float aperture, float lenght_head, float layerDepth, float thickness)
        {
            float rotation;
            rotation = DrawLine(spriteBatch, rectTexture, start, end, color, layerDepth, thickness);
            aperture *= (float)(Math.PI / 180);
            DrawLine(spriteBatch, rectTexture, -lenght_head * (new Vector2((float)Math.Cos(rotation - aperture), (float)Math.Sin(rotation - aperture))) + end, end, color, layerDepth);
            DrawLine(spriteBatch, rectTexture, -lenght_head * (new Vector2((float)Math.Cos(rotation + aperture), (float)Math.Sin(rotation + aperture))) + end, end, color, layerDepth);
            return;
        }
        #endregion

        /// <summary>
        /// Genera una texture rettangolare
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice (da graphics.GraphicsDevice)</param>
        /// <param name="width">Larghezza della texture</param>
        /// <param name="height">Altezza della texture</param>
        /// <param name="color">Colore della texture</param>
        /// <returns>Texture</returns>
        public static Texture2D GenerateRectangularTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
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
