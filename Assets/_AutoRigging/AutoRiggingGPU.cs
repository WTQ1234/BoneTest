using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRiggingGPU : MonoBehaviour
{
    public ComputeShader RigCS;
    ComputeBuffer BoneBuffer; // 三维坐标
    ComputeBuffer PosBuffer; // 传入 mesh 顶点用于计算
    ComputeBuffer WeightBuffer; // 前 4 个 float 后 4 个 int 
    int kernel;

    public Limb limb;

    public int boneNum = 4;
    public Vector3 boneDirection;
    public float boneLength = 0.3f;

    public Bone prefab_bone;

    Mesh mesh;
    SkinnedMeshRenderer skin;

    Bone[] bones;
    Transform[] bones_tran;
    Matrix4x4[] bindPoses;
    public float spread = 0.1f;

    void Start()
    {
        int maxBoneNum = 30;
        int maxVerNum = 100000; // 假设最大的模型顶点数
        BoneBuffer = new ComputeBuffer(maxBoneNum, sizeof(float) * 3);
        WeightBuffer = new ComputeBuffer(maxVerNum, sizeof(float) * 4 + sizeof(int) * 4);
        PosBuffer = new ComputeBuffer(maxVerNum, sizeof(float) * 3);

        bones_tran = new Transform[boneNum];
        bindPoses = new Matrix4x4[boneNum];
        bones = new Bone[boneNum];
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Generate();
            limb.OnSetBezier();
            Debug.Log("count:" + mesh.vertexCount);
        }
    }

    public void Generate()
    {
        for (int i = 0; i < boneNum; i++)
        {
            Bone bone = GameObject.Instantiate<Bone>(prefab_bone);
            Transform trans = bone.transform;
            bones[i] = bone;
            bones_tran[i] = trans;

            limb.OnAddBone(bone);
            trans.name = "Bone" + i;
            if (i == 0)
            {
                trans.parent = transform;
                bone.onInit(limb, null);
            }
            else
            {
                trans.parent = bones[i - 1].transform;
                bone.onInit(limb, bones[i - 1]);
            }

            trans.localRotation = Quaternion.identity;

            // 保证起始的端点在 mesh 的坐标零点
            if (i == 0)
                trans.localPosition = Vector3.zero;
            else
                trans.localPosition = boneDirection.normalized * boneLength;

            bindPoses[i] = trans.worldToLocalMatrix * transform.localToWorldMatrix;
        }
        
        mesh = transform.GetComponent<MeshFilter>().mesh;
        skin = transform.GetComponent<SkinnedMeshRenderer>();
        
        RigCS.SetFloat("Spread", spread);
        RigCS.SetInt("VertexNum", mesh.vertexCount);
        RigCS.SetInt("BoneNum", boneNum);
        
        kernel = RigCS.FindKernel("Calculate");
        PosBuffer.SetData(mesh.vertices);
        RigCS.SetBuffer(kernel, "PosBuffer", PosBuffer);

        Vector3[] boneData = new Vector3[boneNum];
        for (int i = 0; i < boneNum; i++)
            boneData[i] = (bones_tran[i].position  - transform.position)/ transform.localScale.x;
        BoneBuffer.SetData(boneData);
        RigCS.SetBuffer(kernel, "BoneBuffer", BoneBuffer);
        RigCS.SetBuffer(kernel, "WeightBuffer", WeightBuffer);
        RigCS.Dispatch(kernel, (int)Mathf.Ceil(mesh.vertexCount / 8f), 1, 1);

        BoneWeight[] weightData = new BoneWeight[mesh.vertexCount];
        WeightBuffer.GetData(weightData, 0, 0, mesh.vertexCount);

        mesh.boneWeights = weightData;
        mesh.bindposes = bindPoses;
        skin.bones = bones_tran;
        skin.sharedMesh = mesh;

    }
}
