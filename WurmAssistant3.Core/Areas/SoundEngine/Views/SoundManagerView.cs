﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AldursLab.Essentials.Extensions.DotNet;
using AldursLab.WurmAssistant3.Core.Areas.SoundEngine.Contracts;
using AldursLab.WurmAssistant3.Core.Properties;
using AldursLab.WurmAssistant3.Core.Root.Contracts;
using AldursLab.WurmAssistant3.Core.WinForms;
using JetBrains.Annotations;

namespace AldursLab.WurmAssistant3.Core.Areas.SoundEngine.Views
{
    public partial class SoundManagerView : ExtendedForm
    {
        readonly ISoundEngine soundEngine;
        readonly ISoundsLibrary soundsLibrary;
        readonly IProcessStarter processStarter;

        public SoundManagerView([NotNull] ISoundEngine soundEngine, [NotNull] ISoundsLibrary soundsLibrary, IProcessStarter processStarter)
        {
            if (soundEngine == null) throw new ArgumentNullException("soundEngine");
            if (soundsLibrary == null) throw new ArgumentNullException("soundsLibrary");
            if (processStarter == null) throw new ArgumentNullException("processStarter");
            this.soundEngine = soundEngine;
            this.soundsLibrary = soundsLibrary;
            this.processStarter = processStarter;

            InitializeComponent();

            trackBarAdjustedVolume.Enabled = false;
            RefreshSoundsList();
            globalVolumeTrackBar.Value = ((int) (soundEngine.GlobalVolume*100f)).ConstrainToRange(0, 100);

            soundsLibrary.SoundsChanged += SoundsLibraryOnSoundsChanged;
            this.Closed += (sender, args) => soundsLibrary.SoundsChanged -= SoundsLibraryOnSoundsChanged;
        }

        void SoundsLibraryOnSoundsChanged(object sender, EventArgs eventArgs)
        {
            RefreshSoundsList();
        }

        public ISoundResource SelectedSound
        {
            get { return (ISoundResource) listBoxAllSounds.SelectedItem; }
        }

        public IEnumerable<ISoundResource> SelectedSounds
        {
            get { return listBoxAllSounds.SelectedItems.Cast<ISoundResource>(); }
        } 

        private void RefreshSoundsList()
        {
            var selectedItem = SelectedSound;
            listBoxAllSounds.ClearSelected();
            listBoxAllSounds.Items.Clear();
            listBoxAllSounds.Items.AddRange(soundsLibrary.GetAllSounds().Cast<object>().ToArray());
            if (selectedItem != null)
            {
                listBoxAllSounds.SelectedItem = selectedItem;
            }
        }

        private void listBoxAllSounds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSound != null)
            {
                trackBarAdjustedVolume.Enabled = true;
                textBoxSelectedSound.Text = SelectedSound.Name;
                trackBarAdjustedVolume.Value = (int)(SelectedSound.AdjustedVolume * 100f);
                buttonRename.Enabled = buttonRename.Visible = true;
            }
            else
            {
                textBoxSelectedSound.Text = "";
                trackBarAdjustedVolume.Value = 100;
                trackBarAdjustedVolume.Enabled = false;
                buttonRename.Enabled = buttonRename.Visible = false;
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            soundEngine.StopAllSounds();
            if (SelectedSound != null)
            {
                soundEngine.PlayOneShot(SelectedSound);
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var sounds = SelectedSounds.ToArray();
            if (sounds.Any())
            {
                if (
                    MessageBox.Show(
                        "Are you sure to delete selected sounds? Any notification, that might be using these sounds, will stop working.",
                        "Confirm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    foreach (var selectedSound in sounds)
                    {
                        soundsLibrary.Remove(selectedSound);
                    }
                }
            }
        }

        private void buttonAddSounds_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var filename in openFileDialog1.FileNames)
                {
                    soundsLibrary.Import(filename);
                }
            }
        }

        private void listBoxAllSounds_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && SelectedSound != null)
            {
                soundsLibrary.Remove(SelectedSound);
            }
        }

        private void trackBarAdjustedVolume_ValueChanged(object sender, EventArgs e)
        {
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            if (SelectedSound != null)
            {
                SoundRenameView renameViewDialog = new SoundRenameView(SelectedSound.Name);
                if (renameViewDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                        soundsLibrary.Rename(SelectedSound, renameViewDialog.NewName);
                }
            }
        }

        private void trackBarAdjustedVolume_MouseUp(object sender, MouseEventArgs e)
        {
            if (SelectedSound != null)
            {
                soundsLibrary.AdjustVolume(SelectedSound, ((float)trackBarAdjustedVolume.Value) / 100);
            }
        }

        private void trackBarAdjustedVolume_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Right) e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Up) e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Down) e.SuppressKeyPress = true;
        }

        private void listBoxAllSounds_DoubleClick(object sender, EventArgs e)
        {
            buttonPlay.PerformClick();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            soundEngine.StopAllSounds();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void FormSoundBank_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBarAdjustedVolume,
                "Fine tune volume specifically for this sound,\r\n" +
                "this volume is still affected by global volume.");
        }

        private void globalVolumeTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            soundEngine.GlobalVolume = ((float) globalVolumeTrackBar.Value)/100;
        }

        private void globalVolumeTrackBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
                e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Right)
                e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Up)
                e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Down)
                e.SuppressKeyPress = true;
        }

        private void globalVolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            soundEngine.GlobalVolume = ((float)globalVolumeTrackBar.Value) / 100;
        }

        private void SoundManagerView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void sampleSoundPackLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            processStarter.StartSafe(Resources.SampleSoundPackUrl);
        }
    }
}
