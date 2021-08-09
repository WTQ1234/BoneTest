
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class DragMoveByAxisComponent : MonoBehaviour
{
    public Transform axis; //坐标轴模型
    public Transform cube;  //要移动的物体
    public Transform axisCamera; //只渲染坐标轴的摄像机
 
    private const float MOVE_SPEED = 0.005F; 
 
    private int currentAxis = 0;//1:x 2:y 3:z 要移动的轴
    private bool choosedAxis = false;
    private Vector3 lastPos; //上一帧鼠标位置
 
    void Start()
    {
    }
 
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = axisCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Axis"))) //只检测Axis这一层
            {
                choosedAxis = true;
                lastPos = Input.mousePosition;
                if (hit.collider.name == "x") { currentAxis = 1; }
                if (hit.collider.name == "y") { currentAxis = 2; }
                if (hit.collider.name == "z") { currentAxis = 3; }
            }
        }
        if (Input.GetMouseButton(0) && choosedAxis)
        {
            UpdateCubePosition();
        }
        if (Input.GetMouseButtonUp(0))
        {
            choosedAxis = false;
            currentAxis = 0;
        }
    }
 
    private void UpdateCubePosition()
    {
        Camera camera = axisCamera.GetComponent<Camera>();
        Vector3 origin = camera.WorldToScreenPoint(axis.position);  //三个坐标轴向量的原点对应屏幕坐标
        Vector3 mouse = Input.mousePosition - lastPos;   //鼠标两帧之间的移动轨迹在屏幕上的向量
 
        Vector3 axisEnd_x = camera.WorldToScreenPoint(axis.Find("x/x").position); //三个坐标轴的终点对应屏幕坐标
        Vector3 axisEnd_y= camera.WorldToScreenPoint(axis.Find("y/y").position);
        Vector3 axisEnd_z = camera.WorldToScreenPoint(axis.Find("z/z").position);
 
        Vector3 vector_x = axisEnd_x - origin;  //x轴对应屏幕向量
        Vector3 vector_y = axisEnd_y - origin;
        Vector3 vector_z = axisEnd_z - origin;
 
        Vector3 cubePos = cube.position;
        float d = Vector3.Distance(Input.mousePosition, lastPos) * MOVE_SPEED; //鼠标移动距离
        if (currentAxis == 1)
        {
            //鼠标移动轨迹与X轴夹角的余弦值
            float cos= Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_x));
            if (cos < 0) { d = -d; }
            cubePos.x += d;
            cube.position = cubePos;
            axis.position = cubePos;
        }
        if (currentAxis == 2)
        {
           
            float cos = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_y));
            if (cos < 0) { d = -d; }
            cubePos.y += d;
            cube.position = cubePos;
            axis.position = cubePos;
        }
        if (currentAxis == 3)
        {
            
            float cos = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_z));
            if (cos < 0) { d = -d; }
            cubePos.z += d;
            cube.position = cubePos;
            axis.position = cubePos;
        }
 
 
        lastPos = Input.mousePosition;
    }
}
