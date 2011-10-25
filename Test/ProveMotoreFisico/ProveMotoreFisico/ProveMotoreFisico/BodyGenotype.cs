using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace ProveMotoreFisico
{
    
    class BodyGenotype
    {
        struct Arco
        {
            public int link;
            public Boolean simmetric;
            public float position; //percentuale sul valore del perimetro

        }

        struct Parte
        {
            public Vector2 bodySize;
            public float density;
            public Color color;
            public int ricorsione;
            public int num_archi;
            public SortedList<int, Arco> lista_archi;
        }
        
        Parte parte;
        Arco arco;
        SortedList<int, Parte> vettParti;
        int num_parti;

        public BodyGenotype()
        {
            vettParti = new SortedList<int, Parte>();
            num_parti=0;
        }

        public void AddPart(float Larghezza, float Altezza, float densita, int ricorsione, Color colore)
        {
            parte=new Parte();
            parte.bodySize = new Vector2(Larghezza, Altezza);
            parte.lista_archi = new SortedList<int, Arco>();
            parte.density = densita;
            parte.color = colore;
            parte.ricorsione = ricorsione;
            parte.num_archi = 0;
            vettParti.Add(num_parti, parte);
            num_parti++;
        }

        public int AddLink(int _parte, int link, float posizione, Boolean simmetric)
        {
            
            if (link > num_parti || _parte > num_parti)
                return 0;

            else
            {
                arco = new Arco();
                parte=vettParti.Values[_parte];
                arco.link = link;
                arco.position = posizione;
                arco.simmetric = simmetric;
                parte.lista_archi.Add(parte.num_archi, arco);
                parte.num_archi++;
                return 1;
            }
        
        }


    }

    
}
