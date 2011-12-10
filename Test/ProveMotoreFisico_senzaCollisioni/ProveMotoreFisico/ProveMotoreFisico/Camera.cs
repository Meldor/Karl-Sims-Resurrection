using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProveMotoreFisico
{
    public class Camera
    {
        protected float _zoom; // Camera Zoom
        public Matrix _transform; // Matrix Transform
        public Vector2 _pos; // Camera Position
        protected float _rotazione; // Camera Rotation
 
        public Camera()
        {
            _zoom = 1.0f;
            _rotazione = 0.0f;
            _pos = Vector2.Zero;
        }
        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; } // Negative zoom will flip image
        }

        public float Rotazione
        {
            get { return _rotazione; }
            set { _rotazione = value; }
        }

        // Auxiliary function to move the camera
        public void Muovi(Vector2 amount)
        {
            _pos += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }
        public Matrix OttieniTrasformazione(GraphicsDevice graphicsDevice)
        {
            _transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                                         Matrix.CreateRotationZ(Rotazione) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 0)) *
                                         Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));
            return _transform;
        }
    }
}
