using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Clio.Utilities;
using Clio.XmlEngine;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Settings;
using System.Windows.Media;
using TreeSharp;
using Action = TreeSharp.Action;

namespace ff14bot.NeoProfiles
{
    [XmlElement("LoadNeoProfile")]
    public class LoadNeoProfileTag : ProfileBehavior
    {
        private bool _done;

        [XmlAttribute("ProfileName")]
        public string ProfileName { get; set; }

        [XmlAttribute("Path")]
        public string Path { get; set; }

        public override bool IsDone { get { return _done; } }

        public Version Version { get { return new Version(1, 0, 0); } }

        public static readonly HashSet<string> ProfilePaths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        public static readonly Stack<CharacterSettings.RecentProfile> ProfileStack = new Stack<CharacterSettings.RecentProfile>();

        protected override void OnStart()
        {
            TreeRoot.OnStop += bot =>
                {
                    ProfilePaths.Clear();
                    ProfileStack.Clear();
                };
        }

        protected override Composite CreateBehavior()
        {
            return new PrioritySelector(
                new Decorator(ret => true,
                    new Action(r =>
                        {
                            string profilePath = ff14bot.Helpers.Utils.AssemblyDirectory + @"\Profiles\" + this.Path;
                            if (!ProfilePaths.Contains(profilePath))
                            {
                                var currentProfile = CharacterSettings.Instance.RecentNeoProfiles.First();
                                ProfileStack.Push(currentProfile);
                                ProfilePaths.Add(currentProfile.Path);
                                Logging.Write("Added Profile to stack -> " + currentProfile.Name);
                                Logging.Write("Switching Neo Profile -> " + this.ProfileName);
                                Load(profilePath);
                            }
                            else
                            {
                                LogError("Circular Dependency -> {0} has already been loaded as an ancestor profile.", this.Path);
                            }

                            _done = true;
                        })
                )
            );
        }

        protected override void OnResetCachedDone()
        {
            ProfileStack.Clear();
            ProfilePaths.Clear();
            _done = false;
        }

        protected override void OnDone()
        {
            _done = false;
        }

        private static void Load(string profilePath)
        {
            try
            {
                Navigator.Stop();
                NeoProfileManager.Load(profilePath, true);
            }
            catch (Exception treerootexception)
            {
                Logging.Write("[LoadNeoProfile] Error: {0}", treerootexception);
            }
        }
    }
}