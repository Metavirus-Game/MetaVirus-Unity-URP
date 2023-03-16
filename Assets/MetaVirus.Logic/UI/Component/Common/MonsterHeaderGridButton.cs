using FairyGUI;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Data.Provider;

namespace MetaVirus.Logic.UI.Component.Common
{
    public class MonsterHeaderGridButton
    {
        private GButton _headerButton;

        private GComponent _headerComp;

        public GButton HeaderButton => _headerButton;

        private Controller _qualityCtrl;
        private Controller _checkCtrl;

        private IMonsterDataProvider _petData;

        private bool _isEmpty = true;
        private bool _isChecked = false;

        public bool Empty
        {
            get => _isEmpty;
            set
            {
                _isEmpty = value;
                _qualityCtrl.selectedIndex = _isEmpty ? 0 : (int)_petData.Quality + 1;
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

        public IMonsterDataProvider PetData
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
                    RenderHeaderCompNew(_petData, _headerComp);
                }
            }
        }


        public MonsterHeaderGridButton(GButton headerButton = null)
        {
            _headerButton = headerButton ?? UIPackage.CreateObject("Common", "GridButton_MonsterHeader").asButton;
            // _headerComp = _headerButton.GetChild("n7").asCom;
            _headerComp = _headerButton.GetChild("headerComp").asCom;
            _qualityCtrl = _headerComp.GetController("quality");
            _checkCtrl = _headerComp.GetController("checked");
            Empty = true;
        }

        /// <summary>
        /// 将playerPetData的数据填入HeaderComp，HeaderComp必须是CommonUI中的HeaderComp组件
        /// </summary>
        /// <param name="petData"></param>
        /// <param name="headerComp"></param>
        public static void RenderHeaderCompNew(IMonsterDataProvider petData, GComponent headerComp)
        {
            var headerImg = headerComp.GetChildByPath("mask.PortraitLoader").asLoader;
            headerImg.url = Constants.FairyImageUrl.Header(petData.ModelResId);

            // var headerBg = headerComp.GetChild("card_bg").asLoader;
            // headerBg.url = Constants.FairyImageUrl.HeaderBg(petData.Quality);

            var text = headerComp.GetChild("card_level_txt").asTextField;
            text.text = petData.Level.ToString();

            // var rank = headerComp.GetChild("txt_rank").asTextField;
            // rank.text = petData.QualityStr;
            // rank.color = petData.QualityClr;

            var ctrl = headerComp.GetController("quality");
            ctrl.SetSelectedIndex((int)petData.Quality + 1);
        }

        public static void RenderHeaderComp(IMonsterDataProvider petData, GComponent headerComp)
        {
            var headerImg = headerComp.GetChildByPath("PortraitLoader").asLoader;
            headerImg.url = Constants.FairyImageUrl.Header(petData.ModelResId);

            var headerBg = headerComp.GetChild("card_bg").asLoader;
            headerBg.url = Constants.FairyImageUrl.HeaderBg(petData.Quality);

            var text = headerComp.GetChild("card_level_txt").asTextField;
            text.text = petData.Level.ToString();

            var rank = headerComp.GetChild("txt_rank").asTextField;
            // rank.text = petData.QualityStr;
            // rank.color = petData.QualityClr;
        }
    }
}