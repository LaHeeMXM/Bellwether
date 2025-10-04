using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 主要控制器：W前进，A/D旋转，身体跟随前一个节点。
/// </summary>
public class SnakeHead : MonoBehaviour
{
    public float headForwardSpeed = 5f;       // 蛇头前进的速度
    public float headRotationSpeed = 150f;    // 蛇头旋转的角度/秒
    
    [Tooltip("身体节点跟随/旋转平滑追赶的因子。值越大，跟随越快。")]
    public float bodyFollowRate = 10f; 
    
    [Tooltip("相邻两个节点中心点之间的理想最小距离（世界单位）")]
    public float absoluteNodeSpacing = 1.0f; 

    private readonly List<SnakeNode> allNodes = new List<SnakeNode>();

    public GameObject newNodePrefab; // 新节点预制体

    void Start()
    {
        // 确保 SnakeHead GameObject 挂载了 SnakeNode 脚本
        SnakeNode headNode = GetComponent<SnakeNode>();
        if (headNode == null)
        {
            headNode = gameObject.AddComponent<SnakeNode>();
            headNode.size = new Vector3(1f, 1f, 1f); 
        }
        headNode.segmentIndex = 0; 
        allNodes.Add(headNode);
    }
    
    void Update()
    {
        // 获取输入量 (move: W, rotate: A/D)
        (float moveInput, float rotateInput) = HandleInput();
        
        UpdateHead(moveInput, rotateInput);
        UpdateBodyPositions();
        
    }

    // 处理 W A D 输入
    (float move, float rotate) HandleInput()
    {
        float move = 0f;
        float rotate = 0f;
        
        if (Input.GetKey(KeyCode.W)) move = 1f;
        if (Input.GetKey(KeyCode.A)) rotate = -1f;
        else if (Input.GetKey(KeyCode.D)) rotate = 1f;

        return (move, rotate);
    }

    /// <summary>
    /// 更新蛇头的位置和旋转。
    /// </summary>
    void UpdateHead(float moveInput, float rotateInput)
    {
     
        if (moveInput > 0)
        {
            if (rotateInput != 0)
            {
                transform.Rotate(Vector3.up, rotateInput * headRotationSpeed * Time.deltaTime, Space.Self);
            }
            // 确保前进方向只在 XZ 平面上
            Vector3 forwardDirection = transform.forward;
            forwardDirection.y = 0; 
            
            transform.position += forwardDirection.normalized * headForwardSpeed * Time.deltaTime;
        }
    }

    void UpdateBodyPositions()
    {
        for (int i = 1; i < allNodes.Count; i++)
        {
            SnakeNode currentNode = allNodes[i];
            SnakeNode previousNode = allNodes[i - 1];

            Vector3 currentPos = currentNode.transform.position;
            Vector3 previousPos = previousNode.transform.position;
            
            // 1. 位置追赶逻辑 (保持间距)
            float requiredDistance = absoluteNodeSpacing; 
            float distance = Vector3.Distance(currentPos, previousPos);

            if (distance > requiredDistance)
            {
                // 计算从前一个节点指向当前节点的向量
                Vector3 directionToCurrent = (currentPos - previousPos).normalized;

                // 目标跟随点：从前一个节点沿着其反方向后退 requiredDistance
                Vector3 targetFollowPoint = previousPos + directionToCurrent * requiredDistance;
                
                // 使用 Lerp 平滑移动到目标点
                currentNode.transform.position = Vector3.Lerp(
                    currentPos, 
                    targetFollowPoint, 
                    Time.deltaTime * bodyFollowRate
                );
            }
            //确保间距不会被拉得太近，轻微推离
            else if (distance < requiredDistance * 0.9f) 
            {
                 Vector3 separateDirection = (currentPos - previousPos).normalized;
                 currentNode.transform.position += separateDirection * (requiredDistance * 0.9f - distance) * Time.deltaTime * bodyFollowRate * 0.5f;
            }

            // 目标朝向向量：从当前位置指向前一个位置 (previousPos - currentPos)
            Vector3 lookVector = previousPos - currentNode.transform.position;
            
            if (lookVector.sqrMagnitude > 0.0001f)
            {
                lookVector.y = 0; // 确保只在 XZ 平面旋转
                
                Quaternion targetRotation = Quaternion.LookRotation(lookVector);
                
                // 平滑插值到目标旋转
                currentNode.transform.rotation = Quaternion.Slerp(
                    currentNode.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * bodyFollowRate
                );
            }
        }
    }

    /// <summary>
    /// 实例化传入的预制体，并将其作为新的身体节点添加到蛇尾。并返回蛇节点
    /// </summary>
    public SnakeNode AddNode()
    {
        GameObject newCube = Instantiate(newNodePrefab);
        SnakeNode newNode = newCube.GetComponent<SnakeNode>() ?? newCube.AddComponent<SnakeNode>();

        newNode.segmentIndex = allNodes.Count;

        SnakeNode lastNode = allNodes[allNodes.Count - 1];

        // 2. ⭐ 计算新节点的位置：从最后一个节点的位置，沿着其后方 (负 forward 方向) 移动 absoluteNodeSpacing 距离。
        Vector3 newPosition = lastNode.transform.position - lastNode.transform.forward.normalized * absoluteNodeSpacing;

        // 3. 设置位置和旋转
        newCube.transform.position = newPosition;
        newCube.transform.rotation = lastNode.transform.rotation;

        newCube.transform.SetParent(transform.parent);

        // 4. 加入列表
        allNodes.Add(newNode);
        return newNode;
    }
    
    public List<SnakeNode> GetAllNodes()
    {
        return allNodes;
    }
}