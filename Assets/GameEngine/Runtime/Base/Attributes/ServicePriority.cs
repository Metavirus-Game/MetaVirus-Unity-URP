using System;
using GameEngine.Common;
using static GameEngine.Common.EngineConsts;

namespace GameEngine.Base.Attributes
{
    /**
     * 服务器启动优先权，数字越大越靠后，默认1000
     */
    public class ServicePriority : Attribute
    {
        public int Order { get; }

        public ServicePriority(int order = ServicePriorityValue.OrderDefault)
        {
            this.Order = order;
        }
    }
}