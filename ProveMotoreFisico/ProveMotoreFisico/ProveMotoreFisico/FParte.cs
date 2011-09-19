using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;

namespace ProveMotoreFisico
{
    class FParte
    {
        Fixture BodyFixture, JointFixture;
        Body Body;
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
        public MotionSystem PartMotionSystem;
        public RevoluteJoint Joint;
        public Actuator PartActuator;
        public Side ConnectionSide;

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
            BodySize = bodySize;
            BodyColor = bodyColor;
            JointColor = jointColor;
            World = world;
            Density = density;
            Joint = null;
            PartActuator = null;
            PartMotionSystem = MotionSystem.Actuator;
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
            spriteBatch.Draw(rectangularTexture, Coord.ToGraphics(Body.Position, zoomFactor), Coord.GetDrawingRectangle(BodySize, zoomFactor), bodyColor, Body.Rotation, Coord.ToGraphics(BodySize / 2, zoomFactor), 1, SpriteEffects.None, 0);
            if (Joint != null)
            {
                spriteBatch.Draw(circularTexture, Coord.ToGraphics(Coord.TranslateAndRotate(JointOffset, Body.Position, Body.Rotation), zoomFactor), null, jointColor, 0, new Vector2(circularTextureSize) / 2, 2 * Coord.ToGraphics(new Vector2(JointRadius), zoomFactor) / 100, SpriteEffects.None, 0);
                PartActuator.Draw(spriteBatch, Color.Green, Color.Red, rectangularTexture, zoomFactor);
            }
        }
        #endregion

        #region Join
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
            Body.Position = Coord.TranslateAndRotate((otherPartPosition - thisPartPosition), otherPart.Body.Position, otherPart.Body.Rotation);
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

            Body.Position = Coord.TranslateAndRotate(otherPartSidePosition - thisPartSidePosition - jointOffset, otherPart.Body.Position, otherPart.Body.Rotation);
            Body.Rotation = otherPart.Body.Rotation;
            if (Joint != null)
                World.RemoveJoint(Joint);
            Joint = JointFactory.CreateRevoluteJoint(World, otherPart.Body, this.Body, thisPartSidePosition + jointOffset/2);
            Joint.MotorEnabled = true;
            Joint.MaxMotorTorque = Const.MaxMotorTorquePerAreaUnit * this.BodySize.X * this.BodySize.Y;

            JointRadius = jointRadius;
            JointFixture = FixtureFactory.CreateCircle(JointRadius, Density, Body, thisPartSidePosition + jointOffset/2);
            if (JointColor != null)
                JointColor = jointColor;
            JointOffset = thisPartSidePosition + jointOffset / 2;
            Joint.CollideConnected = true;
            PartActuator = new Actuator(this, otherPart, Const.MaxForcePerAreaUnit * BodyArea);
            return true;

        }
        #endregion

        #region Movimento
        public void ApplyMotion(float motionIntensity)
        {
            if (Joint != null)
            {
                if (PartMotionSystem == MotionSystem.Actuator)
                    PartActuator.ApplyForce(motionIntensity);
                else
                    SetMotorSpeed(Const.MaxSpeedPerLenghtUnit * JointRadius * motionIntensity);
            }
        }

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
    }

    enum Side
    {
        Left, Right, Top, Bottom
    }

    enum MotionSystem
    {
        Actuator, Motor
    }
}
