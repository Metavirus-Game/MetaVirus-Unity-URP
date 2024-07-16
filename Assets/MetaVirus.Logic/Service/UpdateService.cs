using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameEngine;
using GameEngine.Base;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.UI;
using UnityEngine;

namespace MetaVirus.Logic.Service
{
    /// <summary>
    /// 更新服务
    /// </summary>
    public class UpdateService : BaseService
    {
        /// <summary>
        /// 异步更新方法
        /// </summary>
        /// <returns>返回更新数据的字节数，=0表示没有更新</returns>
        public async UniTask<long> AsyncUpdate()
        {
            // Addressables.InitializeAsync(true);
            // Debug.Log("Checking Update....");
            // var t = Time.timeSinceLevelLoad;
            // var waitWnd = UIWaitingWindow.ShowWaiting("Loading......");
            //
            // var checkHandler = Addressables.CheckForCatalogUpdates(false);
            // await checkHandler.Task;
            //
            // t = Time.timeSinceLevelLoad - t;
            // var wait = 1 - t;
            // if (wait > 0)
            // {
            //     await Task.Delay((int)(wait * 1000));
            // }
            //
            // List<object> resKeys = new();
            // var downloadSize = 0L;
            //
            // if (checkHandler.Status == AsyncOperationStatus.Succeeded && checkHandler.Result.Count > 0)
            // {
            //     Debug.Log($"Total {checkHandler.Result.Count} Files need to be updated...");
            //     var updateHandler = Addressables.UpdateCatalogs(checkHandler.Result, false);
            //     await updateHandler.Task;
            //
            //     if (updateHandler.Result != null)
            //     {
            //         foreach (var resourceLocator in updateHandler.Result)
            //         {
            //             var sizeHandler = Addressables.GetDownloadSizeAsync(resourceLocator.Keys);
            //             await sizeHandler.Task;
            //             if (sizeHandler.Result > 0)
            //             {
            //                 resKeys.AddRange(resourceLocator.Keys);
            //                 downloadSize += sizeHandler.Result;
            //             }
            //
            //             Addressables.Release(sizeHandler);
            //         }
            //     }
            //
            //     Addressables.Release(updateHandler);
            // }
            //
            // Addressables.Release(checkHandler);
            // waitWnd.Hide();
            //
            // if (downloadSize > 0)
            // {
            //     var sizeStr = "";
            //     if (downloadSize < 1024 * 1024)
            //     {
            //         var kb = downloadSize / 1024f;
            //         sizeStr = $"{kb:F2}KB";
            //     }
            //     else
            //     {
            //         var mb = downloadSize / 1024f / 1024f;
            //         sizeStr = $"{mb:F2}MB";
            //     }
            //
            //     var progWnd = UIProgressWindow.ShowProgress($"Updating.....Total {sizeStr}");
            //     var downloadHandler =
            //         Addressables.DownloadDependenciesAsync(resKeys, Addressables.MergeMode.Union, false);
            //
            //     while (!downloadHandler.IsDone)
            //     {
            //         progWnd.SetProgress(downloadHandler.PercentComplete * 100);
            //         await Task.Delay(1);
            //     }
            //
            //     progWnd.SetProgress(100);
            //     Addressables.Release(downloadHandler);
            //
            //     //通知数据更新了
            //     GameFramework.GetService<EventService>().Emit(GameEvents.GameEvent.GameDataUpdated, "");
            //     await Task.Delay(1000);
            //     progWnd.Hide();
            //
            //     Debug.Log($@"Update Done, Total {downloadSize} Bytes downloaded....");
            //     return downloadSize;
            // }

            return 0;
        }

        public IEnumerator CheckUpdate()
        {
            yield return null;
            // Addressables.InitializeAsync(true);
            //
            // Debug.Log("Checking Update....");
            // var t = Time.timeSinceLevelLoad;
            // var waitWnd = UIWaitingWindow.ShowWaiting("正在检查更新，请稍候...");
            // var checkHandler = Addressables.CheckForCatalogUpdates(false);
            // yield return checkHandler;
            //
            // t = Time.timeSinceLevelLoad - t;
            // var wait = 1 - t;
            // if (wait > 0)
            // {
            //     yield return new WaitForSeconds(wait);
            // }
            //
            // List<object> resKeys = new();
            // var downloadSize = 0L;
            //
            //
            // if (checkHandler.Status == AsyncOperationStatus.Succeeded && checkHandler.Result.Count > 0)
            // {
            //     Debug.Log($@"Total {checkHandler.Result.Count} Files need to be updated...");
            //     var updateHandler = Addressables.UpdateCatalogs(checkHandler.Result, false);
            //     yield return updateHandler;
            //
            //     if (updateHandler.Result != null)
            //     {
            //         foreach (var resourceLocator in updateHandler.Result)
            //         {
            //             var sizeHandler = Addressables.GetDownloadSizeAsync(resourceLocator.Keys);
            //             yield return sizeHandler;
            //             if (sizeHandler.Result > 0)
            //             {
            //                 resKeys.AddRange(resourceLocator.Keys);
            //                 downloadSize += sizeHandler.Result;
            //             }
            //
            //             Addressables.Release(sizeHandler);
            //         }
            //     }
            //
            //     Addressables.Release(updateHandler);
            // }
            //
            // Addressables.Release(checkHandler);
            // waitWnd.Hide();
            //
            // if (downloadSize > 0)
            // {
            //     var sizeStr = "";
            //     if (downloadSize < 1024 * 1024)
            //     {
            //         var kb = downloadSize / 1024f;
            //         sizeStr = $"{kb:F2}KB";
            //     }
            //     else
            //     {
            //         var mb = downloadSize / 1024f / 1024f;
            //         sizeStr = $"{mb:F2}MB";
            //     }
            //
            //     var progWnd = UIProgressWindow.ShowProgress($"正在更新数据，共{sizeStr}");
            //     var downloadHandler =
            //         Addressables.DownloadDependenciesAsync(resKeys, Addressables.MergeMode.Union, false);
            //
            //     while (!downloadHandler.IsDone)
            //     {
            //         progWnd.SetProgress(downloadHandler.PercentComplete * 100);
            //         yield return null;
            //     }
            //
            //     progWnd.SetProgress(100);
            //     Addressables.Release(downloadHandler);
            //
            //     //通知数据更新了
            //     GameFramework.GetService<EventService>().Emit(GameEvents.GameEvent.GameDataUpdated, "");
            //     yield return new WaitForSeconds(1);
            //     progWnd.Hide();
            //
            //     Debug.Log($@"Update Done, Total {downloadSize} Bytes downloaded....");
            // }
        }
    }
}