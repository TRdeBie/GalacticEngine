﻿using System;
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
    class Universe
    {
        //Made by Tim de Bie
        //Couple of notes
        //Measurements are in lightSeconds (this is great and all when inside a solar system, but outside lightyears are needed)
        //1 lightyears = 3.145 * 10^7 lightseconds so yeah
        //Todo: create a decent camera movement system
        List<StarSystem> universe;
        GraphicsDevice device;
        Camera camera;
        Matrix world;
        int[] screenresolution;

        public Universe(GraphicsDevice d, int[] sr)
        {
            device = d;
            screenresolution = sr;
            SetUp();
        }

        private void SetUp()
        {
            //Essential stuff, setting up a world matrix
            world = Matrix.CreateScale(1.0f);

            //Set up the universe, all star systems
            universe = new List<StarSystem>();
            universe.Add(new StarSystem(device));

            //Setup a camera
            Vector3 eye = new Vector3(500, 1, 0);
            Vector3 focus = new Vector3(0, 0, -1);
            Vector3 up = new Vector3(0, 1, 0);
            camera = new Camera(eye, focus, up, device);
        }

        public void Update(float timestep)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            camera.Update(timestep, keyboardState, mouseState, screenresolution);
        }

        public void Draw()
        {
            camera.Draw(world);
            foreach (StarSystem s in universe)
            {
                s.Draw(camera, world);
            }
        }
    }
}
