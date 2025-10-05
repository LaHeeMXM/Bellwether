using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
}
