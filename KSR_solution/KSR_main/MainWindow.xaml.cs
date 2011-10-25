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



namespace KSR_main
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KSR_library.ReteNeurale myNetwork;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void makeCreature_Button_Click(object sender, RoutedEventArgs e)
        {
            myNetwork = new KSR_library.ReteNeurale(1,3);
            myNetwork.generaPercettron();

            

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SortedList<int, double> lista;
            Double[] vett;

            vett = new Double[3];
            vett[0] = Convert.ToDouble(textBox1.Text);
            vett[1] = Convert.ToDouble(textBox2.Text);
            vett[2] = Convert.ToDouble(textBox3.Text);

            myNetwork.sensori(vett);
            listParti_ListBox.Items.Clear();
            lista = myNetwork.Next();
            foreach (Double val in lista.Values)
                listParti_ListBox.Items.Add(val.ToString());


        }

       
    }
}
