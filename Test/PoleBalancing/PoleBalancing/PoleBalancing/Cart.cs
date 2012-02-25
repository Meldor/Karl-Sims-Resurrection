using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;

namespace PoleBalancing
{
    class Cart
    {
        FParte cart;
        FParte pole1;
        FParte pole2;
        Fixture floor;

        World world;
        Vector2 origin;

        public Cart(Vector2 cartPosition, Vector2 cartSize, Vector2 pole1Size, Vector2 pole2Size)
        {
            Vector2 poleOffset = new Vector2(0, -cartSize.Y / 2);

            world = new World(new Vector2(0, Const.Gravity));

            floor = FixtureFactory.CreateRectangle(world, Const.FloorWidth, Const.FloorHeigh, 0.1f, new Vector2(0, Const.FloorYPosition));
            floor.Body.BodyType = BodyType.Static;
            floor.Restitution = 0.2f;
            floor.Friction = 0.1f;
            floor.CollisionFilter.CollisionCategories = Category.Cat4;

            cart = new FParte(cartSize, cartPosition, Const.PartDensity * 3, world);
            cart.CollisionCategory = Category.Cat4;

            pole1 = new FParte(pole1Size, Vector2.Zero, Const.PartDensity, world, Color.Blue, Color.Yellow);
            pole1.Join(cart, new Vector2(0, pole1Size.Y / 2), new Vector2(0, -cartSize.Y / 2), Color.Yellow, 0.1f);
            pole1.CollisionCategory = Category.Cat1;
            pole1.AddCollidesCategory(Category.Cat4);

            pole2 = new FParte(pole2Size, Vector2.Zero, Const.PartDensity, world, Color.Blue, Color.Yellow);
            pole2.Join(cart, new Vector2(0, pole2Size.Y / 2), new Vector2(0, -cartSize.Y / 2), Color.Yellow, 0.1f);
            pole2.CollisionCategory = Category.Cat2;
            pole2.AddCollidesCategory(Category.Cat4);

            origin = cartPosition;
        }

        public void ApplyForce(float force)
        {
            cart.ApplyForce(force*new Vector2(Const.MaxCartForce, 0), cart.Position);
        }

        public float GetPoleRotation(int poleIndex)
        {
            if (poleIndex == 1)
                return pole1.Rotation;
            else
                return pole2.Rotation;
        }

        public float GetPoleAngularSpeed(int poleIndex)
        {
            if (poleIndex == 1)
                return pole1.AngularVelocity;
            else
                return pole2.AngularVelocity;
        }

        public float GetCartPosition()
        {
            return cart.Position.X - origin.X;
        }

        public void Update(float dt)
        {
            world.Step(dt);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D rectTexture, Texture2D circTexture)
        {
            cart.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            pole1.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            pole2.Draw(spriteBatch, rectTexture, circTexture, Const.Zoom);
            spriteBatch.Draw(rectTexture, Const.Zoom * floor.Body.Position, new Rectangle(0, 0, (int)(Const.FloorWidth * Const.Zoom), (int)(Const.FloorHeigh * Const.Zoom)), Color.Black, floor.Body.Rotation, new Vector2(Const.FloorWidth, Const.FloorHeigh) / 2 * Const.Zoom, 1, SpriteEffects.None, 0);
        }
    }
}
