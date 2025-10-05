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
    public UnitAttribute( int attack,int health, int defense)
    {
        Health = health;
        Attack = attack;
        Defense = defense;
    }
    public static UnitAttribute operator +(UnitAttribute a, UnitAttribute b)
    {
        return new UnitAttribute(a.Attack + b.Attack,a.Health + b.Health, a.Defense + b.Defense);
    }
    public static UnitAttribute operator *(UnitAttribute a, int b)
    {
        return new UnitAttribute(a.Attack * b, a.Health * b,a.Defense * b);
    }
}

public class BattleNode : MonoBehaviour
{
    [HideInInspector]
    public GameObject sheepPrefab;
    public string buffName;
    public BuffParams buffParam;

    //基础属性
    [SerializeField]
    private UnitAttribute baseAttribute;
    // //前向增益,给所有前方节点(索引小于自身)提供增益
    // public UnitAttribute forwardBonus;
    // //后向增益,给所有后方节点(索引大于自身)提供增益
    // public UnitAttribute backwardBonus;
    // //自身增益
    // public UnitAttribute selfBonus;


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
        // for (int i = index - 1; i >= 0; i--)
        // {
        //     finalAttribute += nodeList[i].backwardBonus * nodeList[i].Level;
        // }
        // for (int i = index + 1; i < nodeList.Count; i++)
        // {
        //     finalAttribute += nodeList[i].forwardBonus * nodeList[i].Level;
        // }
        // finalAttribute.Health += selfBonus.Health;
        // finalAttribute.Attack += selfBonus.Attack;
        // finalAttribute.Defense += selfBonus.Defense;
        for (int i = 0; i < nodeList.Count; i++)
        {
            //以自身为目标，结算所有其他单位的buff加成
            BuffInfo info = new BuffInfo(i, nodeList[i].Level, index, buffParam);
            finalAttribute += Buff.Execute(nodeList[i].buffName, info);
        }

        _showUnitInfo.SetData(finalAttribute);
        _index = index;
        Debug.Log("CaculateAttribute:" + "Level" + Level + "," + finalAttribute.Attack + "," + finalAttribute.Health + "," + finalAttribute.Defense);
    }

    void Awake()
    {
        _showUnitInfo = GetComponentInChildren<ShowUnitInfo>();
        Level = 1;
    }

    //获取战斗数据
    public CombatantData GetCombatantData()
    {
        CombatantData data = new CombatantData();
        data.Attack = finalAttribute.Attack;
        data.Defense = finalAttribute.Defense;
        data.Health = finalAttribute.Health;
        data.Level = Level;
        int assistance = 0;
        int k = 2;
        //计算增援数值
        for (int i = _index; i < _head.GetList().Count; i++)
        {
            assistance += _head.GetList()[i].finalAttribute.Attack / k;
            k *= 2;
        }
        data.Assistance = assistance;
        data.unitPrefab = sheepPrefab;
        return data;
    }
    public BattleHead GetHead()
    {
        return _head;
    }
    public void SetHead(BattleHead head)
    {
        _head = head;
    }
    public int GetIndex()
    {
        return _index;
    }
    public bool IsHead()
    {
        return _index == 0;
    }
    public bool IsPlayer()
    { 
        return _head.isPlayer;
    }
}