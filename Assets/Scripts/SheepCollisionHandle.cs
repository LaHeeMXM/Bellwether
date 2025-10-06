using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepCollisionHandle : MonoBehaviour
{
    BattleHead _battleHead;

    void OnCollisionEnter(Collision info)
    {
        
        var oppositeNode = info.collider.GetComponent<BattleNode>();
        var selfNode = GetComponent<BattleNode>();


        if(oppositeNode == null || selfNode == null) return;
        //只有head这边触发
        if (!selfNode.IsHead()) return;
        if(selfNode.IsPlayer() == oppositeNode.IsPlayer()) return;

        CombatantData targetNodeData;
        CombatantData playerHeadData;
        CombatantData enemyHeadData;

        targetNodeData = oppositeNode.GetCombatantData();
        if (selfNode.IsPlayer())
        {
            playerHeadData = selfNode.GetCombatantData();
            enemyHeadData = oppositeNode.GetHead().GetList()[0].GetCombatantData();
        }
        else {
            playerHeadData = oppositeNode.GetHead().GetList()[0].GetCombatantData();
            enemyHeadData = selfNode.GetCombatantData();
        }

        BattleHead playerHead = selfNode.IsPlayer() ? selfNode.GetHead() : oppositeNode.GetHead();
        BattleHead enemyHead = selfNode.IsPlayer() ? oppositeNode.GetHead() : selfNode.GetHead();
        CombatResultResolver.Instance.RegisterCombatants(playerHead, enemyHead);


        Debug.Log("Into Combat!");
        CombatManager.Instance.StartCombat(playerHeadData,enemyHeadData,targetNodeData,oppositeNode.IsPlayer());
    }
    void Awake()
    {
        _battleHead = GetComponent<BattleNode>().GetHead();

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
