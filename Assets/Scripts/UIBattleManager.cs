using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIBattleManager : MonoBehaviour
{
    public static UIBattleManager Instance;

    [Header("���UI")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerAttackText;
    public TextMeshProUGUI playerDefenseText;

    [Header("����UI")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI enemyAttackText;
    public TextMeshProUGUI enemyDefenseText;

    [Header("����Ч������")]
    public float shakeDuration = 0.2f; // ��������ʱ��
    public float shakeMagnitude = 5f; // ��������

    [Header("֧ԮUI���")]
    public GameObject supportUIGroup;
    public Image supportIcon;
    public TextMeshProUGUI supportInfoText;

    [Header("֧ԮUI��Դ")]
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
        // ��ʼ�����UI
        playerNameText.text = playerData.unitName;
        playerHealthText.text = playerData.Health.ToString();
        playerAttackText.text = playerData.Attack.ToString();
        playerDefenseText.text = playerData.Defense.ToString();

        // ��ʼ������UI
        enemyNameText.text = enemyData.unitName;
        enemyHealthText.text = enemyData.Health.ToString();
        enemyAttackText.text = enemyData.Attack.ToString();
        enemyDefenseText.text = enemyData.Defense.ToString();
    }

    public void OnDataChange(TextMeshProUGUI uiElement, int newValue)
    {
        // �����ı�
        uiElement.text = newValue.ToString();

        // ����
        StartCoroutine(ShakeEffect(uiElement.transform));
    }

    private IEnumerator ShakeEffect(Transform elementTransform)
    {
        Vector3 originalPosition = elementTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // Բ�η�Χ���������ƫ����
            Vector3 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            elementTransform.localPosition = originalPosition + randomOffset;

            elapsed += Time.deltaTime;
            yield return null; // �ȴ���һ֡
        }

        // ����������ȷ��UIԪ�ػص�ԭʼλ��
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

        // ����֧Ԯ����Ӫ������ͼ��
        supportIcon.sprite = isPlayerBeingRescued ? playerSupportIcon : enemySupportIcon;

        // ����Ԥ�Ƶִ�Ļغ���
        arrivalTurn = CalculateArrivalTurn(initialDistance);

        // ���³�ʼ�ı�
        UpdateSupportTurnCount(0); // ���뵱ǰ�غ���0
    }

    public void UpdateSupportTurnCount(int currentTurn)
    {
        int turnsRemaining = arrivalTurn - currentTurn;

        if (turnsRemaining > 0)
        {
            // �ж�֧Ԯ����˭����ʾ��ͬ�ı�
            bool isPlayerRescuer = CombatManager.Instance.isTargetNodePlayer;
            string rescuerName = isPlayerRescuer ? "Our" : "Enemy";
            supportInfoText.text = $"{rescuerName} Bellwether will arrive in {turnsRemaining} turns.";
        }
    }

    public void ShowArrivalMessage()
    {
        if (!supportUIGroup.activeSelf) return; // ���UIδ�����ִ��

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
            speed = turns; // ��Ԯ�ٶ� = ��ǰ�غ���
            distance -= speed;
        }
        return turns;
    }

    public void SetTimeScale(float speedMultiplier)
    {
        Time.timeScale = speedMultiplier;
        Debug.Log("��Ϸ�ٶ�������Ϊ: " + speedMultiplier + "x");
    }

    public void RestoreOriginalTimeScale()
    {
        Time.timeScale = originalTimeScale;
        Debug.Log("�ָ�ԭʼ�ٶ�: " + originalTimeScale + "x");
    }
}
