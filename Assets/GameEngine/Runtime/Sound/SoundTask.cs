using UnityEngine.Events;

namespace GameEngine.Sound
{
    public class SoundTask
    {
        private SoundPlayer _player;
        internal UnityAction<SoundPlayer> ActionOnStopped;

        internal SoundTask(SoundPlayer player)
        {
            _player = player;
        }

        public SoundTask OnStopped(UnityAction<SoundPlayer> onStopped)
        {
            ActionOnStopped = onStopped;
            return this;
        }

        internal void InvokeAction()
        {
            ActionOnStopped?.Invoke(_player);
        }
    }
}