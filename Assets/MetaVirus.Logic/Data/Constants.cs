using System;
using cfg.common;
using cfg.skill;
using FairyGUI;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using UnityEngine;

namespace MetaVirus.Logic.Data
{
    public static class Constants
    {
        public enum BattleTypes
        {
            /// <summary>
            /// 地图npc触碰战斗
            /// </summary>
            MapNpc,

            /// <summary>
            /// 战斗录像播放
            /// </summary>
            Record,

            /// <summary>
            /// 竞技场战斗
            /// </summary>
            Arena,
        }

        public enum BattleResult
        {
            Win = 0,
            Draw,
            Lose
        }

        public enum MonsterSort
        {
            BySpecies,
            ByQuality,
        }

        public static class LocalizeVarNames
        {
            public static class Battle
            {
                /// <summary>
                /// 行动单位的名字
                /// </summary>
                public const string ActionUnitName = "%actionUnitName";

                /// <summary>
                /// 技能名字，带等级的，例如 3级大火球
                /// </summary>
                public const string SkillLevelName = "%skill";

                /// <summary>
                /// 目标名字
                /// </summary>
                public const string TargetName = "%targetName";

                /// <summary>
                /// 技能效果值，一般指技能造成的伤害或治疗值
                /// </summary>
                public const string EffectValue = "%effectValue";

                /// <summary>
                /// 伤害属性对应的名称
                /// </summary>
                public const string AtkAttributeName = "%atkAttributeName";

                /// <summary>
                /// 技能附加值，一般指技能造成的附加伤害或治疗
                /// </summary>
                public const string AttachValue = "%AttachValue";

                public const string BuffLevelName = "%buff";

                public const string Level = "%level";
                public const string Name = "%name";
            }
        }

        public static class DataKeys
        {
            public const string LoginPlayerId = "CommonData.Login.Player.Id";

            public const string PlayerInfo = "CommonData.Player.Info";

            public const string MapCurrentId = "CommonData.Map.CurrentId";
            public const string MapCurrentLayer = "CommonData.Map.CurrentLayer";

            /**
             * 战斗结束后返回的Procedure，没用了
             */
            public const string BattleBackProcedure = "CommonData.Battle.BackProcedure";

            //ui 相关数据

            public const string UIDataInteractiveNpc = "UIData.Npc.Interactive";

            //UIMonsterDetail 相关
            //需要同时提供以下两项provider
            /**
             * UIMonsterDetail 需要显示的数据，必须是IMonsterDataProvider类型
             */
            public const string UIMonsterDetailData = "UIData.Monster.Detail.Data";

            /**
             * UIMonsterDetail 需要显示的数据列表，必须是IMonsterListProvider类型
             */
            public const string UIMonsterDetailDataList = "UIData.Monster.Detail.Data.List";

            // UIArenaMatching 相关
            // 选择挑战的对手Id
            public const string UIArenaMatchingOpponentData = "UIData.Arena.Matching.Opponent.Data";

            // UIPreparation 相关
            // 当前选择阵容的Id
            // 这个没用
            // public const string UIArenaPreparationCastId = "UIData.Arena.Preparation.Cast.Id";
        }

        public static class UizOrders
        {
            public const int UiWindow = 0;
            public const int LoadingPage = 200;
            public const int UiDialog = 300;
            public const int UiWaitingWindow = 400;
        }

        public static class EntityGroupName
        {
            public const string Player = "EntityGroup_Player";
            public const string MapNpc = "EntityGroup_MapNpc";
            public const string GridItemNetPlayer = "EntityGroup_GridItem_NetPlayer";
            public const string BattleUnit = "EntityGroup_BattleUnit";
        }

        public static class FairyImageUrl
        {
            public static string Common(string imageName)
            {
                return $"ui://Common/{imageName}";
            }

            public static string Header(int npcResId)
            {
                var headUrl = $"ui://UnitPortraits/head-{npcResId}";
                var url = UIPackage.GetItemByURL(headUrl);
                if (url == null)
                {
                    headUrl = "ui://asz9r10dvi4jle"; //头像没找到使用默认的头像代替
                }

                return headUrl;
            }

            // public static string Frame(string frameName)
            // {
            //     return $"ui://Common/Frames/{frameName}";
            // }
            //
            // public static string Label(string labelName)
            // {
            //     return $"ui://Common/Labels/{labelName}";
            // }

