using System.Collections;
using System.Collections.Generic;
using GameEngine;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Service.Battle;
using UnityEngine;

public class BattleArea : MonoBehaviour
{
    public Camera battleAreaCamera;
    public BattleUnitAni attackUnit;
    public BattleUnitAni defenceUnit;
    public Transform defenceCenter;
    public RenderTexture texture;

    public Transform cameraAnchor;

    public GameObject[] defenceSlots;

    private static readonly List<int> RandomMonsterIds = new()
    {
        1011, 1012, 1013, 1023, 1032, 1033, 1041, 1042, 1051, 1052, 1053, 1061, 1062, 1063, 1073, 2011, 2013, 2021,
        2022, 2023, 2033, 2042, 2043, 2051, 2052, 2053, 2061, 2062, 2063, 2073, 2081, 2082, 2083
    };


    // Start is called before the first frame update
    void Start()
    {
        battleAreaCamera.targetTexture = texture;
        //LoadFormation();
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">0-4</param>
    /// <returns></returns>
    public BattleUnitAni GetUnitFromSlot(int index)
    {
        var go = defenceSlots[index];
        var u = go.transform.GetChild(0);
        return u == null ? null : u.GetComponent<BattleUnitAni>();
    }

    public void SwitchToSingleTarget()
    {
        attackUnit.gameObject.SetActive(true);
        defenceUnit.gameObject.SetActive(true);
        defenceUnit.OnActive();

        foreach (var slot in defenceSlots)
        {
            if (slot.transform.childCount == 0) continue;
            var unit = slot.transform.GetChild(0);
            unit.gameObject.SetActive(false);
        }
    }

    /*
     * 多人战斗模拟转移到BattleSimulator中了，此处只隐藏单体战斗对象
     */
    public void SwitchToFormationTarget()
    {
        attackUnit.gameObject.SetActive(false);
        defenceUnit.gameObject.SetActive(false);
        // foreach (var slot in defenceSlots)
        // {
        //     if (slot.transform.childCount == 0)
        //     {
        //         GameFramework.Inst.StartCoroutine(RandomLoadbattleUnit(slot.transform));
        //     }
        //     else
        //     {
        //         var unit = slot.transform.GetChild(0);
        //         unit.gameObject.SetActive(true);
        //         unit.GetComponent<BattleUnitAni>().OnActive();
        //     }
        // }
    }

    public static int RandomUnitId()
    {
        var idx = Random.Range(0, RandomMonsterIds.Count);
        var id = RandomMonsterIds[idx];
        RandomMonsterIds.RemoveAt(idx);
        return id;
    }

    private void LoadFormation()
    {
        foreach (var slot in defenceSlots)
        {
            if (slot.transform.childCount == 0)
            {
                GameFramework.Inst.StartCoroutine(RandomLoadbattleUnit(slot.transform));
            }
        }
    }

    private static IEnumerator RandomLoadbattleUnit(Transform parent)
    {
        // var id = RandomUnitId();
        //
        // var path = Constants.ResAddress.BattleUnitRes(id);
        // var r = Addressables.InstantiateAsync(path);
        // yield return r;
        // var obj = r.Result;
        //
        // obj.transform.SetParent(parent);
        // obj.transform.localPosition = Vector3.zero;
        // obj.transform.localRotation = Quaternion.identity;
        // obj.SetActive(true);
        // obj.GetComponent<BattleUnitAni>().OnActive();
        yield return null;
    }
}