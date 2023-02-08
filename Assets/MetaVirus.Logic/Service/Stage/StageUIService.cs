using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Base;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.NpcInteractive;
using UnityEditor.VersionControl;
using UnityEngine;

namespace MetaVirus.Logic.Service
{
    public class StageUIService : BaseService
    {
        private EntityService _entityService;
        private UIService _uiService;
        private GameDataService _gameDataService;
        private EventService _eventService;
        private FairyGUIService _fairyService;
        private NpcInteractiveListManager _npcInteractiveListManager;

        private UIJoystickController _uiJoystick;

        private GComponent _stageUi;

        private GList _lstButtons;

        private string[] _packages;

        public override void PostConstruct()
        {
            _eventService = Event;
            _uiService = GetService<UIService>();
            _entityService = GetService<EntityService>();
            _fairyService = GetService<FairyGUIService>();
            _gameDataService = GetService<GameDataService>();
        }

        public override void ServiceReady()
        {
            _eventService.On<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
            _eventService.On(GameEvents.ResourceEvent.AllResLoaded, OnAllResLoaded);
        }

        private void OnAllResLoaded()
        {
            StartCoroutine(LoadResources());
        }

        private IEnumerator LoadResources()
        {
            yield return null;
            var task = _fairyService.AddPackageAsync("ui-gameplay");
            yield return task.AsCoroution();

            _packages = task.Result;
            _stageUi = UIPackage.CreateObject("GamePlay", "StageUI").asCom;

            var anchor = _stageUi.GetChild("Anchor").asCom;
            _npcInteractiveListManager = new NpcInteractiveListManager(anchor);

            _lstButtons = _stageUi.GetChild("listButtons").asList;
            _lstButtons.itemRenderer = RenderMenu;
            _lstButtons.onClickItem.Set(ClickMenu);

            var menus = _gameDataService.gameTable.GameMenus.DataList;
            _lstButtons.numItems = menus.Count;
        }

        private void ClickMenu(EventContext context)
        {
            var obj = (GObject)context.data;
            var idx = _lstButtons.GetChildIndex(obj);
            var menus = _gameDataService.gameTable.GameMenus.DataList;
            var item = menus[idx];
            if (!string.IsNullOrEmpty(item.UiName))
            {
                _uiService.OpenWindow(item.UiName);
            }
        }

        private void RenderMenu(int index, GObject item)
        {
            var menus = _gameDataService.gameTable.GameMenus.DataList;
            var btn = item.asButton;
            btn.title = menus[index].Name;
            btn.icon = menus[index].IconUrl;
        }

        public override void PreDestroy()
        {
            _eventService.Remove<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
            _eventService.Remove(GameEvents.ResourceEvent.AllResLoaded, OnAllResLoaded);
            _stageUi.RemoveFromParent();
            _stageUi.Dispose();
            _fairyService.ReleasePackages(_packages);
        }

        private void OnMapChanged(MapChangedEvent evt)
        {
            if (evt.EvtType == MapChangedEvent.MapChangeEventType.Enter)
            {
                _fairyService.AddToGRootFullscreen(_stageUi);
                if (_uiJoystick == null)
                {
                    _uiJoystick = new UIJoystickController(_stageUi);
                }
            }
            else
            {
                _stageUi.RemoveFromParent();
            }
        }
    }
}