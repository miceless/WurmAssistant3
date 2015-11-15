﻿using System;
using System.Collections.Generic;
using System.Linq;
using AldursLab.PersistentObjects;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Persistence.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.SoundEngine.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.TrayPopups.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Data;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Modules.TriggersManager;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Views;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Views.TriggersManager;
using AldursLab.WurmAssistant3.Core.Properties;
using AldursLab.WurmAssistant3.Core.Root.Contracts;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Ninject;

namespace AldursLab.WurmAssistant3.Core.Areas.Triggers.Modules
{
    [PersistentObject("TriggersFeature_TriggerManager")]
    public class TriggerManager : PersistentObjectBase, IInitializable
    {
        [JsonProperty] bool muted = false;

        [JsonProperty]
        public byte[] TriggerListState;

        //todo: this wont work any more, deserializer wont have a clue about actual type

        //merge all trigger types into one type

        ActiveTriggers activeTriggers;

        UcPlayerTriggersController controlUi;

        public readonly string CharacterName;

        FormTriggersConfig triggersConfigUi;

        readonly IWurmApi wurmApi;

        readonly IHostEnvironment hostEnvironment;

        readonly ISoundEngine soundEngine;

        readonly ILogger logger;

        readonly ITrayPopups trayPopups;
        readonly IPersistentObjectResolver<ActiveTriggers> activeTriggersResolver;
        // previous processed line

        string lastLineContent;

        public TriggerManager([NotNull] string persistentObjectId, [NotNull] IWurmApi wurmApi,
            [NotNull] IHostEnvironment hostEnvironment, [NotNull] ISoundEngine soundEngine, [NotNull] ILogger logger,
            [NotNull] ITrayPopups trayPopups, [NotNull] IPersistentObjectResolver<ActiveTriggers> activeTriggersResolver)
            : base(persistentObjectId)
        {
            if (persistentObjectId == null) throw new ArgumentNullException("persistentObjectId");
            if (wurmApi == null) throw new ArgumentNullException("wurmApi");
            if (hostEnvironment == null) throw new ArgumentNullException("hostEnvironment");
            if (soundEngine == null) throw new ArgumentNullException("soundEngine");
            if (logger == null) throw new ArgumentNullException("logger");
            if (trayPopups == null) throw new ArgumentNullException("trayPopups");
            if (activeTriggersResolver == null) throw new ArgumentNullException("activeTriggersResolver");
            CharacterName = persistentObjectId;

            this.wurmApi = wurmApi;
            this.hostEnvironment = hostEnvironment;
            this.soundEngine = soundEngine;
            this.logger = logger;
            this.trayPopups = trayPopups;
            this.activeTriggersResolver = activeTriggersResolver;

            TriggerListState = new byte[0];

            hostEnvironment.HostClosing += Cleanup;
        }

        public void Initialize()
        {
            activeTriggers = activeTriggersResolver.Get(CharacterName);
            activeTriggers.MutedEvaluator = GetMutedEvaluator();

            //create control for Module UI
            controlUi = new UcPlayerTriggersController();

            //create this notifier UI
            triggersConfigUi = new FormTriggersConfig(this, soundEngine);

            UpdateMutedState();
            controlUi.label1.Text = CharacterName;
            controlUi.buttonMute.Click += ToggleMute;
            controlUi.buttonConfigure.Click += Configure;
            controlUi.buttonRemove.Click += StopAndRemove;

            wurmApi.LogsMonitor.Subscribe(this.CharacterName, LogType.AllLogs, OnNewLogEvents);
        }

        public TriggersFeature TriggersFeature { get; set; }

        private bool IsMuted()
        {
            return muted;
        }

        public Func<bool> GetMutedEvaluator()
        {
            return IsMuted;
        }

        public IEnumerable<ITrigger> Triggers
        {
            get { return activeTriggers.All; }
        }

        public bool Muted
        {
            get { return muted; }
            set { muted = value; }
        }

        public void RemoveTrigger(ITrigger trigger)
        {
            activeTriggers.RemoveTrigger(trigger);
        }

        public ITrigger CreateTrigger(TriggerKind kind)
        {
            return activeTriggers.CreateNewTrigger(kind);
        }

        public UcPlayerTriggersController GetUIHandle()
        {
            return controlUi;
        }

        private void ToggleMute(object sender, EventArgs e)
        {
            muted = !muted;
            FlagAsChanged();
            UpdateMutedState();
            triggersConfigUi.UpdateMutedState();
        }

        public void UpdateMutedState()
        {
            if (muted)
                controlUi.buttonMute.BackgroundImage = Resources.SoundDisabledSmall;
            else controlUi.buttonMute.BackgroundImage = Resources.SoundEnabledSmall;
        }

        public void Update()
        {
            var dtNow = DateTime.Now;
            foreach (var trigger in Triggers.ToArray())
            {
                trigger.FixedUpdate(dtNow);
            }
        }

        private void Configure(object sender, EventArgs e)
        {
            ToggleUi();
        }

        void Cleanup(object sender, EventArgs e)
        {
            wurmApi.LogsMonitor.Unsubscribe(this.CharacterName, OnNewLogEvents);
            hostEnvironment.HostClosing -= Cleanup;

            controlUi.Dispose();
            triggersConfigUi.Close();
        }

        public void StopAndRemove(object sender, EventArgs e)
        {
            wurmApi.LogsMonitor.Unsubscribe(this.CharacterName, OnNewLogEvents);
            TriggersFeature.RemoveManager(this);
            hostEnvironment.HostClosing -= Cleanup;
            activeTriggers.DisposeAll();

            controlUi.Dispose();
            triggersConfigUi.Close();
        }

        private void OnNewLogEvents(object sender, LogsMonitorEventArgs e)
        {
            HandleNewLogEvents(e.WurmLogEntries, e.LogType);
        }

        private void ToggleUi()
        {
            triggersConfigUi.ShowAndBringToFront();
        }

        private void HandleNewLogEvents(IEnumerable<LogEntry> newLogEvents, LogType logType)
        {
            lastLineContent = "";
            var dtNow = DateTime.Now;
            foreach (var logMessage in newLogEvents)
            {
                if (lastLineContent != logMessage.Content)
                {
                    foreach (var trigger in Triggers.ToArray())
                    {
                        if (trigger.CheckLogType(logType))
                        {
                            trigger.Update(logMessage, dtNow);
                        }
                    }
                    lastLineContent = logMessage.Content;
                }
            }
        }
    }
}
