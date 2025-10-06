using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

public enum Rarity { N, R, S ,X }  //稀有度

[System.Serializable]

public class UnitSpawnInfo
{
    [Tooltip("Prefab引用")]
    public GameObject unitPrefab; 
    [Tooltip("单位的稀有度")]
    public Rarity rarity;
}

// 追踪已生成的敌人
public class ActiveEnemy
{
    public GameObject rootObject; // 敌人蛇的父对象 (EnemyController所在的那个)
    public BattleHead battleHead;

    public ActiveEnemy(GameObject root, BattleHead head)
    {
        rootObject = root;
        battleHead = head;
    }
}


public class GameManager : MonoBehaviour
{
    [Header("核心Prefabs")]
    public GameObject enemyControllerPrefab;
    public GameObject headLogicPrefab;

    [Header("单位配置池")]
    [Tooltip("在这里配置所有18种单位的Prefab和稀有度")]
    public List<UnitSpawnInfo> unitSpawnPool;

    [Header("刷新区域设置")]
    public float mapSize = 200f; // 地图边界的一半 (从-200到200)
    public float centerSafeRadius = 25f; // 中心安全区半径
    public float innerRingRadius = 100f; // 内环半径
    public float middleRingRadius = 150f; // 中环半径

    [Header("刷新数量与节奏")]
    public int initialEnemyCount = 25; // 初始敌人数量
    public int maxEnemyCount = 35; // 最大敌人数量
    public float respawnInterval = 30f; // 刷新间隔（秒）

    [Header("伏击机制")]
    [Range(0f, 1f)]
    public float ambushChance = 0.5f; // 50%概率在玩家附近刷新
    public float ambushDistance = 20f; // 在玩家周围20距离的位置

    private BattleHead playerBattleHead; // 玩家引用

    // --- 私有状态变量 ---
    private List<ActiveEnemy> activeEnemies = new List<ActiveEnemy>();
    private float respawnTimer = 0f;

    // 用于按稀有度分类的单位池，方便抽卡
    private List<GameObject> commonUnits; // N
    private List<GameObject> rareUnits;   // R
    private List<GameObject> superRareUnits; // S

    void Awake()
    {
        // 在游戏开始时，对单位池进行预处理和分类
        PreprocessUnitPool();
    }

    void Start()
    {
    }

    public void Initialize()
    {
        // 找到并缓存玩家引用
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerBattleHead = playerController.GetComponentInChildren<BattleHead>();
        }
        if (playerBattleHead == null)
        {
            Debug.LogError("GameManager未能找到玩家BattleHead，伏击机制将失效！");
        }

