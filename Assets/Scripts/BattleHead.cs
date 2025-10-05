using System.Collections.Generic;
using UnityEngine;
public class BattleHead : MonoBehaviour
{
    public bool isPlayer;
    SnakeHead snakeHead;
    public List<BattleNode> nodeList = new List<BattleNode>();

    void Awake()
    {
        snakeHead = GetComponent<SnakeHead>();
    }
    void Start()
    {
        
    }
    //OnlyTest
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        { 
            AddSheep("Sheep01");
        }
    }

    public void AddSheep(string sheepName)
    {
        var sheepPrefab = Resources.Load<GameObject>(sheepName);
        var newNode = Instantiate(sheepPrefab);
        var battleNode = newNode.GetComponent<BattleNode>();
        battleNode.sheepPrefab = sheepPrefab;
        AddNode(battleNode);
    }

    private void AddNode(BattleNode newNode)
    {
        newNode.SetHead(this);
        nodeList.Add(newNode);
        newNode.onAdd?.Invoke(this, nodeList.Count - 1);

        var node = snakeHead.AddNode();
        node.SetModel(newNode.gameObject);
        UpdateNodes();
    }

    public void RemoveNode(BattleNode node)
    {
        nodeList.Remove(node);
        node.onRemove?.Invoke(this);
        UpdateNodes();
    }

    //清除指定索引以及之后的所有节点
    public void ClearNodes(int index)
    {
        if (index < 0 || index >= nodeList.Count) return;
        //只触发指定节点的离队事件
        nodeList[index].onRemove?.Invoke(this);
        nodeList.RemoveRange(index, nodeList.Count - index);
        UpdateNodes();
    }

    public List<BattleNode> GetList()
    {
        return nodeList;
    }
    public void SetList(List<BattleNode> newList)
    {
        nodeList = newList;
    }

    public void UpdateNodes()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].CaculateAttribute(this, i);
        }
    }

    public void SwapNodeForward(BattleNode node)
    {
        int currentIndex = nodeList.IndexOf(node);
        // 如果已经是蛇头，或者找不到该节点，则不操作
        if (currentIndex <= 0) return;

        SwapNodes(currentIndex, currentIndex - 1);
    }


    public void SwapNodeBackward(BattleNode node)
    {
        int currentIndex = nodeList.IndexOf(node);
        // 如果已经是蛇尾，或者找不到该节点，则不操作
        if (currentIndex < 0 || currentIndex >= nodeList.Count - 1) return;

        SwapNodes(currentIndex, currentIndex + 1);
    }

    private void SwapNodes(int index1, int index2)
    {
        // --- 数据交换 ---
        BattleNode temp = nodeList[index1];
        nodeList[index1] = nodeList[index2];
        nodeList[index2] = temp;

        // --- 表现交换 ---
        snakeHead.SwapNodeTransforms(index1, index2);

        // --- 全局更新 ---
        UpdateNodes();

        // --- 检查并更新相机跟随目标 ---
        if (index1 == 0 || index2 == 0)
        {
            if (nodeList.Count > 0)
            {
                SnakeNode newHeadNode = snakeHead.GetAllNodes()[0];

                // 区分玩家和敌人
                if (isPlayer)
                {
                    PlayerInputManager.Instance.UpdateOriginalFollowTarget(newHeadNode.transform);
                }
            }
        }

        Debug.Log($"交换了 {index1} 和 {index2} 位置的节点。");
    }
}