﻿using System;
using AldursLab.WurmAssistant3.Core.Areas.SoundManager.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Modules.TriggersManager;
using AldursLab.WurmAssistant3.Core.Areas.Triggers.Views.Notifiers;
using JetBrains.Annotations;

namespace AldursLab.WurmAssistant3.Core.Areas.Triggers.Modules.Notifiers
{
    public class SoundNotifier : NotifierBase, ISoundNotifier
    {
        readonly ITrigger trigger;
        readonly ISoundManager soundManager;

        public Guid SoundId
        {
            get { return trigger.SoundId; }
            set
            {
                trigger.SoundId = value;
                SoundResource = soundManager.GetSoundById(value);
            }
        }

        ISoundResource SoundResource { get; set; }

        public override bool HasEmptySound
        {
            get
            {
                if (SoundResource == null) return true;
                return SoundResource.Id == Guid.Empty;
            }
        }

        public SoundNotifier([NotNull] ITrigger trigger, [NotNull] ISoundManager soundManager)
        {
            if (trigger == null) throw new ArgumentNullException("trigger");
            if (soundManager == null) throw new ArgumentNullException("soundManager");
            this.trigger = trigger;
            this.soundManager = soundManager;
            // restore previous sound, if still in sounds library
            this.SoundResource = soundManager.GetSoundById(trigger.SoundId);
        }

        public override INotifierConfig GetConfig()
        {
            return new SoundConfig(this, soundManager);
        }

        public override void Notify()
        {
            var handle = soundManager.PlayOneShot(SoundResource);
            if (handle.IsNullSound) this.SoundId = Guid.Empty;
        }
    }
}
