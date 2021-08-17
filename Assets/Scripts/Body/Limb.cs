// 躯干肢体

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    public List<Bone> bones = new List<Bone>();
    public int boneNum = 5;                 // 自动生成骨骼的数量
    public float boneSpacing = 10;          // 超过此间距重新计算骨骼
    private Vector3 boneDirection;           // 骨骼朝向
    
    public bool useBezierPosture = false;
    public List<Transform> bezierPoints;
    private List<Vector3> bezierPoints_Vec3;

    public LineRenderer lineRenderer;

    private void Awake()
    {
        boneDirection = new Vector3(0, 1, 0);
        bezierPoints_Vec3 = new List<Vector3>();
    }

    private void Update() {
        if (useBezierPosture) 
        {
            OnSetBezier();
        }
    }

    public void OnSetBezier()
    {
        useBezierPosture = true;
        // if (!useBezierPosture) return;
        bezierPoints_Vec3.Clear();
        for (int i = 0; i < bezierPoints.Count; i++)
        {
            bezierPoints_Vec3.Add(bezierPoints[i].localPosition);
        }        
        List<Vector3> BonePoses = Common_Math.GetBezierCurvePoints(null, bezierPoints_Vec3, boneNum);
        for (int i = 0; i < bones.Count; i++)
        {
            bones[i].transform.position = BonePoses[i] + transform.position;    // 相对与limb的本地位置
        }
        // todo 这里还是不太对
        bones[bones.Count - 1].transform.position = BonePoses[BonePoses.Count - 1] + transform.position;
        // 画线
        lineRenderer.positionCount = BonePoses.Count;
        lineRenderer.SetPositions(BonePoses.ToArray());
    }

    public void OnAddBone(Bone bone)
    {
        bones.Add(bone);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < bezierPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(bezierPoints[i].position, bezierPoints[i + 1].position);
        }

        Gizmos.color = Color.red;
        Vector3[] temp = new Vector3[bezierPoints.Count];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = bezierPoints[i].position;
        }
        int n = temp.Length - 1;
        for (float ratio = 0.5f / boneNum; ratio < 1; ratio += 1.0f / boneNum)
        {
            for (int i = 0; i < n - 2; i++)
            {
                Gizmos.DrawLine(Vector3.Lerp(temp[i], temp[i + 1], ratio), Vector3.Lerp(temp[i + 2], temp[i + 3], ratio));
            }
        }
    }
}
