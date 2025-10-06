using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatResultResolver : MonoBehaviour
{
    public static CombatResultResolver Instance;

    private BattleHead playerBattleHead;
    private BattleHead enemyBattleHeadInCombat;


    [Header("战斗冷却设置")]
    [Tooltip("战斗结束后，碰撞失效的持续时间（秒）")]
    public float combatCooldownDuration = 1.5f;

    [Header("游戏结束设置")]
    [Tooltip("Game Over界面显示的UI Panel")]
    public GameObject gameOverPanel;
    [Tooltip("显示Game Over界面后，等待多久再重载场景（秒）")]
    public float gameOverReloadDelay = 3.0f;

    public bool IsCombatCooldownActive { get; private set; } = false;

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

    public void CheckAndResolveCombat()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.hasCombatResult)
        {
            StartCombatCooldown();

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
                    Debug.Log("玩家胜利！");

                    // 1. 玩家升级
                    playerBattleHead.LevelUp();

                    // 2. 吞并逻辑
                    if (wasRescue) // 击败了救援的蛇头
                    {
                        Debug.Log("击败了敌方蛇头，吞并整条蛇！");
                        // 吞并整条蛇
                        foreach (var node in enemyBattleHeadInCombat.GetList())
                        {
                            playerBattleHead.AddSheep(node.buffName);
                        }
                        // 整条吞并，完全消失
                        Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                    }
                    else // 击败的是身体节点
                    {
                        int defeatedIndex = defeatedNodeData.Location;
                        Debug.Log($"击败了敌方位于 {defeatedIndex} 的节点，开始吞并后半部分。");

                        // 从被击败的节点开始，吞并到蛇尾
                        var enemyList = enemyBattleHeadInCombat.GetList();
                        // 为了安全，我们复制一份名字列表再操作
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

                        // 如果斩断后敌人蛇已经空了（比如打的就是蛇头），那就销毁它
                        if (enemyBattleHeadInCombat.GetList().Count == 0)
                        {
                            Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                        }
                    }
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
                    Debug.Log("全军覆没！游戏结束。");
                    HandleGameOver();
                    // 同样，敌人也消失
                    if (enemyBattleHeadInCombat != null)
                    {
                        Destroy(enemyBattleHeadInCombat.transform.parent.gameObject);
                    }
                    break;
            }


            CombatManager.Instance.ResetTransitionLock();

            // 清理战报，并清空引用
            CombatManager.Instance.ClearCombatResult();
            playerBattleHead = null;
            enemyBattleHeadInCombat = null;
        }
    }

    private void StartCombatCooldown()
    {
        // 停止任何可能正在运行的旧的冷却计时器
        StopAllCoroutines();
        // 启动新的冷却计时器协程
        StartCoroutine(CombatCooldownCoroutine());
    }

    private IEnumerator CombatCooldownCoroutine()
    {
        IsCombatCooldownActive = true;
        Debug.Log($"战斗冷却开始，持续 {combatCooldownDuration} 秒。");

      
        yield return new WaitForSeconds(combatCooldownDuration);

        IsCombatCooldownActive = false;
        Debug.Log("战斗冷却结束，可以再次触发战斗。");
    }



    private void HandleGameOver()
    {
        Debug.Log("--- GAME OVER ---");
  
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        // 1. 显示 Game Over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("GameOverPanel未在CombatResultResolver中设置！");
        }

        // 2. 等待指定的秒数
        Debug.Log($"等待 {gameOverReloadDelay} 秒后重新加载游戏...");

        // ✨ 使用无视时间缩放的等待，以防万一Time.timeScale被设为0
        float timer = 0f;
        while (timer < gameOverReloadDelay)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        Application.Quit();
    }
}
