using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
[Serializable]
public class UnitAttribute
{
    public int Health;
    public int Attack;
    public int Defense;
    public UnitAttribute(int health, int attack, int defense)
    {
        Health = health;
        Attack = attack;
        Defense = defense;
    }
    public static UnitAttribute operator +(UnitAttribute a, UnitAttribute b)
    {
        return new UnitAttribute(a.Health + b.Health, a.Attack + b.Attack, a.Defense + b.Defense);
    }
    public static UnitAttribute operator *(UnitAttribute a, int b)
    {
        return new UnitAttribute(a.Health * b, a.Attack * b, a.Defense * b);
    }
}

public class BattleNode : MonoBehaviour
{
    //基础属性
    [SerializeField]
    private UnitAttribute baseAttribute;
    //前向增益,给所有前方节点(索引小于自身)提供增益
    public UnitAttribute forwardBonus;
    //后向增益,给所有后方节点(索引大于自身)提供增益
    public UnitAttribute backwardBonus;
    //自身增益
    public UnitAttribute selfBonus;

    [HideInInspector]
    //最终属性
    public UnitAttribute finalAttribute;
    [HideInInspector]
    public int Level;
    //刚入队时触发
    public Action<BattleHead, int> onAdd;
    //离队时触发
    public Action<BattleHead> onRemove;

    ShowUnitInfo _showUnitInfo;
    int _index = 0;
    BattleHead _head;
    public void CaculateAttribute(BattleHead head, int index)
    {
        var nodeList = head.GetList();
        finalAttribute = baseAttribute;
        for (int i = index - 1; i >= 0; i--)
        {
            finalAttribute += nodeList[i].backwardBonus * nodeList[i].Level;
        }
        for (int i = index + 1; i < nodeList.Count; i++)
        {
            finalAttribute += nodeList[i].forwardBonus * nodeList[i].Level;
        }
        finalAttribute.Health += selfBonus.Health;
        finalAttribute.Attack += selfBonus.Attack;
        finalAttribute.Defense += selfBonus.Defense;

        _showUnitInfo.SetData(finalAttribute);
        _index = index;
        Debug.Log("CaculateAttribute:" + "Level" + Level + "," + finalAttribute.Attack + "," + finalAttribute.Health + "," + finalAttribute.Defense);
    }
    void Update()
    {

        _showUnitInfo.gameObject.SetActive(Input.GetKey(KeyCode.LeftShift));

    }

    void Awake()
    {
        _showUnitInfo = GetComponentInChildren<ShowUnitInfo>();
        Level = 1;
    }

    public CombatantData GetCombatantData()
    {
        CombatantData data = new CombatantData();
        data.Attack = finalAttribute.Attack;
        data.Defense = finalAttribute.Defense;
        data.Health = finalAttribute.Health;
        data.Level = Level;
        int assistance = 0;
        int k = 2;
        for (int i = _index; i < _head.GetList().Count; i++)
        {
            assistance += _head.GetList()[i].finalAttribute.Attack / k;
            k *= 2;
        }
        data.Assistance = assistance;
        return data;
    }
    public BattleNode GetHeadNode()
    {
        return _head.GetComponent<BattleNode>();
    }
    public bool IsHead()
    {
        return (GetComponent<BattleHead>() != null);
    }
}