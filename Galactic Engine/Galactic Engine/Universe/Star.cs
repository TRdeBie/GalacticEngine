﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Galactic_Engine.Universe
{
    class Star
    {
        GraphicsDevice device;
        Sphere sphere;

        public Star(GraphicsDevice d) 
        {
            device = d;
            SetUp();
        }

        private void SetUp()
        {
            sphere = new Sphere(1.0f, device, Color.Yellow, 90);
        }

        public void Update()
        { }

        public void Draw(Camera camera, Matrix world)
        {
            sphere.Draw(camera, new Vector3(0, 0, 0), world);
        }
    }
}
