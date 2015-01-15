using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPUtil.Math.Math3D
{
    public struct Rotation
    {
        private readonly Angle _x;
        private readonly Angle _y;
        private readonly Angle _z;

        public Angle X { get { return _x; } }
        public Angle Y { get { return _y; } }
        public Angle Z { get { return _z; } }

        public Rotation(Angle x, Angle y, Angle z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Rotation(Angle a1, bool x, bool y, bool z)
        {
            _x = a1;
            _y = a1;
            _z = a1;
        }

        public static Rotation XRotation(Angle a1) { return new Rotation(a1, 0, 0); }
        public static Rotation YRotation(Angle a1) { return new Rotation(0, a1, 0); }
        public static Rotation ZRotation(Angle a1) { return new Rotation(0, 0, a1); }

        public static Rotation XYRotation(Angle a1) { return new Rotation(a1, a1, 0); }
        public static Rotation XZRotation(Angle a1) { return new Rotation(a1, 0, a1); }
        public static Rotation YZRotation(Angle a1) { return new Rotation(0, a1, a1); }
    }

    public struct Attitude
    {
        private readonly Angle _yaw;
        private readonly Angle _pitch;
        private readonly Angle _roll;

        public Angle Yaw { get { return _yaw; } }
        public Angle Pitch { get { return _pitch; } }
        public Angle Roll { get { return _roll; } }

        public Attitude(Angle yaw, Angle pitch, Angle roll)
        {
            _yaw = yaw;
            _pitch = pitch;
            _roll = roll;
        }

        public Attitude(Angle a1, bool yaw, bool pitch, bool roll)
        {
            _yaw = a1;
            _pitch = a1;
            _roll = a1;
        }

        public static Rotation YawAttitude(Angle a1) { return new Rotation(a1, 0, 0); }
        public static Rotation PitchAttitude(Angle a1) { return new Rotation(0, a1, 0); }
        public static Rotation RollAttitude(Angle a1) { return new Rotation(0, 0, a1); }

        public static Rotation YawPitchAttitude(Angle a1) { return new Rotation(a1, a1, 0); }
        public static Rotation YawRollAttitude(Angle a1) { return new Rotation(a1, 0, a1); }
        public static Rotation PitchRollAttitude(Angle a1) { return new Rotation(0, a1, a1); }
    }
}
