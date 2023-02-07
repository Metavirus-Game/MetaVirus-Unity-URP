using GameEngine.Procedure;
using GameEngine.Sound;

namespace GameEngine.Event
{
    public struct ProcedureChangedEvent
    {
        public ProcedureBase OldProcedure { get; }
        public ProcedureBase CurrProcedure { get; }

        public ProcedureChangedEvent(ProcedureBase old, ProcedureBase curr)
        {
            OldProcedure = old;
            CurrProcedure = curr;
        }
    }

    public struct SoundPlayerEvent
    {
        public enum Type
        {
            StartPlaying,
            Completed,
            LoopCompleted,
            Stopped,
        }

        public SoundClip SoundClip { get; }
        public Type EvtType { get; }

        public SoundPlayerEvent(SoundClip soundClip, Type evtType)
        {
            SoundClip = soundClip;
            EvtType = evtType;
        }
    }
}