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
        
        Debug.Log("CaculateAttribute:"+"Level" + Level + ","+ finalAttribute.Attack + "," + finalAttribute.Health + "," + finalAttribute.Defense);
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

}