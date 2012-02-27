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
            public thresholdFunction threshold
            {
                get { return _threshold; }
            }
            thresholdFunction _threshold;
            /// <summary>
            /// Indice della funzione di soglia
            /// </summary>
            public int thresholdIndex
            {
                get
                {
                    if(_threshold == Threshold.SigmoideAttenuante)
                        return 0;
                    else if(_threshold == Threshold.IperbolicTan)
                        return 1;
                    else if(_threshold == Threshold.Modulo)
                        return 2;
                    else if(_threshold == Threshold.Gaussian)
                        return 3;
                    else if(_threshold == Threshold.Transparent)
                        return 4;
                    else if(_threshold == Threshold.Sin)
                        return 5;
                    else
                        return 6;
                }
            }

            #region Costruttori

            public NeuroneG(int idNEAT, TipoNeurone tipo, thresholdFunction threshold)
            {
                _neatID = idNEAT;
                _tipo = tipo;
                if (Params.transparentInput && tipo == TipoNeurone.NSensor)
                    _threshold = Threshold.Transparent;
                else
                    _threshold = threshold;
            }

            public NeuroneG(int idNEAT, TipoNeurone tipo)
            {
                _neatID = idNEAT;
                _tipo = tipo;
                if (Params.transparentInput && tipo == TipoNeurone.NSensor)
                    _threshold = Threshold.Transparent;
                else
                    _threshold = Threshold.getRandomDelegate();
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
        public List<AssoneG> assoni;
        public SortedList<int, NeuroneG> neuroni;
        public ICollection<NeuroneG> neuroniInput;
        public ICollection<NeuroneG> neuroniOutput;
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

        #region Costruttori

        public GenotipoRN()
        {
            t = -1;
            assoni = new List<AssoneG>();
            neuroni = new SortedList<int, NeuroneG>();
            neuroniInput = new List<NeuroneG>();
            neuroniOutput = new List<NeuroneG>();
            nome = Utilita.RandomString(5, false);
        }

        public GenotipoRN(GenotipoRN g)
        {
            t = g.t + 1;
            assoni = new List<AssoneG>(g.assoni);
            neuroni = new SortedList<int, NeuroneG>(g.neuroni);
            neuroniInput = new List<NeuroneG>(g.neuroniInput);
            neuroniOutput = new List<NeuroneG>(g.neuroniOutput);
            nome = Utilita.RandomString(5, false);
        }

        #endregion

        #region add

        public void addAssone(AssoneG a)
        { assoni.Add(a); }

        public void addNeurone(NeuroneG n)
        { neuroni.Add(n.neatID, n); }

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

        public String toString()
        {
            String s = "";
            foreach (AssoneG a in assoni)
                s += a.ToString() + "\n";
            return s;
        }

        public int CompareTo(GenotipoRN other)
        { return t - other.t; }

        public bool contieneNeuroneID(int n)
        {
            return neuroni.ContainsKey(n);
        }

        public String firma()
        {
            return nome;
        }
    }

}
