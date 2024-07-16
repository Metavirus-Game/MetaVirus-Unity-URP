using Firebase.Analytics;
using GameEngine.Base;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events;
using static MetaVirus.Logic.Data.GameEvents;

namespace MetaVirus.Logic.Service
{
    public class FirebaseAnalyticsService : BaseService
    {
        public override void ServiceReady()
        {
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
            Event.On(GameEvent.OpenMainPage, OnOpenMainPage);
            Event.On(GameEvent.CreateActorOpened, OnOpenCreateActor);
            Event.On(GameEvent.CreateActorDone, OnCreatedActor);
            Event.On(AccountEvent.AccountLogin, OnAccountLogin);
            Event.On(AccountEvent.PlayerLogin, OnPlayerLogin);
            Event.On<Constants.BattleResult>(ArenaEvent.ArenaMatch, OnArenaMatch);
            Event.On<MapChangedEvent>(MapEvent.MapChanged, OnMapChanged);
        }

        private void OnOpenMainPage()
        {
            FirebaseAnalytics.LogEvent("open_ui_mainpage");
        }

        private void OnOpenCreateActor()
        {
            FirebaseAnalytics.LogEvent("open_ui_createactor");
        }

        private void OnCreatedActor()
        {
            FirebaseAnalytics.LogEvent("actor_created");
        }

        private void OnAccountLogin()
        {
            FirebaseAnalytics.LogEvent("login_account");
        }

        private void OnPlayerLogin()
        {
            FirebaseAnalytics.LogEvent("login_player");
        }

        private void OnArenaMatch(Constants.BattleResult result)
        {
            var r = result switch
            {
                Constants.BattleResult.Win => "win",
                Constants.BattleResult.Draw => "Draw",
                Constants.BattleResult.Lose => "Lose",
                _ => ""
            };
            FirebaseAnalytics.LogEvent("arena_match", "result", r);
        }

        private void OnMapChanged(MapChangedEvent evt)
        {
            if (evt.EvtType == MapChangedEvent.MapChangeEventType.Enter)
            {
                FirebaseAnalytics.LogEvent("enter_map", "map_id", evt.MapId);
            }
        }
    }
}