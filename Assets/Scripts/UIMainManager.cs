using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text; 

public class UIMainManager : MonoBehaviour
{
    public static UIMainManager Instance;

    [Header("信息框 Prefab")]
    public GameObject tooltipPrefab;

    [Header("阵营图标")]
    public Sprite playerIcon;
    public Sprite enemyIcon;


    [Header("Tooltip位置设置")]

    [Tooltip("Tooltip相对于鼠标指针的偏移量 (像素)")]
    public Vector2 positionOffset = new Vector2(15, -15);

    [Tooltip("Tooltip距离屏幕边缘的最小间距 (像素)")]
    public float screenPadding = 10f;


    private GameObject currentTooltipInstance;
    private RectTransform canvasRectTransform;
    private Tooltip currentTooltipComponents;
    private RectTransform tooltipRectTransform; // 直接引用Tooltip脚本

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    void Update()
    {
        if (currentTooltipInstance != null)
        {
            UpdateTooltipPosition();
        }
    }

    public void ShowTooltip(BattleNode node)
    {
        if (currentTooltipInstance != null) Destroy(currentTooltipInstance);

        currentTooltipInstance = Instantiate(tooltipPrefab, transform);

        CanvasGroup canvasGroup = currentTooltipInstance.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = currentTooltipInstance.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // 获取并缓存组件引用
        currentTooltipComponents = currentTooltipInstance.GetComponent<Tooltip>();
        tooltipRectTransform = currentTooltipInstance.GetComponent<RectTransform>(); 

        if (currentTooltipComponents == null)
        {
            Debug.LogError("Tooltip Prefab上没有挂载Tooltip脚本！");
            Destroy(currentTooltipInstance);
            return;
        }

        UpdateTooltipData(node);
    }

    public void HideTooltip()
    {
        if (currentTooltipInstance != null)
        {
            Destroy(currentTooltipInstance);
            currentTooltipInstance = null;
            currentTooltipComponents = null;
            tooltipRectTransform = null; // 清空缓存
        }
    }

    private void UpdateTooltipData(BattleNode node)
    {
        if (node == null || currentTooltipComponents == null) return;

        currentTooltipComponents.levelText.text = $"LV {node.Level}";
        currentTooltipComponents.factionIcon.sprite = node.IsPlayer() ? playerIcon : enemyIcon;
        currentTooltipComponents.hpText.text = node.finalAttribute.Health.ToString();
        currentTooltipComponents.atkText.text = node.finalAttribute.Attack.ToString();
        currentTooltipComponents.defText.text = node.finalAttribute.Defense.ToString();
        currentTooltipComponents.nameText.text = node.sheepName;
        currentTooltipComponents.buffDescriptionText.text = GenerateBuffDescription(node);
    }

    private void UpdateTooltipPosition()
    {
        if (canvasRectTransform == null || tooltipRectTransform == null) return;

        // --- 1. 计算带偏移量的基础位置 ---
        // 将屏幕坐标的鼠标位置转换为Canvas下的局部坐标
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            Input.mousePosition,
            null,
            out localPoint
        );
        // 应用偏移量
        tooltipRectTransform.localPosition = localPoint + positionOffset;

        // --- 2. 进行屏幕边界检测与修正 ---

        // 获取Tooltip的四个角在Canvas坐标系下的位置
        Vector3[] corners = new Vector3[4];
        tooltipRectTransform.GetWorldCorners(corners); // 获取世界坐标
        // 将世界坐标转换为Canvas的局部坐标
        for (int i = 0; i < 4; i++)
        {
            corners[i] = canvasRectTransform.InverseTransformPoint(corners[i]);
        }

        // 获取Canvas的边界
        Rect canvasRect = canvasRectTransform.rect;

        // 计算需要修正的偏移量
        Vector2 moveOffset = Vector2.zero;

        // 检查右边界
        if (corners[2].x > canvasRect.xMax - screenPadding)
        {
            moveOffset.x = (canvasRect.xMax - screenPadding) - corners[2].x;
        }
        // 检查左边界
        if (corners[0].x < canvasRect.xMin + screenPadding)
        {
            moveOffset.x = (canvasRect.xMin + screenPadding) - corners[0].x;
        }
        // 检查上边界
        if (corners[1].y > canvasRect.yMax - screenPadding)
        {
            moveOffset.y = (canvasRect.yMax - screenPadding) - corners[1].y;
        }
        // 检查下边界
        if (corners[0].y < canvasRect.yMin + screenPadding)
        {
            moveOffset.y = (canvasRect.yMin + screenPadding) - corners[0].y;
        }

        // 应用修正偏移量
        tooltipRectTransform.localPosition += (Vector3)moveOffset;
    }

    // Buff描述生成器
    private string GenerateBuffDescription(BattleNode node)
    {
        if (string.IsNullOrEmpty(node.buffName))
        {
            return "No special ability.";
        }

        // --- 1. 计算数值 ---
        // 注意：复合Buff的a和b有不同含义，我们需要分开处理
        string valueString = "";
        switch (node.buffName)
        {
            case "HPATK":
                valueString = $"+{node.Level * node.buffParam.a} HP and +{node.Level * node.buffParam.b} ATK";
                break;
            case "HPDEF":
                valueString = $"+{node.Level * node.buffParam.a} HP and +{node.Level * node.buffParam.b} DEF";
                break;
            case "ATKDEF":
                valueString = $"+{node.Level * node.buffParam.a} ATK and +{node.Level * node.buffParam.b} DEF";
                break;
            default:
                // 对于所有单属性Buff
                valueString = $"+{node.Level * node.buffParam.a + node.buffParam.b}";
                break;
        }

        // --- 2. 确定影响属性 ---
        string attributeString = "";
        if (node.buffName.Contains("HP")) attributeString += "HP ";
        if (node.buffName.Contains("ATK")) attributeString += "ATK ";
        if (node.buffName.Contains("DEF")) attributeString += "DEF ";
        attributeString = attributeString.Trim().Replace(" ", " and ");

        // --- 3. 确定影响对象 ---
        string targetString = "";
        if (node.buffName.EndsWith("A")) targetString = "all sheeps";
        else if (node.buffName.EndsWith("F")) targetString = "forward sheeps";
        else if (node.buffName.EndsWith("B")) targetString = "backward sheeps";
        else if (node.buffName.EndsWith("N")) targetString = "nearby sheeps";
        else targetString = "itself"; // 默认是对自己

        // --- 4. 拼接最终描述 ---
        // 复合Buff的描述比较特殊
        if (node.buffName == "HPATK" || node.buffName == "HPDEF" || node.buffName == "ATKDEF")
        {
            return $"Provides {valueString} to {targetString}.";
        }
        else // 单属性Buff
        {
            return $"Provides {valueString} {attributeString} to {targetString}.";
        }
    }

}