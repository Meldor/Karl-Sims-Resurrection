using System;
using System.Collections.Generic;
using System.Linq;


using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;




namespace KSR_libraryRN
{

    delegate Double thresholdFunction(Double input);
    class Threshold
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

    class GestoreRN_NEAT
    {
        private int contNeuroni;
        private int contAssoni;
        ICollection<GenotipoRN> genotipi;

        public GestoreRN_NEAT(int input, int output)
        {
            contNeuroni = 0;
            contAssoni = 0;
            genotipi = new SortedSet<GenotipoRN>();
            generaPerceptron(input, output);
        }

        public GenotipoRN getPerceptron()
        { return genotipi.First(); }

        private void generaPerceptron(int input, int output)
        {
            Random generatoreCasuale = new Random();
            GenotipoRN p = new GenotipoRN();
            p.t = 0;

            for (int i = 0; i < input+output; i++)
            {
                p.addNeurone(new GenotipoRN.NeuroneG(contNeuroni, 0));
                contNeuroni++;            
            }

            for (int i = 0; i < (input * output); i++)
            {
                int I = i%input;
                int O = i%output + input;
                double peso = generatoreCasuale.NextDouble();
                p.addAssone(new GenotipoRN.AssoneG(contAssoni, I, O, 1-2*peso));
                contAssoni++;
            }

            genotipi.Add(p);

        }

        public GenotipoRN[] mutazione(GenotipoRN genotipo, int num)
        { 
            GenotipoRN[] r=new GenotipoRN[num];
            Random generatoreCasuale = new Random();
            int numero;

            for (int i = 0; i < num;i++ )
            {
                numero = generatoreCasuale.Next(100);
                if (numero < 3)
                    r[i] = mutazioneAggiungiNeurone(genotipo);
                else if (numero < 8)
                    r[i] = mutazioneAggiungiAssone(genotipo);
                else if (numero < 88)
                {
                    numero = generatoreCasuale.Next(100);
                    if (numero<10)
                        r[i] = mutazioneModificaPesoUniformemente(genotipo);
                    else
                        r[i] = mutazioneModificaPesoRadicalmente(genotipo);
                }
                genotipi.Add(r[i]);
            }

            return r;
        
        }

        public GenotipoRN mutazioneAggiungiNeurone(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();

            int num = generatoreCasuale.Next(g.assoni.Count);
            
            GenotipoRN.AssoneG assoneCorrente = g.assoni[num];
            assoneCorrente.attivo = 0;
            g.assoni[num] = assoneCorrente;

            g.addNeurone(new GenotipoRN.NeuroneG(contNeuroni, 0));
            g.addAssone(new GenotipoRN.AssoneG(contAssoni, g.assoni[num].getInput(), contNeuroni, 1));
            contAssoni++;
            g.addAssone(new GenotipoRN.AssoneG(contAssoni, contNeuroni, g.assoni[num].getOutput(), g.assoni[num].getPeso()));
            contAssoni++;
            contNeuroni++;
            return g;
        }

        private GenotipoRN mutazioneAggiungiAssone(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN();


            return g;
        }

        private GenotipoRN mutazioneModificaPesoUniformemente(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN();
            return g;
        }

        private GenotipoRN mutazioneModificaPesoRadicalmente(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN();
            return g;
        }



        



    
    }


    class ReteNeurale
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

    struct NeuroneStruct
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

    struct AssoneStruct
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

   

  

    class GenotipoRN : IComparable<GenotipoRN>
    {
        public struct NeuroneG : IComparable<NeuroneG>
        {
            int idNEAT;
            int tipo;

            public NeuroneG(int idNEAT, int tipo)
            {
                this.idNEAT = idNEAT;
                this.tipo = tipo;
            }

           
            public int CompareTo(NeuroneG other)
            {
                return idNEAT - other.idNEAT;
            }
        }
        public struct AssoneG : IComparable<AssoneG>
        {
            int idNEAT;
            int input;
            int output;
            double peso;
            public int attivo;

            public AssoneG(int idNEAT, int input, int output, double peso)
            {
                this.idNEAT = idNEAT;
                this.input = input;
                this.output = output;
                this.peso = peso;
                attivo = 1;
                
            }



            public void attiva()
            { this.attivo = 1; }

            public void disattiva()
            { this.attivo = 0; }

            public int getInput()
            { return input; }

            public int getOutput()
            { return output; }

            public double getPeso()
            { return peso; }

         

            public String toString()
            { return "ID: " + idNEAT+ " -> " + input + " - " + output + " Peso "+ peso +  " Attivo: " + attivo; }



            public int CompareTo(AssoneG other)
            {  return idNEAT - other.idNEAT;     }
        }

        public int t;
        public List<AssoneG> assoni;
        public ISet<NeuroneG> neuroni;

        public GenotipoRN()
        {
            t = -1;
            assoni = new List<AssoneG>();
            neuroni = new SortedSet<NeuroneG>();
        }

        public GenotipoRN(GenotipoRN g)
        {
            
            t = g.t+1;
            assoni = new List<AssoneG>(g.assoni);
            neuroni = new SortedSet<NeuroneG>(g.neuroni);
            
        }

        public void addAssone(AssoneG a)
        { assoni.Add(a); }

        public void addNeurone(NeuroneG n)
        { neuroni.Add(n); }

        public int getNumeroAssoni()
        { return assoni.Count; }

        public int getNumeroNeuroni()
        { return neuroni.Count; }

        public String toString()
        {
            String s = "";
            foreach (AssoneG a in assoni)
                s += a.toString() + "\n";
            return s;
            
        }

        public int CompareTo(GenotipoRN other)
        { return t - other.t; }

       
    }

    class FenotipoRN
    {
    }

  
    enum TipoNeurone { NSensor, NHide, NActuator };




}