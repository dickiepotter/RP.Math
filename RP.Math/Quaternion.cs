using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Math.Math3D
{
    public class Quaternion
    {
        private readonly double _x;
        private readonly double _y;
        private readonly double _z;
        private readonly double _w;

        public double X { get { return _x; } }
        public double Y { get { return _y; } }
        public double Z { get { return _z; } }
        public double W { get { return _w; } }

        public Quaternion(double x, double y, double z, double w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }
    }
}
