using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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


public class GameManager : MonoBehaviour
{
    [Header("����Prefabs")]
    [Tooltip("����'Enemy'�������Prefab������Ӧ�ù���EnemyController�ű�")]
    public GameObject enemyControllerPrefab;
    [Tooltip("����'Head'�߼������Prefab���������SnakeHead��BattleHead")]
    public GameObject headLogicPrefab;

    [Header("��������")]
    [Tooltip("��������Ҫ�ڳ��������ɵ����е�����")]
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
            Debug.LogError("GameManagerȱ�ٺ���Prefabs�����ã�");
            return;
        }

        // ���� "Enemy" ������
        GameObject enemyRoot = Instantiate(enemyControllerPrefab, config.spawnPosition, Quaternion.identity);
        enemyRoot.name = "Enemy" + config.nodePrefabNames[0]; // ����ͷ�������������������

        // ��ȡEnemyController����ʼ��
        EnemyController enemyController = enemyRoot.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("EnemyControllerPrefab��û�й���EnemyController�ű���");
            return;
        }
        enemyController.Initialize(headLogicPrefab);

        // ���������������ڵ�
        foreach (var sheepName in config.nodePrefabNames)
        {
            enemyController.battleHead.AddSheep(sheepName);
        }

        //// (��ѡ���Ƽ�) �������нڵ�ĳ�ʼ�ȼ�
        //foreach (var battleNode in enemyController.battleHead.GetList())
        //{
        //    battleNode.Level = config.initialLevel;
        //}
        
        // ����һ��������Ӧ�õȼ�
        //enemyController.battleHead.UpdateNodes();

        Debug.Log("�ɹ�����һ�������ߣ���ͷΪ: " + config.nodePrefabNames[0]);
    }

}