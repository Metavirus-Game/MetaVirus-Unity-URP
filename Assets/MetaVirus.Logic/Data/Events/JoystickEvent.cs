using UnityEngine;

namespace MetaVirus.Logic.Data.Events
{
    public struct JoystickEvent
    {
        public enum JoystickEventType
        {
            Moving,
            Stopped
        }

        public JoystickEventType Type { get; }
        public Vector3 Postion { get; }

        public JoystickEvent(JoystickEventType type, Vector3 postion)
        {
            Type = type;
            Postion = postion;
        }
    }
}