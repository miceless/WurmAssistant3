﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.Areas.CombatAssistant.Views;
using AldursLab.WurmAssistant3.Core.Areas.Features.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Properties;
using AldursLab.WurmAssistant3.Core.Root.Contracts;
using WurmAssistantDataTransfer.Dtos;

namespace AldursLab.WurmAssistant3.Core.Areas.CombatAssistant.Modules
{
    public class CombatAssistantFeature : IFeature
    {
        readonly IWurmApi wurmApi;
        readonly ILogger logger;
        readonly FeatureSettings featureSettings;
        readonly IHostEnvironment hostEnvironment;
        readonly IProcessStarter processStarter;

        readonly CombatAssistantFeatureView view;

        public CombatAssistantFeature(IWurmApi wurmApi, ILogger logger, FeatureSettings featureSettings,
            IHostEnvironment hostEnvironment, IProcessStarter processStarter)
        {
            if (wurmApi == null) throw new ArgumentNullException("wurmApi");
            if (logger == null) throw new ArgumentNullException("logger");
            if (featureSettings == null) throw new ArgumentNullException("featureSettings");
            if (hostEnvironment == null) throw new ArgumentNullException("hostEnvironment");
            if (processStarter == null) throw new ArgumentNullException("processStarter");
            this.wurmApi = wurmApi;
            this.logger = logger;
            this.featureSettings = featureSettings;
            this.hostEnvironment = hostEnvironment;
            this.processStarter = processStarter;
            view = new CombatAssistantFeatureView(wurmApi, logger, featureSettings, hostEnvironment, processStarter);
        }

        #region IFeature

        void IFeature.Show()
        {
            view.ShowAndBringToFront();
        }

        void IFeature.Hide()
        {
        }

        string IFeature.Name
        {
            get { return "Combat Assistant"; }
        }

        Image IFeature.Icon
        {
            get { return Resources.Combat; }
        }

        async Task IFeature.InitAsync()
        {
            await Task.FromResult(true);
        }

        void IFeature.PopulateDto(WurmAssistantDto dto)
        {
        }

        async Task IFeature.ImportDataFromWa2Async(WurmAssistantDto dto)
        {
            await Task.FromResult(true);
        }

        int IFeature.DataImportOrder
        {
            get { return 0; }
        }

        #endregion
    }
}
