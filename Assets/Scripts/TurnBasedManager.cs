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




    void EndBattle()
    {
        if (currentState == BattleState.WON) 
            Debug.Log("胜利");
        else if (currentState == BattleState.LOST) 
            Debug.Log("失败");

        // 返回主场景
    }


}