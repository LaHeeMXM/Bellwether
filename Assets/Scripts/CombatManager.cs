using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum CombatResultType { PlayerWon, EnemyWon, PlayerAnnihilated }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    // 玩家蛇头
    public CombatantData playerHeadData;
    // 敌人蛇头
    public CombatantData enemyHeadData;
    // 被攻击的身体节点数据
    public CombatantData targetNodeData;


    // 战斗结果数据
    public bool hasCombatResult = false; // 是否有待处理的战斗结果
    public CombatResultType combatResult;
    public bool wasRescueActive; // 战斗结束时，救援是否已抵达
    public CombatantData defeatedNodeData; // 被击败的节点的原始数据

    public bool isTargetNodePlayer;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void StartCombat( 
        CombatantData playerHead,
        CombatantData enemyHead,
        CombatantData target,
        bool targetIsPlayer)
    {
        this.playerHeadData = playerHead;
        this.enemyHeadData = enemyHead;
        this.targetNodeData = target;
        this.isTargetNodePlayer = targetIsPlayer;

    }

    public void EndCombat(CombatResultType result, bool rescueActive, CombatantData defeatedNode)
    {
        this.hasCombatResult = true;
        this.combatResult = result;
        this.wasRescueActive = rescueActive;
        this.defeatedNodeData = defeatedNode;

    }

    public void ClearCombatResult()
    {
        this.hasCombatResult = false;
        // 可以选择性地清空其他数据
        this.playerHeadData = null;
        this.enemyHeadData = null;
        this.targetNodeData = null;
    }
}