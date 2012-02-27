using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LibreriaRN
{
    public delegate Double thresholdFunction(Double input, ref Double memoria);
    public enum TipoNeurone { NSensor, NHide, NActuator };

    public class Threshold
    {
        private static int numFunz = 7;
        static Random generatoreCasuale = new Random();

        public static Double Sinusoide(Double input, ref Double memoria) /*Seno e Coseno tra x:[-1,1] y:[-1,1]*/
        {
            return Math.Sin(input * Math.PI);
        }

        public static Double Cosinusoide(Double input, ref Double memoria)
        {
            return Math.Cos(input * Math.PI);
        }

        //qual è la differenza con la sigmoide attenuante??
        public static Double SigmoideInsensibile(Double input, ref Double memoria) /* Sigmoide da x:[0,1] y:[0,1] taglia i valori negativi*/
        {
            return 1 / (1 + Math.Exp(-12 * (input * 0.5f)));
        }

        public static Double SigmoideAttenuante(Double input, ref Double memoria) /* Sigmoide da x:[-1,1] y:[0,1] attenua i valori negativi*/
        {
            return 1 / (1 + Math.Exp(-6 * input));
        }

        public static Double IperbolicTan(Double input, ref Double memoria) /* Tangente iperbolica x:[-1,1] y:[-1,1] */
        {
            return Math.Tanh(2.5f * input);
        }

        public static Double Modulo(Double input, ref Double memoria)
        {
            return (Math.Abs(input) <= 1 ? Math.Abs(input) : 1);
        }

        public static Double Gaussian(Double input, ref Double memoria)
        {
            const double sigma = 0.3;
            return Math.Exp(-(input * input) / (2 * sigma * sigma));
        }

        public static Double Transparent(Double input, ref Double memoria)
        {
            if (input < -1)
                return -1;
            else if (input > 1)
                return 1;
            else
                return input;
        }

        public static Double Sin(Double input, ref Double memoria)
        {   //input = -1 -> f = 0 Hz
            //input = +1 -> f = 2 Hz (se 60 FPS) 
            memoria += 2 * Math.PI * (input + 1) * 0.016;
            return Math.Sin(memoria);
        }

        public static Double SquareWave(Double input, ref Double memoria)
        {
            //input = -1 -> f = 0 Hz
            //input = +1 -> f = 2 Hz (se 60 FPS)
            memoria += (input + 1)/60;
            if ((int)Math.Floor(memoria) % 2 == 0)
                return -1;
            else
                return 1;
        }

        public static thresholdFunction getRandomDelegate()
        {
            Thread.Sleep(10);
            if (Params.onlySigmoid)
                return SigmoideAttenuante;
            int id = generatoreCasuale.Next(0, numFunz);
            switch (id)
            {
                case 0: return SigmoideAttenuante;
                case 1: return IperbolicTan;
                case 2: return Modulo;
                case 3: return Gaussian;
                case 4: return Transparent;
                case 5: return Sin;
                case 6: return SquareWave;
                default: return SquareWave;
            }
        }
    }
    
    public class Utilita
    {
        public static Double pesoUniforme=0.1;
        public static String RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        
    }


   

 
}
