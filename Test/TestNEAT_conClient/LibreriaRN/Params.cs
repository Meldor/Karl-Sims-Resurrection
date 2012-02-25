﻿using System;
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
    }
}
