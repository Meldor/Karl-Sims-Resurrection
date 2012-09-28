using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

using System.Net;
using System.Net.Sockets;

namespace TestCrossover
{
    class ClientNEAT
    {
        private TcpClient client;
        private NetworkStream stream;
        private Boolean verbose;
        private string consoleText=string.Empty;
        private string ip;
        private int port;

        delegate void funzioneVoid();

        public NetworkStream getStream()
        { return stream; }

        public ClientNEAT(string _ip, int _port)
        {
            ip = _ip;
            port = _port;
            verbose = true;
        }

        public bool connect()
        {
            try
            {
                client = new TcpClient(ip, port);
                stream = client.GetStream();
                writeConsole("Connessione Avvenuta...\n");
                return true;
            }
            catch { return false; }          
        
        }

        public void writeConsole(string text)
        {
            if (verbose)
                Console.Write(text);
            return;    
        }

        public void send(string messaggio)
        {
            Byte[] data;
            data = System.Text.Encoding.ASCII.GetBytes(messaggio);
            stream.Write(data, 0, data.Length);
            writeConsole("Inviato -> " + messaggio + "\n");
        }

        public string receive()
        {
            Byte[] data;
            Int32 bytes_ricevuti;
            String messaggio;
            data = new Byte[256];
            while ((bytes_ricevuti = stream.Read(data, 0, data.Length)) == 0) ;
            messaggio = System.Text.Encoding.ASCII.GetString(data, 0, bytes_ricevuti);
            return messaggio;
        }

        

        public string inviaComando(string comando)
        {
            String messaggio;
            
            /* Invio Dati */
            send(comando);
            writeConsole("Inviato -> " + comando + "\n");

            /* Ricezione Dati */
            messaggio = receive();
            writeConsole("Ricevuto -> " + messaggio + "\n");
                        
            return messaggio;
        }

        public string inviaComando(string comando, bool _verbose)
        {
            bool change = false;
            String messaggio = comando;

            if (!_verbose && verbose)
            { verbose = false; change = true; }

            messaggio=inviaComando(comando);

            if (change)
            { verbose = true; }
            
            return messaggio;
        }

        public void disconnect()
        {
            stream.Close();
            client.Close();
            writeConsole("Disconnesso...");
            return;
        }
       
    }

}
    
    


