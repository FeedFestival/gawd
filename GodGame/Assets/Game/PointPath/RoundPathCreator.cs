using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundPathCreator
{
    private const float _lineResolution = 100;
    private const float _pointSpacing = 0.1f;

    public void CreateBezierPath(List<Vector3> cornersPosition, ref BezierCurve bezierCurve)
    {
        // clear points
        bezierCurve.Clear();
        //foreach (Transform bzPointGo in bezierCurve.transform)
        //{
        //    Destroy(bzPointGo.gameObject);
        //}

        // create path
        for (int i = 0; i < cornersPosition.Count; i++)
        {
            var prevI = (i == 0) ? cornersPosition.Count - 1 : i - 1;
            var nextI = (i == cornersPosition.Count - 1) ? 0 : i + 1;
            createBezierPoint(cornersPosition[prevI], cornersPosition[i], cornersPosition[nextI], ref bezierCurve);
            Debug.Log("-----------------" + i + "-----------------");
        }
    }

    private void createBezierPoint(Vector3 prevP, Vector3 cornerPos, Vector3 nextPos, ref BezierCurve bezierCurve)
    {
        var reduceBy = 2.5f;

        var prevDir = (cornerPos - prevP).normalized;
        var distance = Vector3.Distance(cornerPos, prevP);
        var reducedDistance = distance / reduceBy;
        var reducedDir = prevDir * reducedDistance;
        Debug.Log("prevDir: " + prevDir);
        Debug.Log("distance: " + distance);
        Debug.Log("reducedDistance: " + reducedDistance);
        Debug.Log("reducedDir: " + reducedDir);

        var nextDir = (cornerPos - nextPos).normalized;
        var nDistance = Vector3.Distance(cornerPos, nextPos);
        var nReducedDistance = nDistance / reduceBy;
        var nReducedDir = nextDir * nReducedDistance;
        Debug.Log("nextDir: " + nextDir);
        Debug.Log("nDistance: " + nDistance);
        Debug.Log("nReducedDistance: " + nReducedDistance);
        Debug.Log("nReducedDir: " + nReducedDir);
        var oppositeDir = -nReducedDir;

        var dir = MiddlePoint(reducedDir, oppositeDir);

        var bzPoint = bezierCurve.AddPointAt(cornerPos);
        bzPoint.handle2 = dir;
    }

    public static Vector3 MiddlePoint(Vector3 vectorA, Vector3 vectorB)
    {
        return (vectorA + vectorB) / 2.0f;
    }

    public List<Vector3> GetEvenlyDistributedPoints(List<BezierPoint> bezierPoints)
    {
        var points = getPoints(bezierPoints);
        return getEvenlyDistributedPoints(points);
    }

    private Vector3[] getPoints(List<BezierPoint> bezierPoints)
    {
        bezierPoints.Add(bezierPoints[0]);
        var count = (bezierPoints.Count - 1) * (int)_lineResolution;
        //var count = (bezierPoints.Length) * (int)_lineResolution;
        var positions = new Vector3[count];
        var pI = 0;

        for (int i = 0; i < bezierPoints.Count; i++)
        {
            int nextI = i + 1;
            if (bezierPoints.Count == nextI) { break; }

            for (int p = 0; p < (int)_lineResolution; p++)
            {
                var t = p / _lineResolution;
                var point = BezierCurve.GetPoint(bezierPoints[i], bezierPoints[nextI], t);
                positions[pI] = point;
                pI++;
            }
        }

        return positions;
    }

    private List<Vector3> getEvenlyDistributedPoints(Vector3[] points)
    {
        var evenlyDistributedPoints = new List<Vector3>();
        evenlyDistributedPoints.Add(points[0]);

        for (int i = 0; i < points.Length - 1; i++)
        {
            for (int j = i + 1; j < points.Length; j++)
            {
                var a = (Vector2)points[i];
                var b = (Vector2)points[j];
                float d = Vector2.Distance(a, b);
                if (d < _pointSpacing)
                {
                    continue;
                }
                else
                {
                    evenlyDistributedPoints.Add(points[j]);
                    i = j;
                }
            }
        }
        return evenlyDistributedPoints;
    }
}
