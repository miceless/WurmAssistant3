﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AldursLab.Testing;
using AldurSoft.WurmApi.Modules.Events;
using AldurSoft.WurmApi.Tests.Builders;
using AldurSoft.WurmApi.Tests.Builders.WurmClient;
using Telerik.JustMock;

namespace AldurSoft.WurmApi.Tests
{
    class WurmApiFixtureV2
    {
        // note: 
        // using default ThreadPool marshallers for both internal and public events
        // while simple marshaller would speed tests, using thread pool can potentially uncover more bugs

        WurmApiManager wurmApiManager;
        readonly object locker = new object();

        public WurmApiFixtureV2()
        {
            var handle = TempDirectoriesFactory.CreateEmpty();
            WurmApiDataDir = new DirectoryInfo(handle.AbsolutePath);
            WurmClientMock = WurmClientMockBuilder.Create();
        }

        public DirectoryInfo WurmApiDataDir { get; private set; }

        public WurmClientMock WurmClientMock { get; private set; }

        public WurmApiManager WurmApiManager
        {
            get
            {
                lock (locker)
                {
                    if (wurmApiManager == null)
                    {
                        wurmApiManager = new WurmApiManager(WurmApiDataDir.FullName,
                            WurmClientMock.InstallDirectory,
                            Mock.Create<IHttpWebRequests>(),
                            Mock.Create<ILogger>().RedirectToTraceOut());
                    }
                    return wurmApiManager;
                }
            }
        }
    }
}
