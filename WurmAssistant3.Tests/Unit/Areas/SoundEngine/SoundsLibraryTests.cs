﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AldursLab.Testing;
using AldursLab.WurmAssistant3.Core.Areas.SoundEngine.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.SoundEngine.Modules;
using NUnit.Framework;
using Telerik.JustMock.AutoMock;

namespace AldursLab.WurmAssistant3.Tests.Unit.Areas.SoundEngine
{
    class SoundsLibraryTests : UnitTest<SoundsLibrary>
    {
        DirectoryHandle sourceSoundsDir;
        DirectoryHandle soundBankDir;

        [SetUp]
        public void Setup()
        {
            sourceSoundsDir = TempDirectoriesFactory.CreateByUnzippingFile(Path.Combine("Resources", "sounds_wav.7z"));
            soundBankDir = TempDirectoriesFactory.CreateEmpty();
            Kernel.Bind<string>().ToConstant(soundBankDir.FullName).InjectedIntoParameter("soundFilesPath");
        }

        [Test]
        public void AddSound()
        {
            bool eventFired = false;
            Service.SoundsChanged += (sender, args) => eventFired = true;

            var sourcePath = Path.Combine(sourceSoundsDir.FullName, "bvvt.wav");
            var sourceExtension = Path.GetExtension(sourcePath);
            Service.Import(sourcePath);

            var sound = Service.GetAllSounds().Single();
            Expect(sound.Name, EqualTo("bvvt"));
            Expect(sound.FileFullName.Contains(sound.Id.ToString() + sourceExtension));
            Expect(File.Exists(Path.Combine(soundBankDir.FullName, sound.FileFullName)));
            Expect(eventFired, True);
        }

        [Test]
        public void ModifySound()
        {
            bool eventFired = false;
            var sound = SetupSingleSound();
            Service.SoundsChanged += (sender, args) => eventFired = true;
            Service.Rename(sound, "test");
            Service.AdjustVolume(sound, 0.9f);

            var verifySound = Service.TryGetSound(sound.Id);
            Expect(verifySound, !Null);
            Expect(verifySound.Name, EqualTo("test"));
            Expect(verifySound.AdjustedVolume, EqualTo(0.9f));
            Expect(eventFired, True);
        }

        [Test]
        public void RemoveSound()
        {
            bool eventFired = false;
            var sound = SetupSingleSound();
            Service.SoundsChanged += (sender, args) => eventFired = true;
            Service.Remove(sound);

            Expect(Service.TryGetSound(sound.Id), Null);
            Expect(Service.GetAllSounds().FirstOrDefault(resource => resource.Id == sound.Id), Null);
            Expect(eventFired, True);
            Expect(Directory.GetFiles(soundBankDir.FullName).Length, EqualTo(0));
        }

        ISoundResource SetupSingleSound()
        {
            var sourcePath = Path.Combine(sourceSoundsDir.FullName, "bvvt.wav");
            var sourceExtension = Path.GetExtension(sourcePath);
            Service.Import(sourcePath);
            return Service.GetAllSounds().Single(resource => resource.Name == "bvvt");
        }
    }
}
