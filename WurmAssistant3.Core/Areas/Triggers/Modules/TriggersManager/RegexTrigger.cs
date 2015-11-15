﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.SoundEngine.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.TrayPopups.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Data;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Views.TriggersManager;

namespace AldursLab.WurmAssistant3.Core.Areas.Triggers.Modules.TriggersManager
{
    //[DataContract]
    public class RegexTrigger : SimpleConditionTriggerBase
    {
        readonly ILogger logger;

        public RegexTrigger(TriggerData triggerData, ISoundEngine soundEngine, ITrayPopups trayPopups, IWurmApi wurmApi,
            ILogger logger)
            : base(triggerData, soundEngine, trayPopups, wurmApi, logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger;
            ConditionHelp = "Use C# regular expression pattern";
        }

        protected override bool CheckCondition(LogEntry logMessage)
        {
            if (TriggerData.MatchEveryLine) return true;

            if (string.IsNullOrEmpty(TriggerData.Condition))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(logMessage.Content, TriggerData.Condition);
            }
            catch (Exception exception)
            {
                logger.Error(exception,
                    string.Format(
                        "Exception while checking regex trigger condition. Trigger name: {0}, Condition: {1}",
                        Name,
                        TriggerData.Condition));
                return false;
            }
        }

        public override string TypeAspect
        {
            get { return "Regex"; }
        }

        public override IEnumerable<ITriggerConfig> Configs
        {
            get { return new List<ITriggerConfig>(base.Configs) { new RegexTriggerConfig(this, logger) }; }
        }
    }
}
