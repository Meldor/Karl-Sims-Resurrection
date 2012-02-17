using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;

namespace PoleBalancing
{
    class FParte
    {
        Fixture BodyFixture, JointFixture;
        public Body Body;
        public float Rotation
        {
            get
            {
                return Body.Rotation;
            }
            set
            {
                Body.Rotation = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return Body.Position;
            }
            set
            {
                Body.Position = value;
            }
        }
        public BodyType BodyType
        {
            get
            {
                return Body.BodyType;
            }
            set
            {
                Body.BodyType = value;
            }
        }
        public Vector2 BodySize;
        public float BodyArea
        {
            get
            {
                return BodySize.X * BodySize.Y;
            }
        }
        Vector2 JointOffset;
        public Vector2 JointSideOffset
        {
            get
            {
                if (JointOffset.X < (-BodySize.X / 2))
                    return JointOffset + new Vector2(JointRadius, 0);
                else if (JointOffset.X > (BodySize.X / 2))
                    return JointOffset - new Vector2(JointRadius, 0);
                else if (JointOffset.Y < (-BodySize.Y / 2))
                    return JointOffset + new Vector2(0, JointRadius);
                else
                    return JointOffset - new Vector2(0, JointRadius);
            }
        }
        float JointRadius, Density;
        Color BodyColor, JointColor;
        World World;
        public MotionSystem PartMotionSystem
        {
            get
            {
                return _partMotionSystem;
            }
            set
            {
                if (Joint != null)
                {
                    if (value == MotionSystem.Actuator)
                        Joint.MotorEnabled = false;
                    else
                        Joint.MotorEnabled = true;
                }
                _partMotionSystem = value;
            }
        }
        private MotionSystem _partMotionSystem;
        public RevoluteJoint Joint;
        internal Side ConnectionSide;
        bool[] CollidingSides;
        public List<Collision> Collisions;
        List<FParte> ChildParts;

        #region Costruttori e inizializzazione

        /// <summary>
        /// Costruttore di FParte
        /// </summary>
        /// <param name="bodySize">Dimensione (in unità fisiche)</param>
        /// <param name="bodyPosition">Posizione del baricentro della parte (unità fisiche)</param>
        /// <param name="density">Densità della parte</param>
        /// <param name="world">World in cui inserire la parte</param>
        public FParte(Vector2 bodySize, Vector2 bodyPosition, float density, World world)
        {
            Initialize(bodySize, bodyPosition, density, world, Color.Black, Color.Black);
        }

        /// <summary>
        /// Costruttore di FParte
        /// </summary>
        /// <param name="bodySize">Dimensione (in unità fisiche)</param>
        /// <param name="bodyPosition">Posizione del baricentro della parte (unità fisiche)</param>
        /// <param name="density">Densità della parte</param>
        /// <param name="world">World in cui inserire la parte</param>
        /// <param name="bodyColor">Colore della parte</param>
        /// <param name="jointColor">Colore del giunto</param>
        public FParte(Vector2 bodySize, Vector2 bodyPosition, float density, World world, Color bodyColor, Color jointColor)
        {
            Initialize(bodySize, bodyPosition, density, world, bodyColor, jointColor);
        }

        private void Initialize(Vector2 bodySize, Vector2 bodyPosition, float density, World world, Color bodyColor, Color jointColor)
        {
            Body = BodyFactory.CreateBody(world, bodyPosition);
            Body.BodyType = BodyType.Dynamic;
            BodyFixture = FixtureFactory.CreateRectangle(bodySize.X, bodySize.Y, density, Vector2.Zero, Body);
            BodyFixture.UserData = this;
            BodySize = bodySize;
            BodyColor = bodyColor;
            JointColor = jointColor;
            World = world;
            Density = density;
            Joint = null;
            PartMotionSystem = MotionSystem.Actuator;
            CollidingSides = new bool[4];
            for (int i = 0; i < 4; i++)
                CollidingSides[i] = false;
            Collisions = new List<Collision>();
            ChildParts = new List<FParte>();
        }

        #endregion

