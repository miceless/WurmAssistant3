using System;
using AldursLab.WurmAssistant3.Core.Areas.SoundManager.Contracts;

namespace AldursLab.WurmAssistant3.Core.Areas.SoundManager.Modules
{
    public class SoundResourceNullObject : ISoundResource
    {
        public Guid Id { get { return Guid.Empty; } }
        public string Name { get { return "none"; } }
        public float AdjustedVolume { get { return 0; } }
        public string FileFullName { get { return string.Empty; } }
        public bool IsNull { get { return true; }}
    }
}