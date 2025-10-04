using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    public Transform playPoint;
    public Transform enemyPoint; // 被攻击的节点生成在这里

    void Start()
    {

        CombatantData playerHeadData = CombatManager.Instance.playerHeadData;
        CombatantData enemyHeadData = CombatManager.Instance.enemyHeadData;
        CombatantData targetNodeData = CombatManager.Instance.targetNodeData;
        bool isTargetPlayer = CombatManager.Instance.isTargetNodePlayer;


        CombatantData playerDataForBattle = isTargetPlayer ? targetNodeData : playerHeadData;
        CombatantData enemyDataForBattle = isTargetPlayer ? enemyHeadData : targetNodeData;


        GameObject playerUnit = Instantiate(playerDataForBattle.unitPrefab, playPoint.position, playPoint.rotation);
        GameObject enemyUnit = Instantiate(enemyDataForBattle.unitPrefab, enemyPoint.position, enemyPoint.rotation);


        TurnBasedManager.Instance.StartBattle(playerUnit, playerDataForBattle, enemyUnit, enemyDataForBattle, !isTargetPlayer);

    }
}
