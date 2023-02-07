using System;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Procedures;
using UnityEngine;

namespace MetaVirus.Logic.Service
{
    [Obsolete]
    public class GameEntryService : BaseService
    {
        private Type _entryProcedure;

        public override void ServiceReady()
        {
            // GetService<EventService>().On(Events.Engine.EngineStarted, OnGameStarted);
            // _entryProcedure = typeof(GameEntryProcedure);
        }

        // private void OnGameStarted()
        // {
        //     if (_entryProcedure != null)
        //     {
        //         GetService<ProcedureService>().StartProcedure(_entryProcedure);
        //     }
        // }
    }
}