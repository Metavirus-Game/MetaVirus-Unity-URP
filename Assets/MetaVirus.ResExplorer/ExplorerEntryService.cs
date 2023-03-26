using System;
using System.Collections;
using System.Collections.Generic;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Procedure;
using MetaVirus.ResExplorer.Procedures;
using UnityEngine;

public class ExplorerEntryService : BaseService
{
    private Type _entryProcedure;

    public override void PostConstruct()
    {
        GetService<EventService>().On(Events.Engine.EngineStarted, OnGameStarted);
        _entryProcedure = typeof(ExplorerProcedure);
    }

    private void OnGameStarted()
    {
        // if (_entryProcedure != null)
        // {
        //     GetService<ProcedureService>().StartProcedure(_entryProcedure);
        // }
    }

    public override void ServiceReady()
    {
    }
}