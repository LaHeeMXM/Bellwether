using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


public class SnakeHead : MonoBehaviour
{
    public bool isPlayer;

    [Header("速度与加速度")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float acceleration = 2f;
    public float deceleration = 4f;

    [Header("操控")]
    public float headRotationSpeed = 150f;

    [Header("身体跟随")]
    [Tooltip("身体节点跟随/旋转平滑追赶的因子。值越大，跟随越快。")]
    public float bodyFollowRate = 7f;
    [Tooltip("相邻两个节点中心点之间的理想最小距离（世界单位）")]
    public float absoluteNodeSpacing = 1.0f;

    // --- 私有变量 ---
    private readonly List<SnakeNode> allNodes = new List<SnakeNode>();
    private readonly List<Rigidbody> allNodeRigidbodies = new List<Rigidbody>();
    private float _currentSpeed = 0f;
    private Rigidbody _headRigidbody;

    // 使用FixedUpdate处理所有物理相关的逻辑
    void FixedUpdate()
    {
        if (!isPlayer) return;
        if (allNodes.Count == 0)
        {
            _currentSpeed = 0;
            return;
        }

        // 确保我们总是在控制正确的蛇头
        _headRigidbody = allNodeRigidbodies[0];

        (float moveInput, float rotateInput, bool isRunning) = HandleInput();
        UpdateSpeed(moveInput, isRunning);
        UpdateHead(rotateInput);
        UpdateBodyPositions();
        BroadcastSpeedToAllNodes();
    }

    // 处理 W A D 和 Space 输入
    (float move, float rotate, bool running) HandleInput()
    {
        float move = Input.GetKey(KeyCode.W) ? 1f : 0f;
        float rotate = 0f;
        if (Input.GetKey(KeyCode.A)) rotate = -1f;
        else if (Input.GetKey(KeyCode.D)) rotate = 1f;
        bool running = Input.GetKey(KeyCode.Space);

        return (move, rotate, running);
    }

    // 平滑加减速逻辑
    void UpdateSpeed(float moveInput, bool isRunning)
    {
        float targetSpeed = 0f;
        if (moveInput > 0)
        {
            targetSpeed = isRunning ? runSpeed : walkSpeed;
        }

        if (_currentSpeed < targetSpeed)
        {
            _currentSpeed = Mathf.Min(_currentSpeed + acceleration * Time.fixedDeltaTime, targetSpeed);
        }
        else if (_currentSpeed > targetSpeed)
        {
            _currentSpeed = Mathf.Max(_currentSpeed - deceleration * Time.fixedDeltaTime, targetSpeed);
        }
    }

    // 更新蛇头的位置和旋转
    void UpdateHead(float rotateInput)
    {
        if (_headRigidbody == null) return;

        // 旋转
        if (_currentSpeed > 0.1f && rotateInput != 0)
        {
            Quaternion deltaRotation = Quaternion.Euler(Vector3.up * rotateInput * headRotationSpeed * Time.fixedDeltaTime);
            _headRigidbody.MoveRotation(_headRigidbody.rotation * deltaRotation);
        }

        // 前进
        Vector3 targetPosition = _headRigidbody.position + _headRigidbody.transform.forward * _currentSpeed * Time.fixedDeltaTime;
        _headRigidbody.MovePosition(targetPosition);
    }

    // 更新身体节点的位置和旋转
    void UpdateBodyPositions()
    {
        for (int i = 1; i < allNodes.Count; i++)
        {
            Rigidbody currentRigidbody = allNodeRigidbodies[i];
            Rigidbody previousRigidbody = allNodeRigidbodies[i - 1];

            Vector3 currentPos = currentRigidbody.position;
            Vector3 previousPos = previousRigidbody.position;

            // 位置追赶逻辑
            float distance = Vector3.Distance(currentPos, previousPos);
            if (distance > absoluteNodeSpacing)
            {
                Vector3 targetFollowPoint = previousPos - (previousPos - currentPos).normalized * absoluteNodeSpacing;
                Vector3 newPosition = Vector3.Lerp(currentPos, targetFollowPoint, Time.fixedDeltaTime * bodyFollowRate);
                currentRigidbody.MovePosition(newPosition);
            }

            // 朝向追赶逻辑
            Vector3 lookVector = previousPos - currentPos;
            if (lookVector.sqrMagnitude > 0.0001f)
            {
                lookVector.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(lookVector);
                Quaternion newRotation = Quaternion.Slerp(currentRigidbody.rotation, targetRotation, Time.fixedDeltaTime * bodyFollowRate);
                currentRigidbody.MoveRotation(newRotation);
            }
        }
    }

    // 将速度广播给所有节点以更新动画
    void BroadcastSpeedToAllNodes()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            allNodes[i].GetComponent<SheepAnimation>()?.UpdateMovementAnimation(_currentSpeed);
        }
    }

    // 将已实例化的节点添加到物理列表
    public void AddNodeToList(SnakeNode newNode)
    {
        int newIndex = allNodes.Count;
        newNode.segmentIndex = newIndex;

        if (newIndex == 0)
        {
            newNode.transform.SetParent(transform.parent);
            newNode.transform.position = transform.position;
            newNode.transform.rotation = transform.rotation;
        }
        else
        {
            SnakeNode lastNode = allNodes[newIndex - 1];
            Vector3 newPosition = lastNode.transform.position - lastNode.transform.forward.normalized * absoluteNodeSpacing;
            newNode.transform.SetParent(transform.parent);
            newNode.transform.position = newPosition;
            newNode.transform.rotation = lastNode.transform.rotation;
        }

        allNodes.Add(newNode);
        allNodeRigidbodies.Add(newNode.GetComponent<Rigidbody>());
    }

    // 从物理列表中移除节点
    public void RemoveNodeFromList(SnakeNode nodeToRemove)
    {
        int index = allNodes.IndexOf(nodeToRemove);
        if (index != -1)
        {
            allNodes.RemoveAt(index);
            allNodeRigidbodies.RemoveAt(index);
        }
    }

    public List<SnakeNode> GetAllNodes()
    {
        return allNodes;
    }

    // ✨ 瞬时交换Transform以提供即时视觉反馈
    public void SwapNodeTransforms(int index1, int index2)
    {
        if (index1 < 0 || index1 >= allNodes.Count || index2 < 0 || index2 >= allNodes.Count)
        {
            Debug.LogError("交换索引越界！");
            return;
        }

        // 1. 交换列表中的引用
        SnakeNode tempNode = allNodes[index1];
        allNodes[index1] = allNodes[index2];
        allNodes[index2] = tempNode;

        Rigidbody tempRb = allNodeRigidbodies[index1];
        allNodeRigidbodies[index1] = allNodeRigidbodies[index2];
        allNodeRigidbodies[index2] = tempRb;

        // 2. 交换实际的Transform
        // 注意：此时列表中的对象已经交换，所以t1获取的是交换后的allNodes[index1]
        Transform t1 = allNodes[index1].transform;
        Transform t2 = allNodes[index2].transform;

        Vector3 tempPos = t1.position;
        Quaternion tempRot = t1.rotation;

        t1.position = t2.position;
        t1.rotation = t2.rotation;

        t2.position = tempPos;
        t2.rotation = tempRot;
    }
}