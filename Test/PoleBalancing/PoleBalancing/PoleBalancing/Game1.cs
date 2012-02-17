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

namespace PoleBalancing
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Fixture floor;

        FParte pole1;
        Vector2 pole1Size;
        FParte pole2;
        Vector2 pole2Size;
        Vector2 poleOffset;

        FParte cart;
        Vector2 cartPos;
        Vector2 cartSize;

        Texture2D rectTexture;
        Texture2D circTexture;

        World world;

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
            cartSize = new Vector2(2, 1);
            cartPos = new Vector2(10, Const.FloorYPosition - cartSize.Y / 2);

            pole1Size = new Vector2(0.1f, 0.9f);
            pole2Size = new Vector2(0.1f, 0.5f);
            poleOffset = new Vector2(0, -cartSize.Y/2);

            world = new World(new Vector2(0, Const.Gravity));
            rectTexture = DrawingHelper.RectangularTexture(graphics.GraphicsDevice, 1000, 1000, Color.White);

            floor = FixtureFactory.CreateRectangle(world, Const.FloorWidth, Const.FloorHeigh, 0.1f, new Vector2(0, Const.FloorYPosition));
            floor.Body.BodyType = BodyType.Static;
            floor.Restitution = 0.2f;
            floor.Friction = 0.3f;
            floor.CollisionFilter.CollisionCategories = Category.Cat4;

            cart = new FParte(cartSize, cartPos, Const.PartDensity, world);
            cart.CollisionCategory = Category.Cat4;

            pole1 = new FParte(pole1Size, Vector2.Zero, Const.PartDensity*10, world, Color.Blue, Color.Yellow);
            pole1.Join(cart, new Vector2(0, pole1Size.Y / 2), new Vector2(0, -cartSize.Y / 2), Color.Yellow, 0.1f);
            pole1.CollisionCategory = Category.Cat1;
            pole1.AddCollidesCategory(Category.Cat4);

            pole2 = new FParte(pole2Size, Vector2.Zero, Const.PartDensity*10, world, Color.Blue, Color.Yellow);
            pole2.Join(cart, new Vector2(0, pole2Size.Y / 2), new Vector2(0, -cartSize.Y / 2), Color.Yellow, 0.1f);
            pole2.CollisionCategory = Category.Cat2;
            pole2.AddCollidesCategory(Category.Cat4);

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
                cart.ApplyForce(new Vector2(-4, 0), cart.Position);
            if (kCurrentState.IsKeyDown(Keys.Right))
                cart.ApplyForce(new Vector2(4, 0), cart.Position);

            kPreviousState = kCurrentState;
            world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
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
            cart.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            pole1.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            pole2.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            spriteBatch.Draw(rectTexture, Const.Zoom * floor.Body.Position, new Rectangle(0, 0, (int)(Const.FloorWidth * Const.Zoom), (int)(Const.FloorHeigh * Const.Zoom)), Color.Black, floor.Body.Rotation, new Vector2(Const.FloorWidth, Const.FloorHeigh)/2*Const.Zoom, 1, SpriteEffects.None, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
