namespace RP.Math.Demo.SharpDx
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using Vortice.Direct3D9;
    using Vortice.Mathematics;

    /// <summary>
    /// A minimal Direct3D9 demo that renders a spinning, multi-coloured cube.
    /// </summary>
    /// <remarks>
    /// Originally written against SharpDX (archived/unmaintained since 2019) and ported to
    /// Vortice.Windows (Vortice.Direct3D9). The former <c>SharpDX.Windows.RenderForm</c> /
    /// <c>RenderLoop.Run</c> are replaced with a plain WinForms <see cref="Form"/> and a
    /// message-pumped render loop. The fixed-function pipeline is configured explicitly
    /// (lighting disabled, depth test on, vertex colours) and world/view/projection transforms
    /// are set so the cube is actually visible and animated.
    /// </remarks>
    internal static class Program
    {
        private const int Width = 800;
        private const int Height = 600;

        private static readonly int VertexStride = Unsafe.SizeOf<ColoredVertex>();

        private static Form form = null!;
        private static IDirect3D9 direct3D = null!;
        private static IDirect3DDevice9 device = null!;
        private static IDirect3DVertexBuffer9 vertexBuffer = null!;
        private static int triangleCount;

        /// <summary>A position + packed D3DCOLOR vertex for the fixed-function pipeline.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ColoredVertex
        {
            public float X, Y, Z;
            public uint Color; // 0xAARRGGBB

            public ColoredVertex(float x, float y, float z, uint color)
            {
                X = x; Y = y; Z = z; Color = color;
            }
        }

        /// <summary>The entry point for the application.</summary>
        [STAThread]
        public static void Main()
        {
            Initialize();

            // Simple render loop (replaces the old SharpDX RenderLoop.Run(form, Draw)).
            var clock = Stopwatch.StartNew();
            form.Show();
            while (!form.IsDisposed)
            {
                Draw((float)clock.Elapsed.TotalSeconds);
                Application.DoEvents();
            }

            DisposeResources();
        }

        /// <summary>
        /// Create the window, the Direct3D device, the cube geometry and the fixed-function state.
        /// </summary>
        private static void Initialize()
        {
            form = new Form
            {
                Text = "RP.Math — Vortice Direct3D9 Demo",
                ClientSize = new System.Drawing.Size(Width, Height),
            };

            direct3D = D3D9.Direct3DCreate9();

            var presentation = new PresentParameters
            {
                BackBufferWidth = Width,
                BackBufferHeight = Height,
                BackBufferFormat = Format.X8R8G8B8,
                BackBufferCount = 1,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = form.Handle,
                Windowed = true,
                EnableAutoDepthStencil = true,
                AutoDepthStencilFormat = Format.D16,
                PresentationInterval = PresentInterval.Immediate,
            };

            device = direct3D.CreateDevice(
                0,
                DeviceType.Hardware,
                form.Handle,
                CreateFlags.HardwareVertexProcessing,
                presentation);

            CreateCube();

            // Fixed-function render state: show the vertex colours directly (no lighting),
            // depth-test enabled, and draw every face regardless of winding.
            device.SetRenderState(RenderState.Lighting, false);
            device.SetRenderState(RenderState.ZEnable, true);
            device.SetRenderState(RenderState.CullMode, (int)Cull.None);

            // Camera (view) and perspective (projection) are constant; the world matrix spins per frame.
            var view = Matrix4x4.CreateLookAt(new Vector3(0f, 0f, 6f), Vector3.Zero, Vector3.UnitY);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 4f, (float)Width / Height, 0.1f, 100f);
            device.SetTransform(TransformState.View, view);
            device.SetTransform(TransformState.Projection, projection);
        }

        /// <summary>Render a single frame for the given elapsed time in seconds.</summary>
        private static void Draw(float seconds)
        {
            // Spin the cube around two axes.
            var world = Matrix4x4.CreateRotationX(seconds * 0.6f) * Matrix4x4.CreateRotationY(seconds);
            // D3DTS_WORLD == 256. Use the TransformState overload with the raw value: the
            // int overload of SetTransform takes a world-matrix-palette index (it adds 256),
            // so passing 256 there would select an invalid state and leave the world identity.
            device.SetTransform((TransformState)256, world);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color(30, 30, 40), 1.0f, 0);
            device.BeginScene();

            device.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse;
            device.SetStreamSource(0, vertexBuffer, 0, (uint)VertexStride);
            device.DrawPrimitive(PrimitiveType.TriangleList, 0, (uint)triangleCount);

            device.EndScene();
            device.Present();
        }

        /// <summary>Build the cube's vertex buffer (6 coloured faces, 2 triangles each).</summary>
        private static void CreateCube()
        {
            // 8 cube corners.
            var p = new[]
            {
                new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, 1, -1), new Vector3(-1, 1, -1), // 0-3 back
                new Vector3(-1, -1,  1), new Vector3(1, -1,  1), new Vector3(1, 1,  1), new Vector3(-1, 1,  1), // 4-7 front
            };

            const uint Red = 0xFFFF0000, Green = 0xFF00FF00, Blue = 0xFF0000FF;
            const uint Yellow = 0xFFFFFF00, Magenta = 0xFFFF00FF, Cyan = 0xFF00FFFF;

            var verts = new List<ColoredVertex>(36);

            void Face(int a, int b, int c, int d, uint color)
            {
                verts.Add(new ColoredVertex(p[a].X, p[a].Y, p[a].Z, color));
                verts.Add(new ColoredVertex(p[b].X, p[b].Y, p[b].Z, color));
                verts.Add(new ColoredVertex(p[c].X, p[c].Y, p[c].Z, color));
                verts.Add(new ColoredVertex(p[a].X, p[a].Y, p[a].Z, color));
                verts.Add(new ColoredVertex(p[c].X, p[c].Y, p[c].Z, color));
                verts.Add(new ColoredVertex(p[d].X, p[d].Y, p[d].Z, color));
            }

            Face(4, 5, 6, 7, Green);   // front  (z = +1)
            Face(1, 0, 3, 2, Red);     // back   (z = -1)
            Face(0, 4, 7, 3, Blue);    // left   (x = -1)
            Face(5, 1, 2, 6, Yellow);  // right  (x = +1)
            Face(3, 7, 6, 2, Magenta); // top    (y = +1)
            Face(0, 1, 5, 4, Cyan);    // bottom (y = -1)

            var data = verts.ToArray();
            triangleCount = data.Length / 3;

            vertexBuffer = device.CreateVertexBuffer(
                (uint)(VertexStride * data.Length),
                Usage.WriteOnly,
                VertexFormat.Position | VertexFormat.Diffuse,
                Pool.Managed);

            var target = vertexBuffer.Lock<ColoredVertex>(0, 0, LockFlags.None);
            data.CopyTo(target);
            vertexBuffer.Unlock();
        }

        /// <summary>Release the Direct3D resources.</summary>
        private static void DisposeResources()
        {
            vertexBuffer?.Dispose();
            device?.Dispose();
            direct3D?.Dispose();
            form?.Dispose();
        }
    }
}
