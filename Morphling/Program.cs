using System;
using System.Linq;
using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common;
using Ensage.Common.Menu;
using System.Windows.Input;
using SharpDX.Direct3D9;


namespace Morphling
{
    class Program
    {
        private static bool _activated;
        private static Ability waveform, adapt;
        private static Item etheral;
        private static Font _text;
        private const Key KeyCombo = Key.E;
        private const Key BkbToggleKey = Key.F;
        private static bool _bkbToggle;
        private static Hero _me;
        private static Hero _target;
        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            Console.WriteLine("==========>[Morphling#] loaded<=========");
            _text = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Segoe UI",
                   Height = 17,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.ClearType
               });
        }
        public static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame) return;
            _me = ObjectMgr.LocalHero;
            if (_me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Morphling)
            {
                return;
            }
            if (!_activated) return;
            if ((_target == null || !_target.IsVisible) && !_me.IsChanneling())
                _me.Move(Game.MousePosition);
            if (_target == null || _me.Distance2D(_target) > 500)
                _target = _me.ClosestToMouseTarget(1000);
            //Skills & Items
            waveform = _me.Spellbook.Spell1;
            adapt = _me.Spellbook.Spell2;
            etheral = _me.FindItem("item_etheral_blade");
            if (_target != null && _target.IsAlive && !_target.IsInvul())
            {
                if (!Utils.SleepCheck("Morph_Wait")) return;
                var targetDistance = _me.Distance2D(_target);
                var attackRange = 1000;
                if (targetDistance <= attackRange && waveform != null && waveform.CanBeCasted() && Utils.SleepCheck("waveform"))
                {
                    waveform.UseAbility(_target.Position);
                    Utils.Sleep(150 + Game.Ping, "waveform");
                }
                else if (etheral != null && etheral.CanBeCasted() && Utils.SleepCheck("ethereal"))
                {
                    etheral.UseAbility(_target);
                    Utils.Sleep(270 + Game.Ping, "ethereal");
                }
                else if (adapt != null && adapt.CanBeCasted() && Utils.SleepCheck("adapt"))
                {
                    adapt.UseAbility(_target);
                    Utils.Sleep(150 + Game.Ping, "adapt");
                }
            }
        }
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (Game.IsChatOpen) return;
            _activated = Game.IsKeyDown(KeyCombo);
            if (!Game.IsKeyDown(BkbToggleKey) || !Utils.SleepCheck("toggleBKB")) return;
            _bkbToggle = !_bkbToggle;
            Utils.Sleep(250, "toggleBKB");
        }
        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame) return;
            if (_me == null || _me.Player.Team == Team.Observer || _me.ClassID != ClassID.CDOTA_Unit_Hero_Morphling) return;
            if (_activated)
            {
                _text.DrawText(null, "[Morphling#] is COMBOING now!\n", 5, 150, Color.Green);
            }
            if (!_activated && !_bkbToggle)
            {
                _text.DrawText(null, "[Morphling#]: Use  [" + KeyCombo + "] for combo. BKB Disabled. Use " + BkbToggleKey + " to turn it on!", 5, 150, Color.White);
            }
            if (!_activated && _bkbToggle)
            {
                _text.DrawText(null, "[Morphling#]: Use  [" + KeyCombo + "] for combo. BKB Enabled. Use " + BkbToggleKey + " to turn it off!", 5, 150, Color.Yellow);
            }
        }
        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            _text.Dispose();
        }
        static void Drawing_OnPostReset(EventArgs args)
        {
            _text.OnResetDevice();
        }
        static void Drawing_OnPreReset(EventArgs args)
        {

            _text.OnLostDevice();
        }

    }
}
