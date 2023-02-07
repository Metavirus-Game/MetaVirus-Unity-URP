using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEngine.Utils
{
    public class TypeWithAttribute<TA> where TA : Attribute
    {
        public TA Attribute { get; }
        public Type Type { get; }

        public TypeWithAttribute(TA attribute, Type type)
        {
            Attribute = attribute;
            Type = type;
        }
    }

    public static class AssemblyUtil
    {
        public static TypeWithAttribute<TA>[] GetTypesHasAttribute<TA>(this AppDomain appDomain) where TA : Attribute
        {
            var assemblies = appDomain.GetAssemblies();
            var list = new List<TypeWithAttribute<TA>>();
            foreach (var assembly in assemblies)
            {
                var tps = assembly.GetTypes();
                foreach (var type in assembly.GetTypes())
                {
                    var pa = type.GetCustomAttribute<TA>();
                    if (pa != null)
                    {
                        list.Add(new TypeWithAttribute<TA>(pa, type));
                    }
                }
            }

            return list.ToArray();
        }
    }

    public static class TaskExtention
    {
        public static IEnumerator AsCoroution<T>(this Task<T> task)
        {
            return new WaitUntil(() => task.IsCompleted);
        }

        public static IEnumerator AsCoroution(this Task task)
        {
            return new WaitUntil(() => task.IsCompleted);
        }
    }

    public static class AnimatorExtention
    {
        /// <summary>
        /// 等待当前动画播放结束
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="loopWaitTime">loop状态下的动画等待时长</param>
        /// <returns></returns>
        public static IEnumerator WaitForCurrentAni(this Animator animator, float loopWaitTime)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var clips = animator.GetCurrentAnimatorClipInfo(0);

            var loopState = clips.Length > 0 ? clips[0].clip.isLooping : stateInfo.loop;

            if (loopState)
            {
                //循环播放的动画，播放指定时长后进入下一步
                yield return new WaitForSeconds(loopWaitTime);
            }
            else
            {
                var loop = true;

                while (loop)
                {
                    stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    var t = stateInfo.normalizedTime;
                    if (t > 0.95f)
                    {
                        loop = false;
                    }

                    yield return null;
                }
            }
        }
    }

    public static class GameObjectExtention
    {
        public static void SetLayerAll(this GameObject gameObject, int layer)
        {
            GameEngineUtils.ChangeObjectsLayer(gameObject, layer);
        }
    }
}