            /// <summary>
            /// 根据品质返回头像框背景图片
            /// </summary>
            /// <param name="quality"></param>
            /// <returns></returns>
            public static string HeaderBg(Quality quality)
            {
                string url;
                switch (quality)
                {
                    case Quality.GREEN:
                        url = "frame_cardframe_02_back_green";
                        break;
                    case Quality.BLUE:
                        url = "frame_cardframe_02_back_blue";
                        break;
                    case Quality.PURPLE:
                        url = "frame_cardframe_02_back_purple";
                        break;
                    case Quality.Orange:
                        url = "frame_cardframe_02_back_orange";
                        break;
                    case Quality.Red:
                        url = "frame_cardframe_02_back_red";
                        break;
                    case Quality.WHITE:
                    default:
                        url = "frame_cardframe_02_back_white";
                        break;
                }

                url = Common(url);
                return url;
            }


            /// <summary>
            /// 根据品质返回等级背景图片
            /// </summary>
            /// <param name="quality"></param>
            /// <returns></returns>
            public static string LevelFlag(Quality quality)
            {
                string url;
                switch (quality)
                {
                    case Quality.GREEN:
                        url = "label_flag_02_green";
                        break;
                    case Quality.BLUE:
                        url = "label_flag_02_blue";
                        break;
                    case Quality.PURPLE:
                        url = "label_flag_02_purple";
                        break;
                    case Quality.Orange:
                        url = "label_flag_02_orange";
                        break;
                    case Quality.Red:
                        url = "label_flag_02_red";
                        break;
                    case Quality.WHITE:
                    default:
                        url = "label_flag_02_white";
                        break;
                }

                url = Common(url);
                return url;
            }
        }


