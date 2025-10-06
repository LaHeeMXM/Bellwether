using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ��Ϸ��ʼʱ�����Ӽ���������
        StartCoroutine(LoadMainSceneAndInitialize());
    }

    private IEnumerator LoadMainSceneAndInitialize()
    {
        // �첽���Ӽ���MainScene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);

        // �ȴ������������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 2. ���¼��ص�MainScene����Ϊ�������ȷ����������Instantiate���ڴ˳�����
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene.IsValid())
        {
            SceneManager.SetActiveScene(mainScene);
            Debug.Log("GameFlowManager: MainScene �ѱ�����Ϊ�������");

            // ȷ��GameManager�������߼�����ȷ��ʱ������ȷ�ĳ�����ִ��
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.Initialize();
            }
            else
            {
                Debug.LogError("GameFlowManager: ��MainScene��û���ҵ�GameManager��");
            }
        }
    }



    public void GoToCombat()
    {
        StartCoroutine(GoToCombatRoutine());
    }

    private IEnumerator GoToCombatRoutine()
    {
        yield return new WaitForFixedUpdate();

        // ����������
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene.IsValid())
        {
            foreach (var go in mainScene.GetRootGameObjects())
            {
                go.SetActive(false);
            }
        }

        // ���Ӽ���ս������
        yield return SceneManager.LoadSceneAsync("CombatScene", LoadSceneMode.Additive);

        // ������󣬿�������ս������Ϊ�����
        Scene combatScene = SceneManager.GetSceneByName("CombatScene");
        if (combatScene.IsValid())
        {
            SceneManager.SetActiveScene(combatScene);
        }
    }


    public void ReturnFromCombat()
    {
        StartCoroutine(ReturnFromCombatRoutine());
    }

    private IEnumerator ReturnFromCombatRoutine()
    {
        // �첽ж��ս������
        yield return SceneManager.UnloadSceneAsync("CombatScene");

        // ������ʾ������
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene.IsValid())
        {
            foreach (var go in mainScene.GetRootGameObjects())
            {
                go.SetActive(true);
            }
            // ����������������Ϊ�����
            SceneManager.SetActiveScene(mainScene);
        }

        // ����ս����
        CombatResultResolver.Instance.CheckAndResolveCombat();
    }

}