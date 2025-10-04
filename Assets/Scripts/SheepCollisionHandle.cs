using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepCollisionHandle : MonoBehaviour
{
    public bool isPlayer => (GetComponentInParent<PlayerController>() != null);
    void OnCollisionEnter(Collision info)
    {
        Debug.Log("Collision!");
        var oppositeNode = info.collider.GetComponent<BattleNode>();
        var selfNode = GetComponent<BattleNode>();
        if(oppositeNode== null || selfNode==null) return;
        CombatantData targetNodeData;
        CombatantData playerHeadData;
        CombatantData enemyHeadData;
        if (!oppositeNode.IsHead() && !selfNode.IsHead()) return;

        if (!oppositeNode.IsHead())
        {
            targetNodeData = oppositeNode.GetCombatantData();
        }
        else
        {
            targetNodeData = selfNode.GetCombatantData();
        }

        if (isPlayer)
        {
            playerHeadData = GetComponent<BattleNode>().GetHeadNode().GetCombatantData();
            enemyHeadData = oppositeNode.GetHeadNode().GetCombatantData();
        }
        else
        {
            enemyHeadData = GetComponent<BattleNode>().GetHeadNode().GetCombatantData();
            playerHeadData = oppositeNode.GetHeadNode().GetCombatantData();
        }
        Debug.Log("Into Combat!");
        CombatManager.Instance.StartCombat(playerHeadData,enemyHeadData,selfNode.GetCombatantData(),isPlayer);
    }
    void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
