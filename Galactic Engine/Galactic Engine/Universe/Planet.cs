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

namespace Galactic_Engine.Universe
{
    class Planet
    {
        GraphicsDevice device;
        Sphere sphere;
        float diameter;
        float distanceToStar;
        //Need local location and galactic location. First in lightseconds, second in lightyears
        PointF locationLocal, locationGalactic;

        public Planet(GraphicsDevice d)
        {
            device = d;
            SetUp();
        }

        private void SetUp()
        {
            //Earth diameter is 0.042479 light seconds
            diameter = 0.042479f;
            //Distance from Earth to sun is 499.2 light seconds
            distanceToStar = 499.2f;
            sphere = new Sphere(diameter / 2, device, Microsoft.Xna.Framework.Color.White, 90);
        }

        public void Update()
        { }

        public void Draw(Camera camera, Matrix world)
        {
            sphere.Draw(camera, new Vector3(0, 0, distanceToStar), world);
        }
    }
}
