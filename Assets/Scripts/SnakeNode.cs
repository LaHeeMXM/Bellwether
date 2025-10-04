using UnityEngine;

/// <summary>
/// 贪吃蛇的身体/头部节点，存储自身属性。
/// </summary>
public class SnakeNode : MonoBehaviour
{
    // 节点的大小（用于显示）
    public Vector3 size = Vector3.one; 
    
    // 节点的索引：0 为蛇头
    [HideInInspector] 
    public int segmentIndex;

    void Start()
    {
        // 应用设置的大小到 GameObject 的缩放
        transform.localScale = size;
    }
    
    /// <summary>
    /// 更新节点的尺寸。
    /// </summary>
    public void SetSize(Vector3 newSize)
    {
        size = newSize;
        transform.localScale = size;
    }
}