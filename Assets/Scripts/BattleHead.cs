using System.Collections.Generic;
using UnityEngine;
public class BattleHead : MonoBehaviour
{
    public bool isPlayer;
    SnakeHead snakeHead;
    public List<BattleNode> nodeList = new List<BattleNode>();

    // 等级管理
    private List<int> slotLevels = new List<int>();
    private int totalLevelUps = 0; // 记录总升级次数
    private List<int> levelUpSequence = new List<int>(); // 缓存升级顺序
    private int sequenceNextIndex = 0; // 指向下一个要升级的位置



    void Awake()
    {
        snakeHead = GetComponent<SnakeHead>();
        // 为至少100个槽位预设1级
        for (int i = 0; i < 100; i++)
        {
            slotLevels.Add(1);
        }
        GenerateLevelUpSequence(100); // 预生成升级序列
    }

    public void LevelUp()
    {
        if (sequenceNextIndex >= levelUpSequence.Count)
        {
            Debug.LogWarning("升级序列已用尽，请考虑生成更长的序列。");
            return;
        }

        // 获取下一个要升级的槽位索引 (序列中的值是1-based, 列表是0-based)
        int slotToUpgrade = levelUpSequence[sequenceNextIndex] - 1;

        // 确保等级列表足够长
        while (slotLevels.Count <= slotToUpgrade)
        {
            slotLevels.Add(1);
        }

        // 对应槽位等级+1
        slotLevels[slotToUpgrade]++;
        totalLevelUps++;
        sequenceNextIndex++;

        // 升级后必须重新计算所有节点的属性
        UpdateNodes();
        Debug.Log($"槽位 {slotToUpgrade + 1} 升级! 新等级: {slotLevels[slotToUpgrade]}");
    }

    public int GetLevelForSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotLevels.Count)
        {
            return slotLevels[slotIndex];
        }
        return 1; // 安全返回，默认1级
    }


    public void UpdateNodes()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            // 将该节点所在槽位的等级传递过去
            nodeList[i].CaculateAttribute(this, i, GetLevelForSlot(i));
        }
    }

    private void GenerateLevelUpSequence(int length)
    {
        levelUpSequence.Clear();
        int currentTier = 1;
        while (levelUpSequence.Count < length)
        {
            for (int i = 1; i <= currentTier; i++)
            {
                levelUpSequence.Add(i);
                if (levelUpSequence.Count >= length) break;
            }
            currentTier++;
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        { 
            AddSheep("ATK");
        }
    }

    public void AddSheep(string sheepName)
    {
        var sheepPrefab = Resources.Load<GameObject>(sheepName);

        if (sheepPrefab == null)
        {
            Debug.LogError("无法在找到Prefab: " + sheepName);
            return;
        }

        var newNodeObject = Instantiate(sheepPrefab);
        var battleNode = newNodeObject.GetComponent<BattleNode>();
        var snakeNode = newNodeObject.GetComponent<SnakeNode>();
        //battleNode.sheepName = sheepName; 

        if (battleNode == null || snakeNode == null)
        {
            Destroy(newNodeObject); // 销毁有问题的对象
            return;
        }
        battleNode.sheepPrefab = sheepPrefab;
        AddNode(battleNode, snakeNode);
    }

    private void AddNode(BattleNode newBattleNode, SnakeNode newSnakeNode)
    {
        newBattleNode.SetHead(this);
        nodeList.Add(newBattleNode);
        newBattleNode.onAdd?.Invoke(this, nodeList.Count - 1);

        snakeHead.AddNodeToList(newSnakeNode);

        UpdateNodes();
    }

    public void RemoveNode(BattleNode nodeToRemove)
    {
        // 从物理层移除
        var snakeNodeToRemove = nodeToRemove.GetComponent<SnakeNode>();
        if (snakeNodeToRemove != null)
        {
            snakeHead.RemoveNodeFromList(snakeNodeToRemove);
        }

        // 从逻辑层移除
        nodeList.Remove(nodeToRemove);
        nodeToRemove.onRemove?.Invoke(this);

        // 销毁GameObject
        Destroy(nodeToRemove.gameObject);

        UpdateNodes();
    }

    //清除指定索引以及之后的所有节点
    public void ClearNodes(int index)
    {
        if (index < 0 || index >= nodeList.Count) return;

        // 从后往前移除，这样更安全
        for (int i = nodeList.Count - 1; i >= index; i--)
        {
            RemoveNode(nodeList[i]);
        }
    }

    public List<BattleNode> GetList()
    {
        return nodeList;
    }
    public void SetList(List<BattleNode> newList)
    {
        nodeList = newList;
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