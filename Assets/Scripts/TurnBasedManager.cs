using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TurnBasedManager : MonoBehaviour
{
    public static TurnBasedManager Instance;

    private GameObject playerUnitModel;
    private GameObject enemyUnitModel;
    private bool isPlayerFirst;

    private class BattleUnit
    {
        public string unitName;
        public int Health;
        public int Attack;
        public int Defense;
    }
    private BattleUnit playerBattleData;
    private BattleUnit enemyBattleData;



    private enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, PAUSED }
    private BattleState currentState;

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

        // attackerModel.GetComponent<Animator>().SetTrigger("Attack");

        yield return new WaitForSeconds(1f); // 等动画


        int damageDealt = Mathf.Max(1, attacker.Attack - defender.Defense);
        defender.Health -= damageDealt;


        // 受击或死亡动画

        yield return new WaitForSeconds(1f); // 回合间停顿
    }

    void EndBattle()
    {
        if (currentState == BattleState.WON) 
            Debug.Log("胜利");
        else if (currentState == BattleState.LOST) 
            Debug.Log("失败");

        // 返回主场景
    }


}