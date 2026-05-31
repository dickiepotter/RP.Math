namespace VectorVisualizer;

/// <summary>
/// A tiny orthographic camera that projects 3D world points onto the 2D SVG canvas.
/// Kept deliberately independent of <c>RP.Math.Vector</c> so the view itself is trustworthy
/// while the library's own maths is what gets exercised by the operations.
/// (Named SceneCamera to avoid colliding with the library's own RP.Math.Camera type.)
/// </summary>
public sealed class SceneCamera
{
    public double YawDeg { get; set; } = 35;
    public double PitchDeg { get; set; } = 22;
    public double Scale { get; set; } = 46;   // pixels per world unit
    public double Cx { get; set; } = 270;      // canvas centre
    public double Cy { get; set; } = 250;

    /// <summary>Project a world point to (screenX, screenY, depth). Larger depth = nearer the viewer.</summary>
    public (double sx, double sy, double depth) Project(double x, double y, double z)
    {
        double yaw = Deg(YawDeg), pitch = Deg(PitchDeg);

        // Rotate about Y (yaw)
        double cy = Math.Cos(yaw), sy = Math.Sin(yaw);
        double x1 = x * cy + z * sy;
        double z1 = -x * sy + z * cy;
        double y1 = y;

        // Rotate about X (pitch)
        double cp = Math.Cos(pitch), sp = Math.Sin(pitch);
        double y2 = y1 * cp - z1 * sp;
        double z2 = y1 * sp + z1 * cp;
        double x2 = x1;

        return (Cx + x2 * Scale, Cy - y2 * Scale, z2);
    }

    /// <summary>
    /// Convert a screen-space drag (dx, dy in pixels) into a world-space delta lying in the
    /// camera's view plane — used when dragging a vector's tip.
    /// </summary>
    public (double x, double y, double z) ScreenDeltaToWorld(double dx, double dy)
    {
        var right = InverseRotate(1, 0, 0);   // world direction of screen "right"
        var up = InverseRotate(0, 1, 0);      // world direction of screen "up"
        double rx = dx / Scale, ry = -dy / Scale; // screen y points down
        return (right.x * rx + up.x * ry,
                right.y * rx + up.y * ry,
                right.z * rx + up.z * ry);
    }

    private (double x, double y, double z) InverseRotate(double x, double y, double z)
    {
        double yaw = Deg(YawDeg), pitch = Deg(PitchDeg);

        // undo pitch
        double cp = Math.Cos(pitch), sp = Math.Sin(pitch);
        double y1 = y * cp + z * sp;
        double z1 = -y * sp + z * cp;
        double x1 = x;

        // undo yaw
        double cy = Math.Cos(yaw), sy = Math.Sin(yaw);
        double x2 = x1 * cy - z1 * sy;
        double z2 = x1 * sy + z1 * cy;
        return (x2, y1, z2);
    }

    private static double Deg(double d) => d * Math.PI / 180.0;
}
