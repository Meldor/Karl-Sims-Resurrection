using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FormListBox = System.Windows.Forms.ListBox;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Runtime.InteropServices;



namespace ProveMotoreFisico
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        [DllImport("user32.dll")]

        static extern IntPtr GetForegroundWindow(); 

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont arial;

        Texture2D rectTexture;
        Texture2D circTexture;

        Vector2 textureOrigin;
        Vector2 rectangleSize;
        List<Fixture> blocks;

        Camera camera;
        World world;
        
        Vector2 floorSize, floorPosition;
        Fixture floor;

        List<FParte> partList;
        FParte selectedPart;
        Rectangle partWhileDrawed;
        bool drawingPart = false;
        
        FormListBox list;

        Thread trd;
        int comando = 0;
        bool muscoli = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = Const.ScreenHeigh;
            graphics.PreferredBackBufferWidth = Const.ScreenWidth;
            Content.RootDirectory = "Content";

            trd = new Thread(new ThreadStart(this.ServerMethod));
            trd.IsBackground = true;
            trd.Start();
        }

        /*protected override void OnExiting(object sender, EventArgs args)
        {
            client.Close();
            base.OnExiting(sender, args);
        }*/

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            rectangleSize = new Vector2(2, 2);
            floorSize = new Vector2(500, 1);
            floorPosition = new Vector2(0, 40);
            textureOrigin = rectangleSize / 2;
            blocks = new List<Fixture>();
            
            world = new World(new Vector2(0, Const.Gravity));
            rectTexture = RectangularTexture(graphics.GraphicsDevice, 1000, 1000, Color.White);

            floor = FixtureFactory.CreateRectangle(world, floorSize.X, floorSize.Y, 0.1f, floorPosition);
            floor.Body.BodyType = BodyType.Static;
            floor.Restitution = 0.2f;
            floor.Friction = 0.3f;

            camera = new Camera();
            camera.Pos = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height*0.5f);

            String[] listItems = new String[2];
            listItems[0] = "Creazione di blocchi";
            listItems[1] = "Manipolazione creatura";
            list = WinFormsHelper.CreateListBox(listItems, new Vector2(10, Const.ScreenHeigh - Const.ControlsAreaHeigh + 10), null, Window);
            /*
            System.Windows.Forms.TextBox text = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Control.FromHandle(Window.Handle).Controls.Add(text);*/
            //

            partList = new List<FParte>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            arial = Content.Load<SpriteFont>("Arial");
            circTexture = Content.Load<Texture2D>("cerchio");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        KeyboardState kPreviousState, kCurrentState;
        MouseState mPreviousState, mCurrentState;
        Vector2 start, end;  //punto di inizio e fine del drag and drop per creare e lanciare un blocco con velocità iniziale non nulla
        Vector2 mousePos;  //posizione del mouse nelle coordinate grafiche trasformate grazie alla telecamera (coordinate effettive all'interno del mondo simulato)
        Matrix matrice;  //matrice di traslazione dovuta alla telecamera
        bool started = false; //indica se l'utente ha cominciato a tracciare un vettore che indica la velocità iniziale del blocco
        bool moving = false; //indica se è abilitato o meno il movimento delle parti (BodyType.Dynamic o Static)
        bool onSide = false; //indica se il mouse si trova in un punto che permette di posizionare correttamente partRelativeMouseProjection
        bool noCollision = true; //a true se il mouse non si trova all'interno di nessuna parte
        bool creatingJoint = false; //a true durante il trascinamento per definire la dimensione del joint
        bool pendingJoint = false; //a true tra l'istante in cui la dimensione del joint viene definita e quando avviene l'unione tra le due parti
        
        FParte part1; //parte padre coinvolta nel Join
        Vector2 part1JointPos;  //posizione del bordo del joint rispetto al centro di part1 in coordinate grafiche
        float jointRadius;  //raggio attuale del joint (durante la fase di creazione del joint)
        Vector2 jointCenter;   //centro attuale del joint in coordinate grafiche e assolute

        Vector2 partRelativeMouseProjection; //proiezione della posizione del mouse sul bordo più vicino della parte selezionata, se è possibile calcolarla
        //(corrisponde alla posizione, in coordinate grafiche e relative alla parte selezionata, del quadratino rosso)
        Side projectionSide;  //lato in cui si trova la proiezione del mouse

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            kCurrentState = Keyboard.GetState();
            mCurrentState = Mouse.GetState();

            //controlla l'input solo se la finestra ha il focus per evitare input non desiderati mentre si utilizza un altra applicazione
            //(ad esempio il client di controllo)
            if (GetForegroundWindow() == this.Window.Handle)
            {

                #region Telecamera
                //matrice di traslazione per passare dal sistema di riferimento della finestra a quello del mondo, traslato a causa della telecamera
                matrice = Matrix.CreateTranslation(new Vector3(camera.Pos, 0) - new Vector3(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f, 0));
                //posizione del mouse nelle coordinate traslate tenendo conto della telecamera
                mousePos = Vector2.Transform(new Vector2(mCurrentState.X, mCurrentState.Y), matrice);

                //varia lo zoom
                if (kCurrentState.IsKeyDown(Keys.H))
                    Const.Zoom = Const.Zoom + 1;
                if (kCurrentState.IsKeyDown(Keys.L))
                {
                    if ((Const.Zoom - 1) > 0)
                        Const.Zoom = Const.Zoom - 1;
                }



                //movimento telecamera
                if (kCurrentState.IsKeyDown(Keys.W))
                    camera.Muovi(new Vector2(0, -3));
                if (kCurrentState.IsKeyDown(Keys.A))
                    camera.Muovi(new Vector2(-3, 0));
                if (kCurrentState.IsKeyDown(Keys.S))
                    camera.Muovi(new Vector2(0, 3));
                if (kCurrentState.IsKeyDown(Keys.D))
                    camera.Muovi(new Vector2(3, 0));
                #endregion

                //blocca/muove le parti
                if (kCurrentState.IsKeyDown(Keys.P) && kPreviousState.IsKeyUp(Keys.P))
                {
                    moving = !moving;
                    if (moving)
                        foreach (FParte part in partList)
                            part.BodyType = BodyType.Dynamic;
                    else
                        foreach (FParte part in partList)
                            part.BodyType = BodyType.Static;
                }

                #region Selezione parte
                //controlla se si tenta di selezionare una parte
                if (!drawingPart && (mCurrentState.LeftButton == ButtonState.Pressed) && (mPreviousState.LeftButton == ButtonState.Released))
                    foreach (FParte parte in partList)
                    {
                        Vector2 partRelativeMousePos = Coord.InvTranslateAndRotate(mousePos, Coord.ToGraphics(parte.Position, Const.Zoom), parte.Rotation);
                        if ((Math.Abs(partRelativeMousePos.X) <= Coord.ToGraphics(parte.BodySize, Const.Zoom).X) && (Math.Abs(partRelativeMousePos.Y) <= Coord.ToGraphics(parte.BodySize, Const.Zoom).Y))
                            selectedPart = parte;
                    }
                if (selectedPart != null) //se è stata selezionata una parte tenta di calcolare partRelativeMouseProjection
                {
                    //coordinate del mouse rispetto al sistema di riferimento della parte
                    partRelativeMouseProjection = Coord.InvTranslateAndRotate(mousePos, Coord.ToGraphics(selectedPart.Position, Const.Zoom), selectedPart.Rotation);
                    //passa in coordinate fisiche (per evitare di effettuare molte più conversioni in coordinate grafiche
                    partRelativeMouseProjection = Coord.ToPhysics(partRelativeMouseProjection, Const.Zoom);
                    //verifica se il mouse è in corrispondenza di uno dei lati della parte e, in caso affermativo, aggiorna il vettore
                    //con la proiezione della posizione del mouse sul bordo della creatura (altrimenti non la modifica)
                    if ((partRelativeMouseProjection.X < 0) && (Math.Abs(partRelativeMouseProjection.Y) <= (selectedPart.BodySize.Y / 2)))
                    {
                        partRelativeMouseProjection.X = -selectedPart.BodySize.X / 2;
                        onSide = true;
                        projectionSide = Side.Left;
                    }
                    else if ((partRelativeMouseProjection.X >= 0) && (Math.Abs(partRelativeMouseProjection.Y) <= (selectedPart.BodySize.Y / 2)))
                    {
                        partRelativeMouseProjection.X = selectedPart.BodySize.X / 2;
                        onSide = true;
                        projectionSide = Side.Right;
                    }
                    else if ((partRelativeMouseProjection.Y < 0) && (Math.Abs(partRelativeMouseProjection.X) <= (selectedPart.BodySize.X / 2)))
                    {
                        partRelativeMouseProjection.Y = -selectedPart.BodySize.Y / 2;
                        onSide = true;
                        projectionSide = Side.Top;
                    }
                    else if ((partRelativeMouseProjection.Y >= 0) && (Math.Abs(partRelativeMouseProjection.X) <= (selectedPart.BodySize.X / 2)))
                    {
                        partRelativeMouseProjection.Y = selectedPart.BodySize.Y / 2;
                        onSide = true;
                        projectionSide = Side.Bottom;
                    }
                    else
                        onSide = false;
                    //torna in coordinate grafiche
                    partRelativeMouseProjection = Coord.ToGraphics(partRelativeMouseProjection, Const.Zoom);
                }
                //verifica se il mouse si trova o meno all'interno di una parte e aggiorna noCollision
                noCollision = true;
                foreach (FParte parte in partList)
                {
                    Vector2 partRelativeMousePos = Coord.InvTranslateAndRotate(mousePos, Coord.ToGraphics(parte.Position, Const.Zoom), parte.Rotation);
                    if ((Math.Abs(partRelativeMousePos.X) <= (Coord.ToGraphics(parte.BodySize, Const.Zoom).X / 2)) && (Math.Abs(partRelativeMousePos.Y) <= (Coord.ToGraphics(parte.BodySize, Const.Zoom).Y) / 2))
                    {
                        noCollision = false;
                        break;
                    }
                }
                #endregion

                //switch (list.SelectedIndex)
                switch (comando)
                {
                    case 0: //creazione blocchi
                        #region Creazione blocchi
                        if (kCurrentState.IsKeyDown(Keys.Space) && kPreviousState.IsKeyUp(Keys.Space) && (mCurrentState.Y <= (Const.ScreenHeigh - Const.ControlsAreaHeigh)))
                        {
                            //creazione di un blocco inizialmente fermo nella posizione attuale del mouse, premendo spazio
                            Fixture newBlock = FixtureFactory.CreateRectangle(world, rectangleSize.X, rectangleSize.Y, 0.01f, Coord.ToPhysics(mousePos, Const.Zoom));
                            newBlock.Body.BodyType = BodyType.Dynamic;
                            newBlock.Restitution = 0.4f;
                            newBlock.Friction = 0.3f;
                            blocks.Add(newBlock);
                        }
                        if ((mCurrentState.LeftButton == ButtonState.Pressed) && (mPreviousState.LeftButton == ButtonState.Released) && (mCurrentState.Y <= (Const.ScreenHeigh - Const.ControlsAreaHeigh)))
                        {
                            //punto di rilascio del blocco con velocità iniziale non nulla e punto d'inizio del vettore immaginario che ne definisce il vettore velocità
                            start = Coord.ToPhysics(mousePos, Const.Zoom);
                            started = true;
                        }
                        else if ((mCurrentState.LeftButton == ButtonState.Released) && (mPreviousState.LeftButton == ButtonState.Pressed) && started)
                        {
                            //ho rilasciato il mouse sx -> crea il blocco
                            started = false;
                            end = Coord.ToPhysics(mousePos, Const.Zoom);
                            Fixture newBlock = FixtureFactory.CreateRectangle(world, rectangleSize.X, rectangleSize.Y, Const.PartDensity, new Vector2(start.X, start.Y));
                            newBlock.Body.BodyType = BodyType.Dynamic;
                            newBlock.Restitution = 0.4f;
                            newBlock.Friction = 0.3f;
                            newBlock.Body.IsBullet = true;
                            newBlock.Body.LinearVelocity = (end - start);
                            blocks.Add(newBlock);
                        }
                        if ((mCurrentState.RightButton == ButtonState.Released) && (mPreviousState.RightButton == ButtonState.Pressed) && (mCurrentState.Y < (Const.ScreenHeigh - Const.ControlsAreaHeigh)))
                            createExplosion(mousePos / Const.Zoom, blocks);
                        #endregion
                        break;
                    case 1: //manipolazione creatura
                        #region Creazione nuova parte
                        if ((mCurrentState.LeftButton == ButtonState.Pressed) && (mCurrentState.Y < (Const.ScreenHeigh - Const.ControlsAreaHeigh)))
                            if ((mPreviousState.LeftButton == ButtonState.Released) && noCollision)
                            {
                                //ho appena premuto il tasto sx del mouse e non sono in area controlli
                                //-> se il mouse non si trova all'interno di una parte inizia a disegnarne una nuova
                                partWhileDrawed = new Rectangle(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y), 1, 1);
                                drawingPart = true;
                            }
                            else
                            {
                                //sto tenendo premuto il tasto sx -> aggiorna partWhileDrawed
                                if (mousePos.X > partWhileDrawed.X)
                                    partWhileDrawed.Width = Convert.ToInt32(mousePos.X - partWhileDrawed.X);
                                else
                                {
                                    int temp = partWhileDrawed.X;
                                    partWhileDrawed.X = Convert.ToInt32(mousePos.X);
                                    partWhileDrawed.Width = Convert.ToInt32(mousePos.X) - temp;
                                }
                                if (mousePos.Y > partWhileDrawed.Y)
                                    partWhileDrawed.Height = Convert.ToInt32(mousePos.Y) - partWhileDrawed.Y;
                                else
                                {
                                    int temp = partWhileDrawed.Y;
                                    partWhileDrawed.Y = Convert.ToInt32(mousePos.Y);
                                    partWhileDrawed.Height = Convert.ToInt32(mousePos.Y) - temp;
                                }
                            }
                        else if (drawingPart) //else di if(mCurrentState.LeftButton == Pressed) ecc.
                        {
                            //ho appena rilasciato il tasto sx e stavo creando una nuova parte -> la crea
                            drawingPart = false;
                            Vector2 partSize = Coord.ToPhysics(new Vector2(partWhileDrawed.Width, partWhileDrawed.Height), Const.Zoom);
                            Vector2 partPosition = Coord.ToPhysics(new Vector2(partWhileDrawed.Center.X, partWhileDrawed.Center.Y), Const.Zoom);
                            if ((partSize.X >= 0.05f) && (partSize.Y >= 0.05f)) //crea la parte solo se è sufficientemente grande, per evitare di causare blocchi del programma
                            {
                                FParte newPart = new FParte(partSize, partPosition, Const.PartDensity, world, Color.Blue, Color.White);
                                if (moving)
                                    newPart.BodyType = BodyType.Dynamic;
                                else
                                    newPart.BodyType = BodyType.Static;
                                partList.Add(newPart);
                                newPart.BodyFixture.OnCollision = new OnCollisionEventHandler(gestione_evento);
                            }
                        }
                        #endregion

                        #region Creazione giunti
                        if (!pendingJoint) //se devo ancora iniziare a creare il giunto
                        {
                            if ((mCurrentState.RightButton == ButtonState.Pressed) && (mCurrentState.Y <= (Const.ScreenHeigh - Const.ControlsAreaHeigh)) && !moving)
                            {
                                if ((mPreviousState.RightButton == ButtonState.Released) && onSide && !creatingJoint)
                                {   //ho appena cliccato col tasto dx e sono correttamente aggangiato sul bordo della parte selezionata
                                    //-> inizio a creare il giunto
                                    creatingJoint = true;
                                    part1 = selectedPart;
                                    part1JointPos = new Vector2(partRelativeMouseProjection.X, partRelativeMouseProjection.Y);
                                    //(in coordinate grafiche e riferito alla posizione di part1)
                                }
                                else if (noCollision && creatingJoint)
                                {
                                    //il mouse è ancora premuto, quindi sto trascinando per determinare la dimensione del giunto
                                    Vector2 relMousePos = Coord.InvTranslateAndRotate(mousePos, Coord.ToGraphics(part1.Position, Const.Zoom), part1.Rotation);
                                    Vector2 relMousePosPhysics = Coord.ToPhysics(relMousePos, Const.Zoom);
                                    Vector2 part1JointPosPhysics = Coord.ToPhysics(part1JointPos, Const.Zoom);
                                    if ((projectionSide == Side.Right) && (relMousePosPhysics.X > (part1.BodySize.X / 2)))
                                    {   //bordo destro della parte
                                        jointRadius = relMousePos.X - part1JointPos.X;
                                        if (jointRadius < 0.05f) //per evitare dimensioni nulle o negative
                                            jointRadius = 0.05f;
                                        //centro del giunto in coordinate grafiche e assolute
                                        jointCenter = Coord.TranslateAndRotate(part1JointPos + new Vector2(jointRadius, 0), Coord.ToGraphics(part1.Position, Const.Zoom), part1.Rotation);
                                    }
                                    else if ((projectionSide == Side.Left) && (relMousePosPhysics.X < -(part1.BodySize.X / 2)))
                                    {   //bordo sinistro della parte
                                        jointRadius = part1JointPos.X - relMousePos.X;
                                        if (jointRadius < 0.05f)
                                            jointRadius = 0.05f;
                                        jointCenter = Coord.TranslateAndRotate(part1JointPos - new Vector2(jointRadius, 0), Coord.ToGraphics(part1.Position, Const.Zoom), part1.Rotation);
                                    }
                                    else if ((projectionSide == Side.Bottom) && (relMousePosPhysics.Y > (part1.BodySize.Y / 2)))
                                    {   //bordo inferiore della parte
                                        jointRadius = relMousePos.Y - part1JointPos.Y;
                                        if (jointRadius < 0.05f)
                                            jointRadius = 0.05f;
                                        jointCenter = Coord.TranslateAndRotate(part1JointPos + new Vector2(0, jointRadius), Coord.ToGraphics(part1.Position, Const.Zoom), part1.Rotation);
                                    }
                                    else if ((projectionSide == Side.Top) && (relMousePosPhysics.Y < -(part1.BodySize.Y / 2)))
                                    {   //bordo superiore della parte
                                        jointRadius = part1JointPos.Y - relMousePos.Y;
                                        if (jointRadius < 0.05f)
                                            jointRadius = 0.05f;
                                        jointCenter = Coord.TranslateAndRotate(part1JointPos - new Vector2(0, jointRadius), Coord.ToGraphics(part1.Position, Const.Zoom), part1.Rotation);
                                    }
                                }
                            }
                            else if (creatingJoint)
                            {
                                //ho appena rilasciato il tasto dx -> il giunto ha le dimensioni definitive
                                creatingJoint = false;
                                pendingJoint = true;
                            }
                        }   //-> pendingJoint è true -> il giunto è già stato creato, occorre scegliere la seconda parte
                        else if ((mCurrentState.RightButton == ButtonState.Pressed) && (mCurrentState.Y <= (Const.ScreenHeigh - Const.ControlsAreaHeigh)) && !moving && (mPreviousState.RightButton == ButtonState.Released) && onSide && (selectedPart != part1))
                        {
                            Vector2 part2JointPosPhysics = Coord.ToPhysics(partRelativeMouseProjection, Const.Zoom);
                            Vector2 part1JointPosPhysics = Coord.ToPhysics(part1JointPos, Const.Zoom);
                            selectedPart.Join(part1, part2JointPosPhysics, part1JointPosPhysics, Color.Aqua, jointRadius / Const.Zoom);
                            pendingJoint = false;
                        }
                        #endregion
                        break;
                }

                //applica le forze ai muscoli della parte selezionata, se possiede un Actuator (cioè se è una parte figlia)
                if ((selectedPart != null) && moving && (selectedPart.PartActuator != null))
                {
                    if (muscoli)
                        selectedPart.PartMotionSystem = MotionSystem.Actuator;
                    else
                        selectedPart.PartMotionSystem = MotionSystem.Motor;
                    if (kCurrentState.IsKeyDown(Keys.M))
                        selectedPart.ApplyMotion(1.0f);
                    else if (kCurrentState.IsKeyDown(Keys.N))
                        selectedPart.ApplyMotion(-1.0f);
                    else
                        selectedPart.ApplyMotion(0.0f);
                }
            }

            world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            kPreviousState = kCurrentState;
            mPreviousState = mCurrentState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //tiene conto della telecamera
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, camera.OttieniTrasformazione(GraphicsDevice));

            //disegna il pavimento
            spriteBatch.Draw(rectTexture, Const.Zoom*floor.Body.Position, new Rectangle(0,0,Const.Zoom*(int)floorSize.X, Const.Zoom*(int)floorSize.Y), Color.Black, floor.Body.Rotation, (floorSize/2)*Const.Zoom, 1, SpriteEffects.None, 0);
            //disegna i blocchi
            foreach (Fixture fixture in blocks)
                spriteBatch.Draw(rectTexture, Const.Zoom * fixture.Body.Position, new Rectangle(0, 0, Const.Zoom * (int)rectangleSize.X, Const.Zoom * (int)rectangleSize.Y), Color.White, fixture.Body.Rotation, (rectangleSize / 2) * Const.Zoom, 1, SpriteEffects.None, 0);
            //disegna le parti
            foreach (FParte part in partList)
            {
                if (part == selectedPart)
                    part.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom, Color.Yellow, Color.Yellow);
                else
                    part.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            }
            if (drawingPart)
                spriteBatch.Draw(rectTexture, partWhileDrawed, Color.White);
            //disegna il giunto in fase di creazione
            if (creatingJoint || pendingJoint)
                spriteBatch.Draw(circTexture, jointCenter, null, Color.Bisque, 0, new Vector2(circTexture.Width) / 2, 2 * jointRadius / 100, SpriteEffects.None, 0);
            //disegna il cursore sul bordo della parte selezionata
            if ((partList.Count >= 1) && (selectedPart != null))
            {
                spriteBatch.Draw(rectTexture, Coord.TranslateAndRotate(partRelativeMouseProjection, Coord.ToGraphics(selectedPart.Position, Const.Zoom), selectedPart.Rotation), new Rectangle(0, 0, 4, 4), Color.Red, 0f, new Vector2(2, 2), 1f, SpriteEffects.None, 0);
                if(selectedPart.Joint != null)
                    if(muscoli)
                        spriteBatch.DrawString(arial, "MaxForcePerAreaUnit: " + selectedPart.PartActuator.MaxForce.ToString(), new Vector2(0, 40), Color.White);
                    else
                        spriteBatch.DrawString(arial, "MotorTorque: " + selectedPart.Joint.MotorTorque.ToString(), new Vector2(0, 40), Color.White);
            }
            //testo di debug
            spriteBatch.DrawString(arial, "MousePos: " + mousePos.X.ToString() + "," + mousePos.Y.ToString(), Vector2.Zero, Color.Black);
            spriteBatch.DrawString(arial, "Posizione non trasformata: " + mCurrentState.X.ToString() + "," + mCurrentState.Y.ToString(), new Vector2(0, 20), Color.White);
            
            spriteBatch.End();
            

            //evidenzia l'area dei controlli
            spriteBatch.Begin();
            spriteBatch.Draw(rectTexture, new Vector2(0, Const.ScreenHeigh - Const.ControlsAreaHeigh), new Rectangle(0, 0, Const.ScreenWidth, Const.ControlsAreaHeigh), Color.Gray, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
            spriteBatch.End();

            

            base.Draw(gameTime);
        }


        #region Generazione Texture
        /// <summary>
        /// Genera una texture rettangolare
        /// </summary>
        /// <param name="graphicsDevice">GraphicsDevice (da graphics.GraphicsDevice)</param>
        /// <param name="width">Larghezza della texture</param>
        /// <param name="height">ALtezza della texture</param>
        /// <param name="color">Colore della texture</param>
        /// <returns>Texture</returns>
        private Texture2D RectangularTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            int totPixel = width * height;
            Texture2D texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
            Color[] colorArray = new Color[totPixel];

            for (int i = 0; i < totPixel; i++)
                colorArray[i] = color;
            texture.SetData(colorArray);

            return texture;
        }

        /// <summary>
        /// Genera una texture quadrata con un cerchio (non molto bene...)
        /// </summary>
        /// <param name="graphicsDevice">graphics.GraphicsDevice</param>
        /// <param name="radius">Raggio del cerchio (che sarà metà del lato della texture)</param>
        /// <param name="color">Colore del cerchio</param>
        /// <returns>Texture</returns>
        private Texture2D CircularTexture(GraphicsDevice graphicsDevice, int radius, Color color)
        {
            int totPixel = 4 * radius * radius;
            Texture2D texture = new Texture2D(graphicsDevice, 2 * radius, 2 * radius, false, SurfaceFormat.Color);
            Color[] colorArray = new Color[totPixel];

            for (int x = 0; x < 2 * radius; x++)
                for (int y = 0; y < 2 * radius; y++)
                    if (Math.Sqrt((x-radius) * (x-radius) + (y-radius) * (y-radius)) < radius)
                        colorArray[x * 2 * radius + y] = color;
                    else
                        colorArray[x * 2 * radius + y].A = 0;
            texture.SetData(colorArray);
            return texture;
        }
        #endregion

        private void createExplosion(Vector2 position, List<Fixture> blocks)
        {
            foreach (Fixture block in blocks)
            {
                Vector2 offset = block.Body.Position - position;
                float distance = offset.Length();
                offset.Normalize();
                block.Body.ApplyForce((offset / distance)*300); /* 300 è un coefficiente trovato "per tentativi" :) */
            }
        }


        
        private void ServerMethod()
        {
            TcpListener server = null;
            NetworkStream stream = null;
            TcpClient client = null;
            Int32 port = 13000, i;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            bool fine = false;
            Byte[] bytes = new Byte[256];
            String data = null;
            Thread trdBB;
            
            server = new TcpListener(localAddr, port);
            server.Start();

            while (true)
            {
                fine = false;
                client = server.AcceptTcpClient();
                stream = client.GetStream();


                while (!fine)
                    if ((i = stream.Read(bytes, 0, bytes.Length)) != 0) //legge fino a 256 caratteri dallo stream di rete
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i); // interpreta lo stream a blocchi di 1Byte cod.ASCII
                        data.ToLower();
                        /* Qui si mettono i comandi a cui risponde il server */

                        if (data == "b")
                        {
                            comando = 0;
                            data = "Modalita cubetti attivata";
                        }
                        else if (data == "d")
                        {
                            comando = 1;
                            data = "Modalita disegno attivata";
                        }
                        else if (data == "help")
                        {
                            data = "\nb -> modalita' cubetti\nd -> modalita' disegno\na -> abilita muscoli\nm -> abilita motori\nquit -> chiudi client";
                        }
                        else if (data == "a")
                        {
                            muscoli = true;
                            data = "Modalita' di movimento: muscoli";
                        }
                        else if (data == "m")
                        {
                            muscoli = false;
                            data = "Modalita' di movimento: motore";
                        }
                        /*else if (data == "e")
                        {
                            ServerMethod_selezionaParti(stream);
                            data = "End seleziona Parti";
                        }*/
                        else if (data == "BB")
                        {
                            trdBB = new Thread(new ThreadStart(this.ServerMethod_bigBrother));
                            trdBB.IsBackground = true;
                            trdBB.Start();
                            data = "Controllo parti avviato";
                        }
                        else if (data == "exit" || data == "quit")
                        {
                            fine = true;
                            data = "Server disconnesso...";
                        }
                        else
                            data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        stream.Write(msg, 0, msg.Length);
                    }

                // Shutdown and end connection
                if (fine)
                    System.Threading.Thread.Sleep(50); //Aspetta che il client legga

                client.Close();
                
            }

            //return;
        }

        /*private void ServerMethod_selezionaParti(NetworkStream stream)
        {
            Byte[] bytes = new Byte[256];
            bool fine = false;
            int i;
            String data = null;
            byte[] msg;
            
            msg = System.Text.Encoding.ASCII.GetBytes("Selezione Parti\n\ne -> elenca parti\nexit -> uscire");
            stream.Write(msg, 0, msg.Length);
            stream.Flush();

            while (!fine)
                if ((i = stream.Read(bytes, 0, bytes.Length)) != 0) //legge fino a 256 caratteri dallo stream di rete
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    if (data == "e")
                    {
                        data = System.Convert.ToString(partList.Count);
                        data = String.Concat(data, " elementi");
                    }
                    else if (data == "exit")
                    {
                        data = "in uscita...";
                        fine = true;
                    }
                    else
                        data = data.ToUpper();

                    msg = System.Text.Encoding.ASCII.GetBytes(data);
                    stream.Write(msg, 0, msg.Length);

                }

        }*/

        private void ServerMethod_bigBrother()
        {

            TcpListener server = null;
            TcpClient client = null;
            NetworkStream stream = null;
            Int32 port = 14000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            bool fine = false;
            Byte[] bytes = new Byte[256], msg;
            String data = null;
            int i, j=-1;
            
            server = new TcpListener(localAddr, port);
            server.Start();
            
            client = server.AcceptTcpClient();
            stream = client.GetStream();
            
            while (!fine)
            {
                
                if ((i = stream.Read(bytes, 0, bytes.Length)) != 0) //legge fino a 256 caratteri dallo stream di rete
                {
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    if (data == "*")    //Il client è funzionante e richiede informazioni
                    {
                        if (j == -1)  //Invio numero di elementi presenti
                            data = System.Convert.ToString(partList.Count);
                        else if (j >= partList.Count) // Inviate tutte le informazioni
                        {
                            j = -2;
                            data = "#";
                        }
                        else  //Invio le informazioni di un rettangolo
                            data = String.Concat(partList[j].Position.X, "\n", partList[j].Position.Y, "\n", partList[j].Rotation);
                        j++;
                    }
                    else if (data == "#") //Il client si sta chiudendo
                        fine = true;
                    else   //Comando non riconosciuto
                        data = data.ToUpper();

                    msg = System.Text.Encoding.ASCII.GetBytes(data);
                    stream.Write(msg, 0, msg.Length);

                }
            }
        
        client.Close();
        server.Stop();
        return;
        }




        public bool gestione_evento(Fixture f1, Fixture f2, Contact contact)
        {
            string stringa = contact.ToString();
            
            return true;
        }
    }
}
