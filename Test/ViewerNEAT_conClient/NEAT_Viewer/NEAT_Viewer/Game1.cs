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

        GrafoDisegno grafo;
        Thread trd;

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
            GestoreRN_NEAT gestore = new GestoreRN_NEAT(3, 2);
            GenotipoRN genotipo = gestore.getPerceptron();
            GenotipoRN g_mutato = gestore.mutazioneAggiungiNeurone(genotipo);
            g_mutato = gestore.mutazioneAggiungiNeurone(g_mutato);
            grafo = new GrafoDisegno(g_mutato);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            circTexture = Content.Load<Texture2D>("cerchio");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

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
            grafo.Draw(spriteBatch, circTexture, rectTexture, Color.Yellow, Color.Black);
            //DrawingHelper.DrawSpline(spriteBatch, rectTexture, Color.Black, new Vector2(100,100), new Vector2(0, -50), new Vector2(150, 50), new Vector2(50, 0), 4);
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

                            GenotipoRN g = LibreriaRN.GenotipoRN.receiveNetwork(stream);
                            grafo = new GrafoDisegno(g);
                            data = "Ricezione rete neurale avvenuta\n";
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
