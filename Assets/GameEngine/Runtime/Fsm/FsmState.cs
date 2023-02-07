using System;
using System.Collections;

namespace GameEngine.Fsm
{
    public abstract class FsmState<T> where T : class
    {
        public FsmEntity<T> Fsm { get; internal set; }

        public string Name => GetType().Name;

        public FsmState()
        {
        }

        /**
         * FsmEntity初始化时会调用其包含的所有FsmState的OnInit方法
         */
        public virtual void OnInit(FsmEntity<T> fsm)
        {
        }

        /**
         * OnPrepare执行完毕后调用
         */
        public virtual void OnEnter(FsmEntity<T> fsm)
        {
        }

        /**
         * FsmState状态轮询
         * elapseTime 和 realElapseTime都是deltaTime
         */
        public virtual void OnUpdate(FsmEntity<T> fsm, float elapseTime, float realElapseTime)
        {
        }

        /**
         * 离开当前FsmState时调用
         */
        public virtual void OnLeave(FsmEntity<T> fsm, bool isShutdown)
        {
        }

        /**
         * FsmState所在FsmEntity被销毁时调用
         */
        public virtual void OnDestroy(FsmEntity<T> fsm)
        {
        }

        public virtual void OnPause(FsmEntity<T> fsm)
        {
        }

        public virtual void OnResume(FsmEntity<T> fsm)
        {
        }

        protected void ChangeState<TFsmState>(FsmEntity<T> fsm) where TFsmState : FsmState<T>
        {
            fsm.ChangeState<TFsmState>();
        }

        protected void ChangeState(Type state)
        {
            Fsm.ChangeState(state);
        }
    }
}