using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable] 
public class EnemySnakeConfig
{
    [Tooltip("��ʼ����λ��")]
    public Vector3 spawnPosition;
    [Tooltip("��ʼ������ڵ��б�����ͷ��β��˳��")]
    public List<string> nodePrefabNames; // ������Prefab��Resources�ļ����е�����������
    [Tooltip("���������нڵ�ĳ�ʼ�ȼ�")]
    [Range(1, 10)]
    public int initialLevel = 1;
}

public enum Rarity { N, R, S ,X }  //ϡ�ж�

[System.Serializable]

public class UnitSpawnInfo
{
    [Tooltip("Prefab����")]
    public GameObject unitPrefab; 
    [Tooltip("��λ��ϡ�ж�")]
    public Rarity rarity;
}

// ׷�������ɵĵ���
public class ActiveEnemy
{
    public GameObject rootObject; // �����ߵĸ����� (EnemyController���ڵ��Ǹ�)
    public BattleHead battleHead;

    public ActiveEnemy(GameObject root, BattleHead head)
    {
        rootObject = root;
        battleHead = head;
    }
}


public class GameManager : MonoBehaviour
{
    [Header("����Prefabs")]
    public GameObject enemyControllerPrefab;
    public GameObject headLogicPrefab;

    [Header("��λ���ó�")]
    [Tooltip("��������������18�ֵ�λ��Prefab��ϡ�ж�")]
    public List<UnitSpawnInfo> unitSpawnPool;

    [Header("ˢ����������")]
    public float mapSize = 200f; // ��ͼ�߽��һ�� (��-200��200)
    public float centerSafeRadius = 25f; // ���İ�ȫ���뾶
    public float innerRingRadius = 100f; // �ڻ��뾶
    public float middleRingRadius = 150f; // �л��뾶

    [Header("ˢ�����������")]
    public int initialEnemyCount = 25; // ��ʼ��������
    public int maxEnemyCount = 35; // ����������
    public float respawnInterval = 30f; // ˢ�¼�����룩

    [Header("��������")]
    [Range(0f, 1f)]
    public float ambushChance = 0.5f; // 50%��������Ҹ���ˢ��
    public float ambushDistance = 20f; // �������Χ20�����λ��

    private BattleHead playerBattleHead; // �������

    // --- ˽��״̬���� ---
    private List<ActiveEnemy> activeEnemies = new List<ActiveEnemy>();
    private float respawnTimer = 0f;

    // ���ڰ�ϡ�жȷ���ĵ�λ�أ�����鿨
    private List<GameObject> commonUnits; // N
    private List<GameObject> rareUnits;   // R
    private List<GameObject> superRareUnits; // S

    void Awake()
    {
        // ����Ϸ��ʼʱ���Ե�λ�ؽ���Ԥ����ͷ���
        PreprocessUnitPool();
    }

    void Start()
    {
    }

    public void Initialize()
    {
        // �ҵ��������������
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerBattleHead = playerController.GetComponentInChildren<BattleHead>();
        }
        if (playerBattleHead == null)
        {
            Debug.LogError("GameManagerδ���ҵ����BattleHead���������ƽ�ʧЧ��");
        }

