using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    // �����ͷ
    public CombatantData playerHeadData;
    // ������ͷ
    public CombatantData enemyHeadData;
    // ������������ڵ�����
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