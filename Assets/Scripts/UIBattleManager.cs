using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [Header("支援UI组件")]
    public GameObject supportUIGroup;
    public Image supportIcon;
    public TextMeshProUGUI supportInfoText;

    [Header("支援UI资源")]
    public Sprite playerSupportIcon;
    public Sprite enemySupportIcon;

    private int arrivalTurn;
    private float originalTimeScale = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = 1.0f;
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

    public void InitializeSupportUI(bool isPossible, bool isPlayerBeingRescued, int initialDistance)
    {
        if (!isPossible)
        {
            supportUIGroup.SetActive(false);
            return;
        }

        supportUIGroup.SetActive(true);

        // 根据支援方阵营，设置图标
        supportIcon.sprite = isPlayerBeingRescued ? playerSupportIcon : enemySupportIcon;

        // 计算预计抵达的回合数
        arrivalTurn = CalculateArrivalTurn(initialDistance);

        // 更新初始文本
        UpdateSupportTurnCount(0); // 传入当前回合数0
    }

    public void UpdateSupportTurnCount(int currentTurn)
    {
        int turnsRemaining = arrivalTurn - currentTurn;

        if (turnsRemaining > 0)
        {
            // 判断支援方是谁，显示不同文本
            bool isPlayerRescuer = CombatManager.Instance.isTargetNodePlayer;
            string rescuerName = isPlayerRescuer ? "Our" : "Enemy";
            supportInfoText.text = $"{rescuerName} Bellwether will arrive in {turnsRemaining} turns.";
        }
    }

    public void ShowArrivalMessage()
    {
        if (!supportUIGroup.activeSelf) return; // 如果UI未激活，则不执行

        supportInfoText.text = "Bellwether has arrived";
        StartCoroutine(HideSupportUIAfterDelay(3f));
    }

    private IEnumerator HideSupportUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        supportUIGroup.SetActive(false);
    }

    private int CalculateArrivalTurn(int distance)
    {
        int turns = 0;
        int speed = 0;
        while (distance > 0)
        {
            turns++;
            speed = turns; // 救援速度 = 当前回合数
            distance -= speed;
        }
        return turns;
    }

    public void SetTimeScale(float speedMultiplier)
    {
        Time.timeScale = speedMultiplier;
        Debug.Log("游戏速度已设置为: " + speedMultiplier + "x");
    }

    public void RestoreOriginalTimeScale()
    {
        Time.timeScale = originalTimeScale;
        Debug.Log("恢复原始速度: " + originalTimeScale + "x");
    }
}
