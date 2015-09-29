﻿using System;
using NLog;

namespace AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts
{
    /// <summary>
    /// Handles new log events. All logging appliances should forward log messages to this interface.
    /// </summary>
    public interface ILogMessageHandler
    {
        void HandleEvent(LogLevel nlogLevel, string message, Exception exception, string category);
    }
}