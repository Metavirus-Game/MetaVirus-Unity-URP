using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.Frame;
using MetaVirus.Logic.Service.Battle.Instruction;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleFsm
{
    public class BattleStateAction : FsmState<BaseBattleInstance>
    {
        private ActionFrame _currFrame;
        private BattleUnitEntity _actionUnit;

        private BattleInstruction _instruction;

        public override void OnInit(FsmEntity<BaseBattleInstance> fsm)
        {
        }

        public override void OnEnter(FsmEntity<BaseBattleInstance> fsm)
        {
            var bi = fsm.Owner;
            _currFrame = bi.CurrentActionFrame(true);
            // Debug.Log(
            //     $"[Time {_currFrame.FrameTime}] \t-- Unit[{_currFrame.ActionUnitId:x5}] Action[{_currFrame.FrameType}]");

            _actionUnit = bi.GetUnitEntity(_currFrame.ActionUnitId);
            RunCurrentFrame();
        }

        private void RunCurrentFrame()
        {
            BattleInstruction inst = null;

            switch (_currFrame.FrameType)
            {
                case FrameType.SkillCast:
                    //施放技能      
                {
                    var frame = (SkillCastActionFrame)_currFrame;
                    inst = new CastSkillInstruction(frame.Data);
                    break;
                }
                case FrameType.AttachBuff:
                    //附加buff
                {
                    var frame = (BuffAttachActionFrame)_currFrame;
                    inst = new BuffAttachInstruction(frame.Data.AttachData);
                    break;
                }
                case FrameType.BuffAction:
                    //buff效果

                {
                    var frame = (BuffActionFrame)_currFrame;
                    inst = new BuffEffectInstruction(frame.Data);
                    break;
                }
                case FrameType.PropertiesChange:
                    //属性变化
                {
                    var frame = (PropertiesChangeActionFrame)_currFrame;
                    inst = new PropertiesChangeInstruction(frame.Data);
                    break;
                }
            }

            if (inst != null)
            {
                _instruction = inst;
                _actionUnit.RunInstruction(inst);
            }
        }

        public override void OnUpdate(FsmEntity<BaseBattleInstance> fsm, float elapseTime, float realElapseTime)
        {
            //只检测和本次Action相关的Instruction，不理会其他的Instruction，BattleUnitEntity会自己执行
            if (_instruction.State == InstructionState.Done)
            {
                //本次指令完成 
                ChangeState<BattleStateIncActionEnergy>(Fsm);
            }
        }
    }
}