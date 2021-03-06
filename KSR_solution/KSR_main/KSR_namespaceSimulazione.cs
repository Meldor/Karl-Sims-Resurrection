﻿using System;
using System.Collections.Generic;
using System.Linq;

//Motore Grafico

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

//Motore Fisico

using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;

//Rete e Thread

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace KSR_library
{
    delegate Double thresholdFunction(Double input);

    /* Classi per il genotipo */
    class Threshold
    {
        private static int numFunz=5;
        public static Double Sinusoide(Double input) /*Seno e Coseno tra x:[-1,1] y:[-1,1]*/
        {
            return Math.Sin(input*Math.PI);        
        }

        public static Double Cosinusoide(Double input)
        {
            return Math.Cos(input * Math.PI);
        }

        public static Double SigmoideInsensibile(Double input) /* Sigmoide da x:[0,1] y:[0,1] taglia i valori negativi*/
        {
            return 1 / (1 + Math.Exp(-12*(input*0.5f)));
        }

        public static Double SigmoideAttenuante(Double input) /* Sigmoide da x:[-1,1] y:[0,1] attenua i valori negativi*/
        {
            return 1 / (1 + Math.Exp(-6 *input));
        }

        public static Double IperbolicTan(Double input) /* Tangente iperbolica x:[-1,1] y:[-1,1] */
        {
            return Math.Tanh(2.5f * input);
        }

        public static thresholdFunction getRandomDelegate()
        {
            Random generatoreCasuale=new Random();
            int id=generatoreCasuale.Next(0,numFunz-1);
            switch (id)
            {
                case 0: return Sinusoide;
                case 1: return Cosinusoide;
                case 2: return SigmoideInsensibile;
                case 3: return SigmoideAttenuante;
                case 4: return IperbolicTan;
                default: return IperbolicTan;
            }

        }

    }


    class Genotipo
    { 
        SortedList<int, NodoStruct> nodi;
        NodoStruct nodo;
        ArcoStruct arco;
        int contatore;

        public Genotipo()
        { 
            nodi=new SortedList<int, NodoStruct>();
            contatore = 0;
        }

        public void AddNodo(Vector2 _dimensioni, int _maxRicorsione)
        {
            nodo = new NodoStruct();
            nodo.archi=new SortedList<int,ArcoStruct>();
            nodo.dimensioni = _dimensioni;
            nodo.maxRicorsione = _maxRicorsione;
            nodi.Add(contatore, nodo);
            contatore++;
        }

        public void AddArco(int _partenza, int _destinazione, Double _posizione, Boolean _simmetrico)
        {
            
            if (_partenza < contatore && _destinazione < contatore)
            {
                nodo = nodi[_partenza];
                arco = new ArcoStruct();
                arco.posizione = _posizione;
                arco.simmetrico = _simmetrico;
                nodo.archi.Add(nodo.contatore, arco);
                nodo.contatore++;
            }
        
        
        
        }

    
    }

    struct ArcoStruct
    {
        public Double posizione;
        public Boolean simmetrico; 
    }

    struct NodoStruct
    {
        public SortedList<int, ArcoStruct> archi;
        public int maxRicorsione, contatore;
        public Vector2 dimensioni;
        public int neatID;
    }

    class ReteNeurale
    {
        List<NeuroneStruct> neuroni;
        List<NeuroneStruct> neuroniA;
        List<NeuroneStruct> neuroniS;
        SortedList<int, NeuroneStruct> NEATLibrary;
        int NEAT_numID;
        int numNeuroniSensori, numNeuroniAttuatori;
        public ReteNeurale(int numA, int numB)
        {
            neuroni = new List<NeuroneStruct>();
            neuroniA = new List<NeuroneStruct>();
            neuroniS = new List<NeuroneStruct>();
            NEATLibrary=new SortedList<int,NeuroneStruct>();
            NEAT_numID=0;
            numNeuroniSensori=numB;
            numNeuroniAttuatori=numA;

        }

        private void AddNeurone(thresholdFunction funzione,  int id, TipoNeurone tipo)
        {
            NeuroneStruct neurone=new NeuroneStruct();
            neurone.inizializza(funzione,id,tipo);
            return;
        }

        private void AddAssone(NeuroneStruct neurone, NeuroneStruct neuroneLink, Double peso)
        {
            neurone.addAssone(neuroneLink, peso);
            return;
        }

        public void generaPercettron()
        {
            NeuroneStruct neurone;
            
            Random generatoreCasuale=new Random();

            for (int i = 0; i < numNeuroniAttuatori; i++)
            {
                neurone = new NeuroneStruct();
                neurone.inizializza(Threshold.getRandomDelegate(), NEAT_numID, TipoNeurone.NActuator);
                neuroni.Add(neurone);
                neuroniA.Add(neurone);
                NEAT_numID++;
            }

            for (int i = 0; i < numNeuroniSensori; i++)
            {
                neurone = new NeuroneStruct();
                neurone.inizializza(Threshold.getRandomDelegate(), NEAT_numID, TipoNeurone.NSensor);
                foreach(NeuroneStruct n in neuroniA)
                    neurone.addAssone(n, generatoreCasuale.NextDouble());            
                neuroni.Add(neurone);
                NEAT_numID++;
            }

            return;      
        }

        public SortedList<int, Double> Next()
        {
            SortedList<int, Double> output= new SortedList<int,double>();
            Double Out;

            foreach (NeuroneStruct neurone in neuroni)
            { 
                Out = neurone.attiva();
                if (neurone.GetTipo()==TipoNeurone.NActuator)
                    output.Add(neurone.GetID(), Out); 
            
            }
            foreach (NeuroneStruct neurone in neuroni)
                neurone.aggiorna();

                
            return output;
        }

        public void sensori(Double[] vett)
        {
            if (vett.Length == neuroniS.Count)
                for (int i = 0; i < vett.Length; i++)
                    neuroniS[i].addInput(vett[i]);

        
        }

        public int GetNumberSensor()
        { return neuroniS.Count; }
    
    }

    struct NeuroneStruct
    {
        int neatId;
        TipoNeurone tipo;
        thresholdFunction funzioneSoglia;
        List<Double> inputP;
        List<Double> inputF;
        List<AssoneStruct> assoni;

        public TipoNeurone GetTipo()
        { return tipo; }

        public int GetID()
        { return neatId; }

        public void inizializza(thresholdFunction _funzione, int _id, TipoNeurone _tipo)
        {
            neatId = _id;
            tipo = _tipo;
            inputP= new List<double>();
            inputF = new List<double>();
            assoni = new List<AssoneStruct>();
            funzioneSoglia = _funzione;
            return;
        }

        public void addInput(Double _in)
        {
            inputF.Add(_in);
            return;
        }

        public Double attiva()
        {
            Double inputSum = inputP.Sum();
            foreach (AssoneStruct assone in assoni)
                assone.attiva(funzioneSoglia(inputSum));
            return inputSum;
        }

        public void aggiorna()
        {
            inputP.Clear();
            foreach (Double val in inputF)
                inputP.Add(val);
            inputF.Clear();
            return;        
        }

        public void addAssone(NeuroneStruct neuroneLink, Double peso)
        {
            AssoneStruct assone = new AssoneStruct();
            assone.inizializza(peso, neuroneLink);
            assoni.Add(assone);
            return;
        
        }


    
    }

    struct AssoneStruct
    {
        NeuroneStruct neuroneLink;
        Double peso;

        public void inizializza(Double _peso, NeuroneStruct _neuroneLink)
        {
            peso = _peso;
            neuroneLink = _neuroneLink;
            return;
        }

        public void attiva(Double _in)
        {
            Double output;
            output = _in * peso;
            neuroneLink.addInput(output);
            return;
        }

    
    }

    enum TipoNeurone { NSensor, NHide, NActuator };


}