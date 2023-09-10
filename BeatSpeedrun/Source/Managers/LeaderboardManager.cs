using System;
using BeatSpeedrun.Controllers;
using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;

namespace BeatSpeedrun.Managers
{
    internal class LeaderboardManager : CustomLeaderboard, IDisposable
    {
        private readonly CustomLeaderboardManager _customLeaderboardManager;
        private readonly LeaderboardPanelViewController _panel;
        private readonly LeaderboardMainViewController _main;

        internal LeaderboardManager(
            CustomLeaderboardManager customLeaderboardManager,
            LeaderboardPanelViewController panel,
            LeaderboardMainViewController main)
        {
            _customLeaderboardManager = customLeaderboardManager;
            _panel = panel;
            _main = main;
            _customLeaderboardManager.Register(this);
        }

        protected override ViewController panelViewController => _panel;
        protected override ViewController leaderboardViewController => _main;

        public void Dispose()
        {
            _customLeaderboardManager.Unregister(this);
        }
    }
}