        #region Draw
        /// <summary>
        /// Disegna la parte
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch da utilizzare</param>
        /// <param name="rectangularTexture">Texture rettangolare bianca, di dimensioni maggiori o uguali a quelle della parte (p.es. 1000x1000 pixel)</param>
        /// <param name="circularTexture">Texture circolare 100x100</param>
        /// <param name="zoomFactor">Rapporto di scala tra unità grafiche e unità fisiche</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D rectangularTexture, Texture2D circularTexture, float zoomFactor)
        {
            this.Draw(spriteBatch, rectangularTexture, circularTexture, zoomFactor, BodyColor, JointColor);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D rectangularTexture, Texture2D circularTexture, float zoomFactor, Color bodyColor, Color jointColor)
        {
            int circularTextureSize = circularTexture.Width;
            spriteBatch.Draw(rectangularTexture, Utils.ToGraphics(Body.Position, zoomFactor), Utils.GetDrawingRectangle(BodySize, zoomFactor), bodyColor, Body.Rotation, Utils.ToGraphics(BodySize / 2, zoomFactor), 1, SpriteEffects.None, 0);
            if (Joint != null)
            {
                spriteBatch.Draw(circularTexture, Utils.ToGraphics(Utils.TranslateAndRotate(JointOffset, Body.Position, Body.Rotation), zoomFactor), null, jointColor, 0, new Vector2(circularTextureSize) / 2, 2 * Utils.ToGraphics(new Vector2(JointRadius), zoomFactor) / 100, SpriteEffects.None, 0);
            }
            //Vertex[] vertices = GetVertices();
            //for (int i = 0; i < 4; i++)
            //{
            //    if (CollidingSides[i])
            //        DrawingHelper.DrawLine(spriteBatch, rectangularTexture, Utils.ToGraphics(vertices[i%4].Coords), Utils.ToGraphics(vertices[(i + 3)%4].Coords), Color.Red);
            //    else
            //        DrawingHelper.DrawLine(spriteBatch, rectangularTexture, Utils.ToGraphics(vertices[i%4].Coords), Utils.ToGraphics(vertices[(i + 3)%4].Coords), Color.Green);
            //}
        }
        #endregion

