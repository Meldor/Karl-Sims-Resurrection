using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace LibreriaRN
{
    [Serializable]
    public class GenotipoRN : IComparable<GenotipoRN>
    {
        [Serializable]
        public struct NeuroneG : IComparable<NeuroneG>
        {
            public int neatID
            {
                get { return _neatID; }
            }
            private int _neatID;
            public TipoNeurone tipo
            {
                get { return _tipo; }
            }
            TipoNeurone _tipo;
            public int thresholdIndex
            {
                get
                {
                    return _thresholdIndex;
                }
            }

            private List<int> _idAssoniInput;  //assoni in input per il neurone, quindi che hanno il neurone per output
            private List<int> _idAssoniOutput;
            public int numAssoniInput { get { return _idAssoniInput.Count; } }
            public int numAssoniOutput { get { return _idAssoniOutput.Count; } }

            public IEnumerable<int> AssoniInput
            {
                get
                {
                    foreach (int i in _idAssoniInput)
                        yield return i;
                }
            }

            public IEnumerable<int> AssoniOutput
            {
                get
                {
                    foreach (int i in _idAssoniOutput)
                        yield return i;
                }
            }
            private int _thresholdIndex;

            #region Costruttori

            public NeuroneG(int idNEAT, TipoNeurone tipo, int thresholdFunctionIndex)
            {
                _neatID = idNEAT;
                _tipo = tipo;
                _idAssoniInput = new List<int>();
                _idAssoniOutput = new List<int>();
                if (Params.transparentInput && tipo == TipoNeurone.NSensor)
                    _thresholdIndex = 4;
                else
                    _thresholdIndex = thresholdFunctionIndex;
            }

            public NeuroneG(int idNEAT, TipoNeurone tipo): this(idNEAT, tipo, Threshold.getRandomIndex())
            {

            }

            #endregion

            public int CompareTo(NeuroneG other)
            { return neatID - other.neatID; }

            public string ToString()
            {
                string output = "ID=" + neatID;
                switch (tipo)
                {
                    case TipoNeurone.NActuator:
                        output += " Act ";
                        break;
                    case TipoNeurone.NHide:
                        output += " Hid ";
                        break;
                    case TipoNeurone.NSensor:
                        output += " Sen ";
                        break;
                }
                return output;
            }

            public void addAssoneInput(int idAssone)
            {
                _idAssoniInput.Add(idAssone);
            }

            public void addAssoneOutput(int idAssone)
            {
                _idAssoniOutput.Add(idAssone);
            }

            public void clearAssoni()
            {
                _idAssoniInput.Clear();
                _idAssoniOutput.Clear();
            }

            public void recreateAssoni()
            {
                _idAssoniInput = new List<int>();
                _idAssoniOutput = new List<int>();
            }

        }

        [Serializable]
        public struct AssoneG : IComparable<AssoneG>
        {
            public int neatID
            { get { return _neatID; } }
            public int input
            { get { return _input; } }
            public int output
            { get { return _output; } }
            public double peso
            { get { return _peso; } }
            public bool attivo;

            private int _neatID;
            private int _input;
            private int _output;
            private double _peso;

            public AssoneG(int idNEAT, int input, int output, double peso)
            {
                _neatID = idNEAT;
                _input = input;
                _output = output;
                _peso = peso;
                attivo = true;
            }

            #region Modifiche

            public void attiva()
            { this.attivo = true; }

            public void disattiva()
            { this.attivo = false; }

            public void modPeso(double p)
            { 
                _peso += p;
                if (_peso > 1)
                    _peso = 1;
                else if (_peso < -1)
                    _peso = -1;
            }

            public void raddoppia()
            { 
                _peso *= 2;
                if (_peso > 1)
                    _peso = 1;
                else if (_peso < -1)
                    _peso = -1;
            }

            #endregion

            /// <summary>
            /// Indica se è presente o meno un assone tra i neuroni specificati
            /// </summary>
            /// <param name="input">idNEAT del neurone di input</param>
            /// <param name="output">idNEAT del neurone di output</param>
            /// <returns>true se è presente un assone</returns>
            public bool testaCollegamento(int input, int output)
            { return (this.input == input && this.output == output); }

            public String ToString()
            { return "ID: " + neatID + " -> " + input + " - " + output + " Peso " + peso + " Attivo: " + attivo; }

            public int CompareTo(AssoneG other)
            { return neatID - other.neatID; }
        }

        public int t; //cos'è?
        public SortedList<int, AssoneG> assoni;
        public SortedList<int, NeuroneG> neuroni;
        public IEnumerable<NeuroneG> Neuroni
        {
            get
            {
                foreach (KeyValuePair<int, NeuroneG> kvp in neuroni)
                    yield return kvp.Value;
            }
        }
        public ICollection<NeuroneG> neuroniInput;
        public IEnumerable<NeuroneG> NeuroniInput
        {
            get
            {
                foreach (NeuroneG neurone in neuroniInput)
                    yield return neurone;
            }
        }
        public ICollection<NeuroneG> neuroniOutput;
        public IEnumerable<NeuroneG> NeuroniOutput
        {
            get
            {
                foreach (NeuroneG neurone in neuroniOutput)
                    yield return neurone;
            }
        }
        private String nome;
        public int numeroAssoni
        {
            get { return assoni.Count; }
        }
        public int numeroNeuroni
        {
            get { return neuroni.Count; }
        }
        public int numeroNeuroniInput
        {
            get { return neuroniInput.Count; }
        }
        public int maxIDNeuroni
        {
            get { return neuroni.Keys.Max<int>(); }
        }
        public int maxIDAssoni
        {
            get { return assoni.Keys.Max<int>(); }
        }

        #region Costruttori

        public GenotipoRN()
        {
            t = -1;
            assoni = new SortedList<int, AssoneG>();
            neuroni = new SortedList<int, NeuroneG>();
            neuroniInput = new List<NeuroneG>();
            neuroniOutput = new List<NeuroneG>();
            nome = Utilita.RandomString(5, false);
        }

        /// <summary>
        /// Copia il genotipo passato per parametro
        /// </summary>
        /// <param name="g">Genotipo da copiare</param>
        public GenotipoRN(GenotipoRN g)
        {
            t = g.t + 1;
            assoni = new SortedList<int, AssoneG>(g.assoni);
            neuroni = new SortedList<int, NeuroneG>(g.neuroni);
            neuroniInput = new List<NeuroneG>(g.neuroniInput);
            neuroniOutput = new List<NeuroneG>(g.neuroniOutput);
            nome = Utilita.RandomString(5, false);
        }

        /// <summary>
        /// Genera il genotipo per crossover di altri due genotipi
        /// </summary>
        /// <param name="gen1">Genitore 1</param>
        /// <param name="fitness1">Fitness di gen1</param>
        /// <param name="gen2">Genitore 2</param>
        /// <param name="fitness2">Fitness di gen2</param>
        public GenotipoRN(GenotipoRN gen1, double fitness1, GenotipoRN gen2, double fitness2)
        {
            Random random = new Random();
            bool end1, end2;
            IEnumerator<KeyValuePair<int, GenotipoRN.NeuroneG>> enumNeurGen1, enumNeurGen2;
            IEnumerator<NeuroneG> enumNeurInput, enumNeurOutput;
            IEnumerator<KeyValuePair<int, GenotipoRN.AssoneG>> enumAssGen1, enumAssGen2;

            assoni = new SortedList<int, AssoneG>();
            neuroni = new SortedList<int, NeuroneG>();
            neuroniInput = new List<NeuroneG>();
            neuroniOutput = new List<NeuroneG>();
            nome = Utilita.RandomString(5, false);

            t = gen1.t + 1;
            enumNeurGen1 = gen1.GetEnumeratorNeuroni();
            enumNeurGen2 = gen2.GetEnumeratorNeuroni();
            enumAssGen1 = gen1.GetEnumeratorAssoni();
            enumAssGen2 = gen2.GetEnumeratorAssoni();

            //neuroni
            end1 = !enumNeurGen1.MoveNext();
            end2 = !enumNeurGen2.MoveNext();
            while (!(end1 && end2))
            {
                if (end1)
                {
                    addNeurone(enumNeurGen2.Current.Value);
                    end2 = !enumNeurGen2.MoveNext();
                }
                else if (end2)
                {
                    addNeurone(enumNeurGen1.Current.Value);
                    end1 = !enumNeurGen1.MoveNext();
                }
                else
                {
                    if (enumNeurGen1.Current.Key == enumNeurGen2.Current.Key) //geni corrispondenti
                    {
                        double numRand = random.NextDouble();
                        //eredita il neurone (cioè la funzione di soglia) dal genitore con fitness più alto con probabilità prefissata
                        if (((fitness1 > fitness2) && (numRand < Params.mostFitParentInheritingProbability)) || ((fitness1 < fitness2) && (numRand > Params.mostFitParentInheritingProbability)))
                            addNeurone(enumNeurGen1.Current.Value);
                        else
                            addNeurone(enumNeurGen2.Current.Value);
                        end1 = !enumNeurGen1.MoveNext();
                        end2 = !enumNeurGen2.MoveNext();
                    }
                    else if (enumNeurGen1.Current.Key > enumNeurGen2.Current.Key)
                    {
                        addNeurone(enumNeurGen2.Current.Value);
                        end2 = !enumNeurGen2.MoveNext();
                    }
                    else //if(enumNeurGen1.Current.Key < enumAssGen2.Current.Key)
                    {
                        addNeurone(enumNeurGen1.Current.Value);
                        end1 = !enumNeurGen1.MoveNext();
                    }
                }
            }

            foreach (NeuroneG n in gen1.NeuroniInput)
                neuroniInput.Add(neuroni[n.neatID]);

            foreach (NeuroneG n in gen1.NeuroniOutput)
                neuroniOutput.Add(neuroni[n.neatID]);

            //assoni
            end1 = !enumAssGen1.MoveNext();
            end2 = !enumAssGen2.MoveNext();
            while (!(end1 && end2))
            {
                if (end1)
                {
                    //assoni excess presenti solo in gen2
                    if ((fitness2 > fitness1) && (random.NextDouble() < Params.mostFitParentInheritingProbability))
                    {
                        addAssone(enumAssGen2.Current.Value);
                        if (!assoni[enumAssGen2.Current.Key].attivo && (random.NextDouble() < Params.disabledGeneEnablingProbability))
                            assoni[enumAssGen2.Current.Key].attiva();
                    }
                    end2 = !enumAssGen2.MoveNext();
                }
                else if (end2)
                {
                    //assoni excess presenti solo in gen1
                    if ((fitness1 > fitness2) && (random.NextDouble() < Params.mostFitParentInheritingProbability))
                    {
                        addAssone(enumAssGen1.Current.Value);
                        if (!assoni[enumAssGen1.Current.Key].attivo && (random.NextDouble() < Params.disabledGeneEnablingProbability))
                            assoni[enumAssGen1.Current.Key].attiva();
                    }
                    end1 = !enumAssGen1.MoveNext();
                }
                else
                {
                    if (enumAssGen1.Current.Key == enumAssGen2.Current.Key)
                    {
                        double numRand = random.NextDouble();
                        if (((fitness1 > fitness2) && (numRand < Params.mostFitParentInheritingProbability)) || ((fitness1 < fitness2) && (numRand > Params.mostFitParentInheritingProbability)))
                            addAssone(enumAssGen1.Current.Value);
                        else
                            addAssone(enumAssGen2.Current.Value);
                        if (!assoni[enumAssGen1.Current.Key].attivo && !assoni[enumAssGen2.Current.Key].attivo && (random.NextDouble() < Params.disabledGeneEnablingProbability))
                            assoni[enumAssGen1.Current.Key].attiva();
                        end1 = !enumAssGen1.MoveNext();
                        end2 = !enumAssGen2.MoveNext();
                    }
                    else if (enumAssGen1.Current.Key > enumAssGen2.Current.Key)
                    {
                        //gli assoni excess o disjoint sono ereditati solo se vengono dal genotipo con fitness maggiore e con probabilità Params.mostFitParentInheritingProbability
                        if ((fitness2 > fitness1) && (random.NextDouble() < Params.mostFitParentInheritingProbability))
                            addAssone(enumAssGen2.Current.Value);
                        end2 = !enumAssGen2.MoveNext();
                    }
                    else //if(enumAssGen1.Current.Key < enumAssGen2.Current.Key)
                    {
                        if ((fitness1 > fitness2) && (random.NextDouble() < Params.mostFitParentInheritingProbability))
                            addAssone(enumAssGen1.Current.Value);
                        end1 = !enumAssGen1.MoveNext();
                    }
                }
            }

            //elimina i neuroni non raggiunti da nessun arco
            SortedList<int, NeuroneG> neuroniDaEliminare = new SortedList<int, NeuroneG>(neuroni);
            //i neuroni di input e di output sono preservati in ogni caso
            foreach (NeuroneG neurone in neuroniInput)
                neuroniDaEliminare.Remove(neurone.neatID);
            foreach (NeuroneG neurone in neuroniOutput)
                neuroniDaEliminare.Remove(neurone.neatID);
            //salva i neuroni nascosti che hanno almeno un assone entrante e uno uscente
            foreach (KeyValuePair<int, NeuroneG> neurone in neuroni)
            {
                if (neuroniDaEliminare.Count == 0)
                    break;
                if (neurone.Value.numAssoniInput > 0 && neurone.Value.numAssoniOutput > 0)
                    neuroniDaEliminare.Remove(neurone.Key);
            }
            //elimina i neuroni non salvati
            foreach (KeyValuePair<int, NeuroneG> neurone in neuroniDaEliminare)
            {
                //elimina eventuali assoni entranti o uscenti da quel neurone
                foreach (int id in neurone.Value.AssoniInput)
                    assoni.Remove(id);
                foreach (int id in neurone.Value.AssoniOutput)
                    assoni.Remove(id);
                neuroni.Remove(neurone.Key);
            }
        }

        #endregion

        #region add

        public void addAssone(AssoneG a)
        { 
            assoni.Add(a.neatID, a);
            neuroni[a.input].addAssoneOutput(a.neatID);
            neuroni[a.output].addAssoneInput(a.neatID);
        }

        public void addNeurone(NeuroneG n)
        { 
            neuroni.Add(n.neatID, n);
            neuroni[n.neatID].clearAssoni();
            neuroni[n.neatID].recreateAssoni();
        }

        public void addNeuroneInput(NeuroneG n)
        {
            addNeurone(n);
            neuroniInput.Add(n);
        }

        public void addNeuroneOutput(NeuroneG n)
        {
            addNeurone(n);
            neuroniOutput.Add(n);
        }

        #endregion

        #region Serializzazione

        public void sendNetwork(NetworkStream stream)
        {
            BinaryFormatter br = new BinaryFormatter();
            br.Serialize(stream, this);
            return;
        }

        public static GenotipoRN receiveNetwork(NetworkStream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();
            GenotipoRN g = (GenotipoRN)bf.Deserialize(stream);
            return g;
        }

        /* non testata */
        public bool sendFile(String NomeFile)
        {
            bool RetValue = true;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter br = new BinaryFormatter();

            using (FileStream fs = new FileStream(NomeFile, FileMode.OpenOrCreate))
            {
                try
                {
                    br.Serialize(ms, this);
                    ms.WriteTo(fs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    RetValue = false;
                }
                finally
                {
                    fs.Close();
                }

            }
            return RetValue;

        }

        /* non testata */
        public static GenotipoRN receiveFile(String NomeFile)
        {
            using (FileStream fs = new FileStream(NomeFile, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    return (GenotipoRN)bf.Deserialize(fs);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    fs.Close();
                }

            }
        }

        #endregion

        public String ToString()
        {
            String s = "";
            foreach (KeyValuePair<int, AssoneG> a in assoni)
                s += a.Value.ToString() + "\n";
            return s;
        }

        public int CompareTo(GenotipoRN other)
        { return t - other.t; }

        public bool contieneNeuroneID(int n)
        {
            return neuroni.ContainsKey(n);
        }

        public bool contieneAssoneID(int n)
        {
            return assoni.ContainsKey(n);
        }

        public String firma()
        {
            return nome;
        }

        #region Enumeratori
        public IEnumerator<KeyValuePair<int,NeuroneG>> GetEnumeratorNeuroni()
        {
            return neuroni.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<int, AssoneG>> GetEnumeratorAssoni()
        {
            return assoni.GetEnumerator();
        }

        public IEnumerator<NeuroneG> GetEnumeratorNeuroniInput()
        {
            return neuroniInput.GetEnumerator();
        }

        public IEnumerator<NeuroneG> GetEnumeratorNeuroniOutput()
        {
            return neuroniOutput.GetEnumerator();
        }
        #endregion
    }

}
