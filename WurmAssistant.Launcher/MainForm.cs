﻿using System;
using System.IO;
using System.Windows.Forms;
using AldursLab.WurmAssistant.Launcher.Core;

namespace AldursLab.WurmAssistant.Launcher
{
    public partial class MainForm : Form, IGuiHost
    {
        public MainForm()
        {
            InitializeComponent();
            ShowInTaskbar = true;
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            HideHostWindow();

            var localSettings = new LocalLauncherSettings();
            this.Text = localSettings.GetSetting("AppName") + " Launcher";

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var rootDir = Path.Combine(localAppData, "AldursLab", localSettings.GetSetting("AldursLabDirName"));

            var config = new ControllerConfig()
            {
                RootDirFullPath = rootDir,
                WebServiceRootUrl = localSettings.GetSetting("WebServiceRootUrl"),
                WurmAssistantExeFileName = localSettings.GetSetting("WurmAssistantExeFileName")
            };

            var controller = new LaunchController(this, config);
            controller.Execute();
        }

        public void ShowHostWindow()
        {
            var showing = !Visible;
            Visible = true;
            ShowInTaskbar = true;
            if (showing)
            {
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
        }

        public void HideHostWindow()
        {
            Visible = false;
            ShowInTaskbar = false;
        }

        public void SetContent(UserControl userControl)
        {
            panel.Controls.Clear();
            userControl.Dock = DockStyle.Fill;
            panel.Controls.Add(userControl);
        }
    }
}