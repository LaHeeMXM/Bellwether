using System.Collections.Generic;
using UnityEngine;


public class BattleHead : MonoBehaviour
{
    public bool isPlayer;
    SnakeHead snakeHead;
    public List<BattleNode> nodeList = new List<BattleNode>();

    // 等级管理
    private List<int> slotLevels = new List<int>();
    private int totalLevelUps = 0;
    private List<int> levelUpSequence = new List<int>();
    private int sequenceNextIndex = 0;

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

    // 用于测试的Update，可以在最终版本中移除
    void Update()
    {
        if (isPlayer && Input.GetKeyDown(KeyCode.T))
        {
            // 随机添加一个单位用于测试
            string[] testUnits = { "ATKF", "HPB", "DEF" };
            AddSheep(testUnits[Random.Range(0, testUnits.Length)]);
        }
    }

    public void LevelUp()
    {
        if (sequenceNextIndex >= levelUpSequence.Count)
        {
            Debug.LogWarning("升级序列已用尽，请考虑生成更长的序列。");
            return;
        }

        int slotToUpgrade = levelUpSequence[sequenceNextIndex] - 1;

        while (slotLevels.Count <= slotToUpgrade)
        {
            slotLevels.Add(1);
        }

        slotLevels[slotToUpgrade]++;
        totalLevelUps++;
        sequenceNextIndex++;

        UpdateNodes();
        Debug.Log($"槽位 {slotToUpgrade + 1} 升级! 新等级: {slotLevels[slotToUpgrade]}");
    }

    public int GetLevelForSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotLevels.Count)
        {
            return slotLevels[slotIndex];
        }
        return 1;
    }

    public void UpdateNodes()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
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

    public void AddSheep(string sheepName)
    {
        var sheepPrefab = Resources.Load<GameObject>(sheepName);

        if (sheepPrefab == null)
        {
            Debug.LogError("无法在Resources文件夹中找到Prefab: " + sheepName);
            return;
        }

        var newNodeObject = Instantiate(sheepPrefab);
        var battleNode = newNodeObject.GetComponent<BattleNode>();
        var snakeNode = newNodeObject.GetComponent<SnakeNode>();

        if (battleNode == null || snakeNode == null)
        {
            Debug.LogError("实例化的Prefab: " + sheepName + " 上缺少 BattleNode 或 SnakeNode 组件!");
            Destroy(newNodeObject);
            return;
        }

        battleNode.sheepPrefab = sheepPrefab;
        battleNode.sheepName = sheepName; // 确保sheepName也被记录
        AddNode(battleNode, snakeNode);
    }

    private void AddNode(BattleNode newBattleNode, SnakeNode newSnakeNode)
    {
        newBattleNode.SetHead(this);
        nodeList.Add(newBattleNode);

        snakeHead.AddNodeToList(newSnakeNode);

        UpdateNodes();

        // onAdd 可以在所有节点更新后触发，确保它能获取到正确的初始属性
        newBattleNode.onAdd?.Invoke(this, nodeList.Count - 1);
    }

    public void RemoveNode(BattleNode nodeToRemove)
    {
        var snakeNodeToRemove = nodeToRemove.GetComponent<SnakeNode>();
        if (snakeNodeToRemove != null)
        {
            snakeHead.RemoveNodeFromList(snakeNodeToRemove);
        }

        nodeList.Remove(nodeToRemove);
        nodeToRemove.onRemove?.Invoke(this);

        Destroy(nodeToRemove.gameObject);

        UpdateNodes();
    }

    public void ClearNodes(int index)
    {
        if (index < 0 || index >= nodeList.Count) return;

        // 从后往前移除，以避免列表索引变化导致的问题
        for (int i = nodeList.Count - 1; i >= index; i--)
        {
            // 准备移除的节点
            BattleNode nodeToClear = nodeList[i];

            // 物理层移除
            var snakeNodeToClear = nodeToClear.GetComponent<SnakeNode>();
            if (snakeNodeToClear != null)
            {
                snakeHead.RemoveNodeFromList(snakeNodeToClear);
            }

            // 逻辑层移除
            nodeList.RemoveAt(i);
            nodeToClear.onRemove?.Invoke(this);

            // 销毁GameObject
            Destroy(nodeToClear.gameObject);
        }

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

    public void SwapNodeForward(BattleNode node)
    {
        int currentIndex = nodeList.IndexOf(node);
        if (currentIndex <= 0) return;
        SwapNodes(currentIndex, currentIndex - 1);
    }

    public void SwapNodeBackward(BattleNode node)
    {
        int currentIndex = nodeList.IndexOf(node);
        if (currentIndex < 0 || currentIndex >= nodeList.Count - 1) return;
        SwapNodes(currentIndex, currentIndex + 1);
    }

    private void SwapNodes(int index1, int index2)
    {
        // --- 1. 数据层交换 ---
        BattleNode temp = nodeList[index1];
        nodeList[index1] = nodeList[index2];
        nodeList[index2] = temp;

        // --- 2. 表现层交换 (委托给SnakeHead) ---
        snakeHead.SwapNodeTransforms(index1, index2);

        // --- 3. 全局属性更新 ---
        UpdateNodes();

        // --- 4. 相机目标更新 ---
        if (index1 == 0 || index2 == 0)
        {
            if (nodeList.Count > 0)
            {
                SnakeNode newHeadNode = snakeHead.GetAllNodes()[0];
                if (isPlayer)
                {
                    PlayerInputManager.Instance.UpdateOriginalFollowTarget(newHeadNode.transform);
                }
            }
        }

        Debug.Log($"交换了 {index1} 和 {index2} 位置的节点。");
    }
}