﻿using System;
using System.Collections.Generic;
using System.Linq;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.Areas.Granger.Legacy.DBlayer;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.TrayPopups.Contracts;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace AldursLab.WurmAssistant3.Core.Areas.Granger.Legacy.LogFeedManager
{
    public class LogsFeedManager : IDisposable
    {
        public class SkillObtainedEventArgs : EventArgs
        {
            public readonly string PlayerName;
            public SkillObtainedEventArgs(string playerName)
            {
                this.PlayerName = playerName;
            }
        }

        readonly GrangerContext context;
        readonly IWurmApi wurmApi;
        readonly ILogger logger;
        readonly ITrayPopups trayPopups;
        readonly GrangerFeature parentModule;
        readonly Dictionary<string, PlayerManager> playerManagers = new Dictionary<string, PlayerManager>();

        public LogsFeedManager(GrangerFeature parentModule, GrangerContext context, [NotNull] IWurmApi wurmApi,
            [NotNull] ILogger logger, [NotNull] ITrayPopups trayPopups)
        {
            if (wurmApi == null) throw new ArgumentNullException("wurmApi");
            if (logger == null) throw new ArgumentNullException("logger");
            if (trayPopups == null) throw new ArgumentNullException("trayPopups");
            this.parentModule = parentModule;
            this.context = context;
            this.wurmApi = wurmApi;
            this.logger = logger;
            this.trayPopups = trayPopups;
        }

        public void TryRegisterPlayer(string playerName)
        {
            if (!playerManagers.ContainsKey(playerName))
            {
                try
                {
                    playerManagers[playerName] = new PlayerManager(parentModule, context, playerName, wurmApi, logger, trayPopups);
                }
                catch (Exception exception)
                {
                    logger.Error(exception, "Count not register PlayerManager for player name: " + playerName);
                }
            }
        }


        public void UnregisterPlayer(string playerName)
        {
            PlayerManager ph;
            if (playerManagers.TryGetValue(playerName, out ph))
            {
                ph.Dispose();
                playerManagers.Remove(playerName);
            }
        }

        /// <summary>
        /// null if no skill available yet (ah skill search not finished or server group not established)
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public float? GetSkillForPlayer(string playerName)
        {
            PlayerManager ph;
            if (playerManagers.TryGetValue(playerName, out ph))
            {
                return ph.GetAhSkill();
            }
            else return null;
        }

        public void Dispose()
        {
            foreach (var keyval in playerManagers)
            {
                keyval.Value.Dispose();
            }
        }

        internal void UpdatePlayers(IEnumerable<string> players)
        {
            var list = players.ToList();

            foreach (var player in list)
            {
                if (!playerManagers.ContainsKey(player))
                {
                    TryRegisterPlayer(player);
                }
            }

            var managedPlayers = playerManagers.Keys.ToArray();
            foreach (var player in managedPlayers)
            {
                if (!list.Contains(player))
                    UnregisterPlayer(player);
            }
        }

        internal void Update()
        {
            foreach (var keyval in playerManagers)
            {
                keyval.Value.Update();
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AhSkillInfo
    {
        [JsonProperty]
        readonly string serverGroupId;
        [JsonProperty]
        readonly string playerName;
        [JsonProperty]
        float skillValue;
        [JsonProperty] 
        DateTime lastCheck;

        public AhSkillInfo(string serverGroupId, string playerName, float skillValue, DateTime lastCheck)
        {
            this.serverGroupId = serverGroupId;
            this.playerName = playerName;
            this.skillValue = skillValue;
            this.lastCheck = lastCheck;
        }

        public string ServerGroupId
        {
            get { return serverGroupId; }
        }

        public string PlayerName
        {
            get { return playerName; }
        }

        public float SkillValue
        {
            get { return skillValue; }
            set { skillValue = value; }
        }

        public DateTime LastCheck
        {
            get { return lastCheck; }
            set { lastCheck = value; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AhSkillInfo)) return false;
            var other = (AhSkillInfo)obj;
            return Equals(other);
        }

        public bool Equals(AhSkillInfo other)
        {
            return serverGroupId == other.serverGroupId && playerName == other.playerName;
        }

        public override int GetHashCode()
        {
            return unchecked((
                playerName == null ? String.Empty.GetHashCode() : playerName.GetHashCode()) * serverGroupId.GetHashCode());
        }
    }
}