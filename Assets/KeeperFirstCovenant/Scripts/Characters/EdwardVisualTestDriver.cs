using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Characters
{
    [DisallowMultipleComponent]
    public sealed class EdwardVisualTestDriver : MonoBehaviour
    {
        [SerializeField] private PaperDollCharacterVisual visual;
        [SerializeField] private PaperDollMotionAnimator motion;
        [SerializeField] private CombatantRuntime combatant;
        [SerializeField] private EquipmentVisualDefinition travelSword;
        [SerializeField] private EquipmentVisualDefinition greatsword;
        [SerializeField] private EquipmentVisualDefinition leatherArmor;
        [SerializeField] private EquipmentVisualDefinition travelerCloak;
        [SerializeField] private bool showControls = true;

        private bool _armorOn;
        private bool _cloakOn = true;
        private int _weaponIndex;

        public void Configure(
            PaperDollCharacterVisual configuredVisual,
            PaperDollMotionAnimator configuredMotion,
            CombatantRuntime configuredCombatant,
            EquipmentVisualDefinition configuredTravelSword,
            EquipmentVisualDefinition configuredGreatsword,
            EquipmentVisualDefinition configuredLeatherArmor,
            EquipmentVisualDefinition configuredTravelerCloak)
        {
            visual = configuredVisual;
            motion = configuredMotion;
            combatant = configuredCombatant;
            travelSword = configuredTravelSword;
            greatsword = configuredGreatsword;
            leatherArmor = configuredLeatherArmor;
            travelerCloak = configuredTravelerCloak;
        }

        private void Start()
        {
            if (visual == null)
                visual = GetComponentInChildren<PaperDollCharacterVisual>();
            if (motion == null)
                motion = GetComponentInChildren<PaperDollMotionAnimator>();
            if (combatant == null)
                combatant = GetComponent<CombatantRuntime>();

            if (travelerCloak != null)
                visual?.EquipVisual(travelerCloak);
            if (travelSword != null)
                visual?.EquipVisual(travelSword);
        }

        private void Update()
        {
            Keyboard k = Keyboard.current;
            if (k == null || motion == null)
                return;

            if (k.jKey.wasPressedThisFrame) motion.PlayLightAttack();
            if (k.kKey.wasPressedThisFrame) motion.PlayHeavyAttack();
            if (k.cKey.wasPressedThisFrame) motion.PlayCast();
            if (k.eKey.wasPressedThisFrame) motion.PlayInteract();
            if (k.gKey.wasPressedThisFrame) motion.SetGuarding(true);
            if (k.gKey.wasReleasedThisFrame) motion.SetGuarding(false);
            if (k.hKey.wasPressedThisFrame) ApplyTestDamage(7, false);
            if (k.yKey.wasPressedThisFrame) ApplyTestDamage(18, true);
            if (k.xKey.wasPressedThisFrame) CycleWeapon();
            if (k.vKey.wasPressedThisFrame) ToggleArmor();
            if (k.bKey.wasPressedThisFrame) ToggleCloak();
            if (k.deleteKey.wasPressedThisFrame) KillEdward();
            if (k.rKey.wasPressedThisFrame) ResetEdward();
        }

        private void ApplyTestDamage(int amount, bool critical)
        {
            if (combatant != null && combatant.IsAlive)
                combatant.ApplyDamage(new DamagePacket(amount, DamageType.Physical, gameObject, critical));
            else
                motion.PlayHit(critical);
        }

        private void KillEdward()
        {
            if (combatant != null && combatant.IsAlive)
                combatant.ApplyDamage(new DamagePacket(9999, DamageType.Physical, gameObject, true));
            else
                motion.PlayDeath();
        }

        private void ResetEdward()
        {
            combatant?.ResetRuntime();
            motion?.ReviveVisual();
        }

        private void CycleWeapon()
        {
            if (visual == null)
                return;

            _weaponIndex = (_weaponIndex + 1) % 3;
            visual.UnequipVisual(EquipmentVisualSlot.Weapon);

            if (_weaponIndex == 0 && travelSword != null)
                visual.EquipVisual(travelSword);
            else if (_weaponIndex == 1 && greatsword != null)
                visual.EquipVisual(greatsword);
        }

        private void ToggleArmor()
        {
            if (visual == null || leatherArmor == null)
                return;

            _armorOn = !_armorOn;
            if (_armorOn)
                visual.EquipVisual(leatherArmor);
            else
                visual.UnequipVisual(EquipmentVisualSlot.Torso);
        }

        private void ToggleCloak()
        {
            if (visual == null || travelerCloak == null)
                return;

            _cloakOn = !_cloakOn;
            if (_cloakOn)
                visual.EquipVisual(travelerCloak);
            else
                visual.UnequipVisual(EquipmentVisualSlot.Cloak);
        }

        private void OnGUI()
        {
            if (!showControls)
                return;

            const int width = 430;
            GUI.Box(new Rect(18, 18, width, 248), "Edward production 2D rig — test controls");
            GUI.Label(new Rect(34, 50, width - 30, 22), "WASD — move   Shift — run   8 directional facings");
            GUI.Label(new Rect(34, 74, width - 30, 22), "J — sword attack   K — heavy attack   G — guard");
            GUI.Label(new Rect(34, 98, width - 30, 22), "C — cast / fire pose   E — interact");
            GUI.Label(new Rect(34, 122, width - 30, 22), "H — hit   Y — critical hit   Delete — death   R — revive");
            GUI.Label(new Rect(34, 146, width - 30, 22), "X — weapon: sword / greatsword / none");
            GUI.Label(new Rect(34, 170, width - 30, 22), "V — leather armor on/off   B — cloak on/off");
            GUI.Label(new Rect(34, 202, width - 30, 42), "Idle: breathing + blinking + head motion + independent cloak strips.");
        }
    }
}
