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
        Vector3 direction;

        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix viewProjection;

        //Variables to allow changing camera stuff
        public BoundingFrustum Frustum;

        private float aspectRatio = 4.0f / 3.0f;
        private float farplaneDistance; //Added to allow for changing of the far plane distance

        private float yaw, pitch; //Allow calculation of viewing direction
        private Vector3 right, down; //Relative to viewing direction, to move perpendicular to focus

        Sphere sphereFocus;

        public Camera(Vector3 camEye, Vector3 camFocus, Vector3 camUp, GraphicsDevice graphics)
        {
            up = camUp;
            eye = camEye;
            focus = camFocus;
            FindRightDown();
            //500 lightseconds is roughly 1 AU
            farplaneDistance = 500.0f;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, farplaneDistance);
            UpdateViewMatrix();

            sphereFocus = new Sphere(0.1f, graphics, Microsoft.Xna.Framework.Color.Gray, 5);
        }

        private void FindRightDown()
        {
            //Find the vectors for right and down in the screen plane
            float sinP = (float)Math.Sin((double)pitch);
            float cosP = (float)Math.Cos((double)pitch);
            float sinY = (float)Math.Sin((double)yaw);
            float cosY = (float)Math.Cos((double)yaw);

            float sinPdown = (float)Math.Sin((double)(pitch - MathHelper.PiOver4));
            float cosPdown = (float)Math.Cos((double)(pitch - MathHelper.PiOver4));
            float sinYright = (float)Math.Sin((double)(yaw + MathHelper.PiOver4));
            float cosYright = (float)Math.Cos((double)(yaw + MathHelper.PiOver4));
            
            down = new Vector3(-cosPdown * sinY, sinPdown, -cosPdown * cosY);
            right = new Vector3(-cosP * sinYright, sinP, -cosP * cosYright);
        }

        public void Update(float timestep, KeyboardState k, MouseState m, int[] screenresolution)
        {
            //Camera movements here
            Keys[] keys = k.GetPressedKeys();
            //Change the focus with the mouse by pressing left control
            if (m.RightButton == ButtonState.Pressed && m.X >= 0 && m.X <= screenresolution[0] && m.Y >= 0 && m.Y <= screenresolution[1])
                ChangeFocus(timestep, m, screenresolution);
            //Check all keys and move relative to the focus
            //WASD + QE for left, right, forward, backward, up and down
            foreach (Keys key in keys)
            {
                switch (key)
                {
                    case Keys.W: MoveTo(-1, 0, timestep); break;
                    case Keys.A: MoveTo(0, -1, timestep); break;
                    case Keys.S: MoveTo(1, 0, timestep); break;
                    case Keys.D: MoveTo(0, 1, timestep); break;
                }
            }
        }

        private void ChangeFocus(float timestep, MouseState m, int[] screenresolution)
        {
            //Change the pitch and yaw based on the mouse location, then set the mouse to the center of the screen
            float sensitivityX = MathHelper.PiOver4 / (screenresolution[0] / 2); //Multiplier. Half screen distance in movement means angle of 45 degrees
            float sensitivityY = MathHelper.PiOver4 / (screenresolution[1] / 2);
            float dx, dy;
            dx = 0; dy = 0;
            //Find the dx
            if (m.X >= 0 && m.X <= screenresolution[0])
                dx = m.X - screenresolution[0] / 2;
            else if (m.X < 0)
                dx = 0 - screenresolution[0] / 2;
            else if (m.X > screenresolution[0])
                dx = screenresolution[0] / 2;
            if (m.Y >= 0 && m.Y <= screenresolution[1])
                dy = m.Y - screenresolution[1] / 2;
            else if (m.Y < 0)
                dy = 0 - screenresolution[1] / 2;
            else if (m.Y > screenresolution[1])
                dy = screenresolution[1] / 2;
            //Place the mouse in the center of the screen
            Mouse.SetPosition(screenresolution[0] / 2, screenresolution[1] / 2);
            yaw -= dx * sensitivityX;
            yaw = yaw % MathHelper.TwoPi;
            pitch -= dy * sensitivityY;
            pitch = pitch % MathHelper.TwoPi;

            FindFocus();
        }

        private void FindFocus()
        {
            //Using yaw and pitch, calculate the viewing direction and up vector
            float sinP = (float)Math.Sin((double)pitch);
            float cosP = (float)Math.Cos((double)pitch);
            float sinY = (float)Math.Sin((double)yaw);
            float cosY = (float)Math.Cos((double)yaw);
            direction = new Vector3(-cosP * sinY, sinP, -cosP * cosY);
            focus = eye + direction;

            float sinPup = (float)Math.Sin((double)(pitch + MathHelper.PiOver2));
            float cosPup = (float)Math.Cos((double)(pitch + MathHelper.PiOver2));
            up = new Vector3(-cosPup * sinY, sinPup, -cosPup * cosY);

            UpdateViewMatrix();
        }

        private void MoveTo(float d, float r, float timestep)
        {
            eye = eye + d * down * timestep + r * right * timestep;
            focus = eye + direction;
            UpdateViewMatrix();
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

        public void Draw(Matrix world)
        {
            for (int x = -10; x < 11; x++)
            {
                for (int z = -10; z < 11; z++)
                {
                    sphereFocus.Draw(this, new Vector3(eye.X + x, 0, eye.Z + z), world);
                }
            }
        }
    }
}
