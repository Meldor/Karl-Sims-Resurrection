using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using LibreriaRN;

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
        Nodo _selectedNode = null;
        public Nodo selectedNode
        {
            get { return _selectedNode; }
        }
        FenotipoRN fenotipo = null;

        public GrafoDisegno(GenotipoRN input)
        {
            Queue<Nodo> coda = new Queue<Nodo>();
            nodi = new SortedList<int, Nodo>();
            //crea i nodi
            foreach (KeyValuePair<int, GenotipoRN.NeuroneG> neurone in input.neuroni)
            {
                Nodo nuovo = new Nodo(neurone.Value.neatID, neurone.Value.thresholdIndex);
                nuovo.livello = 0;
                nuovo.stato = Nodo.StatoVisita.NonVisitato;
                nuovo.tipo = TipoNeurone.NHide;
                nodi.Add(neurone.Key, nuovo);
            }
            //prepara i neuroni di input
            foreach (GenotipoRN.NeuroneG neurone in input.neuroniInput)
            {
                Nodo nuovoInput = nodi[neurone.neatID];
                nuovoInput.stato = Nodo.StatoVisita.InCoda;
                nuovoInput.tipo = TipoNeurone.NSensor;
                coda.Enqueue(nuovoInput);
            }
            foreach (GenotipoRN.NeuroneG neurone in input.neuroniOutput)
                nodi[neurone.neatID].tipo = TipoNeurone.NActuator;
            //crea gli archi e li inserisce nella lista archi del nodo di partenza
            foreach(GenotipoRN.AssoneG assone in input.assoni)
            {
                if (!assone.attivo)
                    continue;
                Arco nuovo = new Arco(nodi[assone.output],nodi[assone.input], assone.neatID, assone.peso);
                nodi[assone.input].addArco(nuovo);
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
                    if ((a.destinazione.livello <= n.livello) && (a.destinazione.stato != Nodo.StatoVisita.Visitato) && (a.destinazione.tipo != TipoNeurone.NActuator) && (a.destinazione != a.partenza))
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
            foreach (GenotipoRN.NeuroneG neurone in input.neuroniOutput)
                nodi[neurone.neatID].livello = maxLivello+1;
            maxLivello++;
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
        }

        /// <summary>
        /// Disegna il grafo corrente
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch da utilizzare</param>
        /// <param name="circTexture">Texture con un cerchio di raggio pari a Const.RaggioTextureCirc</param>
        /// <param name="rectTexture">Texture di almeno 1x1 pixel</param>
        /// <param name="color">Colore dei nodi</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D circTexture, Texture2D rectTexture, Texture2D[] thresholdFunctions, Color coloreNodoGenotipo, Color coloreNodoMin, Color coloreNodoMax, Color coloreNodoSelezionato, Color coloreArcoPos, Color coloreArcoNeg)
        {
            bool interseca;
            foreach (KeyValuePair<int, Nodo> key_val in nodi)
            {
                IEnumerator<Arco> enumArco = key_val.Value.GetEnumeratorArco();
                //disegna il nodo
                if(key_val.Value == _selectedNode)
                    key_val.Value.Draw(spriteBatch, circTexture, rectTexture, thresholdFunctions, coloreNodoSelezionato);
                else
                    key_val.Value.Draw(spriteBatch, circTexture, rectTexture, thresholdFunctions, coloreNodoMin, coloreNodoMax, coloreNodoGenotipo);
                //scorre gli archi uscenti dal nodo
                while (enumArco.MoveNext())
                {
                    interseca = false;
                    //verifica se, disegnando l'arco con un segmento, si sovrapporrebbe ad un altro nodo
                    //se sì disegna l'arco con una spline
                    foreach (KeyValuePair<int, Nodo> key_val_2 in nodi)
                    {
                        if ((key_val_2.Value == enumArco.Current.partenza) || (key_val_2.Value == enumArco.Current.destinazione))
                            continue;
                        float distanza = enumArco.Current.DistanzaNodo(key_val_2.Value);
                        if (distanza < Const.DimensioneNodo)
                        {
                            interseca = true;
                            break;
                        }
                    }
                    if (interseca)
                    //TODO: verificare che i segmenti di cui è composta la spline non si sovrappongano ad altri nodi
                    //modificare i vettori tangenti finché non ci sono più sovrapposizioni
                        enumArco.Current.DrawSpline(spriteBatch, rectTexture, coloreArcoPos, coloreArcoNeg);
                    else
                        enumArco.Current.DrawLine(spriteBatch, rectTexture, coloreArcoPos, coloreArcoNeg);
                }
            }
        }

        /// <summary>
        /// Restituisce l'eventuale nodo presente in corrispondenza di mousePos.
        /// </summary>
        /// <param name="mousePos">Coordinate dove si vuole testare la presenza di un nodo</param>
        /// <param name="setSelected">Se true l'eventuale nodo trovato viene impostato come selectedNode</param>
        /// <returns>Eventuale nodo selezionato o null se il click non seleziona alcun nodo.</returns>
        public Nodo CheckNodeClick(Vector2 mousePos, bool setSelected)
        {
            foreach (KeyValuePair<int, Nodo> k_val in nodi)
            {
                if (Vector2.Distance(k_val.Value.posizione, mousePos) <= Const.DimensioneNodo / 2)
                {
                    if(setSelected)
                        _selectedNode = k_val.Value;
                    return k_val.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Scambia la posizione dei due nodi nella visualizzazione
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        public void SwitchNodes(Nodo node1, Nodo node2)
        {
            Vector2 temp = new Vector2(node1.posizione.X, node1.posizione.Y);
            node1.posizione = node2.posizione;
            node2.posizione = temp;
            return;
        }

        /// <summary>
        /// Deseleziona il nodo eventualmente selezionato
        /// </summary>
        public void Unselect()
        {
            _selectedNode = null;
        }

        public void AttachFenotipo(FenotipoRN f)
        {
            fenotipo = f;
            foreach (KeyValuePair<int,Nodo> k_val in nodi)
                k_val.Value.nodoFenotipo = f.GetNeuroneById(k_val.Key);
        }

        public class Nodo
        {
            List<Arco> archi;

            private int _livello;
            private int _id;
            private StatoVisita _stato;
            private Vector2 _posizione;
            private TipoNeurone _tipo;
            private FenotipoRN.NeuroneF _nodoFenotipo;
            private int _indiceThreshold;

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
            public FenotipoRN.NeuroneF nodoFenotipo
            {
                get { return _nodoFenotipo; }
                set { _nodoFenotipo = value; }
            }

            public TipoNeurone tipo
            {
                get { return _tipo; }
                set { _tipo = value; }
            }

            public Nodo(int id, int indiceThreshold)
            {
                _id = id;
                _indiceThreshold = indiceThreshold;
                _nodoFenotipo = null;
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
            public void Draw(SpriteBatch spriteBatch, Texture2D circTexture, Texture2D rectTexture, Texture2D[] thresholdTextures, Color coloreMin, Color coloreMax, Color coloreGenotipo)
            {
                if (nodoFenotipo == null)
                {
                    spriteBatch.Draw(circTexture, posizione, null, coloreGenotipo, 0f, new Vector2(Const.RaggioTextureCirc), (float)Const.DimensioneNodo / Const.RaggioTextureCirc, SpriteEffects.None, 0f);
                    spriteBatch.Draw(thresholdTextures[_indiceThreshold], posizione, null, Color.White, 0f, new Vector2(Const.LatoTextureThreshold)/2, (float)2*Const.DimensioneNodo / Const.LatoTextureThreshold, SpriteEffects.None, 0.1f);
                }
                else
                {
                    Color coloreMix = Color.Lerp(coloreMin, coloreMax, (float)(0.5 * nodoFenotipo.output + 0.5));
                    spriteBatch.Draw(circTexture, posizione, null, coloreMix, 0f, new Vector2(Const.RaggioTextureCirc), (float)Const.DimensioneNodo / Const.RaggioTextureCirc, SpriteEffects.None, 0f);
                    spriteBatch.Draw(thresholdTextures[_indiceThreshold], posizione, null, Color.White, 0f, new Vector2(Const.LatoTextureThreshold) / 2, (float)2 * Const.DimensioneNodo / Const.LatoTextureThreshold, SpriteEffects.None, 0.1f);
                }
            }

            public void Draw(SpriteBatch spriteBatch, Texture2D circTexture, Texture2D rectTexture, Texture2D[] thresholdTextures, Color colore)
            {
                spriteBatch.Draw(circTexture, posizione, null, colore, 0f, new Vector2(Const.RaggioTextureCirc), (float)Const.DimensioneNodo / Const.RaggioTextureCirc, SpriteEffects.None, 0f);
                spriteBatch.Draw(thresholdTextures[_indiceThreshold], posizione, null, Color.White, 0f, new Vector2(Const.LatoTextureThreshold) / 2, (float)2 * Const.DimensioneNodo / Const.LatoTextureThreshold, SpriteEffects.None, 0f);
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
                return "Id: " + id + " " + partenza.id + "->" + destinazione.id + " Peso: " + peso;
            }

            /// <summary>
            /// Disegna l'arco con un segmento che congiunge partenza e destinazione.
            /// </summary>
            /// <param name="spriteBatch">SpriteBatch da utilizzare</param>
            /// <param name="rectTexture">Texture rettangolare di almeno 1x1 pixel</param>
            /// <param name="color">Colore con cui disegnare l'arco</param>
            public void DrawLine(SpriteBatch spriteBatch, Texture2D rectTexture, Color colorePos, Color coloreNeg)
            {
                if (partenza != destinazione)
                    if(peso < 0)
                        DrawingHelper.DrawArrow(spriteBatch, rectTexture, coloreNeg, partenza.posizione, destinazione.posizione, Const.AperturaFreccia, Const.LunghezzaFreccia, 1, (float)Math.Abs(peso)*Const.MaxLarghezzaArco);
                    else
                        DrawingHelper.DrawArrow(spriteBatch, rectTexture, colorePos, partenza.posizione, destinazione.posizione, Const.AperturaFreccia, Const.LunghezzaFreccia, 1, (float)Math.Abs(peso) * Const.MaxLarghezzaArco);
                else
                    //se è un cappio lo disegna come spline
                    if(peso < 0)
                        DrawingHelper.DrawSplineArrow(spriteBatch, rectTexture, coloreNeg,
                            new Vector2(-(float)(Const.DimensioneNodo / Math.Sqrt(2)), -(float)(Const.DimensioneNodo / Math.Sqrt(2))) + partenza.posizione, new Vector2(-(float)(Const.DimensioneNodo / Math.Sqrt(2)), -(float)(Const.DimensioneNodo / Math.Sqrt(2)))*Const.DimensioneNodo,
                            new Vector2((float)(Const.DimensioneNodo / Math.Sqrt(2)), -(float)(Const.DimensioneNodo / Math.Sqrt(2))) + partenza.posizione, new Vector2(-(float)(Const.DimensioneNodo / Math.Sqrt(2)), (float)(Const.DimensioneNodo / Math.Sqrt(2)))*Const.DimensioneNodo,
                            Const.NumSegmentiSpline, Const.AperturaFreccia, Const.LunghezzaFreccia, 1, (float)Math.Abs(peso) * Const.MaxLarghezzaArco);
                    else
                        DrawingHelper.DrawSplineArrow(spriteBatch, rectTexture, colorePos,
                            new Vector2(-(float)(Const.DimensioneNodo / Math.Sqrt(2)), -(float)(Const.DimensioneNodo / Math.Sqrt(2))) + partenza.posizione, new Vector2(-(float)(Const.DimensioneNodo / Math.Sqrt(2)), -(float)(Const.DimensioneNodo / Math.Sqrt(2))) * Const.DimensioneNodo,
                            new Vector2((float)(Const.DimensioneNodo / Math.Sqrt(2)), -(float)(Const.DimensioneNodo / Math.Sqrt(2))) + partenza.posizione, new Vector2(-(float)(Const.DimensioneNodo / Math.Sqrt(2)), (float)(Const.DimensioneNodo / Math.Sqrt(2))) * Const.DimensioneNodo,
                            Const.NumSegmentiSpline, Const.AperturaFreccia, Const.LunghezzaFreccia, 1, (float)Math.Abs(peso) * Const.MaxLarghezzaArco);
            }

            /// <summary>
            /// Disegna l'arco con una spline che congiunge partenza e destinazione
            /// </summary>
            /// <param name="spriteBatch">SpriteBatch da utilizzare</param>
            /// <param name="rectTexture">Texture rettangolare di almeno 1x1 pixel</param>
            /// <param name="color">Colore con cui disegnare l'arco</param>
            public void DrawSpline(SpriteBatch spriteBatch, Texture2D rectTexture, Color colorePos, Color coloreNeg)
            {
                Vector2 startTangent = Vector2.Zero;
                Vector2 endTangent = Vector2.Zero;
                float distanza = (destinazione.posizione - partenza.posizione).Length();
                if (destinazione.livello >= partenza.livello)
                    startTangent.X = distanza;
                else
                    startTangent.X = -distanza;
                if (destinazione.posizione.Y < partenza.posizione.Y)
                    endTangent.Y = -distanza;
                else if (destinazione.posizione.Y > partenza.posizione.Y)
                    endTangent.Y = distanza;
                else
                    endTangent.X = -distanza;
                if(peso < 0)
                    DrawingHelper.DrawSplineArrow(spriteBatch, rectTexture, coloreNeg, partenza.posizione, startTangent, destinazione.posizione, endTangent, Const.NumSegmentiSpline, Const.AperturaFreccia, Const.LunghezzaFreccia, 1, (float)Math.Abs(peso)*Const.MaxLarghezzaArco);
                else
                    DrawingHelper.DrawSplineArrow(spriteBatch, rectTexture, colorePos, partenza.posizione, startTangent, destinazione.posizione, endTangent, Const.NumSegmentiSpline, Const.AperturaFreccia, Const.LunghezzaFreccia, 1, (float)Math.Abs(peso) * Const.MaxLarghezzaArco);
            }

            /// <summary>
            /// Calcola la distanza tra la retta passante per i nodi di partenza e destinazione dell'arco corrente e il nodo passato
            /// come argomento.
            /// </summary>
            /// <param name="nodo">Nodo rispetto a cui calcolare la distanza</param>
            /// <returns>Distanza, in pixel</returns>
            internal float DistanzaNodo(Nodo nodo)
            {
                //implementa la formula distanza punto-retta, determinando prima di tutto le costanti a, b, c della retta
                //ax+by+c=0 passante per i centri dei nodi di partenza e destinazione
                float a = partenza.posizione.Y-destinazione.posizione.Y;
                float b = destinazione.posizione.X - partenza.posizione.X;
                float c = partenza.posizione.X * destinazione.posizione.Y - destinazione.posizione.X * partenza.posizione.Y;
                //se la proiezione della posizione del nodo sulla retta congiungente non appartiene al segmento che unisce partenza e destinazione
                //restituisce +oo
                float x_proiez = ((b * b * nodo.posizione.X - b * a * nodo.posizione.Y - c * a) / (a * a + b * b));
                float y_proiez = ((a * a * nodo.posizione.Y - a * b * nodo.posizione.X - b * c) / (a * a + b * b));
                float max_x = partenza.posizione.X > destinazione.posizione.X ? partenza.posizione.X : destinazione.posizione.X;
                float min_x = partenza.posizione.X <= destinazione.posizione.X ? partenza.posizione.X : destinazione.posizione.X;
                float max_y = partenza.posizione.Y > destinazione.posizione.Y ? partenza.posizione.Y : destinazione.posizione.Y;
                float min_y = partenza.posizione.Y <= destinazione.posizione.Y ? partenza.posizione.Y : destinazione.posizione.Y;
                if ((nodo.posizione.X > min_x) && (nodo.posizione.X < max_x) && (nodo.posizione.Y > min_y) && (nodo.posizione.Y < max_y))
                    return (float)(Math.Abs(a * nodo.posizione.X + b * nodo.posizione.Y + c) / Math.Sqrt(a * a + b * b));
                else
                    return float.PositiveInfinity;
            }
        }
    }
}
