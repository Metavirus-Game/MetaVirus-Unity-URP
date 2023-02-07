//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cfg.skill
{
    /// <summary>
    /// 技能伤害计算方式
    /// </summary>
    public enum AtkCalcType
    {
        /// <summary>
        /// 占位用
        /// </summary>
        None = 0,
        /// <summary>
        /// 技能伤害计算方式，直接读取伤害值计算
        /// </summary>
        Value = 1,
        /// <summary>
        /// 技能伤害计算方式，自身攻击力+附加值
        /// </summary>
        Additional = 2,
        /// <summary>
        /// 技能伤害计算方式，技能释放者攻击力 * 伤害百分比
        /// </summary>
        Percent = 3,
    }
}
