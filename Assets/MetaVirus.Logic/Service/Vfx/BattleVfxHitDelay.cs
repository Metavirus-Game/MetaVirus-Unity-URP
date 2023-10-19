using System;
using System.Collections.Generic;
using GameEngine;
using GameEngine.Timer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MetaVirus.Logic.Service.Vfx
{
    /// <summary>
    /// 挂载在特效上，表示特效播放后需要经过一定的延时，才触发OnHit
    /// </summary>
    public class BattleVfxHitDelay : MonoBehaviour
    {
        [Header("特效持续时间，填0表示最后一次伤害为特效完成时间")] public float duration = 0;
        public List<SkillHitInfo> hitDelayInfos = new();

        public void InvokeHitDelay(Action<SkillHitInfo> onHitCallback, Action onFinished)
        {
            var timer = GameFramework.GetService<TimerService>();
            if (hitDelayInfos.Count == 0)
            {
                if (duration == 0)
                {
                    onHitCallback?.Invoke(SkillHitInfo.Default);
                }
                else
                {
                    timer.InvokeDelay(duration, () =>
                    {
                        onHitCallback?.Invoke(SkillHitInfo.Default);
                        onFinished?.Invoke();
                    });
                }
            }
            else
            {
                var d = 0f;
                for (var i = 0; i < hitDelayInfos.Count; i++)
                {
                    var info = hitDelayInfos[i];
                    if (i == hitDelayInfos.Count - 1)
                    {
                        d = info.delay;
                        info.IsHitFinished = true;
                    }

                    timer.InvokeDelay(info.delay, () => { onHitCallback?.Invoke(info); });
                }

                timer.InvokeDelay(Mathf.Max(d, duration), onFinished);
            }
        }
    }

    [Serializable]
    public class SkillHitInfo
    {
        public static readonly SkillHitInfo Default = new()
        {
            delay = 0,
            percent = 1,
            IsHitFinished = true,
        };

        public static readonly SkillHitInfo VfxFinished = new()
        {
            delay = 0,
            percent = 0,
            IsHitFinished = true,
        };

        [Header("延迟时间")] public float delay;
        [Header("伤害百分比")] [Range(0, 1)] public float percent;
        public bool IsHitFinished { get; internal set; }
    }
}