using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LibreriaRN
{
    public delegate Double thresholdFunction(Double input);
    public enum TipoNeurone { NSensor, NHide, NActuator };

    public class Threshold
    {
        private static int numFunz = 5;
        static Random generatoreCasuale = new Random();

        public static Double Sinusoide(Double input) /*Seno e Coseno tra x:[-1,1] y:[-1,1]*/
        {
            return Math.Sin(input * Math.PI);
        }

        public static Double Cosinusoide(Double input)
        {
            return Math.Cos(input * Math.PI);
        }

        public static Double SigmoideInsensibile(Double input) /* Sigmoide da x:[0,1] y:[0,1] taglia i valori negativi*/
        {
            return 1 / (1 + Math.Exp(-12 * (input * 0.5f)));
        }

        public static Double SigmoideAttenuante(Double input) /* Sigmoide da x:[-1,1] y:[0,1] attenua i valori negativi*/
        {
            return 1 / (1 + Math.Exp(-6 * input));
        }

        public static Double IperbolicTan(Double input) /* Tangente iperbolica x:[-1,1] y:[-1,1] */
        {
            return Math.Tanh(2.5f * input);
        }

        public static thresholdFunction getRandomDelegate()
        {
            Thread.Sleep(10);
            int id = generatoreCasuale.Next(0, numFunz - 1);
            switch (id)
            {
                case 0: return Sinusoide;
                case 1: return Cosinusoide;
                case 2: return SigmoideInsensibile;
                case 3: return SigmoideAttenuante;
                case 4: return IperbolicTan;
                default: return IperbolicTan;
            }

        }

    }
    
    public class Utilita
    {
        public static Double pesoUniforme=0.001;
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


    /* Da riscrivere in FenotipoRN
   public class ReteNeurale
   {
       List<NeuroneStruct> neuroni;
       List<NeuroneStruct> neuroniA;
       List<NeuroneStruct> neuroniS;
       SortedList<int, NeuroneStruct> NEATLibrary;
       int NEAT_numID;
       int numNeuroniSensori, numNeuroniAttuatori;

       public ReteNeurale(int numA, int numB)
       {
           neuroni = new List<NeuroneStruct>();
           neuroniA = new List<NeuroneStruct>();
           neuroniS = new List<NeuroneStruct>();
           NEATLibrary = new SortedList<int, NeuroneStruct>();
           NEAT_numID = 0;
           numNeuroniSensori = numB;
           numNeuroniAttuatori = numA;
       }

       private void AddNeurone(thresholdFunction funzione, int id, TipoNeurone tipo)
       {
           NeuroneStruct neurone = new NeuroneStruct();
           neurone.inizializza(funzione, id, tipo);
           return;
       }

       private void AddAssone(NeuroneStruct neurone, NeuroneStruct neuroneLink, Double peso)
       {
           neurone.addAssone(neuroneLink, peso);
           return;
       }

       public void generaPercettron()
       {
           NeuroneStruct neurone;

           Random generatoreCasuale = new Random();

           for (int i = 0; i < numNeuroniAttuatori; i++)
           {
               neurone = new NeuroneStruct();
               neurone.inizializza(Threshold.getRandomDelegate(), NEAT_numID, TipoNeurone.NActuator);
               neuroni.Add(neurone);
               neuroniA.Add(neurone);
               NEAT_numID++;
           }

           for (int i = 0; i < numNeuroniSensori; i++)
           {
               neurone = new NeuroneStruct();
               neurone.inizializza(Threshold.getRandomDelegate(), NEAT_numID, TipoNeurone.NSensor);
               foreach (NeuroneStruct n in neuroniA)
                   neurone.addAssone(n, generatoreCasuale.NextDouble());
               neuroni.Add(neurone);
               neuroniS.Add(neurone);
               NEAT_numID++;
           }

           return;
       }

       public SortedList<int, Double> Calcola()
       {
           SortedList<int, Double> output = new SortedList<int, double>();
           double Out;

           foreach (NeuroneStruct neurone in neuroni)
           {
               Out = neurone.attiva();
               if (neurone.GetTipo() == TipoNeurone.NActuator)
                   output.Add(neurone.GetID(), Out);
           }
            
           return output;
       }

       public SortedList<int, Double> aggiorna()
       {
           SortedList<int, Double> output = new SortedList<int, double>();
           double Out;

           foreach (NeuroneStruct neurone in neuroni)
           {
               Out = neurone.aggiorna();
               if (neurone.GetTipo() == TipoNeurone.NActuator)
                   output.Add(neurone.GetID(), Out);
           }

           return output;
       }

       public void sensori(Double[] vett)
       {
           if (vett.Length == neuroniS.Count)
               for (int i = 0; i < vett.Length; i++)
                   neuroniS[i].addInput(vett[i]);

       }

       public int GetNumberSensor()
       { return neuroniS.Count; }
        
   }

   public struct NeuroneStruct
   {
       int neatId;
       TipoNeurone tipo;
       thresholdFunction funzioneSoglia;
       List<Double> inputP;
       List<Double> inputF;
       List<AssoneStruct> assoni;

       public TipoNeurone GetTipo()
       { return tipo; }

       public int GetID()
       { return neatId; }

       public void inizializza(thresholdFunction _funzione, int _id, TipoNeurone _tipo)
       {
           neatId = _id;
           tipo = _tipo;
           inputP = new List<double>();
           inputF = new List<double>();
           assoni = new List<AssoneStruct>();
           funzioneSoglia = _funzione;
           return;
       }

       public void addInput(Double _in)
       {
           inputF.Add(_in);
           return;
       }

       public Double attiva()
       {
           Double val = this.calcola();
           foreach (AssoneStruct assone in assoni)
               assone.attiva(val);
           return val;
       }

       private double calcola()
       {
           Double inputSum = inputP.Sum();
           return funzioneSoglia(inputSum);
       }

       public Double aggiorna()
       {
           Double valore;
           inputP.Clear();
           foreach (Double val in inputF)
               inputP.Add(val);
           inputF.Clear();

           valore = this.calcola();
           return valore;
       }

       public void addAssone(NeuroneStruct neuroneLink, Double peso)
       {
           AssoneStruct assone = new AssoneStruct();
           assone.inizializza(peso, neuroneLink);
           assoni.Add(assone);
           return;
       }

   }

   public struct AssoneStruct
   {
       NeuroneStruct neuroneLink;
       Double peso;

       public void inizializza(Double _peso, NeuroneStruct _neuroneLink)
       {
           peso = _peso;
           neuroneLink = _neuroneLink;
           return;
       }

       public void attiva(Double _in)
       {
           Double output;
           output = _in * peso;
           neuroneLink.addInput(output);
           return;
       }


   }
    */

 
}
