using System;

namespace GameEngine.Base.Attributes
{
    /**
     * 流程属性，标注流程类
     * isEntry表示为入口流程类，一个项目只能有一个入口流程，配置多个会出warning，只有第一个载入的生效
     */
    public class ProcedureAttribute : Attribute
    {
        public bool IsEntry { get; } = false;

        internal string[] LoadConditions { get; }

        public ProcedureAttribute(bool isEntry = false, params string[] loadConditions)
        {
            IsEntry = isEntry;
            LoadConditions = loadConditions;
        }
    }
}