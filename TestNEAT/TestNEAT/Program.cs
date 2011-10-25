using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNEAT
{
    class Program
    {
        
        static void Main(string[] args)
        {
            KSR_library.ReteNeurale myNetwork = new KSR_library.ReteNeurale(2, 3);
            SortedList<int, Double> lista;
            Double[] vett;
            int passo = 1;
            

            myNetwork.generaPercettron();
            vett = new Double[myNetwork.GetNumberSensor()];
            Console.WriteLine("Rete neurale creata: " + myNetwork.GetNumberSensor() + " Sensori");
            
            Console.WriteLine("\nInput 0\n\n");
            
            for (int i = 0; i < myNetwork.GetNumberSensor(); i++)  //Introduco i dati al passo 0 -> calcolati al passo 1
                Console.Write("Sensor " + i + ": 0\n");

                                               
            while (true)
            {

                Console.WriteLine("\nInput " + passo+"\n\n");
                for (int i = 0; i < myNetwork.GetNumberSensor(); i++)  //Introduco i dati al passo 0 -> calcolati al passo 1
                {
                    Console.Write("Sensor " + i+": ");
                    vett[i] = Convert.ToDouble(Console.ReadLine());
                }
                
                myNetwork.sensori(vett);
                myNetwork.Calcola();
                lista=myNetwork.aggiorna();
                                
                Console.WriteLine("\nOutput " + (passo -1) + "\n\n");
                foreach (double l in lista.Values)
                    Console.WriteLine("\n\tOut -> " + l);
                passo++;
                                   
            }
        }
    }
}
