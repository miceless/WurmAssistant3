﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AldursLab.WurmApi;
using AldursLab.WurmAssistant3.Core.WinForms;

namespace AldursLab.WurmAssistant3.Core.Areas.Granger.Legacy
{
    public partial class FormChoosePlayers : ExtendedForm
    {
        public string[] Result = new string[0];

        public FormChoosePlayers(string[] currentPlayers, IWurmApi wurmApi)
        {
            InitializeComponent();
            string[] allPlayers = wurmApi.Characters.All.Select(character => character.Name.Capitalized).ToArray();
            foreach (var player in allPlayers)
            {
                checkedListBoxPlayers.Items.Add(player, currentPlayers.Contains(player));
            }
            BuildResult();
        }

        void BuildResult()
        {
            List<string> items = new List<string>();
            foreach (var item in checkedListBoxPlayers.CheckedItems)
            {
                items.Add((string)item);
            }
            Result = items.ToArray();
        }

        private void checkedListBoxPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            BuildResult();
        }
    }
}
