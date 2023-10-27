using System;
using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.ResExplorer.UI
{
    public class MonsterAniList
    {
        public struct MonsterAniSetting
        {
            public string ShowName;
            public string AniName;
            public AnimationClip Clip;
        }

        private GList _aniList;
        private List<KeyValuePair<AnimationClip, AnimationClip>> _aniOverrideSetting;
        private List<AnimationClip> _allAniClips;

        public UnityAction<string, AnimationClip> OnAniSelected;

        private readonly List<MonsterAniSetting> _aniSettings = new();

        public List<MonsterAniSetting> AniSettings => _aniSettings;

        private static readonly List<string> AniNames = new()
        {
            "空闲", "出现", "行走", "跑", "死亡", "挨打", "起始动作1", "起始动作2", "起始动作3", "近战攻击动作1", "近战攻击动作2", "近战攻击动作3", "施法动作1",
            "施法动作2", "施法动作3", "远程攻击动作",
        };

        private static readonly Dictionary<string, string> AniNameMap = new()
        {
            { "空闲", "Idle" },
            { "出现", "Spawn" },
            { "行走", "Walk Forward In Place" },
            { "跑", "Run Forward In Place" },
            { "死亡", "Die" },
            { "挨打", "Take Damage" },
            { "起始动作1", "StartAction1" },
            { "起始动作2", "StartAction2" },
            { "起始动作3", "StartAction3" },
            { "近战攻击动作1", "MeleeAttack1" },
            { "近战攻击动作2", "MeleeAttack2" },
            { "近战攻击动作3", "MeleeAttack3" },
            { "施法动作1", "CastSpell1" },
            { "施法动作2", "CastSpell2" },
            { "施法动作3", "CastSpell3" },
            { "远程攻击动作", "Projectile Attack" },
        };


        public MonsterAniList(GList list)
        {
            _aniList = list;
        }

        public void ClearList()
        {
            _aniList.numItems = 0;
        }

        public void SetAniData(List<KeyValuePair<AnimationClip, AnimationClip>> aniOverrideSetting,
            List<AnimationClip> allAniClips)
        {
            _aniOverrideSetting = aniOverrideSetting;
            _allAniClips = allAniClips;

            InitAniList();
        }

        private void InitAniList()
        {
            _aniSettings.Clear();

            foreach (var nameShow in AniNames)
            {
                var nameKey = AniNameMap[nameShow];

                var data = FindAniKeyPair(nameKey);

                var aniName = "";
                if (data.Value != null)
                {
                    var idx = data.Value.name.IndexOf("@", StringComparison.Ordinal);
                    aniName = idx == -1 ? data.Value.name : data.Value.name[(idx + 1)..];
                }
                else
                {
                    aniName = "未配置";
                }

                _aniSettings.Add(new MonsterAniSetting
                {
                    ShowName = nameShow,
                    AniName = aniName,
                    Clip = data.Value
                });
            }

            foreach (var clip in _allAniClips)
            {
                var kp = FindAniKeyPairByValue(clip.name);
                if (kp.Value == null)
                {
                    var idx = clip.name.IndexOf("@", StringComparison.Ordinal);
                    var aniName = idx == -1 ? clip.name : clip.name[(idx + 1)..];

                    _aniSettings.Add(new MonsterAniSetting
                    {
                        ShowName = "未使用",
                        AniName = aniName,
                        Clip = clip
                    });
                }
            }

            _aniList.itemRenderer = RenderAniList;
            _aniList.numItems = _aniSettings.Count;
            _aniList.onClickItem.Set(context =>
            {
                var obj = context.data as GObject;
                var idx = _aniList.GetChildIndex(obj);

                var data = _aniSettings[idx];
                OnAniSelected?.Invoke(data.ShowName, data.Clip);
            });
            _aniList.selectedIndex = 0;
        }

        private KeyValuePair<AnimationClip, AnimationClip> FindAniKeyPair(string keyName)
        {
            var kp = _aniOverrideSetting.FirstOrDefault(kp => kp.Key.name.Equals(keyName));
            return kp;
        }

        private KeyValuePair<AnimationClip, AnimationClip> FindAniKeyPairByValue(string valueClipName)
        {
            var kp = _aniOverrideSetting.FirstOrDefault(
                kp => kp.Value != null && kp.Value.name.Equals(valueClipName));
            return kp;
        }


        private void RenderAniList(int index, GObject item)
        {
            var btn = item.asButton;

            var aniSetting = _aniSettings[index];
            var txtActionName = btn.GetChild("txtActionName").asTextField;
            txtActionName.text = aniSetting.ShowName;
            btn.title = aniSetting.AniName;
            var bgCtrl = btn.GetController("bg");
            bgCtrl.SetSelectedIndex(index % 2);
        }
    }
}