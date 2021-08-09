using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    [Header("挂载部件")]
    public List<Part> parts = new List<Part>();
    [Header("挂载肢体")]
    public List<Limb> limbs = new List<Limb>();
    [Header("所属肢体")]
    public Limb parentLimb;
    [Header("上一骨骼")]
    public Bone preBone;
    [Header("下一骨骼")]
    public Bone nextBone;
}
