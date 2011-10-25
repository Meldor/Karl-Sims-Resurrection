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



namespace KSR_visual
{
    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variabili
        /* Risorse Grafiche */
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont arial;
        Texture2D rectTexture;
        Texture2D circTexture;
        List<FenotipoCell> partList;

        /* Utilità */
        KeyboardState kPreviousState, kCurrentState;
        MouseState mPreviousState, mCurrentState;
        Vector2 mousePos;
        Camera camera;
        Matrix matrice;
        
            Boolean muscoli;
            int comando = 0;
        
        /* Oggetti Fisici */
        World world;
        FloorClass floor;
        #endregion

        #region MetodiXNA
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            
            graphics.PreferredBackBufferHeight = Const.ScreenHeigh;
            graphics.PreferredBackBufferWidth = Const.ScreenWidth;
            
            /*trd = new Thread(new ThreadStart(this.ServerMethod));
            trd.IsBackground = true;
            trd.Start();*/

        }

       
        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            camera = new Camera(); // Creo la telecamera
            camera.Pos = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f);

            rectTexture = Utils.RectangularTexture(graphics.GraphicsDevice, 1000, 1000, Color.White); // Creo una texture rettangolare
                        
            world = new World(new Vector2(0, Const.Gravity));  // Creo il mondo
                        
            floor = new FloorClass(); // Creo il pavimento
            floor.setFixture(world);

                                              
            base.Initialize();
        }

        
        protected override void LoadContent()
        {
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            arial = Content.Load<SpriteFont>("Arial");
            circTexture = Content.Load<Texture2D>("cerchio");
        }

        
        protected override void UnloadContent()
        {
            
        }

        
        protected override void Update(GameTime gameTime)
        {

            kCurrentState = Keyboard.GetState();
            mCurrentState = Mouse.GetState();

            /* Coordinate del Mouse */
            matrice = Matrix.CreateTranslation(new Vector3(camera.Pos, 0) - new Vector3(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f, 0));
            mousePos = Vector2.Transform(new Vector2(mCurrentState.X, mCurrentState.Y), matrice);

            /* Sposto la telecamera */
                if (kCurrentState.IsKeyDown(Keys.H))
                    Const.Zoom = Const.Zoom + 1;
                else if (kCurrentState.IsKeyDown(Keys.L))
                {
                    if ((Const.Zoom - 1) > 0)
                        Const.Zoom = Const.Zoom - 1;
                }   
                else if (kCurrentState.IsKeyDown(Keys.W))
                    camera.Muovi(new Vector2(0, -3));
                else if (kCurrentState.IsKeyDown(Keys.A))
                    camera.Muovi(new Vector2(-3, 0));
                else if (kCurrentState.IsKeyDown(Keys.S))
                    camera.Muovi(new Vector2(0, 3));
                else if (kCurrentState.IsKeyDown(Keys.D))
                    camera.Muovi(new Vector2(3, 0));
            

            /* Fine della draw */
            world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            kPreviousState = kCurrentState;
            mPreviousState = mCurrentState;
            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            /* Disegno gli oggetti sensibili alla telecamera */

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, camera.OttieniTrasformazione(GraphicsDevice));
                //Disegna il pavimento 
                spriteBatch.Draw(rectTexture, Const.Zoom * floor.fixture.Body.Position, new Rectangle(0, 0, Const.Zoom * (int)floor.size.X, Const.Zoom * (int)floor.size.Y), Color.Black, floor.fixture.Body.Rotation, (floor.size / 2) * Const.Zoom, 1, SpriteEffects.None, 0);
            spriteBatch.End();

            /* Disegno gli oggetti non influenzati dalla telecamera */

            spriteBatch.Begin();
                //Disegno il testo
                spriteBatch.DrawString(arial, "InWindowPos: " + mCurrentState.X.ToString() + " " + mCurrentState.Y.ToString(), Vector2.Zero, Color.Black);
                spriteBatch.DrawString(arial, "InCameraPos: " + mousePos.X.ToString() + " " + mousePos.Y.ToString(), new Vector2(0, 20), Color.White);
            spriteBatch.End();
            
            
            base.Draw(gameTime);
        }
        #endregion
        
        /* Altri Metodi */
        #region Server
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
            int i, j = -1;

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
        #endregion

    }

    class FloorClass
    {
        public Vector2 size, position;
        public Fixture fixture;       
        public FloorClass()
        {
            size = new Vector2(Const.FloorWidth, Const.FloorHeigh);
            position = new Vector2(Const.FloorXPosition, Const.FloorYPosition);
            
        }

        public void setFixture(World _world)
        {
            fixture = FixtureFactory.CreateRectangle(_world, Const.FloorWidth, Const.FloorHeigh, Const.FloorDensity, new Vector2(Const.FloorXPosition, Const.FloorYPosition));
            fixture.Body.BodyType = BodyType.Static;
            fixture.Restitution = Const.FloorRestitution;
            fixture.Friction = Const.FloorFriction;
        }
    
    }


}
