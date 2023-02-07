using System.Globalization;
using cfg.battle;
using cfg.common;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Service;
using UnityEngine;

namespace MetaVirus.Logic.UI.Component.MonsterPanel
{
    public class MonsterResistancePanel
    {
        private GameDataService _gameDataService;
        private GComponent _attributePanelComp;

        private PetData _petData;

        public PetData PetData
        {
            get => _petData;
            set
            {
                _petData = value;
                OnPetDataChanged();
            }
        }

        public MonsterResistancePanel(GComponent attributePanelComp)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _attributePanelComp = attributePanelComp;
        }

        private void OnPetDataChanged()
        {
            for (var idx = ResistanceRange.Start; idx <= ResistanceRange.End; idx++)
            {
                var i = (int)idx;
                var attrComp = _attributePanelComp.GetChild("resist_" + i)?.asCom;
                if (attrComp == null) continue;
                var txtValue = attrComp.GetChild("value").asRichTextField;

                var baseResis = _petData.GrowupTable_Ref.Resistances[0].Resis;

                if (baseResis.Length >= i)
                {
                    var resist = baseResis[i - 1];
                    txtValue.text = resist.ToString(CultureInfo.InvariantCulture);
                    if (resist == 0)
                    {
                        txtValue.color = Color.white;
                    }
                    else if (resist > 0)
                    {
                        //green
                        txtValue.color = new Color32(0, 204, 102, 255);
                    }
                    else
                    {
                        //red
                        txtValue.color = new Color32(255, 51, 0, 255);
                    }
                }
                else
                {
                    txtValue.text = "0";
                    txtValue.color = Color.white;
                }
            }
        }
    }
}