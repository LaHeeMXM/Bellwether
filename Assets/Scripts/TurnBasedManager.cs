using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        EndBattle();
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

        yield return new WaitForSeconds(1.5f); // ����ͣ��

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
            Vector3 oldPos = playerUnitModel.transform.position;
            Quaternion oldRot = playerUnitModel.transform.rotation;
            Destroy(playerUnitModel); 
            playerUnitModel = Instantiate(rescuingHeadData.unitPrefab, oldPos, oldRot); // ��ԭλ�������µ���ͷģ��
            playerUnitModel.GetComponent<SheepAnimation>()?.EnterCombatState();
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
            Vector3 oldPos = enemyUnitModel.transform.position;
            Quaternion oldRot = enemyUnitModel.transform.rotation;
            Destroy(enemyUnitModel);
            enemyUnitModel = Instantiate(rescuingHeadData.unitPrefab, oldPos, oldRot);
            enemyUnitModel.GetComponent<SheepAnimation>()?.EnterCombatState();
        }

        // ��ʼ��������һ����ˢ������UI
        UIBattleManager.Instance.InitializeUI(playerBattleData, enemyBattleData);

        yield return new WaitForSeconds(2.0f); // ͣ�٣�����ҿ���仯

        // ˢ����ϣ���ս�����̽�������ǰ�غϵ��ж���
        currentState = isPlayerFirst ? BattleState.PLAYERTURN : BattleState.ENEMYTURN;
    }


    void EndBattle()
    {
        if (currentState == BattleState.WON) 
            Debug.Log("ʤ��");
        else if (currentState == BattleState.LOST) 
            Debug.Log("ʧ��");

        UIBattleManager.Instance.RestoreOriginalTimeScale();
        // ����������
    }


}