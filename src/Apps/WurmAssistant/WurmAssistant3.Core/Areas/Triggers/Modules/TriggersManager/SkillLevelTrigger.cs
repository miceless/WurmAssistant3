using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AldursLab.Essentials.Asynchronous;
using AldursLab.Essentials.Extensions.DotNet;
using AldursLab.WurmApi;
using AldursLab.WurmApi.Modules.Wurm.Characters;
using AldursLab.WurmApi.Modules.Wurm.Characters.Skills;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.SoundManager.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.TrayPopups.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Data;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Views.TriggersManager;

namespace AldursLab.WurmAssistant3.Core.Areas.Triggers.Modules.TriggersManager
{
    public class SkillLevelTrigger : TriggerBase
    {
        readonly string characterName;
        readonly ILogger logger;

        SkillInfo lastSkillLevel = null;
        readonly SkillEntryParser skillEntryParser;
        readonly IWurmCharacter character;

        readonly TriggerableAsyncOperation skillHistoricRefresher;

        WeakReference<SkillLevelTriggerConfig> configViewWeakRef;

        public SkillLevelTrigger(string characterName, TriggerData triggerData, ISoundManager soundManager,
            ITrayPopups trayPopups, IWurmApi wurmApi, ILogger logger)
            : base(triggerData, soundManager, trayPopups, wurmApi, logger)
        {
            if (characterName == null) throw new ArgumentNullException("characterName");
            this.characterName = characterName;
            this.logger = logger;
            skillEntryParser = new SkillEntryParser(wurmApi);
            LockedLogTypes = new[] {LogType.Skills};

            SkillFeedback = "(no data)";

            skillHistoricRefresher = new TriggerableAsyncOperation(RefreshSkill);

            character = wurmApi.Characters.Get(characterName);
            character.LogInOrCurrentServerPotentiallyChanged += CharacterOnLogInOrCurrentServerPotentiallyChanged;

            skillHistoricRefresher.Trigger();
        }

        private void CharacterOnLogInOrCurrentServerPotentiallyChanged(object sender, PotentialServerChangeEventArgs potentialServerChangeEventArgs)
        {
            skillHistoricRefresher.Trigger();
        }

        async Task RefreshSkill()
        {
            try
            {
                var currentServer = await character.TryGetCurrentServerAsync();
                if (currentServer != null)
                {
                    var skillInfo =
                        await character.Skills.TryGetCurrentSkillLevelAsync(TriggerData.SkillTriggerSkillName,
                            currentServer.ServerGroup,
                            TimeSpan.FromDays(90));
                    if (skillInfo != null)
                    {
                        if (skillInfo.IsSkillName(SkillName)
                            && (lastSkillLevel == null || lastSkillLevel.Stamp < skillInfo.Stamp))
                        {
                            UpdateLastSkillLevel(skillInfo);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Skill trigger init error");
            }
        }

        public string SkillName
        {
            get { return TriggerData.SkillTriggerSkillName; }
            set
            {
                TriggerData.SkillTriggerSkillName = value ?? string.Empty;
                lastSkillLevel = null;
                SkillFeedback = "(no data)";
                UpdateConfigView();
                skillHistoricRefresher.Trigger();
            }
        }

        public double TriggerSkillLevel
        {
            get { return TriggerData.SkillTriggerTreshhold; }
            set { TriggerData.SkillTriggerTreshhold = value; }
        }

        public string SkillFeedback { get; private set; }

        public override bool CheckLogType(LogType type)
        {
            return type == LogType.Skills;
        }

        protected override bool CheckCondition(LogEntry logMessage)
        {
            if (string.IsNullOrWhiteSpace(SkillName)) return false;

            bool conditionMatched = false;

            if (logMessage.Content.Contains(" increased") || logMessage.Content.Contains(" decreased"))
            {
                var info = skillEntryParser.TryParseSkillInfoFromLogLine(logMessage);
                if (info != null)
                {
                    if (info.IsSkillName(TriggerData.SkillTriggerSkillName))
                    {
                        if (lastSkillLevel != null)
                        {
                            if (lastSkillLevel.Value <= TriggerData.SkillTriggerTreshhold
                                && info.Value > TriggerData.SkillTriggerTreshhold)
                            {
                                conditionMatched = true;
                            }
                        }
                        UpdateLastSkillLevel(info);
                    }
                }
            }
            return conditionMatched;
        }

        private void UpdateLastSkillLevel(SkillInfo skillInfo)
        {
            lastSkillLevel = skillInfo;
            SkillFeedback = string.Format("Found skill {0} to be {1}",
                skillInfo.NameNormalized.ToLowerInvariant().Capitalize(),
                skillInfo.Value);
            UpdateConfigView();
        }

        void UpdateConfigView()
        {
            SkillLevelTriggerConfig config;
            if (configViewWeakRef != null && configViewWeakRef.TryGetTarget(out config))
            {
                config.RefreshSkillFeedback();
            }
        }

        public override IEnumerable<ITriggerConfig> Configs
        {
            get
            {
                return new List<ITriggerConfig>(base.Configs) { new SkillLevelTriggerConfig(this) };
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (character != null)
            {
                character.LogInOrCurrentServerPotentiallyChanged -= CharacterOnLogInOrCurrentServerPotentiallyChanged;
            }
            base.Dispose(disposing);
        }

        public void SubscribeForRefresh(SkillLevelTriggerConfig skillLevelTriggerConfig)
        {
            configViewWeakRef = new WeakReference<SkillLevelTriggerConfig>(skillLevelTriggerConfig);
        }

        public override string ConditionAspect
        {
            get { return "When " + SkillName + " goes above " + TriggerSkillLevel; }
        }

        public override string TypeAspect
        {
            get { return "Skill level"; }
        }
    }
}