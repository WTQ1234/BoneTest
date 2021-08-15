
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class DragMoveByAxisComponent : MonoBehaviour
{
    public Transform cube;  //要移动的物体

    private Transform axis; //坐标轴模型
    private Transform axis_x;
    private Transform axis_y;
    private Transform axis_z;
    private Camera camera_main; //只渲染坐标轴的摄像机
 
    private const float MOVE_SPEED = 0.15F; 
 
    private bool choosedAxis = false;   // 是否选中的某个轴
    private Vector3 lastPos; //上一帧鼠标位置

    private enum CurrentAxis{x, y, z};
    private CurrentAxis currentAxis = 0;

    private void Awake()
    {
        axis = transform;
        axis_x = axis.Find("Axis_X");
        axis_y = axis.Find("Axis_Y");
        axis_z = axis.Find("Axis_Z");
        camera_main = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera_main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer("Axis"))) //只检测Axis这一层
            {
                choosedAxis = true;
                lastPos = Input.mousePosition;
                if (hit.collider.name == axis_x.name) { currentAxis = CurrentAxis.x; }
                if (hit.collider.name == axis_y.name) { currentAxis = CurrentAxis.y; }
                if (hit.collider.name == axis_z.name) { currentAxis = CurrentAxis.z; }
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
        Vector3 origin = camera_main.WorldToScreenPoint(axis.position);  //坐标轴原点对应屏幕坐标
        Vector3 mouse = Input.mousePosition - lastPos;   //鼠标两帧之间的移动轨迹在屏幕上的向量
 
        Vector3 axisEnd_x = camera_main.WorldToScreenPoint(axis_x.position); //三个坐标轴的终点对应屏幕坐标
        Vector3 axisEnd_y= camera_main.WorldToScreenPoint(axis_y.position);
        Vector3 axisEnd_z = camera_main.WorldToScreenPoint(axis_z.position);
 
        Vector3 vector_x = axisEnd_x - origin;  //x轴对应屏幕向量
        Vector3 vector_y = axisEnd_y - origin;
        Vector3 vector_z = axisEnd_z - origin;
 
        Vector3 cubePos = cube.position;
        float cosLength;
        float d = Vector3.Distance(Input.mousePosition, lastPos) * 0.01f; //鼠标移动距离
        switch (currentAxis){
            case CurrentAxis.x:
                //鼠标移动轨迹与X轴夹角的余弦值
                cosLength = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_x));
                d = d * cosLength;
                cubePos.x += d;
                cube.position = cubePos;
                axis.position = cubePos;
                break;
            case CurrentAxis.y:
                cosLength = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_y));
                d = d * cosLength;
                cubePos.y += d;
                cube.position = cubePos;
                axis.position = cubePos;
                break;
            case CurrentAxis.z:
                cosLength = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_z));
                d = d * cosLength;
                cubePos.z += d;
                cube.position = cubePos;
                axis.position = cubePos;
                break;
            default:
                break;
        }

        // switch (currentAxis){
        //     case CurrentAxis.x:
        //         //鼠标移动轨迹与X轴夹角的余弦值
        //         cosLength = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_x));
        //         d = d * (cosLength < 0 ? -1 : 1);
        //         cubePos.x += d;
        //         cube.position = cubePos;
        //         axis.position = cubePos;
        //         break;
        //     case CurrentAxis.y:
        //         cosLength = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_y));
        //         d = d * (cosLength < 0 ? -1 : 1);
        //         cubePos.y += d;
        //         cube.position = cubePos;
        //         axis.position = cubePos;
        //         break;
        //     case CurrentAxis.z:
        //         cosLength = Mathf.Cos(Mathf.PI / 180 * Vector3.Angle(mouse, vector_z));
        //         d = d * (cosLength < 0 ? -1 : 1);
        //         cubePos.z += d;
        //         cube.position = cubePos;
        //         axis.position = cubePos;
        //         break;
        //     default:
        //         break;
        // }
        lastPos = Input.mousePosition;
    }

    public void onSetMoveObj(Transform trans)
    {
        cube = trans;
    }
}
