using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibreriaRN
{
    class Params
    {
        public const bool transparentInput = true;                   //se true i neuroni di input hanno tutti funzione Transparent
        public const bool onlySigmoid = true;                       //se true l'unica funzione di soglia ammessa è la sigmoide
        public const bool allowThresholdFunctionCrossover = true;    //consente o meno il crossover della funzione di soglia tra due neuroni con lo stesso id

        public const double excessGenesWeight = 1.0;
        public const double disjointGenesWeight = 1.0;
        public const double weightDifferenceWeight = 3.0;
        public const double functionDifferenceWeight = 3.0;
        public const double disabledGeneEnablingProbability = 0.25;
        public const double mostFitParentInheritingProbability = 0.7;

        public const double SPECIES_MAX_DISTANCE = 50;
        public const int INITIAL_POPULATION = 5;
    }
}
