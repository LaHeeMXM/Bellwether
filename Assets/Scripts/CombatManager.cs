using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum CombatResultType { PlayerWon, EnemyWon, PlayerAnnihilated }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    // �����ͷ
    public CombatantData playerHeadData;
    // ������ͷ
    public CombatantData enemyHeadData;
    // ������������ڵ�����
    public CombatantData targetNodeData;


    // ս���������
    public bool hasCombatResult = false; // �Ƿ��д������ս�����
    public CombatResultType combatResult;
    public bool wasRescueActive; // ս������ʱ����Ԯ�Ƿ��ѵִ�
    public CombatantData defeatedNodeData; // �����ܵĽڵ��ԭʼ����

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
        // ����ѡ���Ե������������
        this.playerHeadData = null;
        this.enemyHeadData = null;
        this.targetNodeData = null;
    }
}