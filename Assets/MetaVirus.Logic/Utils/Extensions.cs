using MetaVirus.Net.Messages.Common;
using UnityEngine;

namespace MetaVirus.Logic.Utils
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this PBVector3 vec)
        {
            var ret = new Vector3(vec.X, vec.Y, vec.Z);
            return ret;
        }

        public static PBVector3 ToPbVector3(this Vector3 vec)
        {
            var pbVec = new PBVector3
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z
            };
            return pbVec;
        }
    }
}