using System.Collections;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.Fsm;
using GameEngine.Procedure;
using UnityEngine;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class ReloadAfterUpdateProcedure : ProcedureBase
    {
        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            GameFramework.Inst.StartCoroutine(BackToMainPageProcedure());
        }

        private IEnumerator BackToMainPageProcedure()
        {
            yield return new WaitForSeconds(1);
            ChangeProcedure<MainPageProcedure>();
        }
    }
}