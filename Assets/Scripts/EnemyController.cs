using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector]
    public SnakeHead snakeHead;
    [HideInInspector]
    public BattleHead battleHead;


    public void Initialize(GameObject headPrefab)
    {
        // 在自己下方生成 "Head" 逻辑对象
        GameObject headInstance = Instantiate(headPrefab, transform);
        headInstance.transform.localPosition = Vector3.zero;
        headInstance.transform.localRotation = Quaternion.identity;

        // 获取核心组件的引用
        snakeHead = headInstance.GetComponent<SnakeHead>();
        battleHead = headInstance.GetComponent<BattleHead>();

        // 告知组件不属于玩家
        snakeHead.isPlayer = false;
        battleHead.isPlayer = false;
    }

    // Update is called once per frame
    void Update()
    {
        // AI_DecisionMaking();
    }
}