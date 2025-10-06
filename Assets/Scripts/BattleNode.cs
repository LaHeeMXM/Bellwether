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

public class BattleNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public GameObject sheepPrefab;

    public string sheepName;
    public string buffName;
    public BuffParams buffParam;

    [SerializeField]
    private UnitAttribute baseAttribute;

    [HideInInspector]
    public UnitAttribute finalAttribute;
    [HideInInspector]
    public int Level;

    public Action<BattleHead, int> onAdd;
    public Action<BattleHead> onRemove;

    ShowUnitInfo _showUnitInfo;
    int _index = 0;
    BattleHead _head;

    void Awake()
    {
        //_showUnitInfo = GetComponentInChildren<ShowUnitInfo>();
    }

    public void CaculateAttribute(BattleHead head, int index, int newLevel)
    {
        this.Level = newLevel;

        var nodeList = head.GetList();
        finalAttribute = baseAttribute;

        for (int i = 0; i < nodeList.Count; i++)
        {
            BuffInfo info = new BuffInfo(i, nodeList[i].Level, index, buffParam);
            finalAttribute += Buff.Execute(nodeList[i].buffName, info);
        }

        //_showUnitInfo.SetData(finalAttribute);
        _index = index;
    }

    public CombatantData GetCombatantData()
    {
        CombatantData data = new CombatantData();
        data.unitName = sheepName;
        data.Attack = finalAttribute.Attack;
        data.Defense = finalAttribute.Defense;
        data.Health = finalAttribute.Health;
        data.Level = this.Level;
        data.Location = _index;

        int assistance = 0;
        if (_head != null && _head.GetList() != null)
        {
            var headList = _head.GetList();
            int k = 2;
            for (int i = _index + 1; i < headList.Count; i++)
            {
                assistance += headList[i].finalAttribute.Attack / k;
                k *= 2;
            }
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
        if (_head == null) return false;
        return _head.isPlayer;
    }

    #region Input Events (✨ 核心修正区域 ✨)

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPlayer() || eventData.button != PointerEventData.InputButton.Left) return;

        PlayerInputManager.Instance.OnNodeClicked(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsPlayer() || eventData.button != PointerEventData.InputButton.Left) return;

        PlayerInputManager.Instance.OnNodeReleased();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIMainManager.Instance.ShowTooltip(this);
        PlayerInputManager.Instance.OnNodeHoverEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIMainManager.Instance.HideTooltip();
        PlayerInputManager.Instance.OnNodeHoverExit(this);
    }

    #endregion
}