namespace GameEngine.Sound
{
    public static class SoundConstants
    {
        public enum SoundPlayerStatus
        {
            Idle,
            Playing,
            Fadein,
            Fadeout
        }

        /// <summary>
        /// 音效减弱方式
        /// </summary>
        public enum SoundDecreaseMethod
        {
            立即,
            渐变
        }
    }
}