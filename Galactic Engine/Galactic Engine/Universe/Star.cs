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

namespace Galactic_Engine.Universe
{
    class Star : CelestialObject
    {
        GraphicsDevice device;

        public Star(GraphicsDevice d) 
        {
            device = d;
            SetUp();
        }

        private void SetUp()
        { }

        public void Update()
        { }

        public void Draw(Camera camera, Matrix world)
        { }
    }
}
