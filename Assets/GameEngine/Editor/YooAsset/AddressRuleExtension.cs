using System.IO;
using YooAsset.Editor;

namespace GameEngine.Editor.YooAsset
{
    //为了兼容之前Addressable的地址
    [DisplayName("定位地址: 从CollectPath开始的全路径和文件名(以/分割)")]
    public class AddressByFullPathAndName : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            var collectDir = new DirectoryInfo(data.CollectPath).Name;
            var fileName = Path.GetFileName(data.AssetPath);
            var fileInfo = new FileInfo(data.AssetPath);

            var fileDir = "";

            var dir = fileInfo.Directory;
            while (dir != null && dir.Name != collectDir)
            {
                fileDir = dir.Name + "/" + fileDir;
                dir = dir.Parent;
            }

            return $"{collectDir}/{fileDir}{fileName}";
        }
    }

    //为了兼容之前Addressable的地址
    [DisplayName("定位地址: 以GroupName开头，从CollectPath开始的全路径和文件名(以/分割)")]
    public class AddressByGroupNameFullPathAndName : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            var collectDir = new DirectoryInfo(data.CollectPath).Name;
            var fileName = Path.GetFileName(data.AssetPath);
            var fileInfo = new FileInfo(data.AssetPath);

            var fileDir = "";

            var dir = fileInfo.Directory;
            while (dir != null && dir.Name != collectDir)
            {
                fileDir = dir.Name + "/" + fileDir;
                dir = dir.Parent;
            }

            return $"{data.GroupName}/{collectDir}/{fileDir}{fileName}";
        }
    }
    
    //为了兼容之前Addressable的地址
    [DisplayName("定位地址: 以GroupName开头，从CollectPath的子文件夹开始的全路径和文件名(以/分割)")]
    public class AddressByGroupNameAndSubPathName : IAddressRule
    {
        public string GetAssetAddress(AddressRuleData data)
        {
            var collectDir = new DirectoryInfo(data.CollectPath).Name;
            var fileName = Path.GetFileName(data.AssetPath);
            var fileInfo = new FileInfo(data.AssetPath);

            var fileDir = "";

            var dir = fileInfo.Directory;
            while (dir != null && dir.Name != collectDir)
            {
                fileDir = dir.Name + "/" + fileDir;
                dir = dir.Parent;
            }

            return $"{data.GroupName}/{fileDir}{fileName}";
        }
    }
}