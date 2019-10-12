using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Day Vote", "birthdates", "1.0.0")]
    [Description("When night falls, players can vote for day.")]
    public class DayVote : RustPlugin
    {
        #region Language

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {
                    "VoteDay",
                    "Night time is coming, vote for day with <color=#eb3437>/voteday</color>. <color=#eb3437>{0} vote(s)</color> are needed for day."
                },
                {"Voted", "You have voted for day! <color=#eb3437>{0}</color>"},
                {"AlreadyVoted", "You have already voted!"},
                {"UserVoted", "A user has voted for day! <color=#eb3437>{0}</color>"},
                {"VoteHasPassed", "The vote has completed, turning to day..."},
                {"NotNight", "You can't vote for day when it's day!"}
            }, this);
        }

        #endregion

        #region Variables

        private const string permission_use = "dayvote.use";

        private TOD_Time Time;

        private int VotesNeeded;
        private readonly List<ulong> Voters = new List<ulong>();
        

        #endregion

        #region Hooks

        private void Init()
        {
            permission.RegisterPermission(permission_use, this);
            Time = TOD_Sky.Instance?.Components.Time;
            if (Time == null) return;
            Time.OnSunset += OnNight;
            Time.OnDay += OnDay;
        }

        private void Unload()
        {
            if (Time == null) return;
            Time.OnSunset -= OnNight;
            Time.OnDay -= OnDay;
        }

        private void OnDay()
        {
            Voters.Clear();
        }

        private void OnNight()
        {
            VotesNeeded = BasePlayer.activePlayerList.Count > 1 ? BasePlayer.activePlayerList.Count / 2 : 1;
            BasePlayer.activePlayerList.ForEach(Player =>
                Player.ChatMessage(string.Format(lang.GetMessage("VoteDay", this, Player.UserIDString), VotesNeeded)));
        }

        [ChatCommand("voteday")]
        private void ChatCommand(BasePlayer Player, string Label, string[] Args)
        {
            if (TOD_Sky.Instance.IsDay)
            {
                Player.ChatMessage(lang.GetMessage("NotNight", this, Player.UserIDString));
                return;
            }

            if (Voters.Contains(Player.userID))
            {
                Player.ChatMessage(lang.GetMessage("AlreadyVoted", this, Player.UserIDString));
                return;
            }

            Voters.Add(Player.userID);
            
            var Total = $"({Voters.Count} / {VotesNeeded})";
            Player.ChatMessage(string.Format(lang.GetMessage("Voted", this, Player.UserIDString), Total));
            if (Voters.Count == VotesNeeded)
            {
                BasePlayer.activePlayerList.ForEach(player =>
                    player.ChatMessage(lang.GetMessage("VoteHasPassed", this, Player.UserIDString)));
                TOD_Sky.Instance.Cycle.Hour = TOD_Sky.Instance.SunriseTime;
                return;
            }

            BasePlayer.activePlayerList.ForEach(player =>
                player.ChatMessage(string.Format(lang.GetMessage("UserVoted", this, Player.UserIDString), Total)));
        }

        #endregion
    }
}
//Generated with birthdates' Plugin Maker