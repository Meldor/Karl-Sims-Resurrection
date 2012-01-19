using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using KSR_libraryRN;

namespace NEAT_Viewer
{
    /// <summary>
    /// La classe GrafoDisegno memorizza, per ogni nodo, posizione (in cui disegnarlo) e livello, in modo da non doverli
    /// ricalcolare ad ogni ciclo. Gli archi sono memorizzati all'interno di ogni nodo, cioè ogni nodo contiene gli archi
    /// verso i nodi a cui è collegato; è per rendere più semplice la visita del grafo.
    /// </summary>
    class GrafoDisegno
    {
        //la chiave di ordinamento è il NEAT_id
        SortedList<int, Nodo> nodi;
        int maxLivello = 0;

        public GrafoDisegno(GenotipoRN input)
        {
            Queue<Nodo> coda = new Queue<Nodo>();
            nodi = new SortedList<int, Nodo>();
            //crea i nodi
            foreach (KeyValuePair<int, GenotipoRN.NeuroneG> neurone in input.neuroni)
            {
                Nodo nuovo = new Nodo(neurone.Value.GetId());
                nuovo.livello = 0;
                nuovo.stato = Nodo.StatoVisita.NonVisitato;
                nodi.Add(neurone.Key, nuovo);
            }
            //prepara i neuroni di input
            foreach (GenotipoRN.NeuroneG neurone in input.neuroniInput)
            {
                Nodo nuovoInput = nodi[neurone.GetId()];
                nuovoInput.stato = Nodo.StatoVisita.InCoda;
                coda.Enqueue(nuovoInput);
            }
            //crea gli archi e li inserisce nella lista archi del nodo di partenza
            foreach(GenotipoRN.AssoneG assone in input.assoni)
            {
                Arco nuovo = new Arco(nodi[assone.getOutput()],nodi[assone.getInput()], assone.GetId(), assone.getPeso());
                nodi[assone.getInput()].addArco(nuovo);
            }
            //definisce i livelli come distanza massima da un qualsiasi nodo di input; per far questo fa una visita in ampiezza
            //a partire dai nodi di input
            while (coda.Count > 0)
            {
                Nodo n = coda.Dequeue();
                n.stato = Nodo.StatoVisita.InVisita;
                IEnumerator<Arco> enumerator = n.GetEnumeratorArco();
                while (enumerator.MoveNext())
                {
                    Arco a = enumerator.Current;
                    if (a.destinazione.livello < (n.livello + 1))
                    {
                        a.destinazione.livello = n.livello + 1;
                        if (a.destinazione.livello > maxLivello)
                            maxLivello = a.destinazione.livello;
                    }
                    if (a.destinazione.stato == Nodo.StatoVisita.NonVisitato)
                    {
                        a.destinazione.stato = Nodo.StatoVisita.InCoda;
                        coda.Enqueue(a.destinazione);
                    }
                }
                n.stato = Nodo.StatoVisita.Visitato;
            }
            //y_corrente[i] contiene la coordinata y del prossimo neurone al livello i
            int[] y_corrente = new int[maxLivello+1];
            y_corrente[0] = Const.LatoGriglia / 2;
            for (int i = 1; i <= maxLivello; i++)
                y_corrente[i] = y_corrente[i - 1] + Const.LatoGriglia;
            //assegna le posizioni
            foreach (KeyValuePair<int, Nodo> neurone in nodi)
            {
                neurone.Value.posizione = new Vector2(neurone.Value.livello * Const.LatoGriglia + Const.LatoGriglia / 2, y_corrente[neurone.Value.livello]);
                y_corrente[neurone.Value.livello] += Const.LatoGriglia*(maxLivello+1);
            }
            //TODO: portare tutti i neuroni di output al livello massimo, per far sì che vengano tutti allineati. Bisogna però memorizzare
            //separatamente i neuroni di output nel GenotipoRN, analogamente a quanto ho fatto per i neuroni di input.

        }

