using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Base.Exception;
using GameEngine.ObjectPool;

namespace GameEngine.Fsm
{
    public interface IFsmEntity
    {
        public string Name { get; }

        public int FsmStateCount { get; }

        public bool IsRunning { get; }

        public bool IsDestoryed { get; }

        public string CurrStateName { get; }

        public float CurrStateTime { get; }

        public void Start(Type stateType);


        public void ChangeState(Type stateType);

        void Update(float elapseTime, float realElapseTime);

        public void Shutdown();
    }
}