using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KSR_visual
{
    class Actuator
    {
        //posizione, rispetto al centro delle rispettive parti, dei punti di contatto dei muscoli DIRetto e INVerso
        Vector2 LocalFigliaDir, LocalFigliaInv, LocalPadreDir, LocalPadreInv;
        //forza massima applicabile
        public float MaxForce;
        FenotipoCell PartePadre, ParteFiglia;
        //posizione dei vari punti di contatto in coordinate assolute
        Vector2 WorldFiglioDir
        {
            get
            {
                return Utils.TranslateAndRotate(LocalFigliaDir, ParteFiglia.Position, ParteFiglia.Rotation);
            }
        }
        Vector2 WorldFiglioInv
        {
            get
            {
                return Utils.TranslateAndRotate(LocalFigliaInv, ParteFiglia.Position, ParteFiglia.Rotation);
            }
        }
        Vector2 WorldPadreDir
        {
            get
            {
                return Utils.TranslateAndRotate(LocalPadreDir, PartePadre.Position, PartePadre.Rotation);
            }
        }
        Vector2 WorldPadreInv
        {
            get
            {
                return Utils.TranslateAndRotate(LocalPadreInv, PartePadre.Position, PartePadre.Rotation);
            }
        }

        /// <summary>
        /// Costruttore che calcola automaticamente la posizione dei punti di contatto
        /// </summary>
        /// <param name="parteFiglio">Parte figlia (parte che possiede questo Actuator)</param>
        /// <param name="partePadre">Parte padre (parte a cui è attaccata la parte figlia attraverso questo Actuator)</param>
        /// <param name="maxForce">Forza massima da applicare a ciascun muscolo</param>
        public Actuator(FenotipoCell parteFiglio, FenotipoCell partePadre, float maxForce)
        {
            MaxForce = maxForce;
            PartePadre = partePadre;
            ParteFiglia = parteFiglio;
            Vector2 jointSideOffset = parteFiglio.JointSideOffset;
            LocalFigliaDir = new Vector2();
            LocalFigliaInv = new Vector2();
            LocalPadreDir = new Vector2();
            LocalPadreInv = new Vector2();
            /*
             * noto il lato attraverso cui le due parti sono collegate e dove quindi dovranno essere posizionati i muscoli, questi
             * saranno collocati in corrispondenza dell'estremo più interno tra quelli delle due parti. Occorre quindi calcolare le
             * posizioni degli angoli delle due parti rispetto ad un unico sistema di riferimento locale (in questo caso quello della
             * parte figlia), in modo da poter determinare subito quale dei due sia il più interno.
             */ 
            switch (ParteFiglia.ConnectionSide)
            {
                case Side.Right:
                    Vector2 figlioRelativePadreUpperCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-PartePadre.BodySize.X / 2, PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    Vector2 figlioRelativePadreLowerCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-PartePadre.BodySize.X / 2, -PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    LocalFigliaDir.X = ParteFiglia.BodySize.X / 2;
                    LocalFigliaInv.X = ParteFiglia.BodySize.X / 2;
                    Vector2 padreRelativeFiglioUpperCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(ParteFiglia.BodySize.X / 2, ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    Vector2 padreRelativeFiglioLowerCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(ParteFiglia.BodySize.X / 2, -ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    LocalPadreDir.X = -PartePadre.BodySize.X / 2;
                    LocalPadreInv.X = -PartePadre.BodySize.X / 2;
                    if (figlioRelativePadreUpperCorner.Y < (ParteFiglia.BodySize.Y / 2)) //l'angolo superiore più interno è quello della parte padre (ha la coordinata Y minore)
                    {
                        LocalFigliaDir.Y = figlioRelativePadreUpperCorner.Y;
                        LocalPadreDir.Y = PartePadre.BodySize.Y / 2;
                    }
                    else  //l'angolo superiore più interno è quello della parte figlia
                    {
                        LocalFigliaDir.Y = ParteFiglia.BodySize.Y / 2;
                        LocalPadreDir.Y = padreRelativeFiglioUpperCorner.Y;
                    }

                    if (figlioRelativePadreLowerCorner.Y > (-ParteFiglia.BodySize.Y / 2)) //stessa cosa per gli angoli inferiori (ora però il più interno ha la coordinata Y maggiore)
                    {
                        LocalFigliaInv.Y = figlioRelativePadreLowerCorner.Y;
                        LocalPadreInv.Y = -PartePadre.BodySize.Y / 2;
                    }
                    else
                    {
                        LocalFigliaInv.Y = -ParteFiglia.BodySize.Y / 2;
                        LocalPadreInv.Y = padreRelativeFiglioLowerCorner.Y;
                    }
                    break;
                case Side.Left:
                    //simmetrico al caso precedente, con PartePadre e ParteFiglia invertiti
                    figlioRelativePadreUpperCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(PartePadre.BodySize.X / 2, PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    figlioRelativePadreLowerCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(PartePadre.BodySize.X / 2, -PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    LocalFigliaDir.X = -ParteFiglia.BodySize.X / 2;
                    LocalFigliaInv.X = -ParteFiglia.BodySize.X / 2;
                    padreRelativeFiglioUpperCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-ParteFiglia.BodySize.X / 2, ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    padreRelativeFiglioLowerCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-ParteFiglia.BodySize.X / 2, -ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    LocalPadreDir.X = PartePadre.BodySize.X / 2;
                    LocalPadreInv.X = PartePadre.BodySize.X / 2;
                    if (figlioRelativePadreUpperCorner.Y < (ParteFiglia.BodySize.Y / 2))
                    {
                        LocalFigliaDir.Y = figlioRelativePadreUpperCorner.Y;
                        LocalPadreDir.Y = PartePadre.BodySize.Y / 2;
                    }
                    else
                    {
                        LocalFigliaDir.Y = ParteFiglia.BodySize.Y / 2;
                        LocalPadreDir.Y = padreRelativeFiglioUpperCorner.Y;
                    }

                    if (figlioRelativePadreLowerCorner.Y > (-ParteFiglia.BodySize.Y / 2))
                    {
                        LocalFigliaInv.Y = figlioRelativePadreLowerCorner.Y;
                        LocalPadreInv.Y = -PartePadre.BodySize.Y / 2;
                    }
                    else
                    {
                        LocalFigliaInv.Y = -ParteFiglia.BodySize.Y / 2;
                        LocalPadreInv.Y = padreRelativeFiglioLowerCorner.Y;
                    }
                    break;
                case Side.Top:
                    //simmetrico rispetto al case Side.Right, con X <-> Y, Right <-> Upper, Left <-> Lower
                    Vector2 figlioRelativePadreRightCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(PartePadre.BodySize.X / 2, PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    Vector2 figlioRelativePadreLeftCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-PartePadre.BodySize.X / 2, PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    LocalFigliaDir.Y = -ParteFiglia.BodySize.Y / 2;
                    LocalFigliaInv.Y = -ParteFiglia.BodySize.Y / 2;
                    Vector2 padreRelativeFiglioRightCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(ParteFiglia.BodySize.X / 2, -ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    Vector2 padreRelativeFiglioLeftCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-ParteFiglia.BodySize.X / 2, -ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    LocalPadreDir.Y = PartePadre.BodySize.Y / 2;
                    LocalPadreInv.Y = PartePadre.BodySize.Y / 2;
                    if (figlioRelativePadreRightCorner.X < (ParteFiglia.BodySize.X / 2))
                    {
                        LocalFigliaDir.X = figlioRelativePadreRightCorner.X;
                        LocalPadreDir.X = PartePadre.BodySize.X / 2;
                    }
                    else
                    {
                        LocalFigliaDir.X = ParteFiglia.BodySize.X / 2;
                        LocalPadreDir.X = padreRelativeFiglioRightCorner.X;
                    }

                    if (figlioRelativePadreLeftCorner.X > (-ParteFiglia.BodySize.X / 2))
                    {
                        LocalFigliaInv.X = figlioRelativePadreLeftCorner.X;
                        LocalPadreInv.X = -PartePadre.BodySize.X / 2;
                    }
                    else
                    {
                        LocalFigliaInv.X = -ParteFiglia.BodySize.X / 2;
                        LocalPadreInv.X = padreRelativeFiglioLeftCorner.X;
                    }
                    break;
                case Side.Bottom:
                    //simmetrico rispetto al caso Top, con PartePadre <-> ParteFiglia
                    figlioRelativePadreRightCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(PartePadre.BodySize.X / 2, -PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    figlioRelativePadreLeftCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-PartePadre.BodySize.X / 2, -PartePadre.BodySize.Y / 2), PartePadre.Position, PartePadre.Rotation), ParteFiglia.Position, ParteFiglia.Rotation);
                    LocalFigliaDir.Y = ParteFiglia.BodySize.Y / 2;
                    LocalFigliaInv.Y = ParteFiglia.BodySize.Y / 2;
                    padreRelativeFiglioRightCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(ParteFiglia.BodySize.X / 2, ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    padreRelativeFiglioLeftCorner = Utils.InvTranslateAndRotate(Utils.TranslateAndRotate(new Vector2(-ParteFiglia.BodySize.X / 2, ParteFiglia.BodySize.Y / 2), ParteFiglia.Position, ParteFiglia.Rotation), PartePadre.Position, PartePadre.Rotation);
                    LocalPadreDir.Y = -PartePadre.BodySize.Y / 2;
                    LocalPadreInv.Y = -PartePadre.BodySize.Y / 2;
                    if (figlioRelativePadreRightCorner.X < (ParteFiglia.BodySize.X / 2))
                    {
                        LocalFigliaDir.X = figlioRelativePadreRightCorner.X;
                        LocalPadreDir.X = PartePadre.BodySize.X / 2;
                    }
                    else
                    {
                        LocalFigliaDir.X = ParteFiglia.BodySize.X / 2;
                        LocalPadreDir.X = padreRelativeFiglioRightCorner.X;
                    }

                    if (figlioRelativePadreLeftCorner.X > (-ParteFiglia.BodySize.X / 2))
                    {
                        LocalFigliaInv.X = figlioRelativePadreLeftCorner.X;
                        LocalPadreInv.X = -PartePadre.BodySize.X / 2;
                    }
                    else
                    {
                        LocalFigliaInv.X = -ParteFiglia.BodySize.X / 2;
                        LocalPadreInv.X = padreRelativeFiglioLeftCorner.X;
                    }
                    break;
            }
        }

        /// <summary>
        /// Disegna i muscoli come linee
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch che effettua il disegno</param>
        /// <param name="directColor">Colore del muscolo diretto</param>
        /// <param name="inverseColor">Colore del muscolo inverso</param>
        /// <param name="rectTexture">Texture rettangolare (di almento 1x1 pixel)</param>
        /// <param name="zoomFactor">Fattore di zoom tra coordinate fisiche e grafiche</param>
        public void Draw(SpriteBatch spriteBatch, Color directColor, Color inverseColor, Texture2D rectTexture, float zoomFactor)
        {
            Utils.DrawingHelper.DrawLine(spriteBatch, rectTexture, Utils.ToGraphics(WorldFiglioDir, zoomFactor), Utils.ToGraphics(WorldPadreDir, zoomFactor), directColor);
            Utils.DrawingHelper.DrawLine(spriteBatch, rectTexture, Utils.ToGraphics(WorldFiglioInv, zoomFactor), Utils.ToGraphics(WorldPadreInv, zoomFactor), inverseColor);
        }

        /// <summary>
        /// Applica una forza ai due muscoli
        /// </summary>
        /// <param name="force">Frazione di MaxForce da applicare. Valori positivi provocano un allungamento del muscolo diretto e una contrazione di quello inverso.</param>
        public void ApplyForce(float force)
        {
            //le direzioni sono concordi con le forze da applicare alla parte figlia
            Vector2 directDirection = WorldFiglioDir - WorldPadreDir;
            Vector2 inverseDirection = WorldPadreInv - WorldFiglioInv;
            float appliedForce = force * MaxForce;
            directDirection.Normalize();
            inverseDirection.Normalize();
            ParteFiglia.ApplyForce(appliedForce * directDirection, WorldFiglioDir);
            ParteFiglia.ApplyForce(appliedForce * inverseDirection, WorldFiglioInv);
            PartePadre.ApplyForce(-appliedForce * directDirection, WorldPadreDir);
            PartePadre.ApplyForce(-appliedForce * inverseDirection, WorldPadreInv);
        }
    }
}
