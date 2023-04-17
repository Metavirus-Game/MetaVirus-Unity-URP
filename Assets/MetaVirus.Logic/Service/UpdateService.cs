using System.Collections;
using System.Collections.Generic;
using GameEngine;
using GameEngine.Base;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MetaVirus.Logic.Service
{
    /// <summary>
    /// 更新服务
    /// </summary>
    public class UpdateService : BaseService
    {
        public IEnumerator CheckUpdate()
        {
            Addressables.InitializeAsync(true);

            var waitWnd = UIWaitingWindow.ShowWaiting("正在检查更新，请稍候...");
            var checkHandler = Addressables.CheckForCatalogUpdates(false);
            yield return checkHandler;

            List<object> resKeys = new();
            var downloadSize = 0L;


            if (checkHandler.Status == AsyncOperationStatus.Succeeded && checkHandler.Result.Count > 0)
            {
                var updateHandler = Addressables.UpdateCatalogs(checkHandler.Result, false);
                yield return updateHandler;

                if (updateHandler.Result != null)
                {
                    foreach (var resourceLocator in updateHandler.Result)
                    {
                        var sizeHandler = Addressables.GetDownloadSizeAsync(resourceLocator.Keys);
                        yield return sizeHandler;
                        if (sizeHandler.Result > 0)
                        {
                            resKeys.AddRange(resourceLocator.Keys);
                            downloadSize += sizeHandler.Result;
                        }

                        Addressables.Release(sizeHandler);
                    }
                }

                Addressables.Release(updateHandler);
            }

            Addressables.Release(checkHandler);
            waitWnd.Hide();

            if (downloadSize > 0)
            {
                var sizeStr = "";
                if (downloadSize < 1024 * 1024)
                {
                    var kb = downloadSize / 1024f;
                    sizeStr = $"{kb:F2}KB";
                }
                else
                {
                    var mb = downloadSize / 1024f / 1024f;
                    sizeStr = $"{mb:F2}MB";
                }

                var progWnd = UIProgressWindow.ShowProgress($"正在更新数据，共{sizeStr}");
                var downloadHandler =
                    Addressables.DownloadDependenciesAsync(resKeys, Addressables.MergeMode.Union, false);

                while (!downloadHandler.IsDone)
                {
                    progWnd.SetProgress(downloadHandler.PercentComplete);
                    yield return null;
                }

                progWnd.SetProgress(100);
                Addressables.Release(downloadHandler);

                //通知数据更新了
                GameFramework.GetService<EventService>().Emit(GameEvents.GameEvent.GameDataUpdated, "");
                yield return new WaitForSeconds(1);
                progWnd.Hide();
            }
        }
    }
}