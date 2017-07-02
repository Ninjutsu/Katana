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
    [XmlElement("LoadNeoParent")]
    public class LoadNeoParentTag : ProfileBehavior
    {
        private bool _done;

        public override bool IsDone { get { return _done; } }

        public Version Version { get { return new Version(1, 0, 0); } }

        protected override Composite CreateBehavior()
        {
            return new PrioritySelector(
                new Decorator(
                    ret =>
                        {
                            try
                            {
                                return LoadNeoProfileTag.ProfileStack.Peek() != null;
                            }
                            catch (InvalidOperationException)
                            {
                                Logging.Write("Stack empty, no parent profile to load.");
                                _done = true;
                                return false;
                            }
                            
                        },
                    new Action(r =>
                        {
                            var lastProfile = LoadNeoProfileTag.ProfileStack.Pop();
                            LoadNeoProfileTag.ProfilePaths.Remove(lastProfile.Path);

                            Logging.Write("Removed Profile from stack -> " + lastProfile.Name);
                            Logging.Write("Switching Neo Profile -> " + lastProfile.Name);
                            Load(lastProfile.Path);
                            _done = true;
                        })
                )
            );
        }

        protected override void OnResetCachedDone()
        {
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
                Logging.Write("[LoadNeoParent] Error: {0}", treerootexception);
            }
        }
    }
}