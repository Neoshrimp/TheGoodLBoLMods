using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Intentions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VariantsC.Marisa.C
{
    [OverwriteVanilla]
    public sealed class AntiStallOrbDef : EnemyUnitTemplate
    {
        public override IdContainer GetId() => nameof(YinyangyuBlue);

        [DontOverwrite]
        public override LocalizationOption LoadLocalization()
        {
            throw new NotImplementedException();
        }

        [DontOverwrite]
        public override EnemyUnitConfig MakeConfig()
        {
            throw new NotImplementedException();
        }
    }

    [EntityLogic(typeof(AntiStallOrbDef))]
    public sealed class YinyangyuBlue : YinyangyuBlueOrigin
    {
        enum ExplodeSequence
        {
            Dont,
            Charge,
            Explode
        }

        ExplodeSequence explodeSequence = ExplodeSequence.Dont;

        public override void OnEnterBattle(BattleController battle)
        {
            HandleBattleEvent(this.DamageReceived, (args) =>
            {
                var explodeInt = this.Intentions.FirstOrDefault(i => i is ExplodeIntention) as ExplodeIntention;
                if (explodeInt != null)
                { 
                    explodeInt.Damage = DamageInfo.Attack(this.Shield, false);
                    NotifyIntentionsChanged();
                }
            });
            base.OnEnterBattle(battle);
        }

        public override void UpdateMoveCounters()
        {
            if (explodeSequence == ExplodeSequence.Charge)
            {
                explodeSequence = ExplodeSequence.Explode;
                return;
            }

            if (GameRun.Player.HasExhibit<PachyBagEx>() 
                && TryGetStatusEffect<Spirit>(out var spirit) && spirit.Level >= 16 // 18 after buff 
                && Battle.AllAliveEnemies.All(e => e is YinyangyuBlueOrigin))
            {
                explodeSequence = ExplodeSequence.Charge;
                return;
            }
            base.UpdateMoveCounters();
        }

        public override IEnumerable<IEnemyMove> GetTurnMoves()
        {
            switch (explodeSequence)
            {
                case ExplodeSequence.Dont:
                    break;
                case ExplodeSequence.Charge:
                    yield return new SimpleEnemyMove(Intention.Unknown(), new EnemyMoveAction(this, GetMove(0)));
                    yield break;
                case ExplodeSequence.Explode:
                    yield return new SimpleEnemyMove(Intention.Explode(this.Shield), ExplodeAction());
                    yield break;
                default:
                    break;
            }

            foreach(var m in base.GetTurnMoves())
                yield return m;
        }

        IEnumerable<BattleAction> ExplodeAction()
        {
            yield return new ExplodeAction(this, Battle.Player, DamageInfo.Attack(this.Shield, false), LBoL.Core.Battle.DieCause.Explode, this, "GuihuoExplodeB2", LBoL.Core.Cards.GunType.Single);
            //yield return new ForceKillAction(this, this);

        }
    }
}
