using cfg.battle;
using Google.Protobuf;
using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.Instruction
{
    public class ProjectileSimulatorInstruction : BattleInstruction
    {
        public ProjectileData ProjectileData { get; }
        public FrameSkillCastDataPb[] CastData { get; }

        public ProjectileSimulatorInstruction(ProjectileData projectileData,FrameSkillCastDataPb[] castData) : base(InstructionType.SimulatorProjectile)
        {
            ProjectileData = projectileData;
            CastData = castData;
        }
    }
}