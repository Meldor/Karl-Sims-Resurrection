using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PoleBalancing
{
    class Utils
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
        /// Converte da coordinate fisiche a coordinate grafiche utilizzando come zoom Const.Zoom
        /// </summary>
        /// <param name="input">Coordinate fisiche di input</param>
        /// <returns>Coordinate grafiche</returns>
        public static Vector2 ToGraphics(Vector2 input)
        {
            return ToGraphics(input, Const.Zoom);
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
        /// Converte da coordinate grafiche a coordinate fisiche utilizzando come zoom Const.Zoom
        /// </summary>
        /// <param name="input">Coordinate grafiche</param>
        /// <returns>Coordinate fisiche</returns>
        public static Vector2 ToPhysics(Vector2 input)
        {
            return ToPhysics(input, Const.Zoom);
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
        /// Restituisce le coordinate di un punto in un sistema di riferimento secondario
        /// </summary>
        /// <param name="input">Coordinate del punto nel sistema assoluto</param>
        /// <param name="newOrigin">Coordinate del centro del nuovo sistema di riferimento</param>
        /// <param name="theta">Angolo di rotazione in radianti del nuovo sistema di riferimento</param>
        /// <returns>Coordinate nel sistema di riferimento secondario</returns>
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
        /// Restituisce le coordinate di un punto, espresso in un sistema di riferimento secondario, nel sistema di riferimento assoluto
        /// </summary>
        /// <param name="input">Coordinate del punto nel sistema secondario</param>
        /// <param name="newOrigin">Coordinate del sistema di riferimento secondario nel sistema assoluto</param>
        /// <param name="theta">Rotazione antioraria del sistema di riferimento secondario rispetto a quello assoluto</param>
        /// <returns>Coordinate nel sistema di riferimento assoluto</returns>
        public static Vector2 InvTranslateAndRotate(Vector2 input, Vector2 newOrigin, float theta)
        {
            Vector2 output;
            output.X = (float)Math.Cos(theta)*(input.X - newOrigin.X) + (float)Math.Sin(theta)*(input.Y - newOrigin.Y);
            output.Y = -(float)Math.Sin(theta) * (input.X - newOrigin.X) + (float)Math.Cos(theta) * (input.Y - newOrigin.Y);
            return output;
        }

        /// <summary>
        /// Calcola il logaritmo in base 2 in maniera rapida (se input è una potenza di 2), oppure restituisce la posizione del primo bit a 1 da destra
        /// </summary>
        /// <param name="input">x</param>
        /// <returns>log2(x)</returns>
        public static int RapidLog2(int input)
        {
            int count = 0, mask = 1;
            if (input == 0)
                return 0;
            while ((input & mask) != 1)
            {
                input = input >> 1;
                count++;
            }
            return count;
        }

    }
}
