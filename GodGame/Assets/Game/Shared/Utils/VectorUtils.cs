using System.Collections.Generic;
using System.Linq;
//using Unity.VectorGraphics;
using UnityEngine;

namespace Game.Shared.Utils
{
    public static class VectorUtils
    {
        public static List<Vector3> SortPoints(List<Vector3> points)
        {
            Vector3 center = GetCenter(points);

            return points
                .OrderBy(point => Mathf.Atan2(point.z - center.z, point.x - center.x))
                .ToList();
        }

        public static List<Vector3> SortPointsCounterClockwise(List<Vector3> points)
        {
            Vector3 center = GetCenter(points);
            return points
            .OrderBy(point => Mathf.Atan2(point.z - center.z, point.x - center.x))
            .ThenBy(point => (point - center).sqrMagnitude)
            .ToList();
        }

        public static List<Vector3> RemoveClosePoints(List<Vector3> points, float closeDistance)
        {
            List<Vector3> uniquePoints = new List<Vector3>();

            foreach (Vector3 point in points)
            {
                bool isClose = false;

                foreach (Vector3 uniquePoint in uniquePoints)
                {
                    if (Vector3.Distance(uniquePoint, point) < closeDistance)
                    {
                        isClose = true;
                        break;
                    }
                }

                if (!isClose)
                {
                    uniquePoints.Add(point);
                }
            }

            return uniquePoints;
        }

        //public static void CreateSVG()
        //{
        //    var shape = new Shape();

        //    var bzc1 = new BezierContour();
        //    var bzc2 = new BezierContour();

        //    shape.Contours = new BezierContour[2] { bzc1, bzc2 };

        //    //// Create a new SVG document and set its viewBox and preserveAspectRatio settings.
        //    //var sceneInfo = new SVGParser.SceneInfo();
        //    //sceneInfo.Scene = new Scene();
        //    //sceneInfo.Scene.Root = new SceneNode();
        //    //sceneInfo.PreserveViewport = true;

        //    //// Create a new shape containing the path.
        //    //var shape = new Shape()
        //    //{
        //    //    Contours = new BezierContour[] { new BezierContour() { Segments = PathSegmentBuilder.BuildPath(path.ToArray(), true) } },
        //    //    Fill = new SolidFill() { Color = Color.red },
        //    //    PathProps = new PathProperties()
        //    //    {
        //    //        Stroke = new Stroke() { Color = Color.black, HalfThickness = 0.05f },
        //    //        Head = LineEnd.Round,
        //    //        Tail = LineEnd.Round,
        //    //        Corner = LineCorner.Round,
        //    //        Join = LineJoin.Round,
        //    //    }
        //    //};

        //    //sceneInfo.Scene.Root.Shapes = new List<Shape> { shape };

        //    //// Create an SVG document from the scene.
        //    //var svg = SVGExporter.Export(sceneInfo.Scene);

        //    //// Output the SVG data to a file (you can use any path you want).
        //    //System.IO.File.WriteAllText(Application.dataPath + "/Polygon.svg", svg);
        //}

        private static Vector3 GetCenter(List<Vector3> points)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 point in points)
            {
                sum += point;
            }

            return sum / points.Count;
        }
    }
}