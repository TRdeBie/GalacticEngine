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
    class Star
    {
        GraphicsDevice device;
        Sphere sphere;
        float diameter;
        //Use a local location in light seconds, relative to center of parent star system
        PointF location;

        public Star(GraphicsDevice d) 
        {
            device = d;
            SetUp();
        }

        private void SetUp()
        {
            //Sun diameter is 4.64 light seconds
            diameter = 4.64f;
            sphere = new Sphere(diameter / 2, device, Microsoft.Xna.Framework.Color.Yellow, 90);
        }

        public void Update()
        { }

        public void Draw(Camera camera, Matrix world)
        {
            sphere.Draw(camera, new Vector3(0, 0, 0), world);
        }
    }
}
