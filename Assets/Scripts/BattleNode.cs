using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

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

public class BattleNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public GameObject sheepPrefab;

    public string sheepName;
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

    //[HideInInspector]
    public int Level;
    //刚入队时触发
    public Action<BattleHead, int> onAdd;
    //离队时触发
    public Action<BattleHead> onRemove;

    ShowUnitInfo _showUnitInfo;
    int _index = 0;
    BattleHead _head;

    public void CaculateAttribute(BattleHead head, int index, int newLevel)
    {
        // 更新自己缓存的等级
        this.Level = newLevel;

        var nodeList = head.GetList();
        finalAttribute = baseAttribute;

        for (int i = 0; i < nodeList.Count; i++)
        {
            // 使用 nodeList[i].Level 来获取Buff提供者的等级
            // 因为在UpdateNodes循环中，所有节点的Level都已经被正确更新了
            BuffInfo info = new BuffInfo(i, nodeList[i].Level, index, buffParam);
            finalAttribute += Buff.Execute(nodeList[i].buffName, info);
        }

        _showUnitInfo.SetData(finalAttribute);
        _index = index;
    }

    void Awake()
    {
        _showUnitInfo = GetComponentInChildren<ShowUnitInfo>();
    }


    // 当鼠标指针按下
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPlayer()) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("点击了节点: " + gameObject.name + " at index " + _index);
            // 通知输入管理器，我们开始了一个换位操作
            PlayerInputManager.Instance.StartNodeSwap(this);
        }
    }

    // 当鼠标指针抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsPlayer()) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 通知输入管理器，我们结束了换位操作
            PlayerInputManager.Instance.EndNodeSwap();
        }
    }



    //获取战斗数据
    public CombatantData GetCombatantData()
    {
        CombatantData data = new CombatantData();
        data.unitName = sheepName;
        data.Attack = finalAttribute.Attack;
        data.Defense = finalAttribute.Defense;
        data.Health = finalAttribute.Health;
        data.Level = this.Level; // 使用缓存的等级
        data.Location = _index;

        int assistance = 0;
        int k = 2;
        //计算增援数值

        for (int i = _index+1; i < _head.GetList().Count; i++)
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