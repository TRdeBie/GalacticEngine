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

namespace Galactic_Engine
{
    class Camera
    {
        Vector3 up;
        Vector3 eye;
        Vector3 focus;

        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix viewProjection;

        public BoundingFrustum Frustum;

        float aspectRatio = 4.0f / 3.0f;
        float farplaneDistance; //Added to allow for changing of the far plane distance

        public Camera(Vector3 camEye, Vector3 camFocus, Vector3 camUp)
        {
            up = camUp;
            eye = camEye;
            focus = camFocus;
            farplaneDistance = 300.0f;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, farplaneDistance);
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
    }
}
