using TreeSharp;
using Action = TreeSharp.Action;

using Clio.XmlEngine;

namespace ff14bot.NeoProfiles
{
    [XmlElement("Seppuku")]

    public class SeppukuTag : ProfileBehavior
    {
        private bool _done;

        public override bool IsDone => _done;

        protected override Composite CreateBehavior()
        {
            return new PrioritySelector(
                new Decorator(ret => TreeRoot.IsRunning,
                    new Action(r =>
                    {
                        TreeRoot.Stop();
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

        }
    }
}
