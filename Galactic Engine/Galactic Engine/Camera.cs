using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Galactic_Engine
{
    class Camera
    {
        //Vectors for eye location, 'up' direction and focus
        Vector3 up;
        Vector3 eye;
        Vector3 focus;

        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix viewProjection;

        //Variables to allow changing camera stuff
        float speed;

        public BoundingFrustum Frustum;

        float aspectRatio = 4.0f / 3.0f;
        float farplaneDistance; //Added to allow for changing of the far plane distance

        public Camera(Vector3 camEye, Vector3 camFocus, Vector3 camUp)
        {
            up = camUp;
            eye = camEye;
            focus = camFocus;
            //500 lightseconds is roughly 1 AU
            farplaneDistance = 500.0f;
            speed = 1.0f;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, farplaneDistance);
            UpdateViewMatrix();
        }

        public void Update(float timestep, KeyboardState k)
        {
            //Camera movements here
            Keys[] keys = k.GetPressedKeys();
            //Change the focus with the mouse by pressing left control
            if (k.IsKeyDown(Keys.LeftControl))
                ChangeFocus(timestep);
            //Check all keys and move relative to the focus
            //WASD + QE for left, right, forward, backward, up and down
            foreach (Keys key in keys)
            {
                switch (key)
                {
                    case Keys.W: MoveTo(1, 0, 0, timestep); break;
                    case Keys.A: MoveTo(0, -1, 0, timestep); break;
                    case Keys.S: MoveTo(0, 1, 0, timestep); break;
                    case Keys.D: MoveTo(-1, 0, 0, timestep); break;
                    case Keys.Q: MoveTo(0, 0, 1, timestep); break;
                    case Keys.E: MoveTo(0, 0, -1, timestep); break;
                }
            }
        }

        private void ChangeFocus(float timestep)
        {
            //
        }

        private void MoveTo(float x, float y, float z, float timestep)
        {
            //Take the camera 
        }

        public void ChangeFarPlane(float farPlane)
        {
            farplaneDistance = farPlane;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, farplaneDistance);
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            viewMatrix = Matrix.CreateLookAt(eye, focus, up);
            viewProjection = viewMatrix * projectionMatrix;
            Frustum = new BoundingFrustum(viewProjection);
        }

        public Vector3 Eye
        {
            get { return eye; }
            set { eye = value; UpdateViewMatrix(); }
        }

        public Vector3 Focus
        {
            get { return focus; }
            set { focus = value; UpdateViewMatrix(); }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public float FarplaneDistance
        {
            get { return farplaneDistance; }
        }

        public void SetEffectParameters(Effect effect)
        {
            effect.Parameters["View"].SetValue(ViewMatrix);
            effect.Parameters["Projection"].SetValue(ProjectionMatrix);

            EffectParameter cameraPosition = effect.Parameters["CameraPosition"];
            if (cameraPosition != null)
                effect.Parameters["CameraPosition"].SetValue(Eye);
        }
    }
}
