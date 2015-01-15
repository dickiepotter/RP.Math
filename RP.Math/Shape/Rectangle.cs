using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPUtil.Math
{
    public struct Rectangle
    {
        private readonly double _width;
        private readonly double _height;

        public double Width { get { return _width; } }
        public double Height { get { return _height; } }

        public Rectangle(double width, double height)
        {
            _width = width;
            _height = height;
        }
    }
}
