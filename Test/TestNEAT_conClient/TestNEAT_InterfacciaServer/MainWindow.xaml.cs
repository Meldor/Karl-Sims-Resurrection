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

using LibreriaRN;

namespace TestNEAT_InterfacciaServer
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GestoreRN_NEAT gestore;
        Boolean connesso = false;
        ClientNEAT clientPrincipale;
        Dictionary<String, GenotipoRN> genotipi;
        GenotipoRN genotipoSelezionato;

        delegate void funzioneVoid();

        public MainWindow()
        {
            InitializeComponent();
            genotipi = new Dictionary<String, GenotipoRN>();
        }
        
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!connesso)
            {
                clientPrincipale = new ClientNEAT(ipBox.Text, System.Convert.ToInt32(portaBox.Text), consoleBox);
                if (clientPrincipale.connect())
                {
                    connesso = true;
                    tabSimulazione.IsEnabled = true;
                    connectButton.Content = "Disconnect";
                }
                else
                    consoleBox.Text = "Errore di connessione...";
            }
            else
            {
                clientPrincipale.inviaComando("exit");
                clientPrincipale.disconnect();
                connectButton.Content = "Connect";
                tabSimulazione.IsEnabled = false;
                connesso = false;
                tabFenotipo.IsEnabled = false;
            }
        }

        private void perceptronButton_Click(object sender, RoutedEventArgs e)
        {
            GenotipoRN g;
            genotipi.Clear();
            
            dialogBox.Text = string.Empty;
            visualizzaButton.IsEnabled = false;
            addNeuroneButton.IsEnabled = false;
            addAssoneButton.IsEnabled = false;
            modPesoButton.IsEnabled = false;

            gestore = new GestoreRN_NEAT(Convert.ToInt32(inputBox.Text), Convert.ToInt32(outputBox.Text));
            g = gestore.getPerceptron();
            genotipi.Add(g.firma(), g);

            aggiornaLista();
        }

        private void aggiornaLista()
        {
            listBox1.Items.Clear();
            foreach (String s in genotipi.Keys)
            { listBox1.Items.Add(s); }
        }
        private void aggiornaDialogBox()
        {
            dialogBox.Text = genotipoSelezionato.toString();
        }

        private void visualizzaButton_Click(object sender, RoutedEventArgs e)
        {
            clientPrincipale.send("IRN");
            
            //GenotipoRN g = gestore.getPerceptron();
            genotipoSelezionato.sendNetwork(clientPrincipale.getStream());
            
            String messaggio = clientPrincipale.receive();
            clientPrincipale.writeConsole(messaggio);

            tabFenotipo.IsEnabled = true;
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            
        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            String firma = (String)((ListBox)sender).SelectedItem;

            genotipoSelezionato = genotipi[firma];
            aggiornaDialogBox();
            visualizzaButton.IsEnabled = true;
            addNeuroneButton.IsEnabled = true;
            addAssoneButton.IsEnabled = true;
            modPesoButton.IsEnabled = true;
            //MessageBox.Show(firma, "My Application");
        }

        private void addNeuroneButton_Click(object sender, RoutedEventArgs e)
        {
            GenotipoRN mutato = gestore.mutazioneAggiungiNeurone(genotipoSelezionato);
            genotipi.Add(mutato.firma(), mutato);
            aggiornaLista();
        }

        private void addAssoneButton_Click(object sender, RoutedEventArgs e)
        {
            GenotipoRN mutato = gestore.mutazioneAggiungiAssone(genotipoSelezionato);
            genotipi.Add(mutato.firma(), mutato);
            aggiornaLista();
        }

        private void modPesoButton_Click(object sender, RoutedEventArgs e)
        {
            GenotipoRN mutato = gestore.mutazioneModificaPesoUniformemente(genotipoSelezionato);
            genotipi.Add(mutato.firma(), mutato);
            aggiornaLista();
        }

        private void generaFenotipoButton_Click(object sender, RoutedEventArgs e)
        {
            clientPrincipale.send("GF");

            String messaggio = clientPrincipale.receive();
            clientPrincipale.writeConsole(messaggio);

            sendInputButton.IsEnabled = true;
            inputFenotipoBox.IsEnabled = true;
            outputBox.IsEnabled = true;
            aggiornaButton.IsEnabled = true;
        }

        private void sendInputButton_Click(object sender, RoutedEventArgs e)
        {
            clientPrincipale.send("INPUT");

            clientPrincipale.send(inputFenotipoBox.Text);

            String messaggio = clientPrincipale.receive();
            clientPrincipale.writeConsole(messaggio);
        }

        private void aggiornaButton_Click(object sender, RoutedEventArgs e)
        {
            clientPrincipale.send("AGG");

            outputFenotipoBox.Text = clientPrincipale.receive();
        }
    }
}
