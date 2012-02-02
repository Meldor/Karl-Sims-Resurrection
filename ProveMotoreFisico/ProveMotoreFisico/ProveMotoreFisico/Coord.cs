using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProveMotoreFisico
{
    class Coord
    {
        /// <summary>
        /// Converte da coordinate fisiche in coordinate grafiche
        /// </summary>
        /// <param name="input">Coordinate fisiche di input</param>
        /// <param name="zoomFactor">Rapporto di scala tra le unità grafiche e le unità fisiche</param>
        /// <returns>Coordinate grafiche</returns>
        public static Vector2 ToGraphics(Vector2 input, float zoomFactor)
        {
            Vector2 output = new Vector2();
            output = input * zoomFactor;
            return output;
        }

        /// <summary>
        /// Converte da coordinate grafiche in unità fisiche
        /// </summary>
        /// <param name="input">Coordinate grafiche di input</param>
        /// <param name="zoomFactor">Rapporto di scala tra le unità grafiche e le unità fisiche</param>
        /// <returns>Coordinate fisiche</returns>
        public static Vector2 ToPhysics(Vector2 input, float zoomFactor)
        {
            Vector2 output = new Vector2();
            output = input / zoomFactor;
            return output;
        }

        /// <summary>
        /// Genera un Rectangle di dimensioni pari a input, in unità grafiche
        /// </summary>
        /// <param name="input">Dimensioni del Rectangle in unità fisiche</param>
        /// <param name="zoomFactor">Rapporto di scala tra unità grafiche e unità fisiche</param>
        /// <returns>Rectangle in unità grafiche</returns>
        public static Rectangle GetDrawingRectangle(Vector2 input, float zoomFactor)
        {
            Rectangle output = new Rectangle(0, 0, (int)(input.X * zoomFactor), (int)(input.Y * zoomFactor));
            return output;
        }

        /// <summary>
        /// Genera un vettore ruotato e/o traslato (rotazione riferita rispetto ad un punto qualsiasi)
        /// </summary>
        /// <param name="input">Vettore da trasformare</param>
        /// <param name="newOrigin">Centro di rotazione, ovvero vettore di traslazione rispetto all'origine</param>
        /// <param name="theta">Angolo di rotazione in radianti</param>
        /// <returns>Vettore trasformato</returns>
        public static Vector2 TranslateAndRotate(Vector2 input, Vector2 newOrigin, float theta)
        {
            Vector2 output;
            //rotazione
            output.X = input.X * (float)Math.Cos(theta) - input.Y * (float)Math.Sin(theta);
            output.Y = input.X * (float)Math.Sin(theta) + input.Y * (float)Math.Cos(theta);
            //traslazione
            output += newOrigin;
            return output;
        }

        /// <summary>
        /// Restituisce le coordinate di un vettore in un altro sistema di riferimento traslato e ruotato
        /// </summary>
        /// <param name="input">Vettore di input</param>
        /// <param name="newOrigin">Coordinate, nel sistema di riferimento attuale, dell'origine del nuovo sistema di riferimento</param>
        /// <param name="theta">Rotazione antioraria del nuovo sistema di riferimento rispetto a quello attuale (in radianti)</param>
        /// <returns>Coordinate di input nel nuovo sistema di riferimento</returns>
        public static Vector2 InvTranslateAndRotate(Vector2 input, Vector2 newOrigin, float theta)
        {
            Vector2 output;
            output.X = (float)Math.Cos(theta)*(input.X - newOrigin.X) + (float)Math.Sin(theta)*(input.Y - newOrigin.Y);
            output.Y = -(float)Math.Sin(theta) * (input.X - newOrigin.X) + (float)Math.Cos(theta) * (input.Y - newOrigin.Y);
            return output;
        }

    }
}
