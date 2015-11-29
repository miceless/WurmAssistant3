﻿using System;
using AldursLab.WurmAssistant3.Core.Areas.CombatStats.Modules;

namespace AldursLab.WurmAssistant3.Core.Areas.CombatStats.Contracts
{
    public interface ICombatDataSource
    {
        CombatResults CombatResults { get; }

        event EventHandler<EventArgs> DataChanged;
    }
}