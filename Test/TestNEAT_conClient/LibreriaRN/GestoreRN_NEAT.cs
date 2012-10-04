using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibreriaRN
{
    public class GestoreRN_NEAT
    {
        private int contNeuroni;
        private int contAssoni;
        private int contGenotipo;
        public ICollection<GenotipoRN> genotipi;
        public List<GenotipoRN> population;
        public List<GenotipoRN> futurePopulation;

        GenotipoRN perceptron;
        SpeciesManager speciesManager;

        public GestoreRN_NEAT(int input, int output, int perceptronNumber=1)
        {
            contGenotipo = -1;
            contNeuroni = 0;
            contAssoni = 0;
            genotipi = new SortedSet<GenotipoRN>();
            population = new List<GenotipoRN>();
            futurePopulation = new List<GenotipoRN>();

            speciesManager = new SpeciesManager(this);

            perceptron=generaPerceptron(input, output);
            for (int i = 0; i < perceptronNumber;i++)
            {
                speciesManager.addGenotipo(mutazioneModificaPesoRadicalmenteTuttiAssoni(getPerceptron()));          
            }

            population = speciesManager.GetListGenotipo();
        }

        public GenotipoRN GetGenotipo()
        {
            GenotipoRN g;

            if (contGenotipo >= population.Count)
            {
                contGenotipo = -1;
                speciesManager.DoNextGeneration();
                population = speciesManager.GetListGenotipo();
                
            }
            contGenotipo++;
            return population[contGenotipo];
        }

        public GenotipoRN getPerceptron()
        { return perceptron; }

        private GenotipoRN generaPerceptron(int input, int output)
        {
            Random generatoreCasuale = new Random();
            GenotipoRN p = new GenotipoRN();
            p.t = 0;

            for (int i = 0; i < input; i++)
                p.addNeuroneInput(new GenotipoRN.NeuroneG(contNeuroni++, TipoNeurone.NSensor));

            for (int i = 0; i < output; i++)
                p.addNeuroneOutput(new GenotipoRN.NeuroneG(contNeuroni++, TipoNeurone.NActuator));

            for(int i = 0; i < input; i++)
                for (int j = 0; j < output; j++)
                {
                    double peso = generatoreCasuale.NextDouble();
                    p.addAssone(new GenotipoRN.AssoneG(contAssoni, i, j+input, 1 - 2 * peso));
                    contAssoni++;
                }

            genotipi.Add(p);
            return p;
        }

        public GenotipoRN[] mutazione(GenotipoRN genotipo, int num)
        {
            GenotipoRN[] r = new GenotipoRN[num];
            Random generatoreCasuale = new Random();
            int numero;

            for (int i = 0; i < num; i++)
            {
                numero = generatoreCasuale.Next(100);
                if (numero < 3)
                    r[i] = mutazioneAggiungiNeurone(genotipo);
                else if (numero < 8)
                    r[i] = mutazioneAggiungiAssone(genotipo);
                else if (numero < 88)
                {
                    numero = generatoreCasuale.Next(100);
                    if (numero < 90)
                        r[i] = mutazioneModificaPesoUniformemente(genotipo);
                    else
                        r[i] = mutazioneModificaPesoRadicalmente(genotipo);
                }
                genotipi.Add(r[i]);
            }

            return r;

        }

        public GenotipoRN mutazioneAggiungiNeurone(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();

            int num = generatoreCasuale.Next(g.assoni.Count);

            GenotipoRN.AssoneG assoneCorrente = g.assoni[g.assoni.Keys[num]];
            assoneCorrente.attivo = false;
            g.assoni[g.assoni.Keys[num]] = assoneCorrente;

            g.addNeurone(new GenotipoRN.NeuroneG(contNeuroni, TipoNeurone.NHide));
            g.addAssone(new GenotipoRN.AssoneG(contAssoni, g.assoni[g.assoni.Keys[num]].input, contNeuroni, 1));
            contAssoni++;
            g.addAssone(new GenotipoRN.AssoneG(contAssoni, contNeuroni, g.assoni[g.assoni.Keys[num]].output, g.assoni[g.assoni.Keys[num]].peso));
            contAssoni++;
            contNeuroni++;
            return g;
        }

        public GenotipoRN mutazioneAggiungiAssone(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();
            bool esiste = false;

            int neurone1;
            int neurone2;

            int indice = genotipo.neuroni.Count;


            /* ho impedito la possibilità di creare assoni che hanno come sorgente un nodo di output o come destinazione un nodo di input
             * perché oltre a dare problemi di visualizzazione mi sembra abbiano poco senso: credo che i neuroni di input debbano semplicemente
             * riportare gli input alla rete (secondo me senza farci nessuna elaborazione però si può consentire che il neurone di input abbia una
             * funzione di soglia disabilitando Params.transparentInput) così come i neuroni di output debbano solo portare le uscite agli attuatori.
             */
            do
                neurone1 = genotipo.neuroni.Keys[generatoreCasuale.Next(indice)];
            while (genotipo.neuroni[neurone1].tipo == TipoNeurone.NActuator);
            do
                neurone2=genotipo.neuroni.Keys[generatoreCasuale.Next(indice)];
            while (genotipo.neuroni[neurone2].tipo == TipoNeurone.NSensor);

            for (int i = 0; i < g.numeroAssoni; i++)
                /* io annullerei la possibilità di raddoppio perché trasformerebbe una mutazione del tipo "aggiungi assone" in una "modifica peso"
                 * anche se occorrerebbe gestire in qualche modo il caso in cui si vorrebbe creare un assone già esistente. Continuando a generare a caso
                 * input e output finché non si trovano due neuroni ancora non collegati si rischia infatti o di metterci molto tempo o peggio di entrare
                 * in un ciclo infinito se tutti i collegamenti fossero già stabiliti, situazioni non impossibili con pochi neuroni.
                 */
                if (g.assoni[i].testaCollegamento(neurone1, neurone2))
                {
                    GenotipoRN.AssoneG assone = g.assoni[g.assoni.Keys[i]];
                    assone.raddoppia();
                    g.assoni[g.assoni.Keys[i]] = assone;
                    esiste = true;
                    Console.WriteLine("Raddoppio " + neurone1 + " - " + neurone2);
                    break;
                }

            if (!esiste)
            {
                g.addAssone(new GenotipoRN.AssoneG(contAssoni, neurone1, neurone2, generatoreCasuale.NextDouble()));
                contAssoni++;
            }

            return g;
        }

        public GenotipoRN mutazioneModificaPesoUniformemente(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();

            int num = generatoreCasuale.Next(g.assoni.Count);

            GenotipoRN.AssoneG assoneCorrente = g.assoni[g.assoni.Keys[num]];
            assoneCorrente.modPeso(Utilita.pesoUniforme);
            g.assoni[g.assoni.Keys[num]] = assoneCorrente;
            
            return g;
        }

        private GenotipoRN mutazioneModificaPesoRadicalmente(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();

            int num = generatoreCasuale.Next(g.assoni.Count);

            GenotipoRN.AssoneG assoneCorrente = g.assoni[g.assoni.Keys[num]];
            assoneCorrente.modPeso(1-2*generatoreCasuale.NextDouble());
            g.assoni[g.assoni.Keys[num]] = assoneCorrente;

            return g;
        }

        private GenotipoRN mutazioneModificaPesoRadicalmenteTuttiAssoni(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();

            for (int num = 0; num < g.assoni.Count; num++)
            {
                GenotipoRN.AssoneG assoneCorrente = g.assoni[g.assoni.Keys[num]];
                assoneCorrente.modPeso(1 - 2 * generatoreCasuale.NextDouble());
                g.assoni[g.assoni.Keys[num]] = assoneCorrente;
            }
            return g;
        }
    }
}
