using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    public List<Bone> bones = new List<Bone>();

    // 超过此间距重新计算骨骼
    public float boneSpacing = 10;
}
