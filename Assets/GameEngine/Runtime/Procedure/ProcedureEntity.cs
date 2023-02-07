using System;
using System.Collections;
using GameEngine.Base.Exception;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using UnityEngine;

namespace GameEngine.Procedure
{
    public class ProcedureEntity : FsmEntity<ProcedureService>
    {
        /**
         * 状态机是否正在切换中
         * 切换状态机是一个异步操作
         * 上一个切换操作没有完成时，不能再次切换
         */
        private bool _isSwitching = false;

        private EventService _eventService;

        private ProcedureBase CurrProcedure
        {
            get => (ProcedureBase)currState;
            set => currState = value;
        }

        public new static ProcedureEntity Create(string name, ProcedureService owner,
            params FsmState<ProcedureService>[] states)
        {
            if (owner == null)
            {
                throw new GameEngineException("owner can't be null");
            }

            if (states == null || states.Length == 0)
            {
                throw new GameEngineException("states must have one item at least");
            }

            var fsm = new ProcedureEntity();
            fsm.Init(name, owner, states);

            return fsm;
        }

        public override void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new GameEngineException($"FSM name[{Name}] type[{nameof(ProcedureService)}] has already started");
            }

            _eventService = GameFramework.GetService<EventService>();

            var state = GetState(stateType) as ProcedureBase;

            CurrProcedure = state ?? throw new GameEngineException(
                $"FSM name[{Name}] type[{nameof(ProcedureService)}] state[{stateType.Name}] not found");

            currStateTime = 0;
            GameFramework.Inst.StartCoroutine(CurrProcedure.OnPrepare(this));
            currState.OnEnter(this);

            _eventService.Emit(Events.Engine.ProcedureChanged, new ProcedureChangedEvent(null, CurrProcedure));

            Debug.Log($"FsmEntity[{Name}] - State[{state.Name}] --- OnEnter");
        }

        public override void ChangeState(Type stateType)
        {
            GameFramework.Inst.StartCoroutine(ChangeStateEnum(stateType));
        }

        private IEnumerator ChangeStateEnum(Type stateType)
        {
            if (!IsRunning)
            {
                throw new GameEngineException($"FSM name[{Name}] type[{nameof(ProcedureService)}] has not started");
            }

            var state = GetState(stateType) as ProcedureBase;

            if (state == null)
            {
                throw new GameEngineException(
                    $"FSM name[{Name}] type[{nameof(ProcedureService)}] state[{stateType.Name}] not found");
            }

            var oldState = CurrProcedure;
            CurrProcedure = null;
            if (_isSwitching)
            {
                //还有运行中的切换状态行为，等待
                yield return new WaitUntil(() => _isSwitching == false);
            }

            _isSwitching = true;
            yield return state.OnPrepare(this);

            currState = state;
            Debug.Log($"FSM name[{Name}] Current State Is {stateType.Name}");
            currStateTime = 0;
            oldState.OnLeave(this, false);
            currState.OnEnter(this);
            _eventService.Emit(Events.Engine.ProcedureChanged, new ProcedureChangedEvent(oldState, CurrProcedure));

            _isSwitching = false;
        }
    }
}