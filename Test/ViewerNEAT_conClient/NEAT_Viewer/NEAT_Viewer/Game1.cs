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

using LibreriaRN;

using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Runtime.InteropServices;

namespace NEAT_Viewer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D circTexture;
        Texture2D rectTexture;
        Texture2D[] thresholdFunctions;

        GrafoDisegno grafo;
        Thread trd;
        FenotipoRN fenotipo, fenotipoCorrente;
        GenotipoRN genotipoCorrente;
        double[] input;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            trd = new Thread(new ThreadStart(this.ServerMethod));
            trd.IsBackground = true;
            trd.Start();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            rectTexture = DrawingHelper.GenerateRectangularTexture(graphics.GraphicsDevice, 1000, 1000, Color.White);
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            GestoreRN_NEAT gestore = new GestoreRN_NEAT(3, 2);
            GenotipoRN genotipo = gestore.getPerceptron();
            GenotipoRN g_mutato = gestore.mutazioneAggiungiNeurone(genotipo);
            g_mutato = gestore.mutazioneAggiungiNeurone(g_mutato);
            g_mutato = gestore.mutazioneAggiungiAssone(g_mutato);
            grafo = new GrafoDisegno(g_mutato);
            fenotipo = new FenotipoRN(g_mutato);
            input = new double[fenotipo.numNeuroniSensori];

            input[0] = -1;
            for (int i = 1; i < fenotipo.numNeuroniSensori; i++)
                input[i] = input[i-1] + 2/(double)(fenotipo.numNeuroniSensori-1);
            grafo.AttachFenotipo(fenotipo);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            circTexture = Content.Load<Texture2D>("cerchio");
            thresholdFunctions = new Texture2D[7];
            thresholdFunctions[0] = Content.Load<Texture2D>("sigmoide");
            thresholdFunctions[1] = Content.Load<Texture2D>("tanh");
            thresholdFunctions[2] = Content.Load<Texture2D>("abs");
            thresholdFunctions[3] = Content.Load<Texture2D>("gauss");
            thresholdFunctions[4] = Content.Load<Texture2D>("transp");
            thresholdFunctions[5] = Content.Load<Texture2D>("sin");
            thresholdFunctions[6] = Content.Load<Texture2D>("squared");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        MouseState mPreviousState, mCurrentState;
        KeyboardState kPreviousState, kCurrentState;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mCurrentState = Mouse.GetState();
            kCurrentState = Keyboard.GetState();
            if (mCurrentState.LeftButton == ButtonState.Pressed && mPreviousState.LeftButton == ButtonState.Released)
            {
                GrafoDisegno.Nodo nodoCliccato = grafo.CheckNodeClick(new Vector2(mCurrentState.X, mCurrentState.Y), true);
                if (nodoCliccato == null)
                    grafo.Unselect();
            }
            else if (mCurrentState.LeftButton == ButtonState.Pressed && mPreviousState.LeftButton == ButtonState.Pressed && grafo.selectedNode != null)
                grafo.selectedNode.posizione = new Vector2(mCurrentState.X, mCurrentState.Y);
            //else if (mCurrentState.LeftButton == ButtonState.Released && mPreviousState.LeftButton == ButtonState.Pressed && grafo.selectedNode != null)
            //{
            //    GrafoDisegno.Nodo targetNode = grafo.CheckNodeClick(new Vector2(mCurrentState.X, mCurrentState.Y), false);
            //    if (targetNode != null)
            //        grafo.SwitchNodes(grafo.selectedNode, targetNode);
            //}

            if (kCurrentState.IsKeyDown(Keys.A) && kPreviousState.IsKeyUp(Keys.A))
            {
                fenotipo.sensori(input);
                fenotipo.Calcola();
                fenotipo.aggiorna();
            }


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
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            grafo.Draw(spriteBatch, circTexture, rectTexture, thresholdFunctions, Color.Black, Color.Red, new Color(0, 255, 0), Color.Orange, Color.Green, Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void ServerMethod()
        {
            TcpListener server = null;
            NetworkStream stream = null;
            TcpClient client = null;
            Int32 port = 13001, i;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            bool fine = false;
            Byte[] bytes = new Byte[256];
            String data = null;
            //Thread trdBB;

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

                        if (data == "IRN")
                        {
                            //data = "Inizio Ricezione";

                            genotipoCorrente = LibreriaRN.GenotipoRN.receiveNetwork(stream);
                            grafo = new GrafoDisegno(genotipoCorrente);
                            data = "Ricezione rete neurale avvenuta\n";
                        }
                        else if (data == "GF")
                        {
                            fenotipo = new FenotipoRN(genotipoCorrente);
                            grafo.AttachFenotipo(fenotipo);
                            data = "Fenotipo generato con successo\n";
                        }
                        else if (data == "INPUT")
                        {
                            String sInput;
                            String[] parsedInput;
                            char[] separatori = { ' ' };
                            if ((i = stream.Read(bytes, 0, bytes.Length)) != 0) //legge fino a 256 caratteri dallo stream di rete
                            {
                                sInput = System.Text.Encoding.ASCII.GetString(bytes, 0, i); // interpreta lo stream a blocchi di 1Byte cod.ASCII
                                parsedInput = sInput.Split(separatori, StringSplitOptions.RemoveEmptyEntries);
                                for (i = 0; i < fenotipo.numNeuroniSensori; i++)
                                    input[i] = Double.Parse(parsedInput[i]);
                                data = "Input ricevuti\n";
                            }
                            else
                                data = "Errore nella ricezione degli input\n";
                        }
                        else if (data == "AGG")
                        {
                            SortedList<int, double> output;
                            data = "";
                            fenotipo.sensori(input);
                            fenotipo.Calcola();
                            output = fenotipo.aggiorna();
                            foreach (KeyValuePair<int, double> k_val in output)
                            {
                                data += k_val.Key.ToString() + ": " + k_val.Value.ToString() + "\n";
                            }
                        }
                        else if (data == "exit" || data == "quit")
                        {
                            fine = true;
                            data = "Server disconnesso...\n";
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
        }
    }


   
}
