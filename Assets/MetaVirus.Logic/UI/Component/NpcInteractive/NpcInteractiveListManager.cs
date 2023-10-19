using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.ObjectPool;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Events.Player;

namespace MetaVirus.Logic.UI.Component.NpcInteractive
{
    public class NpcInteractiveListManager
    {
        private readonly ObjectPoolService _objectPoolService;
        private readonly EventService _eventService;
        private readonly GComponent _rootComp;

        private readonly ObjectPool<NpcInteractiveItem> _itemPool;

        private readonly List<NpcInteractiveItem> _npcItems = new();

        // private NpcInteractiveItem _tmpItemCom;

        private const string PoolName = "NpcInteractiveListItemPool";

        public NpcInteractiveListManager(GComponent rootComp)
        {
            _rootComp = rootComp;
            _eventService = GameFramework.GetService<EventService>();
            _objectPoolService = GameFramework.GetService<ObjectPoolService>();
            _itemPool = _objectPoolService.NewObjectPool(PoolName, initSize: 5,
                newObjFunc: () => new NpcInteractiveItem());

            _eventService.On<PlayerInteractiveNpcListChangedEvent>(GameEvents.PlayerEvent.InteractiveNpcListChanged,
                OnInteractiveNpcListChanged);
        }

        public void Release()
        {
            _objectPoolService.ClearObjectPool<NpcInteractiveItem>(PoolName);
            _eventService.Remove<PlayerInteractiveNpcListChangedEvent>(GameEvents.PlayerEvent.InteractiveNpcListChanged,
                OnInteractiveNpcListChanged);
            _npcItems.Clear();
        }

        private void OnInteractiveNpcListChanged(PlayerInteractiveNpcListChangedEvent evt)
        {
            if (evt.Type == PlayerInteractiveNpcListChangedEvent.EventType.Added)
            {
                OnInteractiveNpcAdded(evt.NpcId, evt.NpcList);
            }
            else
            {
                OnInteractiveNpcRemoved(evt.NpcId, evt.NpcList);
            }
        }

        private void RefreshNpcItems(int[] npcList)
        {
            for (var i = 0; i < npcList.Length; i++)
            {
                var id = npcList[i];
                NpcInteractiveItem item = null;
                if (i < _npcItems.Count)
                {
                    item = _npcItems[i];
                    item.SetNpcId(id);
                }
                else
                {
                    item = _itemPool.Get<NpcInteractiveItem>();
                    _npcItems.Add(item);
                    item.SetNpcId(id);
                    _rootComp.AddChild(item.UiComp);
                }

                var comp = item.UiComp;
                comp.SetPosition(0, -comp.size.y / 2 - (comp.size.y + 15) * i, 0);
            }

            while (_npcItems.Count > npcList.Length)
            {
                var last = _npcItems.Count - 1;
                var item = _npcItems[last];
                _npcItems.RemoveAt(last);
                _itemPool.Release(item);
            }
        }

        private void OnInteractiveNpcAdded(int npcId, int[] npcList)
        {
            // _tmpItemCom?.OnRecycle();
            // _tmpItemCom = new NpcInteractiveItem();
            // _tmpItemCom.SetNpcId(npcId);
            // var comp = _tmpItemCom.UiComp;
            // _rootComp.AddChild(comp);
            // comp.SetPosition(-comp.size.x / 2, -comp.size.y / 2, 0);
            RefreshNpcItems(npcList);
        }

        private void OnInteractiveNpcRemoved(int npcId, int[] npcList)
        {
            // _tmpItemCom?.OnRecycle();
            RefreshNpcItems(npcList);
        }
    }
}