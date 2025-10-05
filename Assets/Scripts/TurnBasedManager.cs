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

    private bool isRescuePossible = false; // 本场战斗是否可能触发救援
    private int rescueDistance;            // 救援蛇头剩余的“路程”
    private float turnCounter = 0.5f;           // 用于计算救援速度的回合计数器

    [Header("救援动画设置")]
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

        //在战斗开始前，通知两个单位进入战斗状态
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


        // - 救援机制初始化 -

        CombatantData targetNodeData = CombatManager.Instance.targetNodeData;

        if (targetNodeData.Location > 0)
        {
            isRescuePossible = true;
            rescueDistance = targetNodeData.Location;
            turnCounter = 0; // 重置回合计数器
            Debug.Log("救援机制已激活！初始距离: " + rescueDistance);
        }
        else
        {
            isRescuePossible = false;
            Debug.Log("双方蛇头交战，不触发救援机制。");
        }

        UIBattleManager.Instance.InitializeSupportUI(isRescuePossible, CombatManager.Instance.isTargetNodePlayer, rescueDistance);

        // - 救援机制初始化结束 -

        currentState = BattleState.START;
        StartCoroutine(BattleFlow());
    }

    IEnumerator BattleFlow()
    {
        Debug.Log("回合开始");
        yield return new WaitForSeconds(1f);

        // 主动攻击则先手
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
        // --- 攻击方动画 ---

        Debug.Log(attacker.unitName + " 发起攻击!");
        SheepAnimation attackerAnim = attackerModel.GetComponent<SheepAnimation>();

        if (attackerAnim != null)
        {
            attackerAnim.PlayAttack();
        }

        yield return new WaitForSeconds(0.5f);


        // --- 伤害结算 ---

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

        Debug.Log(defender.unitName + " 受到 " + damageDealt + " 伤害, 剩余生命: " + defender.Health);


        // --- 受击方动画 ---

        SheepAnimation defenderAnim = defenderModel.GetComponent<SheepAnimation>();

        if (defenderAnim != null)
        {
            // 判断是播放死亡还是受击动画
            if (defender.Health <= 0)
            {
                Debug.Log(defender.unitName + " 被击败了!");
                defenderAnim.PlayDeath();
            }
            else
            {
                defenderAnim.PlayHitReaction();
            }
        }

        // 等动画
        yield return new WaitForSeconds(1.0f);

        // 回合间停顿
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
        currentState = BattleState.PAUSED; // 暂停战斗流程以进行刷新

        yield return new WaitForSeconds(1f); // 留出停顿

        // 判断是哪一方被救援
        bool isPlayerSideRescued = CombatManager.Instance.isTargetNodePlayer;

        if (isPlayerSideRescued)
        {
            Debug.Log("玩家的蛇头抵达战场！");
            // 获取待命的玩家蛇头数据
            CombatantData rescuingHeadData = CombatManager.Instance.playerHeadData;

            // --- 数据刷新 ---
            playerBattleData.unitName = rescuingHeadData.unitName;
            playerBattleData.Health = rescuingHeadData.Health; // 刷新为满血
            playerBattleData.Attack = rescuingHeadData.Attack + rescuingHeadData.Assistance;
            playerBattleData.Defense = rescuingHeadData.Defense;

            // --- 模型刷新 ---
            yield return StartCoroutine(AnimateModelSwap(playerUnitModel, rescuingHeadData.unitPrefab, (newModel) => {
                playerUnitModel = newModel; // 更新我们主要的模型引用
            }));
        }
        else // 敌人方被救援
        {
            Debug.Log("敌人的蛇头抵达战场！");
            // 获取待命的敌人蛇头数据
            CombatantData rescuingHeadData = CombatManager.Instance.enemyHeadData;

            // --- 数据刷新 ---
            enemyBattleData.unitName = rescuingHeadData.unitName;
            enemyBattleData.Health = rescuingHeadData.Health;
            enemyBattleData.Attack = rescuingHeadData.Attack + rescuingHeadData.Assistance;
            enemyBattleData.Defense = rescuingHeadData.Defense;

            // --- 模型刷新 ---
            yield return StartCoroutine(AnimateModelSwap(enemyUnitModel, rescuingHeadData.unitPrefab, (newModel) => {
                enemyUnitModel = newModel; // 更新我们主要的模型引用
            }));
        }

        // 初始化函数来一次性刷新所有UI
        UIBattleManager.Instance.InitializeUI(playerBattleData, enemyBattleData);

        yield return new WaitForSeconds(2.0f); // 停顿，让玩家看清变化

        // 刷新完毕，将战斗流程交还给当前回合的行动方
        currentState = isPlayerFirst ? BattleState.PLAYERTURN : BattleState.ENEMYTURN;
    }

    private IEnumerator AnimateModelSwap(GameObject oldModel, GameObject newPrefab, Action<GameObject> onComplete)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = oldModel.transform.position;
        Quaternion startRotation = oldModel.transform.rotation; // 记录原始朝向

        GameObject currentModel = oldModel;
        GameObject newModelInstance = null;
        bool hasSwapped = false;

        while (elapsedTime < swapAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / swapAnimationDuration);

            // --- 平滑缓动的进度值 (用于旋转) ---
            // SmoothStep会让进度在开始和结束时速度为0，中间最快
            float easedProgress = Mathf.SmoothStep(0, 1, progress);

            // --- 计算位置 (跳跃弧线，本身就是平滑的) ---
            float jumpHeight = Mathf.Sin(progress * Mathf.PI) * swapJumpHeight;
            currentModel.transform.position = startPosition + new Vector3(0, jumpHeight, 0);

            // --- 计算相对旋转 ---
            float rotationY = easedProgress * 360f;
            Quaternion spinRotation = Quaternion.Euler(0, rotationY, 0);
            currentModel.transform.rotation = startRotation * spinRotation;

            // --- 切换模型 ---
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

        // --- 清理工作 ---
        currentModel.transform.position = startPosition;
        currentModel.transform.rotation = startRotation;

        onComplete?.Invoke(newModelInstance);
    }



    void EndBattle()
    {
        if (currentState == BattleState.WON) 
            Debug.Log("胜利");
        else if (currentState == BattleState.LOST) 
            Debug.Log("失败");

        UIBattleManager.Instance.RestoreOriginalTimeScale();
        // 返回主场景
    }


}