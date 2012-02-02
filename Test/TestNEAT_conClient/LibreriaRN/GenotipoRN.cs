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
            public int idNEAT;
            int tipo;

            public NeuroneG(int idNEAT, int tipo)
            {
                this.idNEAT = idNEAT;
                this.tipo = tipo;
            }

            public int GetId()
            { return idNEAT; }

            public int CompareTo(NeuroneG other)
            { return idNEAT - other.idNEAT; }
        }

        [Serializable]
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

            public int GetId()
            { return idNEAT; }

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

            public void modPeso(double p)
            { peso += p; }

            public void raddoppia()
            { peso *= 2; }

            public bool testaCollegamento(int input, int output)
            { return (this.input == input && this.output == output); }

            public String toString()
            { return "ID: " + idNEAT + " -> " + input + " - " + output + " Peso " + peso + " Attivo: " + attivo; }

            public int CompareTo(AssoneG other)
            { return idNEAT - other.idNEAT; }
        }

        public int t;
        public List<AssoneG> assoni;
        public SortedList<int, NeuroneG> neuroni;
        public ICollection<NeuroneG> neuroniInput;
        public ICollection<NeuroneG> neuroniOutput;
        public String nome;

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

        public void addAssone(AssoneG a)
        { assoni.Add(a); }

        public void addNeurone(NeuroneG n)
        { neuroni.Add(n.GetId(), n); }

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

        public int getNumeroAssoni()
        { return assoni.Count; }

        public int getNumeroNeuroni()
        { return neuroni.Count; }

        public int getNumeroNeuroniInput()
        { return neuroniInput.Count; }

        public String toString()
        {
            String s = "";
            foreach (AssoneG a in assoni)
                s += a.toString() + "\n";
            return s;
        }

        public String firma()
        { return nome; }

        public int CompareTo(GenotipoRN other)
        { return t - other.t; }

        public bool contieneNeuroneID(int n)
        {
            NeuroneG[] neuroniVector = neuroni.Values.ToArray();
            bool trovato = false;

            for (int i = 0; i < neuroni.Count && !trovato; i++)
                if (neuroniVector[i].idNEAT == n)
                    trovato = true;
            return trovato;
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

    }

}
