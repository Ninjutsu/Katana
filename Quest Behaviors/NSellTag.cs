using Buddy.Coroutines;

using Clio.Utilities;
using Clio.XmlEngine;

using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.RemoteWindows;

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using ff14bot.Navigation;
using ff14bot.Objects;

using TreeSharp;

namespace ff14bot.NeoProfiles
{
    [XmlElement("NSellItem")]
    [XmlElement("NSellItems")]
    public class NSellTag : ProfileBehavior
    {
        public override string StatusText => "Trading with " + _npcName;

        #region Attributes
        [XmlAttribute("SellIds")]
        [XmlAttribute("SellId")]
        private int[] SellIds { get; set; }

        [XmlAttribute("NpcId")]
        private int NpcId { get; set; }

        [XmlAttribute("XYZ")]
        public Vector3 XYZ { get { return Destination; } set { Destination = value; } }
        public Vector3 Destination = Vector3.Zero;

        [XmlAttribute("InteractDistance")]
        [DefaultValue(3.24f)]
        private float InteractDistance { get; set; }

        [XmlAttribute("DialogOption")]
        [DefaultValue(-1)]
        private int DialogOption { get; set; }
        #endregion

        public override bool IsDone => _done;
        private string _npcName;
        private static LocalPlayer Ninja => GameObjectManager.LocalPlayer;

        protected override void OnStart()
        {
            _npcName = DataManager.GetLocalizedNPCName(NpcId);

            if (SellIds == null)
            {
                throw new ArgumentException("[Ninjutsu] No SellIds entered.");
            }
        }

        protected override Composite CreateBehavior() { return new ActionRunCoroutine(ctx => Main()); }

        private async Task<bool> Main()
        {
            await CommonTasks.HandleLoading();
            await MoveTo();
            return await Interact() || await SellItems();
        }

        private async Task MoveTo()
        {
            if (Ninja.Location.Distance(Destination) > InteractDistance)
            {
                if (!Ninja.IsMounted && Ninja.Location.Distance(Destination) > Settings.CharacterSettings.Instance.MountDistance)
                    await CommonTasks.MountUp();

                Navigator.MoveTo(Destination);
            }

            if (Ninja.Location.Distance(Destination) < InteractDistance)
            {
                if (MovementManager.IsMoving)
                {
                    Navigator.PlayerMover.MoveStop();
                    if (MovementManager.IsFlying)
                    {
                        await CommonTasks.CanLand();
                        await CommonTasks.Land();
                    }
                }
                Navigator.Clear();
                await Coroutine.Sleep(50);
                return;
            }
            else
                await Coroutine.Yield();
        }

        private bool WithinReach => Ninja.Location.Distance(Destination) <= InteractDistance;

        private async Task<bool> Interact()
        {
            if (WindowsOpen())
                return false;

            if (!WithinReach) return false;
            if (Core.Player.IsMounted)
                await CommonTasks.StopAndDismount();

            if (!GameObjectManager.GetObjectByNPCId((uint) NpcId).IsVisible) return false;
            GameObjectManager.GetObjectByNPCId((uint)NpcId).Interact();
            await Coroutine.Sleep(750);
            return false;
        }

        private static bool WindowsOpen()
        {
            while (CraftingLog.IsOpen || HousingGardening.IsOpen || JournalAccept.IsOpen || JournalResult.IsOpen || JournalResult.ButtonClickable || MaterializeDialog.IsOpen || Repair.IsOpen || Request.IsOpen || SelectIconString.IsOpen
                || SelectString.IsOpen || SelectYesno.IsOpen || Synthesis.IsOpen || Talk.DialogOpen || NowLoading.IsVisible || Talk.ConvoLock || QuestLogManager.InCutscene)

                return true;

            return false;
        }

        private async Task<bool> SellItems()
        {
            var itemList = InventoryManager.FilledSlots;
            var items = itemList.Where(i => Enumerable.Contains(SellIds, (int)i.RawItemId));
            while (Ninja.HasTarget && WindowsOpen())
            {
                if (SelectIconString.IsOpen)
                {
                    //Log("[Ninjutsu] DialogOption was " + DialogOption);
                    if (DialogOption == -1)
                    {
                        TreeRoot.Stop("[Ninjutsu] No DialogOption supplied, but found dialog window.");
                        throw new ArgumentException("[Ninjutsu] No DialogOption supplied, but found dialog window.");
                    }
                    SelectIconString.ClickSlot((uint)DialogOption);
                    await Coroutine.Wait(1000, () => Shop.Open);
                }
                var bagSlots = items as BagSlot[] ?? items.ToArray();
                if (Shop.Open)
                {
                    foreach (var item in bagSlots)
                    {
                        await CommonTasks.SellItem(item);
                        await Coroutine.Sleep(500);
                    }
                    await Coroutine.Sleep(500);
                    Shop.Close();
                    await Coroutine.Sleep(500);
                }
                _done = true;
                await Coroutine.Yield();
            }
            return false;
        }
        private bool _done;
    }
}