        // 执行初始敌人生成，不是伏击
        Debug.Log("GameManager: Initialize() 被调用，开始生成初始敌人...");
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnNewEnemy(false);
        }

        SpawnBoss();
    }

    void Update()
    {
        // 动态刷新逻辑
        if (activeEnemies.Count < maxEnemyCount)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnInterval)
            {
                respawnTimer = 0f;
                // 动态刷新时，有概率触发伏击
                bool attemptAmbush = Random.value < ambushChance;
                SpawnNewEnemy(attemptAmbush);
            }
        }

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            // 如果GameObject已经被销毁 (比如被CombatResultResolver销毁了)
            if (activeEnemies[i].rootObject == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    // --- 核心实现方法 ---

    private void PreprocessUnitPool()
    {
        commonUnits = unitSpawnPool.Where(u => u.rarity == Rarity.N).Select(u => u.unitPrefab).ToList();
        rareUnits = unitSpawnPool.Where(u => u.rarity == Rarity.R).Select(u => u.unitPrefab).ToList();
        superRareUnits = unitSpawnPool.Where(u => u.rarity == Rarity.S).Select(u => u.unitPrefab).ToList();

        if (commonUnits.Count == 0 || rareUnits.Count == 0 || superRareUnits.Count == 0)
        {
            Debug.LogError("单位配置池中缺少至少一种稀有度的单位！");
        }
    }

    private void SpawnNewEnemy(bool isAmbush)
    {
        // 1. 决定生成位置和所属区域
        Vector3 spawnPosition;

        // --- 决定生成位置 ---
        if (isAmbush && playerBattleHead != null && playerBattleHead.GetList().Count > 0)
        {
            // --- 伏击逻辑 ---
            Debug.Log("尝试在玩家附近刷新敌人...");
            Transform playerHeadTransform = playerBattleHead.GetComponent<SnakeHead>().GetAllNodes()[0].transform;
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 offset = new Vector3(randomDirection.x, 0, randomDirection.y) * ambushDistance;
            spawnPosition = playerHeadTransform.position + offset;

            // 确保不出界
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, -mapSize, mapSize);
            spawnPosition.z = Mathf.Clamp(spawnPosition.z, -mapSize, mapSize);
        }
        else
        {
            // --- 常规随机逻辑 ---
            spawnPosition = GetRandomSpawnPosition();
        }


        float distanceFromCenter = Vector3.Distance(spawnPosition, Vector3.zero);

        // 2. 根据区域决定蛇的长度和等级范围
        int minLength, maxLength, minLevel, maxLevel;

        if (distanceFromCenter <= innerRingRadius) // 内环 (最强)
        {
            minLength = 5; maxLength = 18;
            minLevel = 10; maxLevel = 35;
        }
        else if (distanceFromCenter <= middleRingRadius) // 中环
        {
            minLength = 3; maxLength = 8;
            minLevel = 4; maxLevel = 12;
        }
        else // 外环 (最弱)
        {
            minLength = 1; maxLength = 3;
            minLevel = 1; maxLevel = 5;
        }

        // 3. 随机确定最终的长度和等级
        int snakeLength = Random.Range(minLength, maxLength + 1);
        int snakeLevel = Random.Range(minLevel, maxLevel + 1);

        // 4. 根据区域选择单位池，并构建蛇的节点列表
        List<GameObject> bodyPrefabs = new List<GameObject>();
        for (int i = 0; i < snakeLength; i++)
        {
            bodyPrefabs.Add(GetRandomUnitPrefabByZone(distanceFromCenter));
        }

        // 5. 生成蛇
        Quaternion initialRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        SpawnEnemySnake(spawnPosition, initialRotation, bodyPrefabs, snakeLevel);
    }

    // 用于根据区域随机抽取单位
    private GameObject GetRandomUnitPrefabByZone(float distance)
    {
        // 定义不同区域的抽卡概率 (N, R, S)
        // 这些概率可以根据需要调整
        float n_chance = 0f, r_chance = 0f, s_chance = 0f;

        if (distance <= innerRingRadius) // 内环
        {
            n_chance = 0.5f; // 20%
            r_chance = 0.3f; // 50%
            s_chance = 0.2f; // 30%
        }
        else if (distance <= middleRingRadius) // 中环
        {
            n_chance = 0.6f; // 50%
            r_chance = 0.3f; // 40%
            s_chance = 0.1f; // 10%
        }
        else // 外环
        {
            n_chance = 0.8f; // 80%
            r_chance = 0.2f; // 20%
            s_chance = 0.0f; // 0%
        }

        float randomValue = Random.value; // 生成一个0到1的随机数

        if (randomValue < n_chance)
        {
            return commonUnits[Random.Range(0, commonUnits.Count)];
        }
        else if (randomValue < n_chance + r_chance)
        {
            return rareUnits[Random.Range(0, rareUnits.Count)];
        }
        else
        {
            // 确保S级单位池不为空
            if (superRareUnits.Count > 0)
                return superRareUnits[Random.Range(0, superRareUnits.Count)];
            else // 如果没有S级单位，则降级为R级
                return rareUnits[Random.Range(0, rareUnits.Count)];
        }
    }

    // 新增一个辅助方法，用于获取随机且安全的生成点
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 position;
        int attempts = 0;
        do
        {
            float x = Random.Range(-mapSize, mapSize);
            float z = Random.Range(-mapSize, mapSize);
            position = new Vector3(x, 0, z);
            attempts++;
            if (attempts > 50)
            {
                Debug.LogWarning("无法找到安全的生成点，可能地图太拥挤。");
                break;
            }
        }
        // 循环直到找到一个不在中心安全区的点
        while (Vector3.Distance(position, Vector3.zero) < centerSafeRadius);

        return position;
    }

    // 这是之前创建的生成逻辑，现在被参数化了
    private void SpawnEnemySnake(Vector3 position, Quaternion rotation, List<GameObject> nodePrefabs, int level)
    {
        GameObject enemyRoot = Instantiate(enemyControllerPrefab, position, rotation);
        enemyRoot.transform.SetParent(this.transform); // 将敌人作为GameManager的子对象，方便管理

        EnemyController enemyController = enemyRoot.GetComponent<EnemyController>();
        enemyController.Initialize(headLogicPrefab, rotation);

        foreach (var prefab in nodePrefabs)
        {
            // AddSheep接收的是Resources里的名字，我们需要从Prefab获取
            enemyController.battleHead.AddSheep(prefab.name);
        }

        for (int i = 0; i < level; i++)
        {
            enemyController.battleHead.LevelUp();
        }

        // 将新生成的敌人添加到活动列表中
        activeEnemies.Add(new ActiveEnemy(enemyRoot, enemyController.battleHead));
    }

    private void SpawnBoss()
    {
        // 1. 从单位池中找到Boss的配置信息
        UnitSpawnInfo bossInfo = unitSpawnPool.FirstOrDefault(u => u.rarity == Rarity.X);

        // 如果没有在Inspector中配置Boss，则不生成并给出提示
        if (bossInfo == null || bossInfo.unitPrefab == null)
        {
            Debug.LogWarning("单位配置池中未找到稀有度为 'X' 的Boss配置，Boss将不会生成。");
            return;
        }

        // 2. 根据您的要求，定义Boss的生成参数
        Vector3 bossPosition = Vector3.zero; // 固定位置 (0, 0)
        int bossLevel = 0;                   // 等级为 0

        // Boss只有一个个体，所以它的“身体”列表只包含它自己
        List<GameObject> bossBody = new List<GameObject> { bossInfo.unitPrefab };

        // 随机一个初始朝向
        Quaternion initialRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 3. 调用现有的生成框架来创建Boss
        Debug.Log("正在 (0,0) 位置生成Boss...");
        SpawnEnemySnake(bossPosition, initialRotation, bossBody, bossLevel);
    }

}