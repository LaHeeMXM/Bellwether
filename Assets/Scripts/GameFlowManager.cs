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
        // 游戏开始时，叠加加载主世界
        StartCoroutine(LoadMainSceneAndInitialize());
    }

    private IEnumerator LoadMainSceneAndInitialize()
    {
        // 异步叠加加载MainScene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 2. 将新加载的MainScene设置为活动场景，确保后续所有Instantiate都在此场景中
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene.IsValid())
        {
            SceneManager.SetActiveScene(mainScene);
            Debug.Log("GameFlowManager: MainScene 已被设置为活动场景。");

            // 确保GameManager的生成逻辑在正确的时机、正确的场景中执行
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.Initialize();
            }
            else
            {
                Debug.LogError("GameFlowManager: 在MainScene中没有找到GameManager！");
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

        // 隐藏主世界
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene.IsValid())
        {
            foreach (var go in mainScene.GetRootGameObjects())
            {
                go.SetActive(false);
            }
        }

        // 叠加加载战斗场景
        yield return SceneManager.LoadSceneAsync("CombatScene", LoadSceneMode.Additive);

        // 加载完后，可以设置战斗场景为活动场景
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
        // 异步卸载战斗场景
        yield return SceneManager.UnloadSceneAsync("CombatScene");

        // 重新显示主世界
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        if (mainScene.IsValid())
        {
            foreach (var go in mainScene.GetRootGameObjects())
            {
                go.SetActive(true);
            }
            // 、将主世界重新设为活动场景
            SceneManager.SetActiveScene(mainScene);
        }

        // 触发战后处理
        CombatResultResolver.Instance.CheckAndResolveCombat();
    }

}