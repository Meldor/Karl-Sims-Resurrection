using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LibreriaRN;

namespace PoleBalancing
{
    class Evolution
    {
        public delegate void SetFenotipo(FenotipoRN fenotipo);
        public event SetFenotipo ReadyFenotipo;

        private Cart cart;
        private int genotipoCount=0;

        GestoreRN_NEAT evolutionManager;
        GenotipoRN genotipoInTest;

        GenotipoRN[] vectorGenotipo;

        public Evolution(Cart cart)
        {
            this.cart = cart;
            evolutionManager = new GestoreRN_NEAT(5, 1, Const.INITIAL_POPULATION);

            int j=0;
            vectorGenotipo=new GenotipoRN[evolutionManager.population.Count];
            foreach (GenotipoRN g in evolutionManager.population)
            {
                vectorGenotipo[j] = g;
                j++;
            }
            
            ReadyFenotipo += new SetFenotipo(cart.SetFenotipo);
            cart.ReturnFitnessEvent += new Cart.ReturnFitness(finishedSimulation);
            ReadyFenotipo(new FenotipoRN(vectorGenotipo[genotipoCount]));
       }

        private void finishedSimulation(int fitness)
        {
            vectorGenotipo[genotipoCount].Fitness = fitness;
            genotipoCount++;

            if (genotipoCount < evolutionManager.population.Count)
            {
                ReadyFenotipo(new FenotipoRN(vectorGenotipo[genotipoCount]));
            }
            else
            { 
            
            }
        }

    }
}
