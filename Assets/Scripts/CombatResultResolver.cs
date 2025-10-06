using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatResultResolver : MonoBehaviour
{
    public static CombatResultResolver Instance;

    private BattleHead playerBattleHead;
    private BattleHead enemyBattleHeadInCombat;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // �������������¼����Ա���ÿ�λص�������ʱ�����м��
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ֻ�ڻص�������ʱִ��
        if (scene.name == "MainScene")
        {
            CheckAndResolveCombat();
        }
    }


    public void RegisterCombatants(BattleHead player, BattleHead enemy)
    {
        this.playerBattleHead = player;
        this.enemyBattleHeadInCombat = enemy;
    }

    private void CheckAndResolveCombat()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.hasCombatResult)
        {
            Debug.Log("��⵽ս���������ʼ����...");

            // ���û����ȷ�����ã����޷�����
            if (playerBattleHead == null || enemyBattleHeadInCombat == null)
            {
                Debug.LogError("�޷�����ս���������Ϊ��ս˫��δ����ȷע�ᣡ");
                CombatManager.Instance.ClearCombatResult();
                return;
            }

            var result = CombatManager.Instance.combatResult;
            var wasRescue = CombatManager.Instance.wasRescueActive;
            var defeatedNodeData = CombatManager.Instance.defeatedNodeData;

            switch (result)
            {
                case CombatResultType.PlayerWon:
                    // 1. �������
                    playerBattleHead.LevelUp();

                    // 2. �̲��߼�
                    if (wasRescue) // ����ǻ����˾�Ԯ����ͷ
                    {
                        // �̲�������
                        foreach (var node in enemyBattleHeadInCombat.GetList())
                        {
                            playerBattleHead.AddSheep(node.sheepName);
                        }
                    }
                    else // ���ܵ�������ڵ�
                    {
                        int defeatedIndex = defeatedNodeData.Location;
                        // �ӱ����ܵĽڵ㿪ʼ���̲�����β
                        for (int i = defeatedIndex; i < enemyBattleHeadInCombat.GetList().Count; i++)
                        {
                            playerBattleHead.AddSheep(enemyBattleHeadInCombat.GetList()[i].sheepName);
                        }
                    }

                    // 3. �Ƴ�����������
                    Destroy(enemyBattleHeadInCombat.transform.parent.gameObject); // ����Enemy������
                    break;

                case CombatResultType.EnemyWon:
                    // 1. �������� (��Ȼ���ϱ����٣������Ա�������߼�)
                    enemyBattleHeadInCombat.LevelUp();

                    // 2. ��ұ��̲��߼�
                    if (wasRescue)
                    {
                        // �����߱��̲� (ʵ��������Ϸ���������߼���������)
                    }
                    else
                    {
                        int defeatedIndex = defeatedNodeData.Location;
                        // ��Ҵӱ����ܵĽڵ㿪ʼ������β�����е�λ���Ƴ�
                        playerBattleHead.ClearNodes(defeatedIndex);
                    }

                    // 3. �Ƴ�����������
                    Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                    break;

                case CombatResultType.PlayerAnnihilated:
                    Debug.Log("���ȫ����û����Ϸ������");
                    HandleGameOver(); // ������Ϸ��������
                    break;
            }

            // ����ս�������������
            CombatManager.Instance.ClearCombatResult();
            playerBattleHead = null;
            enemyBattleHeadInCombat = null;
        }
    }

    private void HandleGameOver()
    {
        // ������ʵ�������Ϸ�����߼�
        // ���磺��ʾ����UI����ͣ��Ϸ���ṩ���¿�ʼ�İ�ť��
        Debug.Log("--- GAME OVER ---");
        Time.timeScale = 0; // �򵥵���ͣ��Ϸ
    }
}
