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
    /// 

    delegate void funzioneVoid();

    public partial class MainWindow : Window
    {
        Boolean connesso = false, activeBB=false;
        Thread trdBB;
        MyClient clientPrincipale;

        public MainWindow()
        {
           InitializeComponent(); 
        }

        private void connectButtonClick(object sender, RoutedEventArgs e)
        {
            if (!connesso)
            {
                clientPrincipale = new MyClient(ipBox.Text, System.Convert.ToInt32(portaBox.Text), consoleBox);
                if (clientPrincipale.connect())
                {
                    connesso = true;
                    controlliTab.IsEnabled = true;
                    connectButton.Content = "Disconnect";
                }
                else
                    consoleBox.Text="Errore di connessione...";
                
            }
            else
            {
                if (activeBB)
                    MessageBox.Show("Disconnettere il controllo in RealTime");
                else
                {
                    clientPrincipale.inviaComando("exit");
                    clientPrincipale.disconnect();
                    connectButton.Content = "Connect";
                    controlliTab.IsEnabled = false;
                    connesso = false;
                }
            }

        }


        private void disegnaRadio_Checked(object sender, RoutedEventArgs e)
        {
            if(connesso)
                clientPrincipale.inviaComando("d");
            return;

        }

        private void testRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (connesso)
                clientPrincipale.inviaComando("b");
            return;
        }

        private void aggiornaListe(List<string[]> listaDati)
        {
            List<string> xStringa = new List<string>();
            List<string> yStringa = new List<string>();
            List<string> angoloStringa = new List<string>();

            foreach (string[] vettStr in listaDati)
            {
                if (vettStr.Length == 3)
                {

                    xStringa.Add(vettStr[0]);
                    yStringa.Add(vettStr[1]);
                    angoloStringa.Add(vettStr[2]);
                }
            }
            
            elementiList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { elementiList.ItemsSource = xStringa; });
            posizioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { posizioneList.ItemsSource = yStringa; });
            rotazioneList.Dispatcher.BeginInvoke((funzioneVoid)delegate() { rotazioneList.ItemsSource = angoloStringa; });

            return;
        }
        
        private void bigBrother(Object oggetto)
        {
            MyClient clientBB;
            String[] vettDati;
            List<string[]> listaDati = new List<string[]>();
            string messaggio;
            TextBox consoleBox= (TextBox)oggetto;

            clientBB = new MyClient("127.0.0.1", 14000, consoleBox);
            if (!clientBB.connect())
            {
                activeBB = false;
                bigBrotherButton.Dispatcher.BeginInvoke((funzioneVoid)delegate() { bigBrotherButton.Content = "Avvia Controllo"; });
            }
            else
            {
                clientBB.writeConsole("Connesso...\n");
                while (activeBB)
                {
                    listaDati.Clear();
                    System.Threading.Thread.Sleep(50);

                    messaggio = clientBB.inviaComando("*", false);

                    while (messaggio != "#")
                    {
                        messaggio = clientBB.inviaComando("*", false);
                        vettDati = messaggio.Split('\n');
                        if (vettDati.Length == 3)
                            vettDati[2] = ConvertRadiantString(vettDati[2]);
                        listaDati.Add(vettDati);
                    }

                    aggiornaListe(listaDati);
                }
                clientBB.inviaComando("#", false);
                clientBB.writeConsole("Disconnessione...\n");
                clientBB.disconnect();
            }
            return;
        }

        private string ConvertRadiantString(string stringa)
        {
            double radianti = (System.Convert.ToDouble(stringa) * 180) / Math.PI;
            return System.Convert.ToString(System.Convert.ToInt32(radianti));
        }

        private void bigBrotherButton_Click(object sender, RoutedEventArgs e)
        {
            if (!activeBB)
            {
                clientPrincipale.inviaComando("BB");
                                
                /* Avvio il thread BigBrother */
                trdBB = new Thread(new ParameterizedThreadStart(this.bigBrother));
                trdBB.IsBackground = true;
                trdBB.Start(consoleBBbox);

                bigBrotherButton.Content = "Stop";
                activeBB = true;
            }
            else
            {
                activeBB = false;
                bigBrotherButton.Content = "Avvia Controllo";
            }
            return;    
        }

           
    }
}
