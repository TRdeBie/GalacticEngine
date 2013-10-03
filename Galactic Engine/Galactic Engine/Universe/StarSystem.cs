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
    class StarSystem
    {
        List<Star> stars;
        List<Planet> planets;
        GraphicsDevice device;
        //Need local location and galactic location. First in lightseconds, second in lightyears
        PointF locationGalactic;

        public StarSystem(GraphicsDevice d)
        {
            device = d;
            SetUp();
        }

        private void SetUp()
        {
            //Set up a star system
            stars = new List<Star>();
            planets = new List<Planet>();
            stars.Add(new Star(device));
            planets.Add(new Planet(device));
        }

        public void Update()
        { }

        public void Draw(Camera camera, Matrix world)
        {
            foreach (Star s in stars)
            {
                s.Draw(camera, world);
            }

            foreach (Planet p in planets)
            {
                p.Draw(camera, world);
            }
        }
    }
}
