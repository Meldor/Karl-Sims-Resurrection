using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibreriaRN;

namespace TestCrossover
{
    class Program
    {
        static void Main(string[] args)
        {
            GenotipoRN gen1, gen2;
            GestoreRN_NEAT gestore = new GestoreRN_NEAT(3, 2);
            ClientNEAT client;

            gen1 = new GenotipoRN();
            gen1.addNeuroneInput(new GenotipoRN.NeuroneG(1, TipoNeurone.NSensor));
            gen1.addNeuroneInput(new GenotipoRN.NeuroneG(2, TipoNeurone.NSensor));
            gen1.addNeuroneInput(new GenotipoRN.NeuroneG(3, TipoNeurone.NSensor));
            gen1.addNeuroneOutput(new GenotipoRN.NeuroneG(4, TipoNeurone.NActuator));
            gen1.addNeuroneOutput(new GenotipoRN.NeuroneG(5, TipoNeurone.NActuator));
            gen1.addNeurone(new GenotipoRN.NeuroneG(6, TipoNeurone.NHide));
            gen1.addAssone(new GenotipoRN.AssoneG(1, 1, 4, 1));
            gen1.addAssone(new GenotipoRN.AssoneG(2, 2, 6, 1));
            gen1.addAssone(new GenotipoRN.AssoneG(3, 6, 4, 1));
            gen1.addAssone(new GenotipoRN.AssoneG(4, 2, 5, 1));
            gen1.addAssone(new GenotipoRN.AssoneG(5, 3, 5, 1));

            gen2 = new GenotipoRN();
            gen2.addNeuroneInput(new GenotipoRN.NeuroneG(1, TipoNeurone.NSensor));
            gen2.addNeuroneInput(new GenotipoRN.NeuroneG(2, TipoNeurone.NSensor));
            gen2.addNeuroneInput(new GenotipoRN.NeuroneG(3, TipoNeurone.NSensor));
            gen2.addNeuroneOutput(new GenotipoRN.NeuroneG(4, TipoNeurone.NActuator));
            gen2.addNeuroneOutput(new GenotipoRN.NeuroneG(5, TipoNeurone.NActuator));
            gen2.addNeurone(new GenotipoRN.NeuroneG(7, TipoNeurone.NHide));
            gen2.addNeurone(new GenotipoRN.NeuroneG(6, TipoNeurone.NHide));
            gen2.addAssone(new GenotipoRN.AssoneG(1, 1, 4, -1));
            gen2.addAssone(new GenotipoRN.AssoneG(2, 2, 6, -1));
            gen2.addAssone(new GenotipoRN.AssoneG(4, 2, 5, -1));
            gen2.addAssone(new GenotipoRN.AssoneG(5, 3, 5, -1));
            gen2.addAssone(new GenotipoRN.AssoneG(6, 3, 7, -1));
            gen2.addAssone(new GenotipoRN.AssoneG(7, 6, 5, -1));
            gen2.addAssone(new GenotipoRN.AssoneG(8, 7, 5, -1));

            //GenotipoRN gen_tmp1 = gestore.getPerceptron();
            //gen1 = gestore.mutazioneAggiungiNeurone(gen_tmp1);
            //GenotipoRN gen_tmp2 = gestore.mutazioneAggiungiNeurone(gen_tmp1);
            //GenotipoRN gen_tmp3 = gestore.mutazioneModificaPesoUniformemente(gen_tmp2);
            //gen2 = gestore.mutazioneAggiungiNeurone(gen_tmp3);

            double d = AlgGenRN.distanza(gen1, gen2);
            GenotipoRN gen3 = new GenotipoRN(gen1, 1, gen2, 2);

            client = new ClientNEAT("127.0.0.1", 13001);
            if (client.connect())
            {
                client.writeConsole("Client connesso\n");
                client.writeConsole("Premi un tasto per inviare il genitore 1");
                Console.ReadKey();
                client.send("IRN");
                gen1.sendNetwork(client.getStream());
                client.receive();
                client.writeConsole("Premi un tasto per inviare il genitore 2");
                Console.ReadKey();
                client.send("IRN");
                gen2.sendNetwork(client.getStream());
                client.receive();
                Console.WriteLine("Distanza tra le reti: " + AlgGenRN.distanza(gen1, gen2));
                client.writeConsole("Premi un tasto per inviare il figlio");
                Console.ReadKey();
                client.send("IRN");
                gen3.sendNetwork(client.getStream());
                client.receive();
                client.writeConsole("Premi un tasto per uscire");
                Console.ReadKey();
                client.disconnect();
            }
            else
            {
                client.writeConsole("Errore di connessione\n");
                Console.ReadKey();
            }
        }
    }
}
