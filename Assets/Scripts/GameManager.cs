using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable] 
public class EnemySnakeConfig
{
    [Tooltip("初始生成位置")]
    public Vector3 spawnPosition;
    [Tooltip("初始的身体节点列表（按从头到尾的顺序）")]
    public List<string> nodePrefabNames; // 我们用Prefab在Resources文件夹中的名字来配置
    [Tooltip("这条蛇所有节点的初始等级")]
    [Range(1, 10)]
    public int initialLevel = 1;
}


public class GameManager : MonoBehaviour
{
    [Header("核心Prefabs")]
    [Tooltip("代表'Enemy'父对象的Prefab，上面应该挂载EnemyController脚本")]
    public GameObject enemyControllerPrefab;
    [Tooltip("代表'Head'逻辑对象的Prefab，上面挂载SnakeHead和BattleHead")]
    public GameObject headLogicPrefab;

    [Header("敌人配置")]
    [Tooltip("配置你想要在场景中生成的所有敌人蛇")]
    public List<EnemySnakeConfig> enemySnakesToSpawn;

    void Start()
    {
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        foreach (var config in enemySnakesToSpawn)
        {
            SpawnEnemySnake(config);
        }
    }

    private void SpawnEnemySnake(EnemySnakeConfig config)
    {
        if (enemyControllerPrefab == null || headLogicPrefab == null)
        {
            Debug.LogError("GameManager缺少核心Prefabs的引用！");
            return;
        }

        // 创建 "Enemy" 父对象
        GameObject enemyRoot = Instantiate(enemyControllerPrefab, config.spawnPosition, Quaternion.identity);
        enemyRoot.name = "Enemy" + config.nodePrefabNames[0]; // 用蛇头名字来命名，方便调试

        // 获取EnemyController并初始化
        EnemyController enemyController = enemyRoot.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("EnemyControllerPrefab上没有挂载EnemyController脚本！");
            return;
        }
        enemyController.Initialize(headLogicPrefab);

        // 依次添加所有身体节点
        foreach (var sheepName in config.nodePrefabNames)
        {
            enemyController.battleHead.AddSheep(sheepName);
        }

        for (int i = 0; i < config.initialLevel; i++)
        {
            enemyController.battleHead.LevelUp();
        }


        Debug.Log($"成功生成一条总等级为 {config.initialLevel} 的敌人蛇。");
    }

}