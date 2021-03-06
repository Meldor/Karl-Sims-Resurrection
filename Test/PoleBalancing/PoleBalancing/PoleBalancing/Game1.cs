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

using LibreriaRN;

namespace PoleBalancing
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector2 pole1Size;
        Vector2 pole2Size;
        
        Vector2 cartPos;
        Vector2 cartSize;

        Texture2D rectTexture;
        Texture2D circTexture;
        
        Cart cartObject;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = Const.ScreenHeigh;
            graphics.PreferredBackBufferWidth = Const.ScreenWidth;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 
        private Evolution evolution;
        protected override void Initialize()
        {
            cartSize = new Vector2(2, 1);
            cartPos = new Vector2(Const.ScreenWidth/(2*Const.Zoom), Const.FloorYPosition - cartSize.Y / 2);

            pole1Size = new Vector2(0.1f, 0.9f);
            pole2Size = new Vector2(0.1f, 0.5f);

            cartObject = new Cart(cartPos, cartSize, pole1Size, pole2Size);
            evolution = new Evolution(cartObject);
            rectTexture = DrawingHelper.RectangularTexture(graphics.GraphicsDevice, 1000, 1000, Color.White);
           
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
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            kCurrentState = Keyboard.GetState();

            if (kCurrentState.IsKeyDown(Keys.Left))
                cartObject.ApplyForce(-1);
            if (kCurrentState.IsKeyDown(Keys.Right))
                cartObject.ApplyForce(1);

            kPreviousState = kCurrentState;
           
            cartObject.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            cartObject.Draw(spriteBatch, rectTexture, circTexture);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
