using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public SnakeHead snakeHead;
    [HideInInspector] public BattleHead battleHead;

    private enum AIState { Patrolling, Assessing, Chasing }
    private AIState currentState = AIState.Patrolling;

    [Header("AI感知设置")]
    public float awarenessRadius = 10f; // 警觉半径
    public float chaseDuration = 8f; // 追击持续时间

    // AI行为的协程管理器
    private Coroutine currentBehaviorCoroutine;

    // 对玩家蛇的引用
    private BattleHead playerBattleHead;
    private BattleNode chaseTarget; // 当前追击的目标节点

    public void Initialize(GameObject headPrefab, Quaternion initialRotation)
    {
        // 在自己下方生成 "Head" 逻辑对象
        GameObject headInstance = Instantiate(headPrefab, transform);
        headInstance.transform.localPosition = Vector3.zero;
        headInstance.transform.localRotation = initialRotation;

        // 获取核心组件的引用
        snakeHead = headInstance.GetComponent<SnakeHead>();
        battleHead = headInstance.GetComponent<BattleHead>();

        // 明确告知这些组件，它们不属于玩家
        snakeHead.isPlayer = false;
        battleHead.isPlayer = false;

        // 为AI设置固定的速度值
        snakeHead.walkSpeed = 2f;
        snakeHead.runSpeed = 4f;

        // 查找并缓存玩家的BattleHead引用
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // GetComponentInChildren 会查找包括自身在内的所有子对象
            playerBattleHead = playerController.GetComponentInChildren<BattleHead>();
        }

        if (playerBattleHead == null)
        {
            Debug.LogError($"{gameObject.name}: AI未能找到玩家的BattleHead！将无法索敌。");
        }
    }

    void Start()
    {
        // 游戏开始时，启动巡逻行为
        StartBehavior(PatrolRoutine());
    }

    void Update()
    {
        // AI的主要决策逻辑只在巡逻时触发
        if (currentState == AIState.Patrolling)
        {
            CheckForPlayer();
        }
    }

    // 启动一个新的行为协程（会先停止旧的）
    private void StartBehavior(IEnumerator newRoutine)
    {
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
        }
        currentBehaviorCoroutine = StartCoroutine(newRoutine);
    }

    #region 状态行为 (Coroutines)

    /// <summary>
    /// 1. 巡逻状态的行为
    /// </summary>
    private IEnumerator PatrolRoutine()
    {
        currentState = AIState.Patrolling;
        Debug.Log($"{gameObject.name} 开始巡逻。");

        while (true) // 无限循环巡逻
        {
            // 获取当前蛇的长度
            int currentLength = battleHead.GetList().Count;

            // 定义基础时长和长度系数
            float baseMoveDurationMin = 5f;
            float baseMoveDurationMax = 10f;
            float lengthMultiplier = 0.5f;

            // 计算与长度相关的动态范围
            float dynamicMoveDurationMax = baseMoveDurationMax + (currentLength * lengthMultiplier);

            // 随机生成巡逻指令
            float turnDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            float turnDuration = Random.Range(1f, 3f);
            float moveDuration = Random.Range(baseMoveDurationMin, dynamicMoveDurationMax);

            // --- 执行转弯 ---
            float turnTimer = 0f;
            while (turnTimer < turnDuration)
            {
                snakeHead.SetAIInput(1f, turnDirection, false); // false = walkSpeed
                turnTimer += Time.deltaTime;
                yield return null;
            }

            // --- 执行直行 ---
            float moveTimer = 0f;
            while (moveTimer < moveDuration)
            {
                snakeHead.SetAIInput(1f, 0f, false);
                moveTimer += Time.deltaTime;
                yield return null;
            }
        }
    }

    /// <summary>
    /// 2. 警觉与评估状态的行为
    /// </summary>
    private IEnumerator AssessRoutine()
    {
        currentState = AIState.Assessing;
        Debug.Log($"{gameObject.name} 发现玩家，正在评估...");

        // 停止移动，原地发呆
        snakeHead.SetAIInput(0f, 0f, false);

        // --- 实力对比 ---
        int myLevel = battleHead.GetLevelForSlot(0); // AI蛇头等级
        int playerLevel = playerBattleHead.GetLevelForSlot(0); // 玩家蛇头等级
        int myLength = battleHead.GetList().Count;
        int playerLength = playerBattleHead.GetList().Count;

        bool shouldChase = (myLevel >= playerLevel + 2) || (myLength >= playerLength + 2);

        if (shouldChase)
        {
            Debug.Log($"{gameObject.name} 决定追击！发呆2秒...");
            yield return new WaitForSeconds(2f);

            // 切换到追击行为
            StartBehavior(ChaseRoutine());
        }
        else
        {
            Debug.Log($"{gameObject.name} 认为玩家太强，继续巡逻。");
            // 认为打不过，就切换回巡逻
            StartBehavior(PatrolRoutine());
        }
    }

    /// <summary>
    /// 3. 追击状态的行为
    /// </summary>
    private IEnumerator ChaseRoutine()
    {
        currentState = AIState.Chasing;
        Debug.Log($"{gameObject.name} 开始追击！");

        float chaseTimer = 0f;
        while (chaseTimer < chaseDuration)
        {
            // 每帧更新目标和移动指令
            FindChaseTarget();

            // 安全校验：如果目标突然消失了
            if (chaseTarget == null && playerBattleHead.GetList().Count > 0)
            {
                chaseTarget = playerBattleHead.GetList()[0]; // 备用方案：追蛇头
            }

            if (chaseTarget != null)
            {
                // 获取AI物理蛇头的实时位置
                Transform aiHeadTransform = snakeHead.GetAllNodes()[0].transform;

                // 计算朝向目标的向量
                Vector3 directionToTarget = (chaseTarget.transform.position - aiHeadTransform.position).normalized;

                // 计算与蛇头前方向量的夹角，决定左转还是右转
                float angle = Vector3.SignedAngle(aiHeadTransform.forward, directionToTarget, Vector3.up);
                float rotateInput = Mathf.Clamp(angle / 45f, -1f, 1f);

                // 设置追击指令
                snakeHead.SetAIInput(1f, rotateInput, true); // true = runSpeed
            }

            chaseTimer += Time.deltaTime;
            yield return null;
        }

        // 跟丢，切换回巡逻
        Debug.Log($"{gameObject.name} 跟丢了，返回巡逻状态。");
        StartBehavior(PatrolRoutine());
    }

    #endregion

    #region AI辅助方法

    // 检查玩家是否进入警觉范围
    private void CheckForPlayer()
    {
        // 安全校验
        if (playerBattleHead == null || snakeHead.GetAllNodes().Count == 0) return;

        // 1. 获取AI蛇的物理蛇头Transform
        Transform aiHeadTransform = snakeHead.GetAllNodes()[0].transform;

        // 2. 获取玩家蛇的所有物理节点列表
        var playerNodes = playerBattleHead.GetList();
        if (playerNodes.Count == 0) return;

        // 3. 遍历玩家的所有节点，进行距离检测
        foreach (var playerNode in playerNodes)
        {
            if (playerNode == null) continue; // 额外的安全检查

            if (Vector3.Distance(aiHeadTransform.position, playerNode.transform.position) <= awarenessRadius)
            {
                // 发现玩家，进入评估状态
                StartBehavior(AssessRoutine());
                return; // 找到一个就够了
            }
        }
    }

    // 寻找追击目标
    private void FindChaseTarget()
    {
        if (playerBattleHead == null) return;
        var playerNodes = playerBattleHead.GetList();

        // 蛇太短，无法比较
        if (playerNodes.Count < 3)
        {
            chaseTarget = playerNodes.LastOrDefault(); // 追蛇尾
            return;
        }

        // 从第二个节点（索引1）开始遍历，到倒数第二个节点
        for (int i = 1; i < playerNodes.Count - 1; i++)
        {
            int currentDef = playerNodes[i].finalAttribute.Defense;
            int prevDef = playerNodes[i - 1].finalAttribute.Defense;
            int nextDef = playerNodes[i + 1].finalAttribute.Defense;

            if (currentDef < prevDef && currentDef < nextDef)
            {
                chaseTarget = playerNodes[i];
                return; // 找到第一个就锁定
            }
        }

        // 如果没找到，就追蛇尾
        chaseTarget = playerNodes.Last();
    }

    #endregion
}