        #region Join
        /* VERSIONE OBSOLETA DA AGGIORNARE
        /// <summary>
        /// Posiziona la parte vicino a quella a cui deve unirsi e crea il giunto.
        /// </summary>
        /// <param name="otherPart">Altra parte a cui unire questa</param>
        /// <param name="thisPartPosition">Posizione del centro del giunto rispetto al baricentro di questa parte</param>
        /// <param name="otherPartPosition">Posizione del centro del giunto rispetto al baricentro dell'altra parte</param>
        /// <param name="jointColor">Colore del giunto</param>
        /// <returns>false se thisPartPosizion è all'interno della parte, true altrimenti</returns>
        public bool Join(FParte otherPart, Vector2 thisPartPosition, Vector2 otherPartPosition, Color jointColor)
        {
            Body.Position = Utils.TranslateAndRotate((otherPartPosition - thisPartPosition), otherPart.Body.Position, otherPart.Body.Rotation);
            Body.Rotation = otherPart.Body.Rotation;
            Joint = JointFactory.CreateRevoluteJoint(World, otherPart.Body, this.Body, thisPartPosition);
            JointOffset = new Vector2(thisPartPosition.X, thisPartPosition.Y);
            Joint.MotorEnabled = true;
            Joint.MaxMotorTorque = Const.MaxMotorTorquePerAreaUnit * this.BodySize.X * this.BodySize.Y;

            //determina la dimensione del giunto in base alla posizione rispetto al centro del corpo e alla dimensione del corpo stesso
            if ((thisPartPosition.X > BodySize.X / 2) && (Math.Abs(thisPartPosition.Y) <= BodySize.Y / 2))
            {
                JointRadius = thisPartPosition.X - BodySize.X / 2;
                ConnectionSide = Side.Right;
            }
            else if ((thisPartPosition.X < -BodySize.X / 2) && (Math.Abs(thisPartPosition.Y) <= BodySize.Y / 2))
            {
                JointRadius = -thisPartPosition.X - BodySize.X / 2;
                ConnectionSide = Side.Left;
            }
            else if ((thisPartPosition.Y > BodySize.Y / 2) && (Math.Abs(thisPartPosition.X) <= BodySize.X / 2))
            {
                JointRadius = thisPartPosition.Y - BodySize.Y / 2;
                ConnectionSide = Side.Bottom;
            }
            else if ((thisPartPosition.Y < -BodySize.Y / 2) && (Math.Abs(thisPartPosition.X) <= BodySize.X / 2))
            {
                JointRadius = -thisPartPosition.Y - BodySize.Y / 2;
                ConnectionSide = Side.Top;
            }
            else
                return false;

            JointFixture = FixtureFactory.CreateCircle(JointRadius, Density, Body, JointOffset);
            if(JointColor != null)
                JointColor = jointColor;
            Joint.CollideConnected = true;      //abilita la collisione dei due body connessi dal giunto
            PartActuator = new Actuator(this, otherPart, Const.MaxForcePerAreaUnit * BodyArea);
            return true;
        }
        */
        /// <summary>
        /// Posiziona la parte vicino a quella a cui deve unirsi e crea il giunto.
        /// </summary>
        /// <param name="otherPart">Altra parte a cui unire questa</param>
        /// <param name="thisPartSidePosition">Posizione del punto di contatto del giunto con il bordo di questa parte rispetto al centro di questa parte</param>
        /// <param name="otherPartSidePosition">Posizione del punto di contatto del giunto con il bordo dell'altra parte rispetto al centro dell'altra parte</param>
        /// <param name="jointColor">Colore del giunto</param>
        /// <param name="jointRadius">Raggio del giunto</param>
        /// <returns>false se thisPartSidePosition non è sul bordo della parte, true altrimenti</returns>
        public bool Join(FParte otherPart, Vector2 thisPartSidePosition, Vector2 otherPartSidePosition, Color jointColor, float jointRadius)
        {
            Vector2 jointOffset;
            if (thisPartSidePosition.X == -(BodySize.X / 2))
            {
                jointOffset = new Vector2(-jointRadius * 2, 0);
                ConnectionSide = Side.Left;
            }
            else if (thisPartSidePosition.X == (BodySize.X / 2))
            {
                jointOffset = new Vector2(jointRadius * 2, 0);
                ConnectionSide = Side.Right;
            }
            else if (thisPartSidePosition.Y == -(BodySize.Y / 2))
            {
                jointOffset = new Vector2(0, -jointRadius * 2);
                ConnectionSide = Side.Top;
            }
            else if (thisPartSidePosition.Y == (BodySize.Y / 2))
            {
                jointOffset = new Vector2(0, jointRadius * 2);
                ConnectionSide = Side.Bottom;
            }
            else
                return false;

            Body.Position = Utils.TranslateAndRotate(otherPartSidePosition - thisPartSidePosition - jointOffset, otherPart.Body.Position, otherPart.Body.Rotation);
            Body.Rotation = otherPart.Body.Rotation;
            if (Joint != null)
                World.RemoveJoint(Joint);
            Joint = JointFactory.CreateRevoluteJoint(World, otherPart.Body, this.Body, thisPartSidePosition + jointOffset/2);
            //Joint.MotorEnabled = true;
            //Joint.MaxMotorTorque = Const.MaxMotorTorquePerAreaUnit * this.BodySize.X * this.BodySize.Y;

            JointRadius = jointRadius;
            JointFixture = FixtureFactory.CreateCircle(JointRadius, Density, Body, thisPartSidePosition + jointOffset/2);
            if (JointColor != null)
                JointColor = jointColor;
            JointOffset = thisPartSidePosition + jointOffset / 2;
            Joint.CollideConnected = true;
            otherPart.AddChild(this);
            return true;

        }
        #endregion

        #region Movimento
        //public void ApplyMotion(float motionIntensity)
        //{
        //    if (Joint != null)
        //    {
        //        if (PartMotionSystem == MotionSystem.Actuator)
        //            //PartActuator.ApplyForce(motionIntensity);
        //        else
        //            SetMotorSpeed(Const.MaxSpeedPerLenghtUnit * JointRadius * motionIntensity);
        //    }
        //}

