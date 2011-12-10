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
            /*KSR_libraryRN.ReteNeurale myNetwork = new KSR_libraryRN.ReteNeurale(2, 3);
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
             */
            KSR_libraryRN.GestoreRN_NEAT gestore=new KSR_libraryRN.GestoreRN_NEAT(3,2);
            KSR_libraryRN.GenotipoRN p=gestore.getPerceptron();
            Console.WriteLine("Perceptron\n"+p.toString());
            

            KSR_libraryRN.GenotipoRN mutato=gestore.mutazioneAggiungiNeurone(p);
            Console.WriteLine("Aggiunto Neurone\n"+mutato.toString());

            KSR_libraryRN.GenotipoRN mutato2 = gestore.mutazioneAggiungiAssone(mutato);
            Console.WriteLine("Aggiunto assone\n"+mutato2.toString());

            Console.WriteLine("Perceptron\n" + p.toString());
            Console.Read();
                                   
            }
        }
    }

