using FairyGUI;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.UI
{
    public class LoadingPage
    {
        public GComponent LoadingPageCom { get; private set; }
        public GProgressBar ProgressBar { get; private set; }

        public static LoadingPage Create()
        {
            var ret = new LoadingPage
            {
                LoadingPageCom = UIPackage.CreateObject("LoadingPage", "LoadingPage").asCom
            };
            ret.LoadingPageCom.sortingOrder = UizOrders.LoadingPage;
            ret.LoadingPageCom.alpha = 0;

            ret.ProgressBar = ret.LoadingPageCom.GetChild("n8").asProgress;
            ret.ProgressBar.value = 0;
            return ret;
        }
    }
}