using UnityEngine;

/// <summary>
/// 贪吃蛇的身体/头部节点，存储自身属性。
/// </summary>
public class SnakeNode : MonoBehaviour
{
    // 节点的大小（用于显示）
    public Vector3 size = Vector3.one; 
    GameObject modelInstance;

    // 节点的索引：0 为蛇头
    [HideInInspector] 
    public int segmentIndex;

    void Start()
    {
        transform.localScale = size;
    }

    public void SetModel(GameObject newInstance)
    {
        modelInstance = newInstance;
        modelInstance.transform.SetParent(transform);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
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