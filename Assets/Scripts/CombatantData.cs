using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class CombatantData
{
    public string unitName;
    public GameObject unitPrefab;
    public int Health;
    public int Attack;
    public int Defense;

    public int Level;
    public int Location; //队伍中位置

    public int Assistance; //后方提供的攻击力加成



}

