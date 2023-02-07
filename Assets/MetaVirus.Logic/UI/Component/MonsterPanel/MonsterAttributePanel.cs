using System.Globalization;
using cfg.battle;
using cfg.common;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Service;

namespace MetaVirus.Logic.UI.Component.MonsterPanel
{
    public class MonsterAttributePanel
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

        public MonsterAttributePanel(GComponent attributePanelComp)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _attributePanelComp = attributePanelComp;
        }

        private void OnPetDataChanged()
        {
            for (var idx = AttributeIdRange.Start; idx <= AttributeIdRange.End; idx++)
            {
                var i = (int)idx;
                var attrComp = _attributePanelComp.GetChild("attr_" + i)?.asCom;
                if (attrComp == null) continue;
                var txtValue = attrComp.GetChild("value").asRichTextField;

                var baseAttrs = _petData.GrowupTable_Ref.Attributes[0].Attrs;

                if (baseAttrs.Length >= i)
                {
                    txtValue.text = baseAttrs[i - 1]
                        .ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    txtValue.text = "0";
                }
            }
        }
    }
}