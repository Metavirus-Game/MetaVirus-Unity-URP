using System.Collections.Generic;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;

namespace MetaVirus.Logic.Utils
{
    public static class GameUtil
    {
        public static int PetDataSortByQuality(PlayerPetData p1, PlayerPetData p2)
        {
            if (p2.Quality == p1.Quality)
            {
                return p2.Level - p1.Level;
            }

            return p2.Quality - p1.Quality;
        }

        public static int PetDataSortBySpecies(PlayerPetData p1, PlayerPetData p2)
        {
            if (p2.Type.Id != p1.Type.Id) return p1.Type.Id - p2.Type.Id;

            if (p2.Quality == p1.Quality)
            {
                return p2.Level - p1.Level;
            }

            return p2.Quality - p1.Quality;
        }

        // public static IEnumerator LoadMonsterModelToWrapper(string npcResAddress, GGraph modelAnchor,
        //     GoWrapper modelWrapper)
        // {
        //     var task = Addressables.InstantiateAsync(npcResAddress).Task;
        //     yield return task.AsCoroution();
        //     var modelResObj = task.Result;
        //     modelResObj.SetActive(true);
        //     modelResObj.GetComponent<NavMeshAgent>().enabled = false;
        //
        //     modelResObj.transform.localPosition = new Vector3(50, -80, 1000);
        //     modelResObj.transform.localEulerAngles = new Vector3(0, 180, 0);
        //     modelResObj.transform.localScale = new Vector3(80, 80, 80);
        //     modelAnchor.SetNativeObject(modelWrapper);
        //     modelWrapper.SetWrapTarget(modelResObj, true);
        //
        //     // var ps = modelResObj.GetComponentsInChildren<ParticleSystem>();
        //     // foreach (var p in ps)
        //     // {
        //     //     var position = p.transform.position;
        //     //     var rot = p.transform.rotation;
        //     //     var anchor = new GGraph();
        //     //     modelAnchor.parent.AddChild(anchor);
        //     //     anchor.position = modelAnchor.position;
        //     //     anchor.rotation = modelAnchor.rotation;
        //     //     var wrapper = new GoWrapper();
        //     //     anchor.SetNativeObject(wrapper);
        //     //     wrapper.SetWrapTarget(p.gameObject, true);
        //     //
        //     //     p.transform.position = position;
        //     //     position = p.transform.localPosition;
        //     //     position.z = -500;
        //     //     p.transform.localPosition = position;
        //     //     p.transform.rotation = rot;
        //     // }
        // }

        public static string HourToString(int hours)
        {
            string textKey;

            Dictionary<string, string> replace = new();

            switch (hours)
            {
                case < 1:
                    textKey = "common.text.hour.less1";
                    break;
                case < 24:
                    textKey = "common.text.hour.left";
                    replace["%h"] = hours.ToString();
                    break;
                default:
                    textKey = "common.text.hour.days";
                    replace["%h"] = (hours % 24).ToString();
                    replace["%d"] = (hours / 24).ToString();
                    break;
            }

            return GameDataService.LT(textKey, replace);
        }
    }
}