        internal void ApplyForce(Vector2 force, Vector2 worldPoint)
        {
            Body.ApplyForce(force, worldPoint);
        }

        private void SetMotorSpeed(float speed)
        {
            if (Joint != null)
                Joint.MotorSpeed = speed;
        }
        #endregion

        #region Gestione delle collisioni

        /// <summary>
        /// Restituisce i Vertex relativi agli angoli della parte, nel sistema assoluto
        /// </summary>
        /// <returns>I Vertex corrispondenti agli angoli della parte</returns>
        private Vertex[] GetVertices()
        {
            Vertex[] output = new Vertex[4];
            Vertex newVertex = new Vertex();
            newVertex.Coords = Body.GetWorldPoint(new Vector2(BodySize.X / 2, -BodySize.Y / 2));
            newVertex.Position = Corner.TopRight;
            output[0] = newVertex;
            newVertex.Coords = Body.GetWorldPoint(new Vector2(BodySize.X / 2, BodySize.Y / 2));
            newVertex.Position = Corner.BottomRight;
            output[1] = newVertex;
            newVertex.Coords = Body.GetWorldPoint(new Vector2(-BodySize.X / 2, BodySize.Y / 2));
            newVertex.Position = Corner.BottomLeft;
            output[2] = newVertex;
            newVertex.Coords = Body.GetWorldPoint(new Vector2(-BodySize.X / 2, -BodySize.Y / 2));
            newVertex.Position = Corner.TopLeft;
            output[3] = newVertex;
            return output;
        }

