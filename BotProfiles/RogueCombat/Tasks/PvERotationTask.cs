namespace CombatRogueBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string AdrenalineRush = "Adrenaline Rush";
        const string BladeFlurry = "Blade Flurry";
        const string Evasion = "Evasion";
        const string Eviscerate = "Eviscerate";
        const string Gouge = "Gouge";
        const string BloodFury = "Blood Fury";
        const string Kick = "Kick";
        const string Riposte = "Riposte";
        const string SinisterStrike = "Sinister Strike";
        const string SliceAndDice = "Slice and Dice";

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public override void PerformCombatRotation()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Aggressors.Any(x => x.TargetGuid == ObjectManager.Player.Guid))
            {
                Update(0);
                return;
            }

            if (ObjectManager.CasterAggressors.Any(x => x.TargetGuid == ObjectManager.Player.Guid))
            {
                if (MoveBehindTankSpot(15))
                    return;
            }

            AssignDPSTarget();

            if (!raidLeader.IsMoving && ObjectManager.Player.Target != null && ObjectManager.Player.Target.Position.DistanceTo(raidLeader.Position) <= 5)
            {
                if (MoveBehindTarget(3))
                    return;
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
                    ObjectManager.Player.StartAttack();

                    TryUseAbility(AdrenalineRush, 0, ObjectManager.Aggressors.Count() == 3 && ObjectManager.Player.HealthPercent > 80);

                    TryUseAbilityById(BloodFury, 3, 0, ObjectManager.Player.Target.HealthPercent > 80);

                    TryUseAbility(Evasion, 0, ObjectManager.Aggressors.Count() > 1);

                    TryUseAbility(BladeFlurry, 25, ObjectManager.Aggressors.Count() > 1);

                    TryUseAbility(SliceAndDice, 25, !ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.Player.Target.HealthPercent > 70 && ObjectManager.Player.ComboPoints == 2);

                    TryUseAbility(Riposte, 10, ObjectManager.Player.CanRiposte);

                    TryUseAbility(Kick, 25, ReadyToInterrupt(ObjectManager.Player.Target));

                    TryUseAbility(Gouge, 45, ReadyToInterrupt(ObjectManager.Player.Target) && !ObjectManager.Player.IsSpellReady(Kick));

                    bool readyToEviscerate =
                        ObjectManager.Player.Target.HealthPercent <= 15 && ObjectManager.Player.ComboPoints >= 2
                        || ObjectManager.Player.Target.HealthPercent <= 25 && ObjectManager.Player.ComboPoints >= 3
                        || ObjectManager.Player.Target.HealthPercent <= 35 && ObjectManager.Player.ComboPoints >= 4
                        || ObjectManager.Player.ComboPoints == 5;
                    TryUseAbility(Eviscerate, 35, readyToEviscerate);

                    TryUseAbility(SinisterStrike, 45, ObjectManager.Player.ComboPoints < 5);
                }
            }
            else
                ObjectManager.Player.StopAllMovement();
        }

        bool ReadyToInterrupt(WoWUnit target) => Container.State.IsRole1 && ObjectManager.Player.Target.Mana > 0 && (target.IsCasting || ObjectManager.Player.Target.IsChanneling);
    }
}
