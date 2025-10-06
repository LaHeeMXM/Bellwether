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
        // ���Լ��·����� "Head" �߼�����
        GameObject headInstance = Instantiate(headPrefab, transform);
        headInstance.transform.localPosition = Vector3.zero;
        headInstance.transform.localRotation = Quaternion.identity;

        // ��ȡ�������������
        snakeHead = headInstance.GetComponent<SnakeHead>();
        battleHead = headInstance.GetComponent<BattleHead>();

        // ��֪������������
        snakeHead.isPlayer = false;
        battleHead.isPlayer = false;
    }

    // Update is called once per frame
    void Update()
    {
        // AI_DecisionMaking();
    }
}