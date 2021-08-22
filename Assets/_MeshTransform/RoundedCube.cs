// 生成立方体、球体和胶囊体，做了特殊处理，清除了其中的碰撞体，改为简易碰撞体
// 以Y轴为基准，即x和z两轴会置于中心，y轴会置于底部
// https://zhuanlan.zhihu.com/p/90104885
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(SkinnedMeshRenderer))]
public class RoundedCube : MonoBehaviour {

	public int length, round;
	public int extraSize = 2;	// 数值大了直接卡死
	private int xSize, ySize, zSize;
	private Vector3 offset;
	private int roundness;	// 要生成完全的球面，roundness需等于x y z size的一半

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color32[] cubeUV;

	private void Awake () {
		xSize = zSize = round;
		ySize = length;
		xSize *= extraSize;
		ySize *= extraSize;
		zSize *= extraSize;
		transform.localScale /= extraSize;
		roundness = xSize / 2;
		offset = new Vector3(xSize * -0.5f, 0, zSize * -0.5f);
		Generate();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			Vector3 scale = transform.localScale;
			scale.y *= (float)round / (float)length;
			transform.localScale = scale;
			Debug.Log("count:" + mesh.vertexCount);
		}
	}

	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Cube";
		GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
		CreateVertices();
		CreateTriangles();
		// CreateColliders();
	}

	// 此处顶点的size越大，则mesh的顶点越多，考虑先做较大的size，较多顶点来保证平滑，再调整物体的size来调整大小
	private void CreateVertices () {
		int cornerVertices = 8;
		// 边顶点
		int edgeVertices = (xSize + ySize + zSize - 3) * 4;
		int faceVertices = (
			(xSize - 1) * (ySize - 1) +
			(xSize - 1) * (zSize - 1) +
			(ySize - 1) * (zSize - 1)) * 2;
		vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
		print("======" + vertices.Length);
		normals = new Vector3[vertices.Length];
		cubeUV = new Color32[vertices.Length];

		int v = 0;
		for (int y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++) {
				SetVertex(v++, x, y, 0);
			}
			for (int z = 1; z <= zSize; z++) {
				SetVertex(v++, xSize, y, z);
			}
			for (int x = xSize - 1; x >= 0; x--) {
				SetVertex(v++, x, y, zSize);
			}
			for (int z = zSize - 1; z > 0; z--) {
				SetVertex(v++, 0, y, z);
			}
		}
		for (int z = 1; z < zSize; z++) {
			for (int x = 1; x < xSize; x++) {
				SetVertex(v++, x, ySize, z);
			}
		}
		for (int z = 1; z < zSize; z++) {
			for (int x = 1; x < xSize; x++) {
				SetVertex(v++, x, 0, z);
			}
		}

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.colors32 = cubeUV;
	}

	private void SetVertex (int i, float x, float y, float z) {
		Vector3 inner = vertices[i] = new Vector3(x, y, z);

		// 实现圆角，没有添加新的点，只是将原本的点压进去了
		// inner总体来说是一个内点，在大立方体内部虚拟一个圆度距离的小立方体，再虚拟一个球体，卡在两个立方体之间，球体的半径等于圆度
		// 小立方体上的点是内点，对应的大立方体上是外点
		if (x < roundness) {
			inner.x = roundness;
		}
		else if (x > xSize - roundness) {
			inner.x = xSize - roundness;
		}
		if (y < roundness) {
			inner.y = roundness;
		}
		else if (y > ySize - roundness) {
			inner.y = ySize - roundness;
		}
		if (z < roundness) {
			inner.z = roundness;
		}
		else if (z > zSize - roundness) {
			inner.z = zSize - roundness;
		}
		// 内点与外点之间的方向向量，内点+方向乘以半径（即圆度），就得到应在的圆角外点
		normals[i] = (vertices[i] - inner).normalized;
		vertices[i] = inner + normals[i] * roundness + offset;
		cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
	}

	private void CreateTriangles () {
		int[] trianglesZ = new int[(xSize * ySize) * 12];
		int[] trianglesX = new int[(ySize * zSize) * 12];
		int[] trianglesY = new int[(xSize * zSize) * 12];
		int ring = (xSize + zSize) * 2;
		int tZ = 0, tX = 0, tY = 0, v = 0;

		for (int y = 0; y < ySize; y++, v++) {
			for (int q = 0; q < xSize; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < zSize; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < xSize; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < zSize - 1; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
		}

		tY = CreateTopFace(trianglesY, tY, ring);
		tY = CreateBottomFace(trianglesY, tY, ring);

		mesh.subMeshCount = 3;
		mesh.SetTriangles(trianglesZ, 0);
		mesh.SetTriangles(trianglesX, 1);
		mesh.SetTriangles(trianglesY, 2);
	}

	private int CreateTopFace (int[] triangles, int t, int ring) {
		int v = ring * ySize;
		for (int x = 0; x < xSize - 1; x++, v++) {
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (ySize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
			for (int x = 1; x < xSize - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
		}

		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}

	private int CreateBottomFace (int[] triangles, int t, int ring) {
		int v = 1;
		int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < xSize - 1; x++, v++, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= xSize - 2;
		int vMax = v + 2;

		for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
			for (int x = 1; x < xSize - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}

	// 创建四边形 012 345 由3个顺时针三角形构成 其中14 23同顶点
	private static int SetQuad (int[] triangles, int i, int v00, int v10, int v01, int v11) {
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}

	// 考虑只生成简易的碰撞体 如果有roundness且够大，搞个胶囊，否则搞个box
	private void CreateColliders () {
		AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
		AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
		AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

		Vector3 min = Vector3.one * roundness;
		Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f; 
		Vector3 max = new Vector3(xSize, ySize, zSize) - min;

		AddCapsuleCollider(0, half.x, min.y, min.z);
		AddCapsuleCollider(0, half.x, min.y, max.z);
		AddCapsuleCollider(0, half.x, max.y, min.z);
		AddCapsuleCollider(0, half.x, max.y, max.z);
		
		AddCapsuleCollider(1, min.x, half.y, min.z);
		AddCapsuleCollider(1, min.x, half.y, max.z);
		AddCapsuleCollider(1, max.x, half.y, min.z);
		AddCapsuleCollider(1, max.x, half.y, max.z);
		
		AddCapsuleCollider(2, min.x, min.y, half.z);
		AddCapsuleCollider(2, min.x, max.y, half.z);
		AddCapsuleCollider(2, max.x, min.y, half.z);
		AddCapsuleCollider(2, max.x, max.y, half.z);
	}

	private void AddBoxCollider (float x, float y, float z) {
		BoxCollider c = gameObject.AddComponent<BoxCollider>();
		c.size = new Vector3(x, y, z);
	}

	private void AddCapsuleCollider (int direction, float x, float y, float z) {
		CapsuleCollider c = gameObject.AddComponent<CapsuleCollider>();
		c.center = new Vector3(x, y, z);
		c.direction = direction;
		c.radius = roundness;
		c.height = c.center[direction] * 2f;
	}

//	private void OnDrawGizmos () {
//		if (vertices == null) {
//			return;
//		}
//		for (int i = 0; i < vertices.Length; i++) {
//			Gizmos.color = Color.black;
//			Gizmos.DrawSphere(vertices[i], 0.1f);
//			Gizmos.color = Color.yellow;
//			Gizmos.DrawRay(vertices[i], normals[i]);
//		}
//	}
}