using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatResultResolver : MonoBehaviour
{
    public static CombatResultResolver Instance;

    private BattleHead playerBattleHead;
    private BattleHead enemyBattleHeadInCombat;


    [Header("ս����ȴ����")]
    [Tooltip("ս����������ײʧЧ�ĳ���ʱ�䣨�룩")]
    public float combatCooldownDuration = 1.5f;

    public bool IsCombatCooldownActive { get; private set; } = false;

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

    public void CheckAndResolveCombat()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.hasCombatResult)
        {
            StartCombatCooldown();

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
                    Debug.Log("���ʤ����");

                    // 1. �������
                    playerBattleHead.LevelUp();

                    // 2. �̲��߼�
                    if (wasRescue) // �����˾�Ԯ����ͷ
                    {
                        Debug.Log("�����˵з���ͷ���̲������ߣ�");
                        // �̲�������
                        foreach (var node in enemyBattleHeadInCombat.GetList())
                        {
                            playerBattleHead.AddSheep(node.buffName);
                        }
                        // �����̲�����ȫ��ʧ
                        Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                    }
                    else // ���ܵ�������ڵ�
                    {
                        int defeatedIndex = defeatedNodeData.Location;
                        Debug.Log($"�����˵з�λ�� {defeatedIndex} �Ľڵ㣬��ʼ�̲���벿�֡�");

                        // �ӱ����ܵĽڵ㿪ʼ���̲�����β
                        var enemyList = enemyBattleHeadInCombat.GetList();
                        // Ϊ�˰�ȫ�����Ǹ���һ�������б��ٲ���
                        var namesToSteal = new System.Collections.Generic.List<string>();
                        for (int i = defeatedIndex; i < enemyList.Count; i++)
                        {
                            namesToSteal.Add(enemyList[i].buffName);
                        }
                        foreach (var sheepName in namesToSteal)
                        {
                            playerBattleHead.AddSheep(sheepName);
                        }

                        enemyBattleHeadInCombat.ClearNodes(defeatedIndex);

                        // ���ն�Ϻ�������Ѿ����ˣ������ľ�����ͷ�����Ǿ�������
                        if (enemyBattleHeadInCombat.GetList().Count == 0)
                        {
                            Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                        }
                    }
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
                    Debug.Log("ȫ����û����Ϸ������");
                    HandleGameOver();
                    // ͬ��������Ҳ��ʧ
                    if (enemyBattleHeadInCombat != null)
                    {
                        Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                    }
                    break;
            }


            CombatManager.Instance.ResetTransitionLock();

            // ����ս�������������
            CombatManager.Instance.ClearCombatResult();
            playerBattleHead = null;
            enemyBattleHeadInCombat = null;
        }
    }

    private void StartCombatCooldown()
    {
        // ֹͣ�κο����������еľɵ���ȴ��ʱ��
        StopAllCoroutines();
        // �����µ���ȴ��ʱ��Э��
        StartCoroutine(CombatCooldownCoroutine());
    }

    private IEnumerator CombatCooldownCoroutine()
    {
        IsCombatCooldownActive = true;
        Debug.Log($"ս����ȴ��ʼ������ {combatCooldownDuration} �롣");

      
        yield return new WaitForSeconds(combatCooldownDuration);

        IsCombatCooldownActive = false;
        Debug.Log("ս����ȴ�����������ٴδ���ս����");
    }



    public void HandleGameOver()
    {
        // ������ʵ�������Ϸ�����߼�
        // ���磺��ʾ����UI����ͣ��Ϸ���ṩ���¿�ʼ�İ�ť��
        Debug.Log("--- GAME OVER ---");
        Time.timeScale = 0; // �򵥵���ͣ��Ϸ
    }

}
