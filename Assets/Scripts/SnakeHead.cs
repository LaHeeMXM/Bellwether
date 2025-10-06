using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 主要控制器：W前进，A/D旋转，身体跟随前一个节点。
/// </summary>
public class SnakeHead : MonoBehaviour
{
    public bool isPlayer;

    [Header("速度与加速度")]
    public float walkSpeed = 2.5f;       // 按下W时的目标速度
    public float runSpeed = 5f;        // 按下W+空格时的目标速度
    public float acceleration = 2f;      // 加速度 (单位/秒²)
    public float deceleration = 4f;      // 减速度 (单位/秒²)

    [Header("操控")]
    public float headRotationSpeed = 150f;

    [Header("身体跟随")]
    public float bodyFollowRate = 7f;
    public float absoluteNodeSpacing = 1.0f;

    private readonly List<SnakeNode> allNodes = new List<SnakeNode>();
    private readonly List<Rigidbody> allNodeRigidbodies = new List<Rigidbody>();
    private float _currentSpeed = 0f; // 当前蛇头的实际速度

    private Rigidbody _headRigidbody;

    void FixedUpdate()
    {
        if (!isPlayer) return;
        if (allNodes.Count == 0)
        {
            // 如果没有节点了，确保速度归零
            _currentSpeed = 0;
            return;
        }

        _headRigidbody = allNodeRigidbodies[0];

        (float moveInput, float rotateInput, bool isRunning) = HandleInput();
        UpdateSpeed(moveInput, isRunning);
        UpdateHead(rotateInput);
        UpdateBodyPositions();
        BroadcastSpeedToAllNodes();
    }

    (float move, float rotate, bool running) HandleInput()
    {
        float move = Input.GetKey(KeyCode.W) ? 1f : 0f;
        float rotate = 0f;
        if (Input.GetKey(KeyCode.A)) rotate = -1f;
        else if (Input.GetKey(KeyCode.D)) rotate = 1f;
        bool running = Input.GetKey(KeyCode.Space);

        return (move, rotate, running);
    }


    void UpdateSpeed(float moveInput, bool isRunning)
    {
        float targetSpeed = 0f;

        // 如果有前进输入
        if (moveInput > 0)
        {
            // 根据是否按住空格，确定目标速度
            targetSpeed = isRunning ? runSpeed : walkSpeed;
        }

        // 根据当前速度和目标速度的差距，决定是加速还是减速
        if (_currentSpeed < targetSpeed)
        {
            // 加速
            _currentSpeed += acceleration * Time.fixedDeltaTime;
            // 防止超速
            _currentSpeed = Mathf.Min(_currentSpeed, targetSpeed);
        }
        else if (_currentSpeed > targetSpeed)
        {
            // 减速
            _currentSpeed -= deceleration * Time.fixedDeltaTime;
            // 防止速度低于目标值（特别是减速到0时）
            _currentSpeed = Mathf.Max(_currentSpeed, targetSpeed);
        }
    }

    void UpdateHead(float rotateInput)
    {
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

    void BroadcastSpeedToAllNodes()
    {
        // 蛇头也需要更新自己的动画
        allNodes[0].GetComponent<SheepAnimation>()?.UpdateMovementAnimation(_currentSpeed);

        // 身体节点的速度可以稍微延迟或打折，以产生更自然的摆动效果
        // 但最简单的实现是所有节点速度一致
        for (int i = 1; i < allNodes.Count; i++)
        {
            // 身体的速度就是蛇头的速度
            allNodes[i].GetComponent<SheepAnimation>()?.UpdateMovementAnimation(_currentSpeed);
        }
    }


    void UpdateBodyPositions()
    {
        for (int i = 1; i < allNodes.Count; i++)
        {
            Rigidbody currentRigidbody = allNodeRigidbodies[i];
            Rigidbody previousRigidbody = allNodeRigidbodies[i - 1];

            Vector3 currentPos = currentRigidbody.position;
            Vector3 previousPos = previousRigidbody.position;

            // 1. 位置追赶逻辑 (保持间距)
            float requiredDistance = absoluteNodeSpacing;
            float distance = Vector3.Distance(currentPos, previousPos);

            Vector3 targetPosition = currentPos; // 默认目标位置是当前位置

            if (distance > requiredDistance)
            {
                Vector3 directionToPrevious = (previousPos - currentPos).normalized;
                targetPosition = currentPos + directionToPrevious * (distance - requiredDistance);
            }
            else if (distance < requiredDistance * 0.9f)
            {
                Vector3 separateDirection = (currentPos - previousPos).normalized;
                targetPosition = currentPos + separateDirection * (requiredDistance * 0.9f - distance);
            }

            currentRigidbody.MovePosition(targetPosition);

            // --- 朝向追赶逻辑 ---
            Vector3 lookVector = previousPos - currentPos;
            if (lookVector.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookVector);
                // 使用Slerp计算目标旋转，然后用MoveRotation应用
                Quaternion newRotation = Quaternion.Slerp(
                    currentRigidbody.rotation,
                    targetRotation,
                    Time.deltaTime * bodyFollowRate
                );
                currentRigidbody.MoveRotation(newRotation);
            }
        }
    }

    /// <summary>
    /// 实例化传入的预制体，并将其作为新的身体节点添加到蛇尾。并返回蛇节点
    /// </summary>
    public void AddNodeToList(SnakeNode newNode)
    {
        int newIndex = allNodes.Count;
        newNode.segmentIndex = newIndex;

        // 第一个节点（蛇头）的特殊处理
        if (newIndex == 0)
        {
            newNode.transform.SetParent(transform.parent); // 放在与 "Head" 同级
            newNode.transform.position = transform.position;
            newNode.transform.rotation = transform.rotation;
        }

        // 后续身体节点的处理
        else
        {
            SnakeNode lastNode = allNodes[newIndex - 1];

            Vector3 newPosition = lastNode.transform.position - lastNode.transform.forward.normalized * absoluteNodeSpacing;

            newNode.transform.SetParent(transform.parent); // 放在与 "Head" 同级
            newNode.transform.position = newPosition;
            newNode.transform.rotation = lastNode.transform.rotation;
        }

        allNodes.Add(newNode);
        allNodeRigidbodies.Add(newNode.GetComponent<Rigidbody>());
    }



    public void RemoveNodeFromList(SnakeNode nodeToRemove)
    {
        int index = allNodes.IndexOf(nodeToRemove);
        if (index != -1)
        {
            allNodes.RemoveAt(index);
            // 移除Rigidbody引用
            allNodeRigidbodies.RemoveAt(index);
        }
    }

    public List<SnakeNode> GetAllNodes()
    {
        return allNodes;
    }

    public void SwapNodeTransforms(int index1, int index2)
    {
        if (index1 < 0 || index1 >= allNodes.Count || index2 < 0 || index2 >= allNodes.Count)
        {
            Debug.LogError("交换索引越界！");
            return;
        }

        // 交换物理节点列表
        SnakeNode tempNode = allNodes[index1];
        allNodes[index1] = allNodes[index2];
        allNodes[index2] = tempNode;

        // 交换Rigidbody缓存列表
        Rigidbody tempRb = allNodeRigidbodies[index1];
        allNodeRigidbodies[index1] = allNodeRigidbodies[index2];
        allNodeRigidbodies[index2] = tempRb;

        // --- 不再有任何操作Transform的代码 ---
        // 这将允许UpdateBodyPositions()来平滑处理位置变化

        Debug.Log($"物理节点列表已交换 {index1} 和 {index2}。");
    }


}