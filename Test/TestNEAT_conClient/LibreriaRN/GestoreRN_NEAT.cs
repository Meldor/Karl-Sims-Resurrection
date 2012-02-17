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
        public ICollection<GenotipoRN> genotipi;

        public GestoreRN_NEAT(int input, int output)
        {
            contNeuroni = 0;
            contAssoni = 0;
            genotipi = new SortedSet<GenotipoRN>();
            generaPerceptron(input, output);
        }

        public GenotipoRN getPerceptron()
        { return genotipi.First(); }

        private void generaPerceptron(int input, int output)
        {
            Random generatoreCasuale = new Random();
            GenotipoRN p = new GenotipoRN();
            p.t = 0;

            for (int i = 0; i < input; i++)
                p.addNeuroneInput(new GenotipoRN.NeuroneG(contNeuroni++, 0));

            for (int i = 0; i < output; i++)
                p.addNeuroneOutput(new GenotipoRN.NeuroneG(contNeuroni++, 0));
                       
            //for (int i = 0; i < (input * output); i++)
            //{
            //    int I = i % input;
            //    int O = i % output + input;
            //    double peso = generatoreCasuale.NextDouble();
            //    p.addAssone(new GenotipoRN.AssoneG(contAssoni, I, O, 1 - 2 * peso));
            //    contAssoni++;
            //}

            for(int i = 0; i < input; i++)
                for (int j = 0; j < output; j++)
                {
                    double peso = generatoreCasuale.NextDouble();
                    p.addAssone(new GenotipoRN.AssoneG(contAssoni, i, j+input, 1 - 2 * peso));
                    contAssoni++;
                }

            genotipi.Add(p);
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

            GenotipoRN.AssoneG assoneCorrente = g.assoni[num];
            assoneCorrente.attivo = 0;
            g.assoni[num] = assoneCorrente;

            g.addNeurone(new GenotipoRN.NeuroneG(contNeuroni, 0));
            g.addAssone(new GenotipoRN.AssoneG(contAssoni, g.assoni[num].getInput(), contNeuroni, 1));
            contAssoni++;
            g.addAssone(new GenotipoRN.AssoneG(contAssoni, contNeuroni, g.assoni[num].getOutput(), g.assoni[num].getPeso()));
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
            

            neurone1=genotipo.neuroni[generatoreCasuale.Next(indice)].GetId();
            neurone2=genotipo.neuroni[generatoreCasuale.Next(indice)].GetId();

            for (int i = 0; i < g.getNumeroAssoni(); i++)
                if (g.assoni[i].testaCollegamento(neurone1, neurone2))
                {
                    GenotipoRN.AssoneG assone = g.assoni[i];
                    assone.raddoppia();
                    g.assoni[i] = assone;
                    esiste = true;
                    Console.WriteLine("Raddoppio " + neurone1 + " - " + neurone2);
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

            GenotipoRN.AssoneG assoneCorrente = g.assoni[num];
            assoneCorrente.modPeso(Utilita.pesoUniforme);
            g.assoni[num] = assoneCorrente;
            
            return g;
        }

        private GenotipoRN mutazioneModificaPesoRadicalmente(GenotipoRN genotipo)
        {
            GenotipoRN g = new GenotipoRN(genotipo);
            Random generatoreCasuale = new Random();

            int num = generatoreCasuale.Next(g.assoni.Count);

            GenotipoRN.AssoneG assoneCorrente = g.assoni[num];
            assoneCorrente.modPeso(1-2*generatoreCasuale.NextDouble());
            g.assoni[num] = assoneCorrente;

            return g;
        }

    }
}
