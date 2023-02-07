using FairyGUI;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;

namespace MetaVirus.Logic.UI.Component.Common
{
    public class MonsterHeaderGridButton
    {
        private GButton _headerButton;

        private GComponent _headerComp;

        public GButton HeaderButton => _headerButton;

        private Controller _emptyCtrl;
        private Controller _checkCtrl;

        private PlayerPetData _petData;

        private bool _isEmpty = true;
        private bool _isChecked = false;

        public bool Empty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                _emptyCtrl.selectedIndex = _isEmpty ? 1 : 0;
            }
        }

        public bool Checked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                _checkCtrl.selectedIndex = _isChecked ? 1 : 0;
            }
        }

        public PlayerPetData PetData
        {
            get => _petData;
            set
            {
                _petData = value;
                if (_petData == null)
                {
                    Empty = true;
                }
                else
                {
                    Empty = false;
                    RenderHeaderComp(_petData, _headerComp);
                }
            }
        }


        public MonsterHeaderGridButton(GButton headerButton = null)
        {
            _headerButton = headerButton ?? UIPackage.CreateObject("Common", "GridButton_MonsterHeader").asButton;
            _headerComp = _headerButton.GetChild("n7").asCom;
            _emptyCtrl = _headerComp.GetController("empty");
            _checkCtrl = _headerComp.GetController("checked");
            Empty = true;
        }

        /// <summary>
        /// 将playerPetData的数据填入HeaderComp，HeaderComp必须是CommonUI中的HeaderComp组件
        /// </summary>
        /// <param name="petData"></param>
        /// <param name="headerComp"></param>
        public static void RenderHeaderComp(PlayerPetData petData, GComponent headerComp)
        {
            var headerImg = headerComp.GetChild("PortraitLoader").asLoader;
            headerImg.url = Constants.FairyImageUrl.Header(petData.PetData.ResDataId);

            var headerBg = headerComp.GetChild("card_bg").asLoader;
            headerBg.url = Constants.FairyImageUrl.HeaderBg(petData.Quality);

            var text = headerComp.GetChild("card_level_txt").asTextField;
            text.text = petData.Level.ToString();

            var rank = headerComp.GetChild("txt_rank").asTextField;
            rank.text = petData.QualityStr;
            rank.color = petData.QualityClr;
        }
    }
}