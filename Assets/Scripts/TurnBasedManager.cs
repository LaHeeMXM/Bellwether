using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnBasedManager : MonoBehaviour
{
    public static TurnBasedManager Instance;

    public  GameObject playerUnitModel;
    public  GameObject enemyUnitModel;
    public  bool isPlayerFirst;

    [System.Serializable]
    public  class BattleUnit
    {
        public string unitName;
        public int Health;
        public int Attack;
        public int Defense;
    }
    public  BattleUnit playerBattleData;
    public  BattleUnit enemyBattleData;

    public  enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, PAUSED }
    public  BattleState currentState;

    private bool isRescuePossible = false; // ����ս���Ƿ���ܴ�����Ԯ
    private int rescueDistance;            // ��Ԯ��ͷʣ��ġ�·�̡�
    private float turnCounter = 0.5f;           // ���ڼ����Ԯ�ٶȵĻغϼ�����
    private bool wasRescueTriggered = false;

    [Header("��Ԯ��������")]
    public float swapAnimationDuration = 1.5f;
    public float swapJumpHeight = 2.0f;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void StartBattle(GameObject playerModel, CombatantData playerData, GameObject enemyModel, CombatantData enemyData, bool isPlayerFirst)
    {
        this.isPlayerFirst = isPlayerFirst;

        this.playerUnitModel = playerModel;
        this.enemyUnitModel = enemyModel;

        //��ս����ʼǰ��֪ͨ������λ����ս��״̬
        this.playerUnitModel.GetComponent<SheepAnimation>()?.EnterCombatState();
        this.enemyUnitModel.GetComponent<SheepAnimation>()?.EnterCombatState();

        this.playerBattleData = new BattleUnit
        {
            unitName = playerData.unitName,
            Health = playerData.Health,
            Attack = playerData.Attack + playerData.Assistance, 
            Defense = playerData.Defense
        };

        this.enemyBattleData = new BattleUnit
        {
            unitName = enemyData.unitName,
            Health = enemyData.Health,
            Attack = enemyData.Attack + enemyData.Assistance,
            Defense = enemyData.Defense
        };

        UIBattleManager.Instance.InitializeUI(playerBattleData, enemyBattleData);


        // - ��Ԯ���Ƴ�ʼ�� -

        CombatantData targetNodeData = CombatManager.Instance.targetNodeData;

        if (targetNodeData.Location > 0)
        {
            isRescuePossible = true;
            rescueDistance = targetNodeData.Location;
            turnCounter = 0; // ���ûغϼ�����
            Debug.Log("��Ԯ�����Ѽ����ʼ����: " + rescueDistance);
        }
        else
        {
            isRescuePossible = false;
            Debug.Log("˫����ͷ��ս����������Ԯ���ơ�");
        }

        UIBattleManager.Instance.InitializeSupportUI(isRescuePossible, CombatManager.Instance.isTargetNodePlayer, rescueDistance);

        // - ��Ԯ���Ƴ�ʼ������ -

        currentState = BattleState.START;
        StartCoroutine(BattleFlow());
    }

    IEnumerator BattleFlow()
    {

        Debug.Log("�غϿ�ʼ");
        yield return new WaitForSeconds(1f);

        // ��������������
        currentState = isPlayerFirst? BattleState.PLAYERTURN : BattleState.ENEMYTURN;

        wasRescueTriggered = false;

        while (currentState != BattleState.WON && currentState != BattleState.LOST)
        {
            if (currentState == BattleState.PAUSED)
            {
                yield return null;
                continue;
            }

            if (currentState == BattleState.PLAYERTURN)
            {
                yield return StartCoroutine(AttackRoutine(playerBattleData, enemyBattleData, playerUnitModel, enemyUnitModel));
                if (enemyBattleData.Health <= 0) currentState = BattleState.WON;
                else currentState = BattleState.ENEMYTURN;
            }
            else if (currentState == BattleState.ENEMYTURN)
            {
                yield return StartCoroutine(AttackRoutine(enemyBattleData, playerBattleData, enemyUnitModel, playerUnitModel));
                if (playerBattleData.Health <= 0) currentState = BattleState.LOST;
                else currentState = BattleState.PLAYERTURN;
            }

            if (isRescuePossible)
            {
                turnCounter += 0.5f;

                int rescueSpeed = Mathf.CeilToInt(turnCounter);

                if (turnCounter % 1 == 0)
                {
                    rescueDistance -= rescueSpeed;
                    UIBattleManager.Instance.UpdateSupportTurnCount((int)turnCounter);

                    if (rescueDistance <= 0)
                    {
                        yield return StartCoroutine(TriggerRescue());
                        isRescuePossible = false;
                    }
                }
            }
        }

        StartCoroutine(EndBattleCoroutine());
    }

    IEnumerator AttackRoutine(BattleUnit attacker, BattleUnit defender, GameObject attackerModel, GameObject defenderModel)
    {
        // --- ���������� ---

        Debug.Log(attacker.unitName + " ���𹥻�!");
        SheepAnimation attackerAnim = attackerModel.GetComponent<SheepAnimation>();

        if (attackerAnim != null)
        {
            attackerAnim.PlayAttack();
        }

        yield return new WaitForSeconds(0.5f);


        // --- �˺����� ---

        int damageDealt = Mathf.Max(1, attacker.Attack - defender.Defense);
        defender.Health -= damageDealt;

        if (defender == playerBattleData)
        {
            UIBattleManager.Instance.OnDataChange(UIBattleManager.Instance.playerHealthText, defender.Health);
        }
        else
        {
            UIBattleManager.Instance.OnDataChange(UIBattleManager.Instance.enemyHealthText, defender.Health);
        }

        Debug.Log(defender.unitName + " �ܵ� " + damageDealt + " �˺�, ʣ������: " + defender.Health);


        // --- �ܻ������� ---

        SheepAnimation defenderAnim = defenderModel.GetComponent<SheepAnimation>();

        if (defenderAnim != null)
        {
            // �ж��ǲ������������ܻ�����
            if (defender.Health <= 0)
            {
                Debug.Log(defender.unitName + " ��������!");
                defenderAnim.PlayDeath();
            }
            else
            {
                defenderAnim.PlayHitReaction();
            }
        }

        // �ȶ���
        yield return new WaitForSeconds(1.0f);

        // �غϼ�ͣ��
        yield return new WaitForSeconds(0.5f);

    }

    //public void ChangeAttack(BattleUnit targetUnit, int newAttackValue)
    //{
    //    targetUnit.Attack = newAttackValue;

    //    if (targetUnit == playerBattleData)
    //    {
    //        UIBattleManager.Instance.OnDataChange(UIBattleManager.Instance.playerAttackText, newAttackValue);
    //    }
    //    else
    //    {
    //        UIBattleManager.Instance.OnDataChange(UIBattleManager.Instance.enemyAttackText, newAttackValue);
    //    }
    //}

    IEnumerator TriggerRescue()
    {
        UIBattleManager.Instance.ShowArrivalMessage();
        currentState = BattleState.PAUSED; // ��ͣս�������Խ���ˢ��
        wasRescueTriggered = true;

        yield return new WaitForSeconds(1f); // ����ͣ��

        // �ж�����һ������Ԯ
        bool isPlayerSideRescued = CombatManager.Instance.isTargetNodePlayer;

        if (isPlayerSideRescued)
        {
            Debug.Log("��ҵ���ͷ�ִ�ս����");
            // ��ȡ�����������ͷ����
            CombatantData rescuingHeadData = CombatManager.Instance.playerHeadData;

            // --- ����ˢ�� ---
            playerBattleData.unitName = rescuingHeadData.unitName;
            playerBattleData.Health = rescuingHeadData.Health; // ˢ��Ϊ��Ѫ
            playerBattleData.Attack = rescuingHeadData.Attack + rescuingHeadData.Assistance;
            playerBattleData.Defense = rescuingHeadData.Defense;

            // --- ģ��ˢ�� ---
            yield return StartCoroutine(AnimateModelSwap(playerUnitModel, rescuingHeadData.unitPrefab, (newModel) => {
                playerUnitModel = newModel; // ����������Ҫ��ģ������
            }));
        }
        else // ���˷�����Ԯ
        {
            Debug.Log("���˵���ͷ�ִ�ս����");
            // ��ȡ�����ĵ�����ͷ����
            CombatantData rescuingHeadData = CombatManager.Instance.enemyHeadData;

            // --- ����ˢ�� ---
            enemyBattleData.unitName = rescuingHeadData.unitName;
            enemyBattleData.Health = rescuingHeadData.Health;
            enemyBattleData.Attack = rescuingHeadData.Attack + rescuingHeadData.Assistance;
            enemyBattleData.Defense = rescuingHeadData.Defense;

            // --- ģ��ˢ�� ---
            yield return StartCoroutine(AnimateModelSwap(enemyUnitModel, rescuingHeadData.unitPrefab, (newModel) => {
                enemyUnitModel = newModel; // ����������Ҫ��ģ������
            }));
        }

        // ��ʼ��������һ����ˢ������UI
        UIBattleManager.Instance.InitializeUI(playerBattleData, enemyBattleData);

        yield return new WaitForSeconds(2.0f); // ͣ�٣�����ҿ���仯

        // ˢ����ϣ���ս�����̽�������ǰ�غϵ��ж���
        currentState = isPlayerFirst ? BattleState.PLAYERTURN : BattleState.ENEMYTURN;
    }

    private IEnumerator AnimateModelSwap(GameObject oldModel, GameObject newPrefab, Action<GameObject> onComplete)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = oldModel.transform.position;
        Quaternion startRotation = oldModel.transform.rotation; // ��¼ԭʼ����

        GameObject currentModel = oldModel;
        GameObject newModelInstance = null;
        bool hasSwapped = false;

        while (elapsedTime < swapAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / swapAnimationDuration);

            // --- ƽ�������Ľ���ֵ (������ת) ---
            // SmoothStep���ý����ڿ�ʼ�ͽ���ʱ�ٶ�Ϊ0���м����
            float easedProgress = Mathf.SmoothStep(0, 1, progress);

            // --- ����λ�� (��Ծ���ߣ��������ƽ����) ---
            float jumpHeight = Mathf.Sin(progress * Mathf.PI) * swapJumpHeight;
            currentModel.transform.position = startPosition + new Vector3(0, jumpHeight, 0);

            // --- ���������ת ---
            float rotationY = easedProgress * 360f;
            Quaternion spinRotation = Quaternion.Euler(0, rotationY, 0);
            currentModel.transform.rotation = startRotation * spinRotation;

            // --- �л�ģ�� ---
            if (progress >= 0.5f && !hasSwapped)
            {
                hasSwapped = true;
                Vector3 swapPosition = currentModel.transform.position;
                Quaternion swapRotation = currentModel.transform.rotation;
                Destroy(oldModel);
                newModelInstance = Instantiate(newPrefab, swapPosition, swapRotation);
                newModelInstance.GetComponent<SheepAnimation>()?.EnterCombatState();
                currentModel = newModelInstance;
            }

            yield return null;
        }

        // --- ������ ---
        currentModel.transform.position = startPosition;
        currentModel.transform.rotation = startRotation;

        onComplete?.Invoke(newModelInstance);
    }



    IEnumerator EndBattleCoroutine()
    {
        CombatResultType result;
        CombatantData defeatedNode = null;
        bool playerLostAllNodes = false;

        if (currentState == BattleState.WON)
        {
            Debug.Log("ʤ��");
            result = CombatResultType.PlayerWon;
            // �����ܵ��ǵ��ˣ�������Ҫ����ԭʼ����
            // ������˱���Ԯ����ô�����ܵ��ǵ�����ͷ�������Ǳ������Ľڵ�
            defeatedNode = wasRescueTriggered ? CombatManager.Instance.enemyHeadData : CombatManager.Instance.targetNodeData;
        }
        else // LOST
        {
            Debug.Log("ʧ��");
            result = CombatResultType.EnemyWon;
            defeatedNode = wasRescueTriggered ? CombatManager.Instance.playerHeadData : CombatManager.Instance.targetNodeData;

            // �������Ƿ�ȫ����û (ֻ���������ͷ��ս��ʧ��ʱ)
            bool isPlayerNodeTarget = CombatManager.Instance.isTargetNodePlayer;
            if ((!isPlayerNodeTarget && wasRescueTriggered) || (isPlayerNodeTarget && !wasRescueTriggered && defeatedNode.Location == 0))
            {
                // �����жϣ�1.����͵Ϯ��ң���Ԯ�ִ�������ͷս�ܡ� 2.���͵Ϯ�������壬������ɱ�����Ǹ�����պ�����ͷ
                // ���򵥵��жϣ������ܵĽڵ��������ͷ
                if (defeatedNode == CombatManager.Instance.playerHeadData)
                {
                    playerLostAllNodes = true;
                    result = CombatResultType.PlayerAnnihilated;
                }
            }
        }

        Debug.Log("ս���������ȴ�3��󷵻�������...");
        yield return new WaitForSeconds(3.0f);

        // ������ʹ������ս������
        UIBattleManager.Instance.RestoreOriginalTimeScale(); // ȷ��ʱ��ָ�����
        CombatManager.Instance.EndCombat(result, wasRescueTriggered, defeatedNode);
    }


}