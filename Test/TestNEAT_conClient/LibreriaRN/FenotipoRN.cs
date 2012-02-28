using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibreriaRN
{
   public class FenotipoRN
   {
       SortedList<int,NeuroneF> neuroni;
       List<NeuroneF> neuroniA;
       List<NeuroneF> neuroniS;
       //SortedList<int, NeuroneStruct> NEATLibrary;
       int NEAT_numID;
       public int numNeuroniSensori
       {
           get { return neuroniS.Count; }
       }
       public int numNeuroniAttuatori
       {
           get { return neuroniA.Count; }
       }

       #region Costruttori

       //public FenotipoRN(int numS, int numA)
       //{
       //    neuroni = new SortedList<int,NeuroneF>();
       //    neuroniA = new List<NeuroneF>();
       //    neuroniS = new List<NeuroneF>();
       //    //NEATLibrary = new SortedList<int, NeuroneStruct>();
       //    NEAT_numID = 0;
       //    numNeuroniSensori = numS;
       //    numNeuroniAttuatori = numA;
       //}

       public FenotipoRN(GenotipoRN genotipo)
       {
           neuroni = new SortedList<int,NeuroneF>();
           neuroniA = new List<NeuroneF>();
           neuroniS = new List<NeuroneF>();
           foreach(KeyValuePair<int, GenotipoRN.NeuroneG> k_val in genotipo.neuroni)
           {
               NeuroneF nuovo = new NeuroneF(k_val.Value);
               neuroni.Add(k_val.Key, nuovo);
           }
           foreach (GenotipoRN.NeuroneG neurone in genotipo.neuroniInput)
               neuroniS.Add(neuroni[neurone.neatID]);
           foreach (GenotipoRN.NeuroneG neurone in genotipo.neuroniOutput)
               neuroniA.Add(neuroni[neurone.neatID]);
           foreach (GenotipoRN.AssoneG assone in genotipo.assoni)
               if(assone.attivo)
                    neuroni[assone.input].addAssone(assone, neuroni);
           NEAT_numID = 0;
           //numNeuroniSensori = neuroniS.Count;
           //numNeuroniAttuatori = neuroniA.Count;
       }

       #endregion

       #region Modifica e creazione

       private void AddNeurone(thresholdFunction funzione, int id, TipoNeurone tipo)
       {
           NeuroneF neurone = new NeuroneF(funzione, id, tipo);
           return;
       }

       private void AddAssone(NeuroneF neurone, NeuroneF neuroneLink, Double peso)
       {
           neurone.addAssone(neuroneLink, peso);
           return;
       }

       public void generaPercettron()
       {
           NeuroneF neurone;

           Random generatoreCasuale = new Random();

           for (int i = 0; i < numNeuroniAttuatori; i++)
           {
               neurone = new NeuroneF(Threshold.getRandomDelegate(), NEAT_numID, TipoNeurone.NActuator);
               neuroni.Add(neurone.neatID,neurone);
               neuroniA.Add(neurone);
               NEAT_numID++;
           }

           for (int i = 0; i < numNeuroniSensori; i++)
           {
                if(Params.transparentInput)
                    neurone = new NeuroneF(Threshold.Transparent, NEAT_numID, TipoNeurone.NSensor);
                else
                    neurone = new NeuroneF(Threshold.getRandomDelegate(), NEAT_numID, TipoNeurone.NSensor);
                foreach (NeuroneF n in neuroniA)
                    neurone.addAssone(n, generatoreCasuale.NextDouble());
                neuroni.Add(neurone.neatID,neurone);
                neuroniS.Add(neurone);
                NEAT_numID++;
           }
            
           return;
       }

       #endregion

       #region Input/output

       /// <summary>
       /// Propaga gli output del ciclo precedente lungo gli assoni
       /// </summary>
       /// <returns>Lista dei valori prodotti dagli output ordinata in base all'idNEAT</returns>
       public SortedList<int, Double> Calcola()
       {
           SortedList<int, Double> output = new SortedList<int, double>();
           double Out;

           foreach (KeyValuePair<int,NeuroneF> k_val in neuroni)
           {
               Out = k_val.Value.attiva();
               if (k_val.Value.tipo == TipoNeurone.NActuator)
                   output.Add(k_val.Key, Out);
           }
            
           return output;
       }

       /// <summary>
       /// Gli input futuri diventano input presenti e viene calcolata la funzione di soglia di ogni neurone.
       /// </summary>
       /// <returns>Lista dei valori prodotti dagli output ordinata in base all'idNEAT</returns>
       public SortedList<int, Double> aggiorna()
       {
           SortedList<int, Double> output = new SortedList<int, double>();
           double Out;

           foreach (KeyValuePair<int, NeuroneF> k_val in neuroni)
           {
               Out = k_val.Value.aggiorna();
               if (k_val.Value.tipo == TipoNeurone.NActuator)
                   output.Add(k_val.Key, Out);
           }

           return output;
       }

       /// <summary>
       /// Applica in ingresso i dati contenuti in vett
       /// </summary>
       /// <param name="vett">Ingressi da applicare, nell'ordine di neuroniS</param>
       public void sensori(Double[] vett)
       {
           if (vett.Length == neuroniS.Count)
               for (int i = 0; i < vett.Length; i++)
                   neuroniS[i].addInput(vett[i]);

       }

       #endregion

       public NeuroneF GetNeuroneById(int id)
       {
           return neuroni[id];
       }

       public class NeuroneF
       {

           private int _neatID;
           private TipoNeurone _tipo;
           private double _output;

           private double memoria;
                      
           thresholdFunction funzioneSoglia;
           List<Double> inputP;
           List<Double> inputF;
           List<AssoneF> assoni;

           public int neatID
           {
               get { return _neatID; }
           }
           public TipoNeurone tipo
           {
               get { return _tipo; }
           }
           public double output
           {
               get { return _output; }
           }

           #region Costruttori

           /// <summary>
           /// Traduce il genotipo passato per argomento
           /// </summary>
           /// <param name="neuroneG">Genotipo da tradurre</param>
           public NeuroneF(GenotipoRN.NeuroneG neuroneG)
           {
               _neatID = neuroneG.neatID;
               memoria = 0;
               _tipo = neuroneG.tipo;
               funzioneSoglia = Threshold.getThresholdFromIndex(neuroneG.thresholdIndex);
               inputP = new List<double>();
               inputF = new List<double>();
               assoni = new List<AssoneF>();
               return;
           }

           public NeuroneF(thresholdFunction funzione, int id, TipoNeurone tipo)
           {
               _neatID = id;
               _tipo = tipo;
               memoria = 0;
               inputP = new List<double>();
               inputF = new List<double>();
               assoni = new List<AssoneF>();
               funzioneSoglia = funzione;
               return;
           }

           #endregion

           #region Input/output

           public void addInput(Double _in)
           {
               inputF.Add(_in);
               return;
           }

           public Double attiva()
           {
               //_output = this.calcola();  cancellato per evitare di sballare le funzioni con memoria
               foreach (AssoneF assone in assoni)
                    assone.attiva(output);
               return output;
           }

           private double calcola()
           {
               Double inputSum = inputP.Sum();
               _output = funzioneSoglia(inputSum, ref memoria);
               return output;
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

           #endregion

           #region addAssone

           public void addAssone(NeuroneF neuroneLink, Double peso)
           {
               AssoneF assone = new AssoneF(peso, neuroneLink);
               assoni.Add(assone);
               return;
           }

           public void addAssone(GenotipoRN.AssoneG assone, SortedList<int, NeuroneF> neuroni)
           {
               AssoneF nuovoAssone = new AssoneF(assone, neuroni);
               assoni.Add(nuovoAssone);
               return;
           }

           #endregion

       }

       public class AssoneF
       {
           NeuroneF neuroneLink;
           Double peso;

           #region Costruttori

           public AssoneF(GenotipoRN.AssoneG assoneG, SortedList<int, NeuroneF> neuroni)
           {
               neuroneLink = neuroni[assoneG.output];
               peso = assoneG.peso;
               return;
           }

           public AssoneF(Double _peso, NeuroneF _neuroneLink)
           {
               peso = _peso;
               neuroneLink = _neuroneLink;
               return;
           }

           #endregion

           /// <summary>
           /// Invia l'ingresso pesato al neurone a cui è collegato, come input futuro
           /// </summary>
           /// <param name="_in">Ingresso da inviare</param>
           public void attiva(Double _in)
           {
               Double output;
               output = _in * peso;
               neuroneLink.addInput(output);
               return;
           }


       }
   }

   
}
