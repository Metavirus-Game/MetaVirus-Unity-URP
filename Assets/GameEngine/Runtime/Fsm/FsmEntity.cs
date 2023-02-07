using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Base.Exception;
using GameEngine.ObjectPool;
using UnityEngine;

namespace GameEngine.Fsm
{
    public class FsmEntity<T> : IFsmEntity, IRecyclable where T : class
    {
        private T _owner;
        private readonly Dictionary<Type, FsmState<T>> _states = new Dictionary<Type, FsmState<T>>();
        protected FsmState<T> currState = null;
        protected float currStateTime = 0;
        private bool _isDestoryed = true;
        private string _name;

        private FsmState<T> pausedState = null;

        public FsmEntity()
        {
        }

        public string Name => _name;

        public T Owner => _owner;
        public Type OwnerType => typeof(T);

        public int FsmStateCount => _states.Count;

        public bool IsRunning => currState != null;

        public bool IsPaused => currState == null && pausedState != null;

        public bool IsDestoryed => _isDestoryed;

        public FsmState<T> CurrState => currState;

        public string CurrStateName => currState?.GetType().FullName;

        public float CurrStateTime => currStateTime;

        private static ObjectPool<FsmEntity<T>> FsmPool
        {
            get
            {
                var opService = GameFramework.GetService<ObjectPoolService>();
                var pool = opService.GetObjectPool<FsmEntity<T>>("fsm");
                return pool;
            }
        }

        public static FsmEntity<T> Create(string name, T owner, params FsmState<T>[] states)
        {
            if (owner == null)
            {
                throw new GameEngineException("owner can't be null");
            }

            if (states == null || states.Length == 0)
            {
                throw new GameEngineException("states must have one item at least");
            }

            var fsm = FsmPool.Get<FsmEntity<T>>();
            fsm.Init(name, owner, states);

            return fsm;
        }

        protected void Init(string name, T owner, params FsmState<T>[] states)
        {
            _name = name;
            _owner = owner;
            _isDestoryed = false;
            currState = null;

            foreach (var state in states)
            {
                var t = state.GetType();
                if (_states.ContainsKey(t))
                {
                    throw new GameEngineException(
                        $"FSM name[{name}] type[{typeof(T).Name}] state[{state.GetType().FullName}] is already existed");
                }

                _states.Add(t, state);
                state.Fsm = this;
                state.OnInit(this);
            }
        }

        public TFsmState GetState<TFsmState>() where TFsmState : FsmState<T>
        {
            var t = typeof(TFsmState);
            return (TFsmState)GetState(t);
        }

        public FsmState<T> GetState(Type stateType)
        {
            return _states.TryGetValue(stateType, out var fsmState) ? fsmState : null;
        }

        public virtual void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new GameEngineException($"FSM name[{Name}] type[{typeof(T).Name}] has already started");
            }

            var state = GetState(stateType);

            currState = state ?? throw new GameEngineException(
                $"FSM name[{Name}] type[{typeof(T).Name}] state[{stateType.Name}] not found");

            currStateTime = 0;
            currState.OnEnter(this);

            //Debug.Log($"FsmEntity[{_name}] - State[{state.Name}] --- OnEnter");
        }


        public void Start<TFsmState>() where TFsmState : FsmState<T>
        {
            Start(typeof(TFsmState));
        }

        public virtual void ChangeState(Type stateType)
        {
            if (!IsRunning)
            {
                throw new GameEngineException($"FSM name[{Name}] type[{typeof(T).Name}] has not started");
            }

            var state = GetState(stateType);

            if (state == null)
            {
                throw new GameEngineException(
                    $"FSM name[{Name}] type[{typeof(T).Name}] state[{stateType.Name}] not found");
            }

            var oldState = currState;
            currState = state;
            currStateTime = 0;
            oldState.OnLeave(this, false);
            currState.OnEnter(this);
        }

        public void ChangeState<TFsmState>() where TFsmState : FsmState<T>
        {
            ChangeState(typeof(TFsmState));
        }

        public void Pause()
        {
            if (IsRunning)
            {
                pausedState = currState;
                currState = null;
                pausedState.OnPause(this);
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                currState = pausedState;
                pausedState = null;
                currState.OnResume(this);
            }
        }

        public void Update(float elapseTime, float realElapseTime)
        {
            if (!IsRunning)
            {
                return;
            }

            currStateTime += elapseTime;
            currState?.OnUpdate(this, elapseTime, realElapseTime);
        }

        public void Shutdown()
        {
            if (IsRunning)
            {
                currState.OnLeave(this, true);
            }

            foreach (var state in _states.Values)
            {
                state.OnDestroy(this);
            }

            _states.Clear();
            currState = null;
            currStateTime = 0;

            FsmPool.Release(this);
        }

        public void OnSpawn()
        {
        }

        public void OnRecycle()
        {
            _isDestoryed = true;
        }

        public void OnDestroy()
        {
        }
    }
}