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


        if (isTargetPlayer)
        {
            GameObject playerUnit = Instantiate(targetNodeData.unitPrefab, playPoint.position, playPoint.rotation);
            GameObject enemyUnit = Instantiate(enemyHeadData.unitPrefab, enemyPoint.position, enemyPoint.rotation);
        }

        else
        {
            GameObject playerUnit = Instantiate(playerHeadData.unitPrefab, playPoint.position, playPoint.rotation);
            GameObject enemyUnit = Instantiate(targetNodeData.unitPrefab, enemyPoint.position, enemyPoint.rotation);
        }
    }
}
