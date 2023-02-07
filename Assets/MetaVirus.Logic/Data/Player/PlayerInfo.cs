using System.Collections.Generic;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.Common;
using UnityEngine;

namespace MetaVirus.Logic.Data.Player
{
    public enum Gender
    {
        Female,
        Male,
    }

    public class PlayerInfo
    {
        public long AccountId { get; set; }

        public int PlayerId { get; set; }

        public int AttackPartyId { get; set; }

        //玩家名字
        public string Name { get; set; }

        //玩家性别
        public Gender Gender { get; set; }

        //玩家所在地图Id
        public int CurrentMapId { get; set; }

        public int NextMapId { get; set; }

        //玩家在地图上的位置
        public Vector3 Position { get; set; }

        //玩家所在分层Id
        public int CurrentLayerId { get; set; }

        //玩家avatar setting
        public ulong AvatarSetting { get; set; }

        //玩家级别
        public int Level { get; set; }

        //当前所在scene服务器id
        public int sceneServerId { get; set; }

        //玩家阵型信息
        public PlayerParty[] PlayerParties { get; set; }

        public static PlayerInfo FromPlayerData(PBPlayerData playerData, int sceneServerId)
        {
            var parties = new List<PlayerParty>();

            foreach (var f in playerData.Formations)
            {
                var party = new PlayerParty(f.FormationId, f.FormationDataId, f.Name);
                for (var i = 0; i < f.Slots.Count; i++)
                {
                    party[i] = f.Slots[i];
                }

                parties.Add(party);
            }

            var p = new PlayerInfo
            {
                PlayerId = playerData.PlayerId,
                AccountId = playerData.AccountId,
                Name = playerData.Name,
                Gender = (Gender)playerData.Gender,
                Level = playerData.Level,
                CurrentLayerId = playerData.LayerId,
                Position = playerData.Position.ToVector3(),
                CurrentMapId = -1,
                sceneServerId = sceneServerId,
                PlayerParties = parties.ToArray(),
                AttackPartyId = playerData.AttackParty,
                AvatarSetting = (ulong)playerData.AvatarSetting
            };

            return p;
        }
    }
}