using FairyGUI;
using UnityEngine;

namespace GameEngine.Utils
{
    public static class GameEngineUtils
    {
        public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
        {
            var mid = Vector2.Lerp(start, end, t);
            var y = 4 * (-height * t * t + height * t);
            return new Vector2(mid.x, y + Mathf.Lerp(start.y, end.y, t));
        }

        public static Vector2 Bezier(Vector2 start, Vector2 end, Vector2 control, float t)
        {
            var p1 = (1 - t) * (1 - t) * start;
            var p2 = 2 * t * (1 - t) * control;
            var p3 = t * t * end;

            return p1 + p2 + p3;
        }

        /**
         * 将小于0的欧拉角角度转换为正数
         */
        public static Vector3 ProcessEulerAngle(Vector3 euler)
        {
            if (euler.x < 0)
            {
                euler.x = 360 + euler.x;
            }

            if (euler.y < 0)
            {
                euler.y = 360 + euler.y;
            }

            if (euler.z < 0)
            {
                euler.z = 360 + euler.z;
            }

            return euler;
        }

        public static bool IsVectorClose(Vector3 v1, Vector3 v2, bool ignoreY = true)
        {
            if (ignoreY)
            {
                v1.y = v2.y = 0;
            }

            return Vector3.Distance(v1, v2) < 0.1f;
        }

        public static bool IsDirectionClose(Vector3 d1, Vector3 d2)
        {
            return Vector3.Distance(d1, d2) < 0.001f;
        }

        public static bool IsEulerClose(Vector3 e1, Vector3 e2)
        {
            return IsVectorClose(e1, e2, false);
        }

        public static T GetObjectComponent<T>(GameObject gameObject)
        {
            return gameObject == null ? default : gameObject.GetComponent<T>();
        }

        public static void ChangeObjectsLayer(GameObject gameObject, int toLayer, bool includeChildren = true)
        {
            gameObject.layer = toLayer;
            if (!includeChildren) return;
            foreach (var trans in gameObject.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = toLayer;
            }
        }
    }
}