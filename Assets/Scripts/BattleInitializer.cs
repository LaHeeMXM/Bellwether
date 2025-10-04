using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CombatantData playerData = CombatManager.Instance.playerData;
        CombatantData enemyData = CombatManager.Instance.enemyData;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
