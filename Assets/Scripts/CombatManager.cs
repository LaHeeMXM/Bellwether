using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    public CombatantData playerData;
    public CombatantData enemyData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartCombat(CombatantData player, CombatantData enemy)
    {
        Debug.Log(player.unitName + " vs " + enemy.unitName);

        this.playerData = player;
        this.enemyData = enemy;

        SceneManager.LoadScene("CombatScene"); 
    }
}