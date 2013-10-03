using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galactic_Engine.Support
{
    class Location
    {
        //A structure for a 3D location
        private float Xloc, Yloc, Zloc;

        public Location(float x, float y, float z)
        {
            Xloc = x;
            Yloc = y;
            Zloc = z;
        }

        public float X
        { 
            get { return Xloc; }
            set { Xloc = value; }
        }

        public float Y
        {
            get { return Yloc; }
            set { Yloc = value; }
        }

        public float Z
        {
            get { return Zloc; }
            set { Zloc = value; }
        }
    }
}
