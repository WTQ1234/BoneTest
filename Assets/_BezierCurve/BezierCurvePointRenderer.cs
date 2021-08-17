using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BezierCurvePointRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int vertexCount;

    public Transform[] positions;

    private List<Vector3> pointList = new List<Vector3>();

    private void Update()
    {
        // BezierCurveWithUnlimitPoints();
        List<Vector3> temp = new List<Vector3>();
        for (int i = 0; i < positions.Length; i++)
        {
            temp.Add(positions[i].position);
        }
        pointList = Common_Math.GetBezierCurvePoints(null, temp, vertexCount);
        if (lineRenderer)
        {
            lineRenderer.positionCount = pointList.Count;
            lineRenderer.SetPositions(pointList.ToArray());
        }
    }

    public void BezierCurveWithUnlimitPoints()
    {
        pointList.Clear();
        for (float ratio = 0; ratio <= 1; ratio += 1.0f / vertexCount)
        {
            pointList.Add(getBezierCurvePoint(positions, ratio));
        }
        pointList.Add(positions[positions.Length - 1].position);
    }
    public Vector3 getBezierCurvePoint(Transform[] trans, float t)
    {
        Vector3[] temp = new Vector3[trans.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = trans[i].position;
        }
        int n = temp.Length - 1;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n - i; j++)
            {
                temp[j] = Vector3.Lerp(temp[j], temp[j + 1], t);
            }
        }
        return temp[0];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Gizmos.DrawLine(positions[i].position, positions[i + 1].position);
        }

        Gizmos.color = Color.red;
        Vector3[] temp = new Vector3[positions.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = positions[i].position;
        }
        int n = temp.Length - 1;
        for (float ratio = 0.5f / vertexCount; ratio < 1; ratio += 1.0f / vertexCount)
        {
            for (int i = 0; i < n - 2; i++)
            {
                Gizmos.DrawLine(Vector3.Lerp(temp[i], temp[i + 1], ratio), Vector3.Lerp(temp[i + 2], temp[i + 3], ratio));
            }
        }
    }
}

