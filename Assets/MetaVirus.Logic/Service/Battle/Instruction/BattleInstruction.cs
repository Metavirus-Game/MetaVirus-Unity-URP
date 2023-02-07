using cfg;
using GameEngine;
using Google.Protobuf;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public enum InstructionType
    {
        CastSkill = 1,
        AttachBuff = 2,
        BuffEffect = 3,
        PropertiesChange = 4,
        OnDamage = 5,
        Relive = 6,
        Dead
    }

    public enum InstructionState
    {
        Init = 1,
        Running = 2,
        Done = 3
    }

    public enum InstructionAddMethod
    {
        AddLast = 1,
        AddFirst = 2,
    }

    public class BattleInstruction
    {
        protected readonly IMessage InsData;

        public InstructionType Type { get; }

        public virtual InstructionAddMethod AddMethod { get; } = InstructionAddMethod.AddLast;

        private InstructionState _state = InstructionState.Init;

        public InstructionState State
        {
            get => _state;
            private set
            {
                if (Equals(_state, value)) return;
                _state = value;
            }
        }


        public BattleInstruction(InstructionType type, IMessage data = null)
        {
            Type = type;
            InsData = data;
        }

        public T GetInstructionData<T>() where T : class, IMessage<T>
        {
            return InsData as T;
        }

        public void SetRunning()
        {
            State = InstructionState.Running;
        }

        public void SetDone()
        {
            State = InstructionState.Done;
        }

        public void OnAdded()
        {
        }

        public void OnRemoved()
        {
        }
    }

    public class BattleInstruction<T> : BattleInstruction where T : class, IMessage<T>
    {
        public GameDataService GameData { get; }

        public T Data => (T)InsData;

        public BattleInstruction(InstructionType type, T data) : base(type, data)
        {
            GameData = GameFramework.GetService<GameDataService>();
        }
    }
}