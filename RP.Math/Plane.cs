using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Math
{
    public struct Plane
    {
        private readonly double _a;
        private readonly double _b;
        private readonly double _c;
        private readonly double _d;

        public double A { get{ return _a; }}
        public double B { get{ return _b; }}
        public double C { get{ return _c; }}
        public double D { get{ return _d; }}

        public Plane(double a, double b, double c, double d) 
        {
	        _a = a;
	        _b = b;
            _c = c;
            _d = d;
        }

    }
}
