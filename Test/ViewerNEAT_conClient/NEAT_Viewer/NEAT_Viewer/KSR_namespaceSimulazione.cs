using System;
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

//using FarseerPhysics.Common;
//using FarseerPhysics.Dynamics;
//using FarseerPhysics.Factories;
//using FarseerPhysics.Collision.Shapes;
//using FarseerPhysics.Dynamics.Joints;
//using FarseerPhysics.Dynamics.Contacts;

//Rete e Thread

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace KSR_library
{
      


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
        public int maxRicorsione, contatore, neatID;
        public Vector2 dimensioni;
    }

  


}