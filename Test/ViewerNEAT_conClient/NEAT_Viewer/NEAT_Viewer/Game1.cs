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

using KSR_libraryRN;

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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            //genotipo.addNeurone(new GenotipoRN.NeuroneG(5, 0));
            //genotipo.addAssone(new GenotipoRN.AssoneG(7, 0, 5, 0));
            //genotipo.addAssone(new GenotipoRN.AssoneG(8, 5, 4, 0));
            //genotipo.addAssone(new GenotipoRN.AssoneG(9, 5, 5, 0));
            GenotipoRN g_mutato = gestore.mutazioneAggiungiNeurone(genotipo);
            for(int i = 0; i < 5; i++)
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
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
    }
}
