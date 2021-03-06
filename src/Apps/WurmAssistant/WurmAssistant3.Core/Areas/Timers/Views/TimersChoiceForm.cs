﻿using System;
using System.Collections.Generic;
using AldursLab.WurmAssistant3.Core.Areas.Timers.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Timers.Modules;
using AldursLab.WurmAssistant3.Core.WinForms;

namespace AldursLab.WurmAssistant3.Core.Areas.Timers.Views
{
    public partial class TimersChoiceForm : ExtendedForm
    {
        public TimersChoiceForm()
        {
            InitializeComponent();
        }

        TimersForm parentForm;

        public TimersChoiceForm(IEnumerable<TimerDefinition> availableTypes, TimersForm parent)
            : this()
        {
            parentForm = parent;
            foreach (var type in availableTypes)
            {
                checkedListBox1.Items.Add(type);
            }
        }

        public HashSet<TimerDefinition> Result = new HashSet<TimerDefinition>();

        private void button1_Click(object sender, EventArgs e)
        {
            Result = new HashSet<TimerDefinition>();
            foreach (var item in checkedListBox1.CheckedItems)
            {
                Result.Add((TimerDefinition)item);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
