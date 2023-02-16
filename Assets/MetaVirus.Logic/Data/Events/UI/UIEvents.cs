using GameEngine.Event;
using MetaVirus.Logic.Service.UI;

namespace MetaVirus.Logic.Data.Events.UI
{
    public struct TopLayerFullscreenUIChangedEvent
    {
        public enum State
        {
            Shown,
            Closing
        }

        public State EventState { get; }
        public BaseUIWindow EventWnd { get; }

        public TopLayerFullscreenUIChangedEvent(State state, BaseUIWindow window)
        {
            EventState = state;
            EventWnd = window;
        }
    }
}