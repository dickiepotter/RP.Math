using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;
using RPUtil.Math;
using RPUtil.Math.Math3D;
using Vector = RPUtil.Math.Math3D.Vector;

namespace CGMathSample
{
    public partial class VectorViewer
    {
        private readonly ScreenSpaceLines3D line = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D xyline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D xzline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D yzline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D rotline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D xyrotline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D xzrotline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D yzrotline = new ScreenSpaceLines3D();
        private readonly ScreenSpaceLines3D vol = new ScreenSpaceLines3D();
        private Vector vect = new Vector(0,0,0);
        private Axis axis;

        public VectorViewer()
        {
            InitializeComponent();

            Trackball trackball = new Trackball {EventSource = border};
            viewport.Camera.Transform = trackball.Transform;

            viewport.Children.Add(line);
            XY.Children.Add(xyline);
            XZ.Children.Add(xzline);
            YZ.Children.Add(yzline);
            line.Thickness = 2;
            line.Color = Colors.Green;
            xyline.Thickness = 2;
            xyline.Color = Colors.Green;
            xzline.Thickness = 2;
            xzline.Color = Colors.Green;
            yzline.Thickness = 2;
            yzline.Color = Colors.Green;

            viewport.Children.Add(rotline);
            XY.Children.Add(xyrotline);
            XZ.Children.Add(xzrotline);
            YZ.Children.Add(yzrotline);
            rotline.Thickness = 2;
            rotline.Color = Colors.Red;
            xyrotline.Thickness = 2;
            xyrotline.Color = Colors.Red;
            xzrotline.Thickness = 2;
            xzrotline.Color = Colors.Red;
            yzrotline.Thickness = 2;
            yzrotline.Color = Colors.Red;

            axis = Axis.VE_Axis_Default;

            xAxis.ItemsSource = Enum.GetNames(typeof(AxisAlignment));
            yAxis.ItemsSource = Enum.GetNames(typeof(AxisAlignment));
            zAxis.ItemsSource = Enum.GetNames(typeof(AxisAlignment));

            /*
            vol.Points.Add(new Point3D(100, 100, -100));
            vol.Points.Add(new Point3D(100, -100, -100));

            vol.Points.Add(new Point3D(100, -100, -100));
            vol.Points.Add(new Point3D(-100, -100, -100));

            vol.Points.Add(new Point3D(-100, -100, -100));
            vol.Points.Add(new Point3D(-100, 100, -100));

            vol.Points.Add(new Point3D(-100, 100, -100));
            vol.Points.Add(new Point3D(100, 100, -100));

            vol.Points.Add(new Point3D(100, 100, 100));
            vol.Points.Add(new Point3D(100, -100, 100));

            vol.Points.Add(new Point3D(100, -100, 100));
            vol.Points.Add(new Point3D(-100, -100, 100));

            vol.Points.Add(new Point3D(-100, -100, 100));
            vol.Points.Add(new Point3D(-100, 100, 100));

            vol.Points.Add(new Point3D(-100, 100, 100));
            vol.Points.Add(new Point3D(100, 100, 100));

            vol.Points.Add(new Point3D(100, 100, 100));
            vol.Points.Add(new Point3D(100, 100, -100));

            vol.Points.Add(new Point3D(-100, -100, 100));
            vol.Points.Add(new Point3D(-100, -100, -100));

            vol.Points.Add(new Point3D(100, -100, 100));
            vol.Points.Add(new Point3D(100, -100, -100));

            vol.Points.Add(new Point3D(-100, 100, 100));
            vol.Points.Add(new Point3D(-100, 100, -100));

            vol.Thickness = 1;
            vol.Color = Colors.Gray;
            viewport.Children.Add(vol);
            */

            xAxis.SelectedItem = axis.X.ToString();
            yAxis.SelectedItem = axis.Y.ToString();
            zAxis.SelectedItem = axis.Z.ToString();

            Draw(null, null);    
        }

        void Draw(object sender, EventArgs e)
        {
            try
            {
                axis = new Axis
                    (
                    (AxisAlignment) Enum.Parse(typeof (AxisAlignment), (string) xAxis.SelectedItem),
                    (AxisAlignment) Enum.Parse(typeof (AxisAlignment), (string) yAxis.SelectedItem),
                    (AxisAlignment) Enum.Parse(typeof (AxisAlignment), (string) zAxis.SelectedItem)
                    );
            }
            catch { return; }

            position.Content = vect.ToString("m0.##", null);

            Vector rotVect =
                vect
                .Roll(new Angle(Roll.Value, AngleUnits.DEG), axis)
                .Pitch(new Angle(Pitch.Value, AngleUnits.DEG), axis)
                .Yaw(new Angle(Yaw.Value, AngleUnits.DEG), axis);

            RollTB.Text = Roll.Value.ToString();
            PitchTB.Text = Pitch.Value.ToString();
            YawTB.Text = Yaw.Value.ToString();

            Point3D o = new Point3D(0, 0, 0);
            Point3D t = new Point3D(vect.X, vect.Y, vect.Z);
            Point3D r = new Point3D(rotVect.X, rotVect.Y, rotVect.Z);

            line.Points.Clear();
            line.Points.Add(o);
            line.Points.Add(t);
            xyline.Points.Clear();
            xyline.Points.Add(o);
            xyline.Points.Add(t);
            xzline.Points.Clear();
            xzline.Points.Add(o);
            xzline.Points.Add(t);
            yzline.Points.Clear();
            yzline.Points.Add(o);
            yzline.Points.Add(t);

            rotPosition.Content = rotVect.ToString("m0.##", null);

            rotline.Points.Clear();
            rotline.Points.Add(o);
            rotline.Points.Add(r);
            xyrotline.Points.Clear();
            xyrotline.Points.Add(o);
            xyrotline.Points.Add(r);
            xzrotline.Points.Clear();
            xzrotline.Points.Add(o);
            xzrotline.Points.Add(r);
            yzrotline.Points.Clear();
            yzrotline.Points.Add(o);
            yzrotline.Points.Add(r);

        }

        private void Z_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vect = new Vector(vect.X, vect.Y, e.NewValue);
            Draw(null, null);
        }

        private void Y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vect= new Vector(vect.X, e.NewValue, vect.Z);
            Draw(null, null);
        }

        private void X_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            vect = new Vector(e.NewValue, vect.Y, vect.Z);
            Draw(null, null);
        }

        private void PitchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == null) return;
            try
            {
                Pitch.Value = Double.Parse(PitchTB.Text);
            }catch {}
        }

        private void RollTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == null) return;
            try
            {
                Roll.Value = Double.Parse(RollTB.Text);
            }
            catch { }
        }

        private void YawTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == null) return;
            try
            {
                Yaw.Value = Double.Parse(YawTB.Text);
            }
            catch { }
        }
    }
}
