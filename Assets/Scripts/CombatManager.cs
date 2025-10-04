using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    // 玩家蛇头
    public CombatantData playerHeadData;
    // 敌人蛇头
    public CombatantData enemyHeadData;
    // 被攻击的身体节点数据
    public CombatantData targetNodeData;


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

        SceneManager.LoadScene("CombatScene");
    }
}