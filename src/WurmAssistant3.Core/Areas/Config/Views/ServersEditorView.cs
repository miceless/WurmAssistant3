﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.Areas.Config.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Config.Modules;
using JetBrains.Annotations;

namespace AldursLab.WurmAssistant3.Core.Areas.Config.Views
{
    public partial class ServersEditorView : Form
    {
        readonly ServerInfoManager serverInfoManager;

        BindingList<WurmServerInfo> currentMappings;
        BindingList<ServerInformation> customMappings;

        public ServersEditorView([NotNull] ServerInfoManager serverInfoManager, IWurmApi wurmApi)
        {
            if (serverInfoManager == null) throw new ArgumentNullException("serverInfoManager");
            this.serverInfoManager = serverInfoManager;
            InitializeComponent();

            currentMappings = new BindingList<WurmServerInfo>(wurmApi.ServersList.All.ToArray());
            defaultsGrid.DataSource = currentMappings;

            customMappings = new BindingList<ServerInformation>(
                serverInfoManager.GetAllMappings().ToList());
            customGrid.DataSource = customMappings;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                serverInfoManager.UpdateAllMappings(customMappings);
                MessageBox.Show(
                    "Restart Wurm Assistant for these changes to take effect. " + Environment.NewLine +
                    "Remember that you might need to update some features, eg. switch timers and granger herds to the new server group.");
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