        public static string AtkAttributeToImageName(AtkAttribute atkAttribute)
        {
            switch (atkAttribute)
            {
                case AtkAttribute.Fire:
                    return "set_icon_stat_5";
                case AtkAttribute.Ice:
                    return "btn_icon_snowflake";
                case AtkAttribute.Electricity:
                    return "btn_icon_thunder";
                case AtkAttribute.Light:
                    return "set_icon_stat_8";
                case AtkAttribute.Dark:
                    return "set_icon_stat_9";
                case AtkAttribute.Poison:
                    return "btn_icon_potion_1";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 返回攻击属性对应的属性颜色，[0] = 填充色 [1] = 亮边色
        /// </summary>
        /// <param name="atkAttribute"></param>
        /// <returns></returns>
        public static Color[] AtkAttributeToColor(AtkAttribute atkAttribute)
        {
            switch (atkAttribute)
            {
                case AtkAttribute.Fire:
                    return new Color[] { new Color32(255, 63, 0, 255), new Color32(233, 86, 0, 255) };
                case AtkAttribute.Ice:
                    return new Color[] { new Color32(0, 175, 255, 255), new Color32(156, 156, 156, 255) };
                case AtkAttribute.Electricity:
                    return new Color[] { new Color32(254, 255, 0, 255), new Color32(176, 176, 176, 255) };
                case AtkAttribute.Light:
                    return new Color[] { new Color32(255, 187, 121, 255), new Color32(255, 132, 0, 255) };
                case AtkAttribute.Dark:
                    return new Color[] { new Color32(0, 0, 0, 255), new Color32(164, 0, 162, 255) };
                case AtkAttribute.Poison:
                    return new Color[] { new Color32(145, 0, 255, 255), new Color32(129, 19, 212, 255) };
                default:
                    return new Color[] { new Color32(255, 255, 255, 255), new Color32(233, 86, 0, 255) };
            }
        }

        public static class ResAddress
        {
            private const string Npc = "npc-prefabs";
            private const string Map = "map-";
            public const string GameDatas = "GameDatas";

            public const string UIModelLoader = "prefabs/UIModelLoader.prefab";

            public const string PlayerPrefab = "actor-player/Player.prefab";

            public const string NetPlayerPrefab = "actor-player/NetPlayer.prefab";

            /// <summary>
            /// 根据性别返回玩家对应的资源模型prefab
            /// 目前性别不需要了
            /// </summary>
            /// <param name="gender"></param>
            /// <returns></returns>
            public static string PlayerResPrefab(Gender gender)
            {
                // var prefabName = PlayerInfo.Gender == Gender.Male
                //     ? "MaleCharacter.prefab"
                //     : "FemaleCharacter.prefab";
                var prefabName = "PlayerTemplate.prefab";
                return $"actor-player/{prefabName}";
            }

            public static string BattleUnitRes(int npcId)
            {
                return $"{Npc}/{npcId}/{npcId}-battle.prefab";
            }

            public static string NpcRes(int npcId)
            {
                return $"{Npc}/{npcId}/{npcId}.prefab";
            }

            public static string MapRes(int mapId)
            {
                return $"{Map}{mapId:d4}/map.unity";
            }
        }

        /// <summary>
        /// 玩家的行走动画加速1.6倍播放
        /// </summary>
        public const float PlayerWalkAniSpeed = 1.6f;

        public static class AniStateName
        {
            public const string Idle = "Idle";
            public const string TakeDamage = "Take Damage";
        }

        public static class AniParamName
        {
            public static readonly int IsBorn = Animator.StringToHash("IsBorn");

            public static readonly int State = Animator.StringToHash("State");

            public static readonly int Velocity = Animator.StringToHash("Velocity");

            public static readonly int TriggerTakeDamage = Animator.StringToHash("Trigger_TakeDamage");

            public static readonly int TriggerAttack = Animator.StringToHash("Trigger_Attack");

            public static readonly int TriggerDead = Animator.StringToHash("Trigger_Dead");

            public static readonly int TriggerResurrect = Animator.StringToHash("Trigger_Resurrect");

            public static readonly int StartAction = Animator.StringToHash("StartAction");
            public static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
            public static readonly int CastSpell = Animator.StringToHash("CastSpell");

            public static readonly int Weapon = Animator.StringToHash("Weapon");
            public static readonly int Greeting = Animator.StringToHash("Greeting");
            public static readonly int IdleStateType = Animator.StringToHash("IdleStateType");

            /// <summary>
            /// 将Action Name转化为动画状态机中state对应的值
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static int UnitActionNameToStateValue(UnitAnimationNames name)
            {
                switch (name)
                {
                    case UnitAnimationNames.StartAction1:
                    case UnitAnimationNames.StartAction2:
                    case UnitAnimationNames.StartAction3:
                        return NpcAniState.StartAction;
                    case UnitAnimationNames.MeleeAttack1:
                    case UnitAnimationNames.MeleeAttack2:
                    case UnitAnimationNames.MeleeAttack3:
                        return NpcAniState.MeleeAttack;
                    case UnitAnimationNames.CastSpell1:
                    case UnitAnimationNames.CastSpell2:
                    case UnitAnimationNames.CastSpell3:
                        return NpcAniState.CastSpell;
                    case UnitAnimationNames.ProjectileAttack:
                        return NpcAniState.Projectile;
                    default:
                        return NpcAniState.MeleeAttack;
                }
            }

            /// <summary>
            /// 根据ActionName返回攻击动画播放时的相关事件，共4个
            /// 事件顺序是  进入攻击, 生成飞行道具, 攻击关键帧, 退出攻击
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static BattleAniEvtNames[] GetAniEventsByUnitActionName(UnitAnimationNames name)
            {
                switch (name)
                {
                    case UnitAnimationNames.ProjectileAttack:
                        //飞行道具
                        return new[]
                        {
                            BattleAniEvtNames.EnterProjectileAttack,
                            BattleAniEvtNames.SpawnProjectile, BattleAniEvtNames.MeleeAttack,
                            BattleAniEvtNames.ExitProjectileAttack
                        };
                    case UnitAnimationNames.CastSpell1:
                    case UnitAnimationNames.CastSpell2:
                    case UnitAnimationNames.CastSpell3:
                        return new[]
                        {
                            BattleAniEvtNames.EnterCastSpell,
                            BattleAniEvtNames.SpawnProjectile, BattleAniEvtNames.CastSpell,
                            BattleAniEvtNames.ExitCastSpell
                        };
                    default:
                        return new[]
                        {
                            BattleAniEvtNames.EnterMeleeAttack,
                            BattleAniEvtNames.SpawnProjectile, BattleAniEvtNames.MeleeAttack,
                            BattleAniEvtNames.ExitMeleeAttack
                        };
                }
            }

            /// <summary>
            /// 将Action Name转化成动画状态机中对应的parameter name hash
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static int UnitActionNameToParamHash(UnitAnimationNames name)
            {
                switch (name)
                {
                    case UnitAnimationNames.StartAction1:
                    case UnitAnimationNames.StartAction2:
                    case UnitAnimationNames.StartAction3:
                        return StartAction;
                    case UnitAnimationNames.MeleeAttack1:
                    case UnitAnimationNames.MeleeAttack2:
                    case UnitAnimationNames.MeleeAttack3:
                        return MeleeAttack;
                    case UnitAnimationNames.CastSpell1:
                    case UnitAnimationNames.CastSpell2:
                    case UnitAnimationNames.CastSpell3:
                        return CastSpell;
                    case UnitAnimationNames.ProjectileAttack:
                        return 0;
                    default:
                        return MeleeAttack;
                }
            }


            /// <summary>
            /// 将Action Name转化成动画状态机中的对应Parameter的值
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public static int UnitActionNameToParamValue(UnitAnimationNames name)
            {
                return name switch
                {
                    UnitAnimationNames.StartAction1 => 0,
                    UnitAnimationNames.StartAction2 => 1,
                    UnitAnimationNames.StartAction3 => 2,
                    UnitAnimationNames.MeleeAttack1 => 0,
                    UnitAnimationNames.MeleeAttack2 => 1,
                    UnitAnimationNames.MeleeAttack3 => 2,
                    UnitAnimationNames.CastSpell1 => 0,
                    UnitAnimationNames.CastSpell2 => 1,
                    UnitAnimationNames.CastSpell3 => 2,
                    _ => 0
                };
            }
        }

        public enum NpcWeaponType
        {
            NoWeapon = 0,
            SingleSword,
            DoubleSword,
            SwordAndShield,
            TwoHands,
            MagicWand,
            Spear
        }

        public static class NpcAniState
        {
            public const int Idle = 0;
            public const int Walk = 1;

            public const int Run = 2;

            //TinyHero资源冲刺动作
            public const int Sprint = 30;

            //战斗动画状态机专用
            public const int WalkBackward = 3;
            public const int Die = 4; //死亡状态改成trigger了
            public const int MeleeAttack = 5;

            public const int CastSpell = 6;

            public const int Projectile = 7;

            public const int StartAction = 10;
        }

        public enum BattleAniEvtNames
        {
            CastSpell = 1,
            MeleeAttack,
            SpawnProjectile,
            EnterMeleeAttack,
            ExitMeleeAttack,
            EnterCastSpell,
            ExitCastSpell,
            EnterProjectileAttack,
            ExitProjectileAttack,
            EnterStartAction,
            ExitStartAction,
        }

        public enum BattleSkillCastEvents
        {
            EnterSkillCast = 0,
            SpawnProjectile,
            HitTarget,
            ExitSkillCast,
            ExitProjectile,
        }

        public enum BattleSourceType
        {
            MonsterData = 1,
            PlayerData = 2,
        }

        // public struct BattleProjectileInfo
        // {
        //     public int BindVfx { get; }
        //     public ProjectileType Type { get; }
        //     public float Speed { get; }
        //
        //     public BattleProjectileInfo(int bindVfx, ProjectileType type, float speed)
        //     {
        //         BindVfx = bindVfx;
        //         Type = type;
        //         Speed = speed == 0 ? 1 : speed;
        //     }
        // }

        // public static class NpcIdMaker
        // {
        //     public static int Make(int mapId, int npcInfoId)
        //     {
        //         return (mapId << 16) | npcInfoId;
        //     }
        // }

        //对应 NpcRelation 的值
        private static readonly int[] NpcNameColors = { 0x33CCFF, 0xFFDD00, 0xFF0033 };

        private static readonly int[] NetPlayerNameColors = { 0x14FF00, 0xFFDD00, 0xFF0033 };

        public enum NpcRelation
        {
            Friendly,
            OpposedPassive,
            OpposedPositive,
        }

        private static Color32 RGBToColor(int c)
        {
            var clr = new Color32((byte)((c >> 16) & 0xff), (byte)((c >> 8) & 0xff), (byte)(c & 0xff), 255);
            return clr;
        }

        public static Color32 NpcRelationToColor(NpcRelation relation)
        {
            var r = (int)relation;
            if (r >= NpcNameColors.Length)
            {
                r = NpcNameColors.Length - 1;
            }

            var c = NpcNameColors[r];
            return RGBToColor(c);
        }

        public static Color32 NetPlayerRelationToColor(NpcRelation relation)
        {
            var r = (int)relation;
            if (r >= NetPlayerNameColors.Length)
            {
                r = NetPlayerNameColors.Length - 1;
            }

            var c = NetPlayerNameColors[r];
            return RGBToColor(c);
        }


        /**
         * 返回Npc与玩家之间的关系
         * 友善、敌对被动(敌对关系但不主动攻击)、敌对主动
         */
        public static NpcRelation GetNpcRelationWithPlayer(PlayerEntity player, NpcEntity npc)
        {
            var npcRealm = npc.Info.NpcTempId_Ref.Realm;
            var npcAtkMode = npc.Info.AttackMode;

            if (npcRealm == Camp.Npc)
            {
                return NpcRelation.Friendly;
            }

            if (npcAtkMode == NpcAttackMode.Passive)
            {
                return NpcRelation.OpposedPassive;
            }

            return NpcRelation.OpposedPositive;
        }

        public static NpcRelation GetCampRelation(Camp camp1, Camp camp2)
        {
            if (camp2 == Camp.Npc || camp1 == camp2)
            {
                return NpcRelation.Friendly;
            }

            return NpcRelation.OpposedPositive;
        }

        public const string EnJosefinSans = "JosefinSans-Bold";

        public const string EnLilitaOne = "LilitaOne-Regular";


        public static string QualityToStr(Quality quality)
        {
            return quality switch
            {
                Quality.WHITE => "D",
                Quality.GREEN => "C",
                Quality.BLUE => "B",
                Quality.PURPLE => "A",
                Quality.Orange => "S",
                Quality.Red => "SS",
                _ => ""
            };
        }
    }
}