        // ִ�г�ʼ�������ɣ����Ƿ���
        Debug.Log("GameManager: Initialize() �����ã���ʼ���ɳ�ʼ����...");
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnNewEnemy(false);
        }

        SpawnBoss();
    }

    void Update()
    {
        // ��̬ˢ���߼�
        if (activeEnemies.Count < maxEnemyCount)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnInterval)
            {
                respawnTimer = 0f;
                // ��̬ˢ��ʱ���и��ʴ�������
                bool attemptAmbush = Random.value < ambushChance;
                SpawnNewEnemy(attemptAmbush);
            }
        }

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            // ���GameObject�Ѿ������� (���类CombatResultResolver������)
            if (activeEnemies[i].rootObject == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    // --- ����ʵ�ַ��� ---

    private void PreprocessUnitPool()
    {
        commonUnits = unitSpawnPool.Where(u => u.rarity == Rarity.N).Select(u => u.unitPrefab).ToList();
        rareUnits = unitSpawnPool.Where(u => u.rarity == Rarity.R).Select(u => u.unitPrefab).ToList();
        superRareUnits = unitSpawnPool.Where(u => u.rarity == Rarity.S).Select(u => u.unitPrefab).ToList();

        if (commonUnits.Count == 0 || rareUnits.Count == 0 || superRareUnits.Count == 0)
        {
            Debug.LogError("��λ���ó���ȱ������һ��ϡ�жȵĵ�λ��");
        }
    }

    private void SpawnNewEnemy(bool isAmbush)
    {
        // 1. ��������λ�ú���������
        Vector3 spawnPosition;

        // --- ��������λ�� ---
        if (isAmbush && playerBattleHead != null && playerBattleHead.GetList().Count > 0)
        {
            // --- �����߼� ---
            Debug.Log("��������Ҹ���ˢ�µ���...");
            Transform playerHeadTransform = playerBattleHead.GetComponent<SnakeHead>().GetAllNodes()[0].transform;
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 offset = new Vector3(randomDirection.x, 0, randomDirection.y) * ambushDistance;
            spawnPosition = playerHeadTransform.position + offset;

            // ȷ��������
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, -mapSize, mapSize);
            spawnPosition.z = Mathf.Clamp(spawnPosition.z, -mapSize, mapSize);
        }
        else
        {
            // --- ��������߼� ---
            spawnPosition = GetRandomSpawnPosition();
        }


        float distanceFromCenter = Vector3.Distance(spawnPosition, Vector3.zero);

        // 2. ������������ߵĳ��Ⱥ͵ȼ���Χ
        int minLength, maxLength, minLevel, maxLevel;

        if (distanceFromCenter <= innerRingRadius) // �ڻ� (��ǿ)
        {
            minLength = 5; maxLength = 18;
            minLevel = 10; maxLevel = 35;
        }
        else if (distanceFromCenter <= middleRingRadius) // �л�
        {
            minLength = 3; maxLength = 8;
            minLevel = 4; maxLevel = 12;
        }
        else // �⻷ (����)
        {
            minLength = 1; maxLength = 3;
            minLevel = 1; maxLevel = 5;
        }

        // 3. ���ȷ�����յĳ��Ⱥ͵ȼ�
        int snakeLength = Random.Range(minLength, maxLength + 1);
        int snakeLevel = Random.Range(minLevel, maxLevel + 1);

        // 4. ��������ѡ��λ�أ��������ߵĽڵ��б�
        List<GameObject> bodyPrefabs = new List<GameObject>();
        for (int i = 0; i < snakeLength; i++)
        {
            bodyPrefabs.Add(GetRandomUnitPrefabByZone(distanceFromCenter));
        }

        // 5. ������
        Quaternion initialRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        SpawnEnemySnake(spawnPosition, initialRotation, bodyPrefabs, snakeLevel);
    }

    // ���ڸ������������ȡ��λ
    private GameObject GetRandomUnitPrefabByZone(float distance)
    {
        // ���岻ͬ����ĳ鿨���� (N, R, S)
        // ��Щ���ʿ��Ը�����Ҫ����
        float n_chance = 0f, r_chance = 0f, s_chance = 0f;

        if (distance <= innerRingRadius) // �ڻ�
        {
            n_chance = 0.5f; // 20%
            r_chance = 0.3f; // 50%
            s_chance = 0.2f; // 30%
        }
        else if (distance <= middleRingRadius) // �л�
        {
            n_chance = 0.6f; // 50%
            r_chance = 0.3f; // 40%
            s_chance = 0.1f; // 10%
        }
        else // �⻷
        {
            n_chance = 0.8f; // 80%
            r_chance = 0.2f; // 20%
            s_chance = 0.0f; // 0%
        }

        float randomValue = Random.value; // ����һ��0��1�������

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
            // ȷ��S����λ�ز�Ϊ��
            if (superRareUnits.Count > 0)
                return superRareUnits[Random.Range(0, superRareUnits.Count)];
            else // ���û��S����λ���򽵼�ΪR��
                return rareUnits[Random.Range(0, rareUnits.Count)];
        }
    }

    // ����һ���������������ڻ�ȡ����Ұ�ȫ�����ɵ�
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
                Debug.LogWarning("�޷��ҵ���ȫ�����ɵ㣬���ܵ�ͼ̫ӵ����");
                break;
            }
        }
        // ѭ��ֱ���ҵ�һ���������İ�ȫ���ĵ�
        while (Vector3.Distance(position, Vector3.zero) < centerSafeRadius);

        return position;
    }

    // ����֮ǰ�����������߼������ڱ���������
    private void SpawnEnemySnake(Vector3 position, Quaternion rotation, List<GameObject> nodePrefabs, int level)
    {
        GameObject enemyRoot = Instantiate(enemyControllerPrefab, position, rotation);
        enemyRoot.transform.SetParent(this.transform); // ��������ΪGameManager���Ӷ��󣬷������

        EnemyController enemyController = enemyRoot.GetComponent<EnemyController>();
        enemyController.Initialize(headLogicPrefab, rotation);

        foreach (var prefab in nodePrefabs)
        {
            // AddSheep���յ���Resources������֣�������Ҫ��Prefab��ȡ
            enemyController.battleHead.AddSheep(prefab.name);
        }

        for (int i = 0; i < level; i++)
        {
            enemyController.battleHead.LevelUp();
        }

        // �������ɵĵ�����ӵ���б���
        activeEnemies.Add(new ActiveEnemy(enemyRoot, enemyController.battleHead));
    }

    private void SpawnBoss()
    {
        // 1. �ӵ�λ�����ҵ�Boss��������Ϣ
        UnitSpawnInfo bossInfo = unitSpawnPool.FirstOrDefault(u => u.rarity == Rarity.X);

        // ���û����Inspector������Boss�������ɲ�������ʾ
        if (bossInfo == null || bossInfo.unitPrefab == null)
        {
            Debug.LogWarning("��λ���ó���δ�ҵ�ϡ�ж�Ϊ 'X' ��Boss���ã�Boss���������ɡ�");
            return;
        }

        // 2. ��������Ҫ�󣬶���Boss�����ɲ���
        Vector3 bossPosition = Vector3.zero; // �̶�λ�� (0, 0)
        int bossLevel = 0;                   // �ȼ�Ϊ 0

        // Bossֻ��һ�����壬�������ġ����塱�б�ֻ�������Լ�
        List<GameObject> bossBody = new List<GameObject> { bossInfo.unitPrefab };

        // ���һ����ʼ����
        Quaternion initialRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 3. �������е����ɿ��������Boss
        Debug.Log("���� (0,0) λ������Boss...");
        SpawnEnemySnake(bossPosition, initialRotation, bossBody, bossLevel);
    }

}