        public void UpdateCollisionPoints()
        {
            Collisions.Clear();
            for (int i = 0; i < 4; i++)
                CollidingSides[i] = false;
            if (Body.ContactList != null)
            {
                Contact currentContact = Body.ContactList.Contact;
                while (currentContact != null)
                {   //tiene conto solo dei Contact relativi alla BodyFixture
                    //(non so perché il Body contiene dei Contact che non sono più relativi alla sua Fixture)
                    //e ignora quelli dovuti al giunto
                    if (((currentContact.FixtureA == BodyFixture) || (currentContact.FixtureB == BodyFixture)) && 
                        (currentContact.FixtureA != JointFixture) && (currentContact.FixtureB != JointFixture))
                    {
                        bool childJoint = false;
                        foreach (FParte child in ChildParts)
                            if ((currentContact.FixtureA == child.JointFixture) || (currentContact.FixtureB == child.JointFixture))
                            {
                                childJoint = true;
                                break;
                            }
                        //ignora la collisione anche se è dovuta al giunto di una parte figlia
                        if (!childJoint)
                        {
                            //estrae i punti di collisione del contatto corrente
                            FixedArray2<Vector2> points = new FixedArray2<Vector2>();
                            Vector2 vec = new Vector2();
                            currentContact.GetWorldManifold(out vec, out points);
                            
                            for (int i = 0; i < 2; i++)
                            {
                                //alcuni Manifold contengono dei punti (0,0), non so perché. Vanno ignorati.
                                if (!points[i].Equals(Vector2.Zero))
                                {
                                    bool belongsToThisPart = false;
                                    ContactType contactType = ContactType.Corner;
                                    Corner corner;
                                    Side side = Side.Bottom;
                                    if (IsCorner(points[i], out corner))
                                    {
                                        contactType = ContactType.Corner;
                                        belongsToThisPart = true;
                                    }
                                    else if (IsSide(points[i], out side))
                                    {
                                        contactType = ContactType.Side;
                                        belongsToThisPart = true;
                                    }
                                    //non so perché ma alcuni contatti, relativi alla fixture giusta, non stanno sul bordo della fixture stessa
                                    //quindi prima verifico che siano o su un angolo o su un lato della parte
                                    if (belongsToThisPart)
                                    {
                                        Collision newCollision = new Collision();
                                        newCollision.Position = new Vector2(points[i].X, points[i].Y);
                                        newCollision.OtherFixture = (currentContact.FixtureA == BodyFixture) ? currentContact.FixtureB : currentContact.FixtureA;
                                        newCollision.Type = contactType;
                                        if (contactType == ContactType.Corner)
                                            newCollision.CollisionCorner = corner;
                                        else
                                            newCollision.CollisionSide = side;
                                        Collisions.Add(newCollision);
                                    }
                                }
                            }
                        }
                    }
                    //scorre la lista dei Contact
                    currentContact = currentContact.Next;
                }
                if (Collisions.Count > 0)
                {
                    //una collisione è "risolta" se è già stata trattata
                    bool[] solvedCollisions = new bool[Collisions.Count];
                    /* cerco coppie di collisioni relative alla stessa fixture, il che accade quando due fixture sono appoggiate l'una sull'altra
                     * o una a fianco dell'altra e sono tra loro "parallele"
                     * In questo caso solo il lato interessato viene contrassegnato come interessato da una collisione.
                     * (l'algoritmo credo sia quadratico ma non penso sia un problema, spesso ci sono 2-3 collisioni, mai più di 5-6)
                     * */
                    for (int i = 0; i < Collisions.Count; i++)
                        for (int j = i + 1; j < Collisions.Count; j++)
                        {
                            if (Collisions[i].OtherFixture == Collisions[j].OtherFixture)
                            {
                                /*se entrambe le collisioni sono segnate su angoli facendo l'AND tra i due Corner e valutando
                                 * la posizione dell'1 che si ottiene con RapidLog2 si ottiene il Side in comune tra i due
                                 * Corner (es. TopRight = 0011, TopLeft = 1001, TopRight & TopLeft = 0001, Log2(0001) = 0 = Top)
                                 */
                                if((Collisions[i].Type == ContactType.Corner) && (Collisions[j].Type == ContactType.Corner))
                                {
                                    int sideIndex = (int)Collisions[i].CollisionCorner & (int)Collisions[j].CollisionCorner;
                                    if (sideIndex != 0)
                                    {
                                        sideIndex = Utils.RapidLog2(sideIndex);
                                        CollidingSides[sideIndex] = true;
                                        solvedCollisions[i] = true;
                                        solvedCollisions[j] = true;
                                    }
                                }
                                //se almeno una collisione è su un lato ho già il lato interessato da una delle collisioni
                                //registrate su un lato
                                else if (Collisions[i].Type == ContactType.Side)
                                {
                                    CollidingSides[(int)Collisions[i].CollisionSide] = true;
                                    solvedCollisions[i] = true;
                                    solvedCollisions[j] = true;
                                }
                                else
                                {
                                    CollidingSides[(int)Collisions[j].CollisionSide] = true;
                                    solvedCollisions[i] = true;
                                    solvedCollisions[j] = true;
                                }

                            }
                        }
                    /* restano solo le collisioni singole (cioè con Fixture con cui la BodyFixture attuale ha un solo
                     * punto di contatto): se è un Corner abilita entrambe i lati che fanno capo a quel vertice, altrimenti
                     * se è un Side abilita solo il relativo lato
                     */
                    for (int i = 0; i < Collisions.Count; i++)
                        if (!solvedCollisions[i])
                        {
                            if (Collisions[i].Type == ContactType.Corner)
                            {
                                int sideIndex = Utils.RapidLog2((int)Collisions[i].CollisionCorner);
                                int otherSideIndex = Utils.RapidLog2((0xFFFE << sideIndex) & (int)Collisions[i].CollisionCorner);
                                CollidingSides[sideIndex] = true;
                                CollidingSides[otherSideIndex] = true;
                            }
                            else
                                CollidingSides[(int)Collisions[i].CollisionSide] = true;
                        }
                }
            }
        }

