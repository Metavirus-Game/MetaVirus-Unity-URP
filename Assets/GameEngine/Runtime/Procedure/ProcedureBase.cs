using System;
using System.Collections;
using GameEngine.Fsm;

namespace GameEngine.Procedure
{
    [Serializable]
    public abstract class ProcedureBase : FsmState<ProcedureService>
    {
        public ProcedureEntity FsmEntity => (ProcedureEntity)Fsm;

        /**
         * 进入当前ProcedureBase时调用
         * OnPrepare是一个协程，只有完全执行完毕后，才会卸载上一个FsmState
         * OnPrepare在OnEnter之前调用
         */
        public virtual IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            yield return null;
        }

        public void ChangeProcedure<T>() where T : ProcedureBase
        {
            ChangeState<T>(Fsm);
        }

        public void ChangeProcedure(Type procedureType)
        {
            ChangeState(procedureType);
        }
    }
}