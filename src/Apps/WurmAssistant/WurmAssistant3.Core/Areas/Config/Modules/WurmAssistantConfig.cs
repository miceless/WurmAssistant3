using System;
using AldursLab.PersistentObjects;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.Areas.Config.Contracts;
using AldursLab.WurmAssistant3.Core.Root.Components;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace AldursLab.WurmAssistant3.Core.Areas.Config.Modules
{
    [PersistentObject("WurmAssistantConfig")]
    public class WurmAssistantConfig : PersistentObjectBase, IWurmAssistantConfig
    {
        readonly ConsoleArgsManager consoleArgsManager;

        [JsonProperty]
        int version = 0;

        [JsonProperty]
        string wurmGameClientInstallDirectory;

        [JsonProperty]
        Platform runningPlatform;

        [JsonProperty]
        bool reSetupRequested;

        [JsonProperty]
        bool dropAllWurmApiCachesToggle;

        public WurmAssistantConfig([NotNull] ConsoleArgsManager consoleArgsManager)
        {
            if (consoleArgsManager == null) throw new ArgumentNullException("consoleArgsManager");
            this.consoleArgsManager = consoleArgsManager;
        }

        protected override void OnPersistentDataLoaded()
        {
            if (version == 0)
            {
                dropAllWurmApiCachesToggle = true;
                version = 1;
            }
        }

        public string WurmGameClientInstallDirectory
        {
            get { return wurmGameClientInstallDirectory; }
            set
            {
                if (value == wurmGameClientInstallDirectory)
                    return;
                wurmGameClientInstallDirectory = value;
                FlagAsChanged();
            }
        }

        public Platform RunningPlatform
        {
            get { return runningPlatform; }
            set
            {
                if (value == runningPlatform)
                    return;
                runningPlatform = value;
                FlagAsChanged();
            }
        }

        public bool ReSetupRequested
        {
            get { return reSetupRequested; }
            set
            {
                if (value == reSetupRequested)
                    return;
                reSetupRequested = value;
                FlagAsChanged();
            }
        }

        public bool DropAllWurmApiCachesToggle
        {
            get { return dropAllWurmApiCachesToggle; }
            set
            {
                if (value == dropAllWurmApiCachesToggle)
                    return;
                dropAllWurmApiCachesToggle = value;
                FlagAsChanged();
            }
        }

        public bool WurmUnlimitedMode
        {
            get { return consoleArgsManager.WurmUnlimitedMode; }
        }
    }
}