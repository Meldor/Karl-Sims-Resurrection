using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace ClientKSR
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Boolean connesso = false, activeBB=false;
        TcpClient client;
        NetworkStream stream;
        String console = "";
        String ip="127.0.0.1";
        Thread trdBB;
        int porta;
        delegate void funzioneVoid();
        delegate System.Windows.Threading.DispatcherProcessingDisabled funzioneVar();

        public MainWindow()
        {
           InitializeComponent(); 
        }


        private void connectButtonClick(object sender, RoutedEventArgs e)
        {
            if (!connesso)
            {
                try
                {
                    porta = System.Convert.ToInt32(portaBox.Text);
                    ip = ipBox.Text;
                    
                    client = new TcpClient(ip, porta);
                    stream = client.GetStream();
                    console = String.Concat(console, "Connessione avvenuta...\n");
                    consoleBox.Text = console;
                    controlliTab.IsEnabled = true;
                    connesso = true;
                    connectButton.Content = "SignOut";

                }
                catch
                {
                    consoleBox.Text = "Errore di connessione...";
                }
            }
            else
            {
                connesso = false;
                stream.Close();
                client.Close();
                connectButton.Content = "Connect";
                consoleBox.Text = "Disconnesso...";
            }

        }

        public string inviaComando(string comando, NetworkStream _stream, Boolean verbose, string _consoleText, TextBox _box)
        {
            Int32 bytes_ricevuti;
            String messaggio=comando;
            Byte[] data;
            
            data = System.Text.Encoding.ASCII.GetBytes(messaggio);
            _stream.Write(data, 0, data.Length);

            if (verbose)
            {
                _consoleText = String.Concat(_consoleText, "Inviato -> ", messaggio, "\n");
                if (!_box.CheckAccess())
                    _box.Dispatcher.Invoke((funzioneVoid)delegate() { _box.Text = _consoleText; }); 
                else
                    _box.Text = _consoleText;
            }
            
            data = new Byte[256];
            
            while((bytes_ricevuti = _stream.Read(data, 0, data.Length))==0);
            messaggio = System.Text.Encoding.ASCII.GetString(data, 0, bytes_ricevuti);

            if (verbose)
            {
                _consoleText = String.Concat(_consoleText, "Ricevuto -> ", messaggio, "\n");
                if (!_box.CheckAccess())
                    _box.Dispatcher.Invoke((funzioneVoid)delegate() { _box.Text = _consoleText; });
                else
                    _box.Text = _consoleText;
            }

            return messaggio;
        
        }

        private void disegnaRadio_Checked(object sender, RoutedEventArgs e)
        {
            if(connesso)
                inviaComando("d", stream, true, console, consoleBox);

        }

        private void testRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (connesso)
                inviaComando("b", stream, true, console, consoleBox);
        }

        private void aggiornaListe(List<string[]> listaDati)
        {
           
            List<string> xStringa = new List<string>();
            List<string> yStringa = new List<string>();
            List<string> angoloStringa = new List<string>();

            //elementiList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { elementiList.Items.Clear(); });
            //posizioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { posizioneList.Items.Clear(); });
            //rotazioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { rotazioneList.Items.Clear(); });
            
            foreach (string[] vettStr in listaDati)
            {
                if (vettStr.Length == 3)
                {

                    xStringa.Add(vettStr[0]);
                    yStringa.Add(vettStr[1]);
                    angoloStringa.Add(vettStr[2]);
                    //elementiList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { elementiList.Items.Add(vettStr[0]); });
                    //posizioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { posizioneList.Items.Add(vettStr[1]); });
                    //rotazioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { rotazioneList.Items.Add(vettStr[2]); });
                }
            }



            elementiList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { elementiList.ItemsSource = xStringa; });
            posizioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { posizioneList.ItemsSource = yStringa; });
            rotazioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { rotazioneList.ItemsSource = angoloStringa; });

            return;
        
        
        }
        
        private void bigBrother(Object oggetto)
        {
            TextBox _box =(TextBox)oggetto;
            double angolo; 
            int gradi;
            TcpClient clientBB;
            NetworkStream streamBB;
            String ipBB = "127.0.0.1";
            String messaggio, console="" ;
            String[] vettDati;
            List<string[]> listaDati;
            int portaBB=14000;
           
            clientBB = new TcpClient(ipBB, portaBB);
            streamBB = clientBB.GetStream();

            while (activeBB)
            {
                listaDati = new List<string[]>();
                System.Threading.Thread.Sleep(50);  
                messaggio = inviaComando("*", streamBB, false, console, _box);
                //MessageBox.Show(messaggio);
                              
                while (messaggio != "#")
                {
                    
                    messaggio = inviaComando("*", streamBB, false, console, _box);
                    vettDati=messaggio.Split('\n');
                    if (vettDati.Length == 3)
                    {
                        angolo = System.Convert.ToDouble(vettDati[2]);
                        angolo = (angolo * 180) / Math.PI;
                        gradi = System.Convert.ToInt32(angolo);
                        vettDati[2] = System.Convert.ToString(gradi);
                    }
                    listaDati.Add(vettDati);
                    //MessageBox.Show(messaggio);
                }

                aggiornaListe(listaDati);           
            
            }

            inviaComando("#", streamBB, false, console, _box);

            streamBB.Close();
            clientBB.Close();
        
        }

        private void aggionaBox()
        {
            consoleBBbox.Text += "Hello from a static thread procedure.\n";
            return;
        }

        private void bigBrotherButton_Click(object sender, RoutedEventArgs e)
        {
            if (!activeBB)
            {
                inviaComando("BB", stream, true, console, consoleBox);
                activeBB = true;
                bigBrotherButton.Content = "Stop";
                trdBB = new Thread(new ParameterizedThreadStart(this.bigBrother));
                trdBB.IsBackground = true;
                trdBB.Start(consoleBBbox);
            }
            else
            {
                activeBB = false;
                bigBrotherButton.Content = "Avvia Controllo";
            
            }


        }

      

      
    }
}
