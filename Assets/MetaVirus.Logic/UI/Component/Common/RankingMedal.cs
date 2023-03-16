using System;
using System.Collections.Generic;
using FairyGUI;
using MetaVirus.Logic.Service.Arena.data;

namespace MetaVirus.Logic.UI.Component.Common
{
    public class RankingMedal
    {
        // private readonly GComponent _comp;
        // private readonly int _index;
        // private readonly List<ArenaPlayerData> _data;
        public struct Position
        {
            public int X;
            public int Y;
        }

        public static void RenderMedal(GComponent comp, int ranking)
        {
            //     switch (ranking)
            //     {
            //         case 1:
            //             var goldMedal = UIPackage.CreateObject("Common", "icon_itemicon_medalgold").asImage;
            //             goldMedal.SetSize(40,50);
            //             comp.AddChild(goldMedal).SetXY(pos.X,pos.Y);
            //             break;
            //         case 2:
            //             var silverMedal = UIPackage.CreateObject("Common", "icon_itemicon_medalsilver").asImage;
            //             silverMedal.SetSize(40,50);
            //             comp.AddChild(silverMedal).SetXY(pos.X,pos.Y);
            //             break;
            //         case 3:
            //             var bronzeMedal = UIPackage.CreateObject("Common", "icon_itemicon_medalbronze").asImage;
            //             bronzeMedal.SetSize(40,50);
            //             comp.AddChild(bronzeMedal).SetXY(pos.X,pos.Y);
            //             break;
            //         default:
            //             var textPlayerRanking = comp.GetChild("text_ranking").asTextField;
            //             // var ranking = data[index].ArenaInfo.Rank;
            //             textPlayerRanking.text = Convert.ToString(ranking);
            //             break;
            //     }
            var textPlayerRanking = comp.GetChild("text_ranking").asTextField;
            textPlayerRanking.text = ranking.ToString();
            var ctrl = comp.GetController("rank");
            ctrl?.SetSelectedIndex(ranking is > 0 and <= 3 ? ranking : 0);
        }
    }
}