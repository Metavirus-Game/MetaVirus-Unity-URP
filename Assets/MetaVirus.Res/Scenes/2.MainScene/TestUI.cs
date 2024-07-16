using FairyGUI;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    GComponent _mainView;

    // Start is called before the first frame update

    private void Awake()
    {
    }

    async void Start()
    {
        // _mainView = GetComponent<UIPanel>().ui;
        // _mainView.GetChild("btnMain").onClick.Add(() => { Debug.Log("按了按了按了啊！"); });
        //
        // var go = await Addressables.LoadAssetAsync<GameObject>(
        //     "Assets/MetaVirus.Res/Scenes/2.MainScene/Res/Particle_Tonado.prefab").Task;
        // go = Instantiate(go);
        // go.transform.localScale = new Vector3(70, 70, 70);
        // _mainView.GetChild("effect").asGraph.SetNativeObject(new GoWrapper(go));
    }

    // Update is called once per frame
    void Update()
    {
    }
}