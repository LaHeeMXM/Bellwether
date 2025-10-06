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

    [Header("AI��֪����")]
    public float awarenessRadius = 10f; // �����뾶
    public float chaseDuration = 8f; // ׷������ʱ��

    // AI��Ϊ��Э�̹�����
    private Coroutine currentBehaviorCoroutine;

    // ������ߵ�����
    private BattleHead playerBattleHead;
    private BattleNode chaseTarget; // ��ǰ׷����Ŀ��ڵ�

    public void Initialize(GameObject headPrefab, Quaternion initialRotation)
    {
        // ���Լ��·����� "Head" �߼�����
        GameObject headInstance = Instantiate(headPrefab, transform);
        headInstance.transform.localPosition = Vector3.zero;
        headInstance.transform.localRotation = initialRotation;

        // ��ȡ�������������
        snakeHead = headInstance.GetComponent<SnakeHead>();
        battleHead = headInstance.GetComponent<BattleHead>();

        // ��ȷ��֪��Щ��������ǲ��������
        snakeHead.isPlayer = false;
        battleHead.isPlayer = false;

        // ΪAI���ù̶����ٶ�ֵ
        snakeHead.walkSpeed = 2f;
        snakeHead.runSpeed = 4f;

        // ���Ҳ�������ҵ�BattleHead����
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // GetComponentInChildren ����Ұ����������ڵ������Ӷ���
            playerBattleHead = playerController.GetComponentInChildren<BattleHead>();
        }

        if (playerBattleHead == null)
        {
            Debug.LogError($"{gameObject.name}: AIδ���ҵ���ҵ�BattleHead�����޷����С�");
        }
    }

    void Start()
    {
        // ��Ϸ��ʼʱ������Ѳ����Ϊ
        StartBehavior(PatrolRoutine());
    }

    void Update()
    {
        // AI����Ҫ�����߼�ֻ��Ѳ��ʱ����
        if (currentState == AIState.Patrolling)
        {
            CheckForPlayer();
        }
    }

    // ����һ���µ���ΪЭ�̣�����ֹͣ�ɵģ�
    private void StartBehavior(IEnumerator newRoutine)
    {
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
        }
        currentBehaviorCoroutine = StartCoroutine(newRoutine);
    }

    #region ״̬��Ϊ (Coroutines)

    /// <summary>
    /// 1. Ѳ��״̬����Ϊ
    /// </summary>
    private IEnumerator PatrolRoutine()
    {
        currentState = AIState.Patrolling;
        Debug.Log($"{gameObject.name} ��ʼѲ�ߡ�");

        while (true) // ����ѭ��Ѳ��
        {
            // ��ȡ��ǰ�ߵĳ���
            int currentLength = battleHead.GetList().Count;

            // �������ʱ���ͳ���ϵ��
            float baseMoveDurationMin = 5f;
            float baseMoveDurationMax = 10f;
            float lengthMultiplier = 0.5f;

            // �����볤����صĶ�̬��Χ
            float dynamicMoveDurationMax = baseMoveDurationMax + (currentLength * lengthMultiplier);

            // �������Ѳ��ָ��
            float turnDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            float turnDuration = Random.Range(1f, 3f);
            float moveDuration = Random.Range(baseMoveDurationMin, dynamicMoveDurationMax);

            // --- ִ��ת�� ---
            float turnTimer = 0f;
            while (turnTimer < turnDuration)
            {
                snakeHead.SetAIInput(1f, turnDirection, false); // false = walkSpeed
                turnTimer += Time.deltaTime;
                yield return null;
            }

            // --- ִ��ֱ�� ---
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
    /// 2. ����������״̬����Ϊ
    /// </summary>
    private IEnumerator AssessRoutine()
    {
        currentState = AIState.Assessing;
        Debug.Log($"{gameObject.name} ������ң���������...");

        // ֹͣ�ƶ���ԭ�ط���
        snakeHead.SetAIInput(0f, 0f, false);

        // --- ʵ���Ա� ---
        int myLevel = battleHead.GetLevelForSlot(0); // AI��ͷ�ȼ�
        int playerLevel = playerBattleHead.GetLevelForSlot(0); // �����ͷ�ȼ�
        int myLength = battleHead.GetList().Count;
        int playerLength = playerBattleHead.GetList().Count;

        bool shouldChase = (myLevel >= playerLevel + 2) || (myLength >= playerLength + 2);

        if (shouldChase)
        {
            Debug.Log($"{gameObject.name} ����׷��������2��...");
            yield return new WaitForSeconds(2f);

            // �л���׷����Ϊ
            StartBehavior(ChaseRoutine());
        }
        else
        {
            Debug.Log($"{gameObject.name} ��Ϊ���̫ǿ������Ѳ�ߡ�");
            // ��Ϊ�򲻹������л���Ѳ��
            StartBehavior(PatrolRoutine());
        }
    }

    /// <summary>
    /// 3. ׷��״̬����Ϊ
    /// </summary>
    private IEnumerator ChaseRoutine()
    {
        currentState = AIState.Chasing;
        Debug.Log($"{gameObject.name} ��ʼ׷����");

        float chaseTimer = 0f;
        while (chaseTimer < chaseDuration)
        {
            // ÿ֡����Ŀ����ƶ�ָ��
            FindChaseTarget();

            // ��ȫУ�飺���Ŀ��ͻȻ��ʧ��
            if (chaseTarget == null && playerBattleHead.GetList().Count > 0)
            {
                chaseTarget = playerBattleHead.GetList()[0]; // ���÷�����׷��ͷ
            }

            if (chaseTarget != null)
            {
                // ��ȡAI������ͷ��ʵʱλ��
                Transform aiHeadTransform = snakeHead.GetAllNodes()[0].transform;

                // ���㳯��Ŀ�������
                Vector3 directionToTarget = (chaseTarget.transform.position - aiHeadTransform.position).normalized;

                // ��������ͷǰ�������ļнǣ�������ת������ת
                float angle = Vector3.SignedAngle(aiHeadTransform.forward, directionToTarget, Vector3.up);
                float rotateInput = Mathf.Clamp(angle / 45f, -1f, 1f);

                // ����׷��ָ��
                snakeHead.SetAIInput(1f, rotateInput, true); // true = runSpeed
            }

            chaseTimer += Time.deltaTime;
            yield return null;
        }

        // �������л���Ѳ��
        Debug.Log($"{gameObject.name} �����ˣ�����Ѳ��״̬��");
        StartBehavior(PatrolRoutine());
    }

    #endregion

    #region AI��������

    // �������Ƿ���뾯����Χ
    private void CheckForPlayer()
    {
        // ��ȫУ��
        if (playerBattleHead == null || snakeHead.GetAllNodes().Count == 0) return;

        // 1. ��ȡAI�ߵ�������ͷTransform
        Transform aiHeadTransform = snakeHead.GetAllNodes()[0].transform;

        // 2. ��ȡ����ߵ���������ڵ��б�
        var playerNodes = playerBattleHead.GetList();
        if (playerNodes.Count == 0) return;

        // 3. ������ҵ����нڵ㣬���о�����
        foreach (var playerNode in playerNodes)
        {
            if (playerNode == null) continue; // ����İ�ȫ���

            if (Vector3.Distance(aiHeadTransform.position, playerNode.transform.position) <= awarenessRadius)
            {
                // ������ң���������״̬
                StartBehavior(AssessRoutine());
                return; // �ҵ�һ���͹���
            }
        }
    }

    // Ѱ��׷��Ŀ��
    private void FindChaseTarget()
    {
        if (playerBattleHead == null) return;
        var playerNodes = playerBattleHead.GetList();

        // ��̫�̣��޷��Ƚ�
        if (playerNodes.Count < 3)
        {
            chaseTarget = playerNodes.LastOrDefault(); // ׷��β
            return;
        }

        // �ӵڶ����ڵ㣨����1����ʼ�������������ڶ����ڵ�
        for (int i = 1; i < playerNodes.Count - 1; i++)
        {
            int currentDef = playerNodes[i].finalAttribute.Defense;
            int prevDef = playerNodes[i - 1].finalAttribute.Defense;
            int nextDef = playerNodes[i + 1].finalAttribute.Defense;

            if (currentDef < prevDef && currentDef < nextDef)
            {
                chaseTarget = playerNodes[i];
                return; // �ҵ���һ��������
            }
        }

        // ���û�ҵ�����׷��β
        chaseTarget = playerNodes.Last();
    }

    #endregion
}