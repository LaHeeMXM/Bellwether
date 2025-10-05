using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffParams
{
    //系数1
    public int a;
    //系数2
    public int b;
}


[Serializable]
public class BuffInfo
{
    //持有者的索引
    public int index;
    //持有者的等级
    public int level;
    //buff目标的索引
    public int targetIndex;
    //buff参数
    public BuffParams param;
    public BuffInfo(int index, int level, int targetIndex, BuffParams param)
    {
        this.index = index;
        this.level = level;
        this.targetIndex = targetIndex;
        this.param = param;
    }
}
public static class Buff
{
    //可以使用字符串来配置Buff函数
    public static Dictionary<string, BuffFunction> buffFunctionDict = new Dictionary<string, BuffFunction>{
        {"HPF",AddHealthForward},
        {"HPB",AddHealthForward},
        {"ATKF",AddAttackForward},
        {"ATKB",AddAttackBackward},

    };



    public static UnitAttribute Execute(string funcName, BuffInfo info)
    {
        if(!buffFunctionDict.ContainsKey(funcName)) return new UnitAttribute(0,0,0);
        return buffFunctionDict[funcName](info);
    }


    #region Buff Functions
    public static UnitAttribute AddHealthForward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index > info.targetIndex) res += new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        return res;
    }
    public static UnitAttribute AddHealthBackward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index < info.targetIndex) 
            res += new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        return res;
    }
    public static UnitAttribute AddAttackForward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if(info == null) Debug.Log("Info is null");
        if (info.param == null) Debug.Log("Info.Param is null");
        if (info.index > info.targetIndex)
            res += new UnitAttribute(info.level * info.param.a + info.param.b, 0, 0);
        return res;
    }
    public static UnitAttribute AddAttackBackward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index < info.targetIndex)
            res += new UnitAttribute(info.level * info.param.a + info.param.b, 0,0);
        return res;
    }
    #endregion



}
public delegate UnitAttribute BuffFunction(BuffInfo info);