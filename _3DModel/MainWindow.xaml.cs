using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using Microsoft.Win32;

namespace _3DModel
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Viewport3D viewport;
        ModelVisual3D model;
        GeometryModel3D model3D;
        Model3DGroup model3DGroup;
        Transform3DGroup transformGroup;
        double rotateAngleX;
        double rotateAngleY;
        public MainWindow()
        {
            InitializeComponent();
            rotateAngleX = 0;
            rotateAngleY = 0;
            viewport = (Viewport3D)(this.Content as Grid).Children[1];
            model = (viewport.Children[1] as ModelVisual3D);
            model3DGroup = model.Content as Model3DGroup;
            model3D = model3DGroup.Children[0] as GeometryModel3D;
            transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0)));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
            this.PreviewKeyDown += AnythingKeyDown;
        }

        private void AnythingKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                rotateAngleY = FigureRotation(new Vector3D(1, 0, 0), rotateAngleY, -1);
            if (e.Key == Key.Down)
                rotateAngleY = FigureRotation(new Vector3D(1, 0, 0), rotateAngleY, 1);
            if (e.Key == Key.Left)
                rotateAngleX = FigureRotation(new Vector3D(0, 1, 0), rotateAngleX, -1);
            if (e.Key == Key.Right)
                rotateAngleX = FigureRotation(new Vector3D(0, 1, 0), rotateAngleX, 1);
        }

        /// <summary>
        /// Figure rotation
        /// </summary>
        /// <param name="axis">Axis of rotation</param>
        /// <param name="angle">Angle of rotation</param>
        /// <param name="direction">Direction of rotation(1 or -1)</param>
        private double FigureRotation(Vector3D axis, double angle, int direction)
        {
            if (direction < 0)
                direction = -1;
            else
                direction = 1;
            angle += direction * 0.5;
            if (angle >= 360 || angle <= -360)
                angle = 0;
            if (axis == new Vector3D(1, 0, 0))
            {
                transformGroup.Children.RemoveAt(0);
                transformGroup.Children.Insert(0, new RotateTransform3D(new AxisAngleRotation3D(axis, angle)));
            }
            if (axis == new Vector3D(0, 1, 0))
            {
                transformGroup.Children.RemoveAt(1);
                transformGroup.Children.Insert(1, new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angle)));
            }
            model3DGroup.Transform = transformGroup;
            return angle;
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CAD File (*.dxf) | *.dxf";
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
                SaveToDXF(model3DGroup, filePath);
            }

        }
        /// <summary>
        /// Save figure to dxf file
        /// </summary>
        /// <param name="model3D">3D model</param>
        /// <param name="path">Save path</param>
        private void SaveToDXF(Model3DGroup model3DGroup, string path)
        {
            DxfDocument doc = new DxfDocument();
            foreach (GeometryModel3D model3D in model3DGroup.Children)
            {
                List<Vector3> points = new List<Vector3>();
                List<Solid> solids = new List<Solid>();
                foreach (var point in (model3D.Geometry as MeshGeometry3D).Positions)
                {
                    points.Add(new Vector3(point.X, point.Y, point.Z));
                }
                Int32Collection indices = (model3D.Geometry as MeshGeometry3D).TriangleIndices;
                for (int i = 0; i < indices.Count - 2; i += 3)
                {
                    Solid solid = new Solid(points[indices[i]], points[indices[i + 1]], points[indices[i + 2]]);
                    Color color = ((model3D.Material as DiffuseMaterial).Brush as SolidColorBrush).Color;
                    solid.Color = new AciColor(color.R, color.G, color.B);
                    solids.Add(solid);
                }

                doc.AddEntity(solids);
            }
            doc.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            doc.Save(path);
        }
    }
}
