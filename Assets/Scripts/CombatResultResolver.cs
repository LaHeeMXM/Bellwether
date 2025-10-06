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
        // 监听场景加载事件，以便在每次回到主场景时都进行检查
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 只在回到主场景时执行
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
            Debug.Log("检测到战斗结果，开始处理...");

            // 如果没有正确的引用，则无法处理
            if (playerBattleHead == null || enemyBattleHeadInCombat == null)
            {
                Debug.LogError("无法处理战斗结果，因为参战双方未被正确注册！");
                CombatManager.Instance.ClearCombatResult();
                return;
            }

            var result = CombatManager.Instance.combatResult;
            var wasRescue = CombatManager.Instance.wasRescueActive;
            var defeatedNodeData = CombatManager.Instance.defeatedNodeData;

            switch (result)
            {
                case CombatResultType.PlayerWon:
                    // 1. 玩家升级
                    playerBattleHead.LevelUp();

                    // 2. 吞并逻辑
                    if (wasRescue) // 如果是击败了救援的蛇头
                    {
                        // 吞并整条蛇
                        foreach (var node in enemyBattleHeadInCombat.GetList())
                        {
                            playerBattleHead.AddSheep(node.sheepName);
                        }
                    }
                    else // 击败的是身体节点
                    {
                        int defeatedIndex = defeatedNodeData.Location;
                        // 从被击败的节点开始，吞并到蛇尾
                        for (int i = defeatedIndex; i < enemyBattleHeadInCombat.GetList().Count; i++)
                        {
                            playerBattleHead.AddSheep(enemyBattleHeadInCombat.GetList()[i].sheepName);
                        }
                    }

                    // 3. 移除整条敌人蛇
                    Destroy(enemyBattleHeadInCombat.transform.parent.gameObject); // 销毁Enemy父对象
                    break;

                case CombatResultType.EnemyWon:
                    // 1. 敌人升级 (虽然马上被销毁，但可以保留这个逻辑)
                    enemyBattleHeadInCombat.LevelUp();

                    // 2. 玩家被吞并逻辑
                    if (wasRescue)
                    {
                        // 整条蛇被吞并 (实际上是游戏结束，但逻辑上是这样)
                    }
                    else
                    {
                        int defeatedIndex = defeatedNodeData.Location;
                        // 玩家从被击败的节点开始，到蛇尾的所有单位被移除
                        playerBattleHead.ClearNodes(defeatedIndex);
                    }

                    // 3. 移除整条敌人蛇
                    Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                    break;

                case CombatResultType.PlayerAnnihilated:
                    Debug.Log("玩家全军覆没！游戏结束。");
                    HandleGameOver(); // 调用游戏结束方法
                    break;
            }

            // 清理战报，并清空引用
            CombatManager.Instance.ClearCombatResult();
            playerBattleHead = null;
            enemyBattleHeadInCombat = null;
        }
    }

    private void HandleGameOver()
    {
        // 在这里实现你的游戏结束逻辑
        // 比如：显示结束UI，暂停游戏，提供重新开始的按钮等
        Debug.Log("--- GAME OVER ---");
        Time.timeScale = 0; // 简单地暂停游戏
    }
}
