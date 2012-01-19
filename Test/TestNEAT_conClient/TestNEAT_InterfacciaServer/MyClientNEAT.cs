using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

using System.Net;
using System.Net.Sockets;

namespace TestNEAT_InterfacciaServer
{
    class ClientNEAT
    {
        private TcpClient client;
        private NetworkStream stream;
        private TextBox consoleBox;
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
            verbose = false;
        }

        public ClientNEAT(string _ip, int _port, TextBox _box)
        {
            ip = _ip;
            port = _port;
            verbose = false;
            consoleBox = _box;
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
            {
                consoleText += text;
                if (!consoleBox.CheckAccess())
                    consoleBox.Dispatcher.Invoke((funzioneVoid)delegate() { consoleBox.Text = consoleText; });
                else
                    consoleBox.Text = consoleText;
            }
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
    
    


