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
    
}