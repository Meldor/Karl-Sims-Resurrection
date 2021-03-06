﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using System.Net;
using System.Net.Sockets;

namespace ClientKSR
{
    class MyClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private TextBox consoleBox;
        private Boolean verbose;
        private string consoleText=string.Empty;
        private string ip;
        private int port;

        public MyClient(string _ip, int _port)
        {
            ip = _ip;
            port = _port;
            verbose = false;
        }

        public MyClient(string _ip, int _port, TextBox _box)
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


        public string inviaComando(string comando)
        {
            Int32 bytes_ricevuti;
            String messaggio = comando;
            Byte[] data;
                        

            /*Invio Dati*/
            data = System.Text.Encoding.ASCII.GetBytes(messaggio);
            stream.Write(data, 0, data.Length);
            writeConsole("Inviato -> " + messaggio + "\n");

            /*Ricezione Dati*/
            data = new Byte[256];
            while ((bytes_ricevuti = stream.Read(data, 0, data.Length)) == 0) ;        //Aspetta la risposta del Server
            messaggio = System.Text.Encoding.ASCII.GetString(data, 0, bytes_ricevuti);
            writeConsole("Ricevuto -> " + messaggio + "\n");

            
            return messaggio;

        }

        public string inviaComando(string comando, bool _verbose)
        {
            bool change = false;
            Int32 bytes_ricevuti;
            String messaggio = comando;
            Byte[] data;

            if (!_verbose && verbose)
            { verbose = false; change = true; }

            /*Invio Dati*/
            data = System.Text.Encoding.ASCII.GetBytes(messaggio);
            stream.Write(data, 0, data.Length);
            writeConsole("Inviato -> " + messaggio + "\n");
            
            /*Ricezione Dati*/
            data = new Byte[256];
            while ((bytes_ricevuti = stream.Read(data, 0, data.Length)) == 0) ;        //Aspetta la risposta del Server
            messaggio = System.Text.Encoding.ASCII.GetString(data, 0, bytes_ricevuti);
            writeConsole("Ricevuto -> " + messaggio + "\n");

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
