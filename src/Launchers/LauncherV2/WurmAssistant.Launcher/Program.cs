﻿using System;
using System.IO;
using System.Windows.Forms;
using AldursLab.WurmAssistant.Launcher.Root;

namespace AldursLab.WurmAssistant.Launcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(args));
        }
    }
}
