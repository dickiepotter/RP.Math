namespace RP.Math.Demo.SharpDx
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    using Vortice.Direct3D9;
    using Vortice.Mathematics;

    /// <summary>
    /// A minimal Direct3D9 demo that renders a spinning-free coloured cube.
    /// </summary>
    /// <remarks>
    /// Originally written against SharpDX (archived/unmaintained since 2019). Ported to
    /// Vortice.Windows (Vortice.Direct3D9), the maintained successor. The former
    /// <c>SharpDX.Windows.RenderForm</c> / <c>RenderLoop.Run</c> have been replaced with a
    /// plain WinForms <see cref="Form"/> and a simple message-pumped render loop.
    /// </remarks>
    internal static class Program
    {
        private const int Width = 800;
        private const int Height = 600;

        /// <summary>Number of bytes in a single <see cref="Vector4"/> (one vertex attribute).</summary>
        private static readonly int Vector4Size = Unsafe.SizeOf<Vector4>();

        /// <summary>The form on which rendering will be done.</summary>
        private static Form form = null!;

        private static IDirect3D9 direct3D = null!;
        private static IDirect3DDevice9 device = null!;
        private static IDirect3DVertexBuffer9 vertexBuffer = null!;
        private static IDirect3DVertexDeclaration9 vertexDeclaration = null!;

        /// <summary>
        /// The entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Initialize();

            // Show the window and run a simple render loop. This replaces the old
            // SharpDX RenderLoop.Run(form, Draw); Vortice ships no equivalent helper.
            form.Show();
            while (!form.IsDisposed)
            {
                Draw();
                Application.DoEvents();
            }

            DisposeResources();
        }

        /// <summary>
        /// Create the render form, the Direct3D device and the cube geometry.
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
        }

        /// <summary>
        /// The drawing loop body, run once per frame.
        /// </summary>
        private static void Draw()
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color(0f, 0f, 0f, 1f), 1.0f, 0);
            device.BeginScene();

            // Each vertex is a position Vector4 followed by a colour Vector4 (stride = 2 * Vector4).
            device.SetStreamSource(0, vertexBuffer, 0, (uint)(Vector4Size * 2));
            device.VertexDeclaration = vertexDeclaration;
            device.DrawPrimitive(PrimitiveType.TriangleList, 0, 12);

            device.EndScene();
            device.Present();
        }

        /// <summary>
        /// Build the cube's vertex buffer and vertex declaration once, up front.
        /// </summary>
        private static void CreateCube()
        {
            var vertices = new[]
            {
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Front
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Back
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Top
                new Vector4(-1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4( 1.0f, 1.0f,  1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                new Vector4( 1.0f, 1.0f, -1.0f,  1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),

                new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Bottom
                new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,-1.0f, -1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,-1.0f,  1.0f,  1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),

                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Left
                new Vector4(-1.0f, -1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-1.0f, -1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-1.0f,  1.0f,  1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),
                new Vector4(-1.0f,  1.0f, -1.0f, 1.0f), new Vector4(1.0f, 0.0f, 1.0f, 1.0f),

                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Right
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4( 1.0f, -1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f, -1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
                new Vector4( 1.0f,  1.0f,  1.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f),
            };

            vertexBuffer = device.CreateVertexBuffer(
                (uint)(Vector4Size * vertices.Length),
                Usage.WriteOnly,
                VertexFormat.None,
                Pool.Managed);

            var target = vertexBuffer.Lock<Vector4>(0, 0, LockFlags.None);
            vertices.CopyTo(target);
            vertexBuffer.Unlock();

            var vertexElements = new[]
            {
                new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                new VertexElement(0, 16, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Color, 0),
                VertexElement.VertexDeclarationEnd,
            };

            vertexDeclaration = device.CreateVertexDeclaration(vertexElements);
        }

        /// <summary>
        /// Release the Direct3D resources.
        /// </summary>
        private static void DisposeResources()
        {
            vertexDeclaration?.Dispose();
            vertexBuffer?.Dispose();
            device?.Dispose();
            direct3D?.Dispose();
            form?.Dispose();
        }
    }
}