        /// <summary>
        /// Indica se un punto si trova su un lato, e se sì su quale (da richiamare dopo IsCorner solo se il punto non
        /// è su un angolo, altrimenti restituisce solo uno dei due lati relativi all'angolo)
        /// </summary>
        /// <param name="point">Punto da valutare, nel sistema assoluto</param>
        /// <param name="side">Lato su cui si trova eventualmente il punto (se la funzione restituisce false side vale Side.Bottom a prescindere)</param>
        /// <returns>true se il punto si trova ad una distanza inferiore di Const.Epsilon da un lato</returns>
        private bool IsSide(Vector2 point, out Side side)
        {
            Vector2 localPoint = this.Body.GetLocalPoint(point);
            bool returnValue = true;
            side = Side.Bottom;
            //collisione da sotto
            if (MathUtils.FloatEquals(localPoint.Y, BodySize.Y / 2, Const.Epsilon))
                side = Side.Bottom;
            //collisione da sopra
            else if (MathUtils.FloatEquals(localPoint.Y, -BodySize.Y / 2, Const.Epsilon))
                side = Side.Top;
            //collisione da destra
            else if (MathUtils.FloatEquals(localPoint.X, BodySize.X / 2, Const.Epsilon))
                side = Side.Right;
            //collisione da sinistra
            else if (MathUtils.FloatEquals(localPoint.X, -BodySize.X / 2, Const.Epsilon))
                side = Side.Left;
            else
                returnValue = false;

            return returnValue;
        }

        /// <summary>
        /// Indica se un punto si trova in corrispondenza di un angolo della parte e in caso affermativo di quale angolo
        /// </summary>
        /// <param name="point">Punto da valutare, nel sistema assoluto</param>
        /// <param name="corner">Angolo corrispondente eventualmente al punto (se la funzione restituisce false il valore di corner non è significativo)</param>
        /// <returns>true se point si trova ad una distranza inferiore di Const.Epsilon da un angolo</returns>
        private bool IsCorner(Vector2 point, out Corner corner)
        {
            Vertex[] vertices = GetVertices();
            foreach (Vertex vertex in vertices)
            {
                if ((point - vertex.Coords).Length() < Const.Epsilon)
                {
                    corner = vertex.Position;
                    return true;
                }
            }
            corner = 0;
            return false;
        }

        /// <summary>
        /// Valuta se un punto si trova o meno all'interno della BodyFixture
        /// </summary>
        /// <param name="point">Punto da valutare, nel sistema assoluto</param>
        /// <returns>true se point si trova all'inetrno della BodyFixture</returns>
        public bool TestPoint(Vector2 point)
        {
            return BodyFixture.TestPoint(ref point);
        }

        public Category CollisionCategory
        {
            get { return BodyFixture.CollisionFilter.CollisionCategories; }
            set 
            { 
                BodyFixture.CollisionFilter.CollisionCategories = value;
                if (JointFixture != null)
                {
                    JointFixture.CollisionFilter.CollisionCategories = value;
                    JointFixture.CollisionFilter.RemoveCollidesWithCategory(Category.All);
                    JointFixture.CollisionFilter.AddCollidesWithCategory(value);
                }
                BodyFixture.CollisionFilter.RemoveCollidesWithCategory(Category.All);
                BodyFixture.CollisionFilter.AddCollidesWithCategory(value);
            }
        }

        public void AddCollidesCategory(Category cat)
        {
            BodyFixture.CollisionFilter.AddCollidesWithCategory(cat);
            JointFixture.CollisionFilter.AddCollidesWithCategory(cat);
        }

        public void RemoveCollidesCategory(Category cat)
        {
            BodyFixture.CollisionFilter.RemoveCollidesWithCategory(cat);
            JointFixture.CollisionFilter.RemoveCollidesWithCategory(cat);
        }

        #endregion

        public void AddChild(FParte part)
        {
            ChildParts.Add(part);
        }
    }

    #region enum Side, Corner, MotionSystem, ContactType

    enum Side
    {
        Top = 0, Right = 1, Bottom = 2, Left = 3
    }

    enum Corner
    {
        TopRight = (1 << Side.Top) | (1 << Side.Right),
        TopLeft = (1 << Side.Top) | (1 << Side.Left),
        BottomRight = (1 << Side.Bottom) | (1 << Side.Right),
        BottomLeft = (1 << Side.Bottom) | (1 << Side.Left)
    }

    enum MotionSystem
    {
        Actuator, Motor
    }

    enum ContactType
    {
        Side, Corner
    }

    #endregion

    #region struct Vertex, Collision

    struct Vertex
    {
        public Corner Position;
        public Vector2 Coords;
    }

    struct Collision
    {
        public Vector2 Position;
        public Corner CollisionCorner;
        public Side CollisionSide;
        public ContactType Type;
        public Fixture OtherFixture;
    }

    #endregion
}
