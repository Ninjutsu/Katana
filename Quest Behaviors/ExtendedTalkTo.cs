using Clio.Utilities;
using Clio.XmlEngine;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.RemoteWindows;
using System.ComponentModel;
using TreeSharp;

namespace ff14bot.NeoProfiles.Tags {
    [XmlElement("ExtendedTalkTo")]
    class ExtendedTalkToTag : TalkToTag {
        [XmlAttribute("YesnoPopup")]
        [DefaultValue("No")]
        public string YesnoPopup { get; set; }

        [XmlAttribute("While")]
        public string IfCondition { get; set; }

        protected override Composite CreateBehavior() {
            return new PrioritySelector(
                CommonBehaviors.HandleLoading,
                new Decorator(ret => QuestLogManager.InCutscene, new ActionAlwaysSucceed()),
                new Decorator(ctx => SelectYesno.IsOpen, new Action(ctx => {
                    if (YesnoPopup.ToLower() == "yes") SelectYesno.ClickYes();
                    else SelectYesno.ClickNo();
                })),
                base.CreateBehavior()
            );
        }

        public override bool IsDone {
            get {
                if (!ScriptManager.GetCondition(IfCondition)()) {
                    return true;
                }
                return base.IsDone;
            }
        }
    }
}
