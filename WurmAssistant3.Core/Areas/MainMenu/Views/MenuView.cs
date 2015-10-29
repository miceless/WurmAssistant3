﻿using System;
using System.Windows.Forms;
using AldursLab.WurmAssistant3.Core.Areas.Config.Contracts;
using AldursLab.WurmAssistant3.Core.Root.Contracts;
using JetBrains.Annotations;

namespace AldursLab.WurmAssistant3.Core.Areas.MainMenu.Views
{
    public partial class MenuView : UserControl
    {
        readonly ISettingsEditViewFactory settingsEditViewFactory;
        readonly IProcessStarter processStarter;
        readonly IUserNotifier userNotifier;
        readonly IServersEditorViewFactory serversEditorViewFactory;

        public MenuView([NotNull] ISettingsEditViewFactory settingsEditViewFactory,
            [NotNull] IProcessStarter processStarter, [NotNull] IUserNotifier userNotifier,
            [NotNull] IServersEditorViewFactory serversEditorViewFactory)
        {
            if (settingsEditViewFactory == null) throw new ArgumentNullException("settingsEditViewFactory");
            if (processStarter == null) throw new ArgumentNullException("processStarter");
            if (userNotifier == null) throw new ArgumentNullException("userNotifier");
            if (serversEditorViewFactory == null) throw new ArgumentNullException("serversEditorViewFactory");
            this.settingsEditViewFactory = settingsEditViewFactory;
            this.processStarter = processStarter;
            this.userNotifier = userNotifier;
            this.serversEditorViewFactory = serversEditorViewFactory;
            InitializeComponent();
        }

        private void changeSettingsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var view = settingsEditViewFactory.CreateSettingsEditView();
            view.ShowDialog();
        }

        private void officialForumToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            processStarter.StartSafe(
                "http://forum.wurmonline.com/index.php?/topic/68031-wurm-assistant-enrich-your-wurm-experience/?view=getnewpost");
        }

        private void wikiToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            processStarter.StartSafe(
                "http://wurmassistant.wikia.com/wiki/Wurm_Assistant_Wiki");
        }

        private void pMAldurToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            processStarter.StartSafe(
                "http://forum.wurmonline.com/index.php?/user/6302-aldur/");
        }

        private void contributorsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            processStarter.StartSafe(
                "http://blog.aldurcraft.com/WurmAssistant/page/Contributors-and-Supporters");
        }

        private void donatorsToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            processStarter.StartSafe(
                "http://blog.aldurcraft.com/WurmAssistant/page/Contributors-and-Supporters");
        }

        private void viewRoadmapToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
             processStarter.StartSafe(
                "https://trello.com/b/FlIPQ7TW/wurm-assistant-3-roadmap");
        }

        void modifyServersListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var view = serversEditorViewFactory.CreateServersEditorView();
            view.ShowDialog();
        }
    }
}
