using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIBattleManager : MonoBehaviour
{
    public static UIBattleManager Instance;

    [Header("玩家UI")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerAttackText;
    public TextMeshProUGUI playerDefenseText;

    [Header("敌人UI")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI enemyAttackText;
    public TextMeshProUGUI enemyDefenseText;

    [Header("抖动效果参数")]
    public float shakeDuration = 0.2f; // 抖动持续时间
    public float shakeMagnitude = 5f; // 抖动幅度

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeUI(TurnBasedManager.BattleUnit playerData, TurnBasedManager.BattleUnit enemyData)
    {
        // 初始化玩家UI
        playerNameText.text = playerData.unitName;
        playerHealthText.text = playerData.Health.ToString();
        playerAttackText.text = playerData.Attack.ToString();
        playerDefenseText.text = playerData.Defense.ToString();

        // 初始化敌人UI
        enemyNameText.text = enemyData.unitName;
        enemyHealthText.text = enemyData.Health.ToString();
        enemyAttackText.text = enemyData.Attack.ToString();
        enemyDefenseText.text = enemyData.Defense.ToString();
    }

    public void OnDataChange(TextMeshProUGUI uiElement, int newValue)
    {
        // 更新文本
        uiElement.text = newValue.ToString();

        // 抖动
        StartCoroutine(ShakeEffect(uiElement.transform));
    }

    private IEnumerator ShakeEffect(Transform elementTransform)
    {
        Vector3 originalPosition = elementTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // 圆形范围内随机生成偏移量
            Vector3 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            elementTransform.localPosition = originalPosition + randomOffset;

            elapsed += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 抖动结束后，确保UI元素回到原始位置
        elementTransform.localPosition = originalPosition;
    }
}