        /// <summary>
        /// Disegna il grafo corrente
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch da utilizzare</param>
        /// <param name="circTexture">Texture con un cerchio di raggio pari a Const.RaggioTextureCirc</param>
        /// <param name="rectTexture">Texture di almeno 1x1 pixel</param>
        /// <param name="color">Colore dei nodi</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D circTexture, Texture2D rectTexture, Color color)
        {
            foreach (KeyValuePair<int, Nodo> key_val in nodi)
                key_val.Value.Draw(spriteBatch, circTexture, rectTexture, color);
        }

        public class Nodo
        {
            List<Arco> archi;

            private int _livello;
            private int _id;
            private StatoVisita _stato;
            private Vector2 _posizione;

            public int id
            {
                get
                {
                    return _id;
                }
            }
            public int livello
            {
                get
                {
                    return _livello;
                }
                set
                {
                    _livello = value;
                }
            }
            public StatoVisita stato
            {
                get { return _stato; }
                set { _stato = value; }
            }
            public Vector2 posizione
            {
                get { return _posizione; }
                set { _posizione = new Vector2(value.X, value.Y); }
            }

            public Nodo(int id)
            {
                _id = id;
                archi = new List<Arco>();
            }

            public void addArco(Arco a)
            {
                archi.Add(a);
            }

            public IEnumerator<Arco> GetEnumeratorArco()
            {
                return archi.GetEnumerator();
            }
            public void Draw(SpriteBatch spriteBatch, Texture2D circTexture, Texture2D rectTexture, Color color)
            {
                spriteBatch.Draw(circTexture, posizione, null, color, 0f, new Vector2(Const.RaggioTextureCirc), (float)Const.DimensioneNodo / Const.RaggioTextureCirc, SpriteEffects.None, 0f);
                foreach (Arco a in archi)
                    a.Draw(spriteBatch, rectTexture, Color.Black);
            }
            public override string ToString()
            {
                return "Id: " + id + " Livello: " + livello;
            }

            public enum StatoVisita
            {
                NonVisitato, Visitato, InCoda, InVisita
            }
        }

        public class Arco
        {
            public Nodo destinazione
            {
                get { return _destinazione; }
            }
            public Nodo partenza
            { get { return _partenza; } }
            //memorizzo il peso per permettere ad esempio di cambiare colore o spessore dell'arco in base al peso
            public double peso
            { get { return _peso; } }
            int id; //NEAT_id, in realtà qui potrebbe non servire

            private Nodo _destinazione;
            private Nodo _partenza;
            private double _peso;

            public Arco(Nodo dest, Nodo start, int id, double peso)
            {
                _destinazione = dest;
                _partenza = start;
                _peso = peso;
                this.id = id;
            }

            public override string ToString()
            {
                return "Id: " + id + " Dest: " + destinazione.id + " Peso: " + peso;
            }

            /*
             * Se scommenti le righe sotto e commenti la prima utilizza le spline anziché le linee rette; probabilmente l'ideale sarebbe usare le rette
             * quando possibile e le spline quando la retta si sovrapporrebbe ad un'altra già disegnata. E' la prossima modifica che penso di fare.
             */
            public void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, Color color)
            {
                DrawingHelper.DrawArrow(spriteBatch, rectTexture, color, partenza.posizione, destinazione.posizione, Const.AperturaFreccia, Const.LunghezzaFreccia, 1);

                //Vector2 startTangent = Vector2.Zero;
                //Vector2 endTangent = Vector2.Zero;
                //float distanza = (destinazione.posizione - partenza.posizione).Length();
                //if(destinazione.livello > partenza.livello)
                //    startTangent.X = distanza;
                //else
                //    startTangent.X = -distanza;
                //if(destinazione.posizione.Y < partenza.posizione.Y)
                //    endTangent.Y = -distanza;
                //else
                //    endTangent.Y = distanza;
                //DrawingHelper.DrawSplineArrow(spriteBatch, rectTexture, color, partenza.posizione, startTangent, destinazione.posizione, endTangent, 6, Const.AperturaFreccia, Const.LunghezzaFreccia, 1);
            }
        }

    }
}
