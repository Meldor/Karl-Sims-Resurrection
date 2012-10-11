using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibreriaRN
{
    public class AlgGenRN
    {
        /// <summary>
        /// Calcola la distanza tra due genotipi come geni_excess*peso1/N + geni_disjoint*peso2/N + differenza_pesi_geni_uguali*peso3 +
        /// neuroni_uguali_con_funzioni_di_soglia_diverse*peso4. Sono considerati geni diversi sia neuroni che assoni
        /// presenti in uno dei due genotipi e non nell'altro. N è il massimo numero di geni nei due genotipi. I pesi sono fissati in Params.
        /// </summary>
        /// <param name="gen1"></param>
        /// <param name="gen2"></param>
        /// <returns></returns>
        public static double distanza(GenotipoRN gen1, GenotipoRN gen2)
        {
            int maxIDNeuroniGen1, maxIDNeuroniGen2, maxIDAssoniGen1, maxIDAssoniGen2;
            double weightDifference = 0;
            int disjointAss = 0, excessAss = 0, disjointNeur = 0, excessNeur = 0, functionDifferences = 0, maxGenes;
            bool end1 = false, end2 = false;
            IEnumerator<KeyValuePair<int, GenotipoRN.NeuroneG>> enumNeurGen1, enumNeurGen2;
            IEnumerator<KeyValuePair<int, GenotipoRN.AssoneG>> enumAssGen1, enumAssGen2;

            maxIDNeuroniGen1 = gen1.maxIDNeuroni;
            maxIDNeuroniGen2 = gen2.maxIDNeuroni;
            maxIDAssoniGen1 = gen1.maxIDAssoni;
            maxIDAssoniGen2 = gen2.maxIDAssoni;

            maxGenes = Math.Max(gen1.numeroNeuroni + gen1.numeroAssoni, gen2.numeroNeuroni + gen2.numeroAssoni);

            enumNeurGen1 = gen1.GetEnumeratorNeuroni();
            enumNeurGen2 = gen2.GetEnumeratorNeuroni();
            enumAssGen1 = gen1.GetEnumeratorAssoni();
            enumAssGen2 = gen2.GetEnumeratorAssoni();

            end1 = !enumNeurGen1.MoveNext();
            end2 = !enumNeurGen2.MoveNext();
            while (!(end1 && end2))
            {
                if (end1)
                {
                    if (enumNeurGen2.Current.Key > maxIDNeuroniGen1)
                        excessNeur++;
                    else
                        disjointNeur++;
                    end2 = !enumNeurGen2.MoveNext();
                }
                else if (end2)
                {
                    if (enumNeurGen1.Current.Key > maxIDNeuroniGen2)
                        excessNeur++;
                    else
                        disjointNeur++;
                    end1 = !enumNeurGen1.MoveNext();
                }
                else
                {
                    if (enumNeurGen1.Current.Key == enumNeurGen2.Current.Key)
                    {
                        if (enumNeurGen1.Current.Value.thresholdIndex != enumNeurGen2.Current.Value.thresholdIndex)
                            functionDifferences++;
                        end1 = !enumNeurGen1.MoveNext();
                        end2 = !enumNeurGen2.MoveNext();
                    }
                    else if (enumNeurGen1.Current.Key > enumAssGen2.Current.Key)
                    {
                            /*ho trovato un gene in gen1 successivo al gene corrente di gen2 però ho altri geni in gen2 con neatID maggiore
                             *-> determina se è excess o disjoint e avanza l'enumerator più indietro, cioè gen2
                             */
                        if (!gen1.contieneNeuroneID(enumNeurGen2.Current.Key))
                            if (enumNeurGen2.Current.Key > maxIDNeuroniGen1)
                                excessNeur++;
                            else
                                disjointNeur++;
                        end2 = !enumNeurGen2.MoveNext();
                    }
                    else //if(enumNeurGen1.Current.Key < enumAssGen2.Current.Key)
                    {
                        if (!gen2.contieneNeuroneID(enumNeurGen1.Current.Key))
                            if (enumNeurGen1.Current.Key > maxIDNeuroniGen2)
                                excessNeur++;
                            else
                                disjointNeur++;
                        end1 = !enumNeurGen1.MoveNext();
                    }
                }
            }

            end1 = !enumAssGen1.MoveNext();
            end2 = !enumAssGen2.MoveNext();
            while (!(end1 && end2))
            {
                if (end1)
                {
                    if (enumAssGen2.Current.Key > maxIDAssoniGen1)
                        excessAss++;
                    else
                        disjointAss++;
                    end2 = !enumAssGen2.MoveNext();
                }
                else if (end2)
                {
                    if (enumAssGen1.Current.Key > maxIDAssoniGen2)
                        excessAss++;
                    else
                        disjointAss++;
                    end1 = !enumAssGen1.MoveNext();
                }
                else
                {
                    if (enumAssGen1.Current.Key == enumAssGen2.Current.Key)
                    {
                        weightDifference += Math.Abs(enumAssGen1.Current.Value.peso - enumAssGen2.Current.Value.peso);
                        end1 = !enumAssGen1.MoveNext();
                        end2 = !enumAssGen2.MoveNext();
                    }
                    else if (enumAssGen1.Current.Key > enumAssGen2.Current.Key)
                    {
                        if (!gen1.contieneAssoneID(enumAssGen2.Current.Key))
                            if (enumAssGen2.Current.Key > maxIDAssoniGen1)
                                excessAss++;
                            else
                                disjointAss++;
                        end2 = !enumAssGen2.MoveNext();
                    }
                    else //if(enumAssGen1.Current.Key < enumAssGen2.Current.Key)
                    {
                        if (!gen2.contieneAssoneID(enumAssGen1.Current.Key))
                            if (enumAssGen1.Current.Key > maxIDAssoniGen2)
                                excessAss++;
                            else
                                disjointAss++;
                        end1 = !enumAssGen1.MoveNext();
                    }
                }
            }
            return (excessAss + excessNeur) * Params.excessGenesWeight/maxGenes + (disjointAss + disjointNeur) * Params.disjointGenesWeight/maxGenes + weightDifference * Params.weightDifferenceWeight + functionDifferences * Params.functionDifferenceWeight;
        }
    }

    public class Species
    {
        SpeciesManager manager;
        GenotipoRN champion;

        List<GenotipoRN> gList;

        public Species(GenotipoRN champion, SpeciesManager manager)
        {
            this.manager = manager;
            this.champion = champion;

            gList = new List<GenotipoRN>();
            gList.Add(champion);            
        }

        public bool addGenotipo(GenotipoRN genotipo)
        {
            bool inSpecies = false;
            double distance = AlgGenRN.distanza(champion, genotipo);

            if (distance < Params.SPECIES_MAX_DISTANCE)
            {
                inSpecies = true;
                gList.Add(genotipo);
            }

            return inSpecies;
        }

        public double fitnessSum()
        {
            double sum = 0;
            foreach (GenotipoRN g in gList)
                sum += g.Fitness;
            return sum / gList.Count;        
        }

        public void GetNextGeneration(List<GenotipoRN> poolCrossingOver, int numberMax)
        {
            Random r = new Random();
            GenotipoRN parent1 = null;
            GenotipoRN parent2 = null;

            gList.Sort();

            int n= gList.Count;
            int gaussNumber = (n * (n + 1)) / 2;
            int extractNumber;
            int generateCreatureNumber=1;

            poolCrossingOver.Add(gList.First());

            while (generateCreatureNumber < (3*numberMax/4 -1))
            {
                extractNumber=r.Next(gaussNumber);
                int accumulator = n;
                for (int i = 0; i < gList.Count; i++)
                {
                    if (extractNumber < accumulator)
                    {
                        parent1 = gList.ElementAt(i);
                        break;
                    }
                    else
                        accumulator += gList.Count - i - 1;
                }

                extractNumber = r.Next(gaussNumber);
                accumulator = n;
                for (int i = 0; i < gList.Count; i++)
                {
                    if (extractNumber < accumulator)
                    {
                        parent2 = gList.ElementAt(i);
                        break;
                    }
                    else
                        accumulator += gList.Count - i - 1;
                }
                GenotipoRN child = new GenotipoRN(parent1, parent1.Fitness, parent2, parent2.Fitness);

                poolCrossingOver.Add(child);
                generateCreatureNumber++;
            }

            while (generateCreatureNumber < numberMax)
            {
                GenotipoRN randomGenotipo = null;
                extractNumber = r.Next(gaussNumber);
                int accumulator = n;
                for (int i = 0; i < gList.Count; i++)
                {
                    if (extractNumber < accumulator)
                    {
                        randomGenotipo = gList[i];
                        break;
                    }
                    else
                        accumulator += gList.Count - i - 1;
                }

                 poolCrossingOver.Add(manager.gestore.mutazione(randomGenotipo, 1)[0]);

            }
        }

        public List<GenotipoRN> GetListGenotipo()
        {
            return new List<GenotipoRN>(gList);
        }

        public void resetList()
        { gList.Clear(); }
    
    }

    public class SpeciesManager
    {
        List<Species> speciesList;
        public GestoreRN_NEAT gestore;
        public SpeciesManager(GestoreRN_NEAT gestore)
        {
            this.gestore = gestore;
            speciesList = new List<Species>();
        }

        public void addGenotipo(GenotipoRN genotipo)
        {
            bool speciesFind = false;
            foreach (Species s in speciesList)
            {
                if (s.addGenotipo(genotipo))
                {
                    speciesFind = true;
                    break;
                }            
            }

            if (!speciesFind)
                speciesList.Add(new Species(genotipo, this));
        
        }

        public void DoNextGeneration()
        {
            List<GenotipoRN> poolCrossingOver=new List<GenotipoRN>();
            
            double sum =0;
            foreach (Species s in speciesList)
                sum += s.fitnessSum();

            int assignedPlace = 0;
            for (int i = 0; i < speciesList.Count; i++)
            {
                int placeInSpecies;
                if (i != (speciesList.Count - 1))
                    placeInSpecies = Convert.ToInt32(speciesList[i].fitnessSum() / sum * Params.INITIAL_POPULATION);
                else
                    placeInSpecies = Params.INITIAL_POPULATION - assignedPlace;
                speciesList[i].GetNextGeneration(poolCrossingOver, placeInSpecies);
                assignedPlace += placeInSpecies;            
            }

            foreach (Species s in speciesList)
                s.resetList();

            foreach (GenotipoRN g in poolCrossingOver)
                addGenotipo(g);             
        
        }

        public List<GenotipoRN> GetListGenotipo()
        {
            List<GenotipoRN> lista = new List<GenotipoRN>();

            foreach (Species specie in speciesList)
                lista.AddRange(specie.GetListGenotipo());

            return lista;
        
        }


    }
}
