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

using System.Net;
using System.Net.Sockets;

namespace ClientKSR
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Boolean connesso = false;
        TcpClient client;
        NetworkStream stream;
        String console = "";
        String ip="127.0.0.1";
        int porta;
        
        
        public MainWindow()
        {
            InitializeComponent();
        
            porta = System.Convert.ToInt32(portaBox.Text);
            ip = ipBox.Text;
        }


        private void connectButtonClick(object sender, RoutedEventArgs e)
        {
            if (!connesso)
            {
                try
                {
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

        private void inviaComando(string comando)
        {
            Int32 bytes_ricevuti;
            String messaggio=comando;
            Byte[] data;

            data = System.Text.Encoding.ASCII.GetBytes(messaggio);
            stream.Write(data, 0, data.Length);

            console = String.Concat(console, "Inviato -> ", messaggio, "\n");
            consoleBox.Text = console;

            data = new Byte[256];
            bytes_ricevuti = stream.Read(data, 0, data.Length);
            messaggio = System.Text.Encoding.ASCII.GetString(data, 0, bytes_ricevuti);

            console = String.Concat(console, "Ricevuto -> ", messaggio, "\n");
            consoleBox.Text = console;

            return;

        
        }

        private void disegnaRadio_Checked(object sender, RoutedEventArgs e)
        {
            if(connesso)
                inviaComando("d");

        }

        private void testRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (connesso) 
                inviaComando("b");
        }

      

      
    }
}
