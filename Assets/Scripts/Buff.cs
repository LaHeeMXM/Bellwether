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

        {"HP",AddHealthSelf},
        {"HPF",AddHealthForward},
        {"HPB",AddHealthBackward},
        
        {"ATK",AddAttackSelf},
        {"ATKF",AddAttackForward},
        {"ATKB",AddAttackBackward},
       
        {"DEF",AddDefenceSelf},
        {"DEFF",AddDefenceForward},
        {"DEFB",AddDefenceBackward},

        // --- 全体 (All) ---
        {"HPA", AddHealthAll},
        {"ATKA", AddAttackAll},
        {"DEFA", AddDefenseAll},

        // --- 相邻 (Near) ---
        {"HPN", AddHealthNear},
        {"ATKN", AddAttackNear},
        {"DEFN", AddDefenseNear},
        
        // --- 复合自身 (Compound Self) ---
        {"HPATK", AddHealthAttackSelf},
        {"HPDEF", AddHealthDefenseSelf},
        {"ATKDEF", AddAttackDefenseSelf},

        {"BOSS", IMBOSS}
    };



    public static UnitAttribute Execute(string funcName, BuffInfo info)
    {
        if(!buffFunctionDict.ContainsKey(funcName)) return new UnitAttribute(0,0,0);
        return buffFunctionDict[funcName](info);
    }


    #region Buff Functions
    public static UnitAttribute AddHealthSelf(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index == info.targetIndex) 
            res += new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        return res;
    }

    public static UnitAttribute AddHealthForward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index >= info.targetIndex) 
            res += new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        return res;
    }

    public static UnitAttribute AddHealthBackward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index <= info.targetIndex) 
            res += new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        return res;
    }

    public static UnitAttribute AddAttackSelf(BuffInfo info) 
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index == info.targetIndex)
            res += new UnitAttribute(info.level * info.param.a + info.param.b, 0, 0);
        return res;
    }

    public static UnitAttribute AddAttackForward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index >= info.targetIndex)
            res += new UnitAttribute(info.level * info.param.a + info.param.b, 0, 0);
        return res;
    }

    public static UnitAttribute AddAttackBackward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index <= info.targetIndex)
            res += new UnitAttribute(info.level * info.param.a + info.param.b, 0, 0);
        return res;
    }

    public static UnitAttribute AddDefenceSelf(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index == info.targetIndex)
            res += new UnitAttribute(0, 0, info.level * info.param.a + info.param.b);
        return res;
    }

    public static UnitAttribute AddDefenceForward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index >= info.targetIndex)
            res += new UnitAttribute(0, 0, info.level * info.param.a + info.param.b);
        return res;
    }

    public static UnitAttribute AddDefenceBackward(BuffInfo info)
    {
        UnitAttribute res = new UnitAttribute(0, 0, 0);
        if (info.index <= info.targetIndex)
            res += new UnitAttribute(0, 0, info.level * info.param.a + info.param.b);
        return res;
    }

    #endregion

    #region All Buffs (为全体提供加成)

    public static UnitAttribute AddHealthAll(BuffInfo info)
    {
        // 全体Buff，排除对自己生效，避免与自身Buff重复计算
        if (info.index != info.targetIndex)
        {
            return new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        }
        return new UnitAttribute(0, 0, 0);
    }

    public static UnitAttribute AddAttackAll(BuffInfo info)
    {
        if (info.index != info.targetIndex)
        {
            return new UnitAttribute(info.level * info.param.a + info.param.b, 0, 0);
        }
        return new UnitAttribute(0, 0, 0);
    }

    public static UnitAttribute AddDefenseAll(BuffInfo info)
    {
        if (info.index != info.targetIndex)
        {
            return new UnitAttribute(0, 0, info.level * info.param.a + info.param.b);
        }
        return new UnitAttribute(0, 0, 0);
    }

    #endregion

    #region Near Buffs (为前后相邻提供加成)

    public static UnitAttribute AddHealthNear(BuffInfo info)
    {
        // Math.Abs计算索引差的绝对值。如果等于1，说明是紧邻的
        if (Math.Abs(info.index - info.targetIndex) == 1)
        {
            return new UnitAttribute(0, info.level * info.param.a + info.param.b, 0);
        }
        return new UnitAttribute(0, 0, 0);
    }

    public static UnitAttribute AddAttackNear(BuffInfo info)
    {
        if (Math.Abs(info.index - info.targetIndex) == 1)
        {
            return new UnitAttribute(info.level * info.param.a + info.param.b, 0, 0);
        }
        return new UnitAttribute(0, 0, 0);
    }

    public static UnitAttribute AddDefenseNear(BuffInfo info)
    {
        if (Math.Abs(info.index - info.targetIndex) == 1)
        {
            return new UnitAttribute(0, 0, info.level * info.param.a + info.param.b);
        }
        return new UnitAttribute(0, 0, 0);
    }

    #endregion

    #region Compound Self Buffs (为自己提供复合加成)

    public static UnitAttribute AddHealthAttackSelf(BuffInfo info)
    {
        // 同样只对自己生效
        if (info.index == info.targetIndex)
        {
            // 参数a用于HP, 参数b用于ATK (这是一个约定)
            return new UnitAttribute(info.level * info.param.b, info.level * info.param.a, 0);
        }
        return new UnitAttribute(0, 0, 0);
    }

    public static UnitAttribute AddHealthDefenseSelf(BuffInfo info)
    {
        if (info.index == info.targetIndex)
        {
            // 参数a用于HP, 参数b用于DEF
            return new UnitAttribute(0, info.level * info.param.a, info.level * info.param.b);
        }
        return new UnitAttribute(0, 0, 0);
    }

    public static UnitAttribute AddAttackDefenseSelf(BuffInfo info)
    {
        if (info.index == info.targetIndex)
        {
            // 参数a用于ATK, 参数b用于DEF
            return new UnitAttribute(info.level * info.param.a, 0, info.level * info.param.b);
        }
        return new UnitAttribute(0, 0, 0);
    }

    #endregion

    public static UnitAttribute IMBOSS(BuffInfo info)
    {
        return new UnitAttribute(0, 0, 0);
    }

}
public delegate UnitAttribute BuffFunction(BuffInfo info);