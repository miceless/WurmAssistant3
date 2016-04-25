﻿using System;
using System.IO;
using JetBrains.Annotations;

namespace AldursLab.WurmApi.Modules.Wurm.Paths
{
    class WurmPaths : IWurmPaths
    {
        readonly IWurmApiConfig wurmApiConfig;

        private readonly string playersDirPath;

        public WurmPaths(IWurmClientInstallDirectory wurmInstallDirectory, [NotNull] IWurmApiConfig wurmApiConfig)
        {
            if (wurmApiConfig == null) throw new ArgumentNullException(nameof(wurmApiConfig));
            this.wurmApiConfig = wurmApiConfig;
            ConfigsDirFullPath = Path.Combine(wurmInstallDirectory.FullPath, "configs");
            playersDirPath = Path.Combine(wurmInstallDirectory.FullPath, "players");
        }

        public string LogsDirName => wurmApiConfig.WurmUnlimitedMode ? "test_logs" : "logs";

        public string ConfigsDirFullPath { get; }

        public string CharactersDirFullPath => playersDirPath;

        public string GetSkillDumpsFullPathForCharacter(CharacterName characterName)
        {
            return Path.Combine(playersDirPath,
                characterName.Capitalized,
                wurmApiConfig.WurmUnlimitedMode ? "test_dumps" : "dumps");
        }

        public string GetLogsDirFullPathForCharacter(CharacterName characterName)
        {
            return Path.Combine(playersDirPath,
                characterName.Capitalized,
                LogsDirName);
        }

        public string GetScreenshotsDirFullPathForCharacter(CharacterName characterName)
        {
            return Path.Combine(playersDirPath,
                characterName.Capitalized,
                wurmApiConfig.WurmUnlimitedMode ? "test_screenshots" : "screenshots");
        }
    }
}
