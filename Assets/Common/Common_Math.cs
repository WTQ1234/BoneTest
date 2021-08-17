using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Common_Math
{
    public static List<Vector3> GetBezierCurvePoints(List<Vector3> pointList, List<Vector3> positions, int vertexCount)
    {
        if (pointList == null)
        {
            pointList = new List<Vector3>();
        }
        // 复用
        pointList.Clear();
        for (float ratio = 0; ratio <= 1; ratio += 1.0f / vertexCount)
        {
            pointList.Add(_GetBezierCurvePoints(positions, ratio));
        }
        pointList.Add(positions[positions.Count - 1]);
        return pointList;
    }
    private static Vector3 _GetBezierCurvePoints(List<Vector3> positions, float ratio)
    {
        List<Vector3> temp = new List<Vector3>();
        for (int i = 0; i < positions.Count; i++)
        {
            temp.Add(positions[i]);
        }
        int n = temp.Count - 1;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n - i; j++)
            {
                temp[j] = Vector3.Lerp(temp[j], temp[j + 1], ratio);
            }
        }
        return temp[0];
    }
}
