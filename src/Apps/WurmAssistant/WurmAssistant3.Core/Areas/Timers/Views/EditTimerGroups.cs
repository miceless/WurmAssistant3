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
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Timers.Modules;
using AldursLab.WurmAssistant3.Core.WinForms;
using JetBrains.Annotations;

namespace AldursLab.WurmAssistant3.Core.Areas.Timers.Views
{
    public partial class EditTimerGroups : ExtendedForm
    {
        readonly TimersFeature timersFeature;
        readonly IWurmApi wurmApi;
        private readonly ILogger logger;

        public EditTimerGroups([NotNull] TimersFeature timersFeature, [NotNull] IWurmApi wurmApi, ILogger logger)
        {
            if (timersFeature == null) throw new ArgumentNullException("timersFeature");
            if (wurmApi == null) throw new ArgumentNullException("wurmApi");
            if (logger == null) throw new ArgumentNullException("logger");
            this.timersFeature = timersFeature;
            this.wurmApi = wurmApi;
            this.logger = logger;
            InitializeComponent();
            
            RebuildGroups();
            playerCb.Items.AddRange(wurmApi.Characters.All.Select(character => character.Name).Cast<object>().ToArray());
            serverGroupCb.Items.AddRange(
                wurmApi.ServerGroups.AllKnown.Select(@group => @group.ServerGroupId).Cast<object>().ToArray());
        }

        void removeBtn_Click(object sender, EventArgs e)
        {
            var selectedGroup = TryGetSelected();
            if (selectedGroup != null)
            {
                if (
                    MessageBox.Show(
                        "Selected group will be removed, along with any timers and their settings. Confirm?",
                        "Confirm removal",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    timersFeature.RemovePlayerGroup(selectedGroup.Id);
                    RebuildGroups();
                }
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(playerCb.Text) || string.IsNullOrWhiteSpace(serverGroupCb.Text))
            {
                MessageBox.Show("Please choose character name and server group.");
            }
            else
            {
                try
                {
                    timersFeature.CreateGroup(Guid.NewGuid(), playerCb.Text, serverGroupCb.Text);
                }
                catch (Exception exception)
                {
                    var message = string.Format("Error at create timer group for player {0}, server group {1}",
                        playerCb.Text,
                        serverGroupCb.Text);
                    logger.Error(exception, message);
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                RebuildGroups();
            }
        }

        void RebuildGroups()
        {
            var selectedItem = TryGetSelected();
            currentGroupsLb.Items.Clear();
            currentGroupsLb.Items.AddRange(
                timersFeature.GetActivePlayerGroups().OrderBy(@group => @group.SortingOrder).Cast<object>().ToArray());
            if (selectedItem != null)
            {
                currentGroupsLb.SelectedItem = selectedItem;
            }
        }

        private void moveUpBtn_Click(object sender, EventArgs e)
        {
            var selectedGroup = TryGetSelected();
            if (selectedGroup != null)
            {
                timersFeature.IncreaseSortingOrder(selectedGroup);
                RebuildGroups();
            }
        }

        private void moveDownBtn_Click(object sender, EventArgs e)
        {
            var selectedGroup = TryGetSelected();
            if (selectedGroup != null)
            {
                timersFeature.ReduceSortingOrder(selectedGroup);
                RebuildGroups();
            }
        }

        PlayerTimersGroup TryGetSelected()
        {
            return currentGroupsLb.SelectedItem as PlayerTimersGroup;
        }

        private void hideGroupBtn_Click(object sender, EventArgs e)
        {            
            var selectedGroup = TryGetSelected();
            if (selectedGroup != null)
            {
                selectedGroup.Hidden = !selectedGroup.Hidden;
                RebuildGroups();
            }
        }
    }
}
