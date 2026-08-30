using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeeperFirstCovenant.Characters
{
    [DisallowMultipleComponent]
    public sealed class EdwardHighResTestDriver : MonoBehaviour
    {
        [SerializeField] private HighResFrameCharacter2D animator2D;
        [SerializeField] private CombatantRuntime combatant;

        [Header("Optional equipment test layers")]
        [SerializeField] private FrameEquipmentLayerDefinition armor;
        [SerializeField] private FrameEquipmentLayerDefinition cloak;
        [SerializeField] private FrameEquipmentLayerDefinition sword;
        [SerializeField] private FrameEquipmentLayerDefinition alternateWeapon;

        [SerializeField] private bool showControls = true;

        private bool _armorOn;
        private bool _cloakOn = true;
        private int _weaponIndex;

        private void Awake()
        {
            if (animator2D == null)
                animator2D = GetComponentInChildren<HighResFrameCharacter2D>();

            if (combatant == null)
                combatant = GetComponent<CombatantRuntime>();
        }

        private void Start()
        {
            if (cloak != null)
                animator2D?.Equip(cloak);

            if (sword != null)
                animator2D?.Equip(sword);
        }

        private void Update()
        {
            Keyboard k = Keyboard.current;
            if (k == null || animator2D == null)
                return;

            if (k.jKey.wasPressedThisFrame)
                animator2D.PlayOneShot(CharacterFrameState.AttackLight);

            if (k.kKey.wasPressedThisFrame)
                animator2D.PlayOneShot(CharacterFrameState.AttackHeavy);

            if (k.gKey.wasPressedThisFrame)
                animator2D.PlayLoop(CharacterFrameState.Guard);

            if (k.gKey.wasReleasedThisFrame)
                animator2D.PlayLoop(CharacterFrameState.Idle);

            if (k.cKey.wasPressedThisFrame)
                animator2D.PlayOneShot(CharacterFrameState.Cast);

            if (k.eKey.wasPressedThisFrame)
                animator2D.PlayOneShot(CharacterFrameState.Interact);

            if (k.hKey.wasPressedThisFrame)
                ApplyDamage(7, false);

            if (k.yKey.wasPressedThisFrame)
                ApplyDamage(18, true);

            if (k.deleteKey.wasPressedThisFrame)
                ApplyDamage(9999, true);

            if (k.rKey.wasPressedThisFrame)
            {
                combatant?.ResetRuntime();
                animator2D.PlayLoop(CharacterFrameState.Idle);
            }

            if (k.xKey.wasPressedThisFrame)
                CycleWeapon();

            if (k.vKey.wasPressedThisFrame)
                ToggleArmor();

            if (k.bKey.wasPressedThisFrame)
                ToggleCloak();
        }

        private void ApplyDamage(int amount, bool critical)
        {
            if (combatant != null && combatant.IsAlive)
            {
                combatant.ApplyDamage(
                    new DamagePacket(
                        amount,
                        DamageType.Physical,
                        gameObject,
                        critical));
            }
            else
            {
                animator2D.PlayOneShot(
                    critical
                        ? CharacterFrameState.CriticalHit
                        : CharacterFrameState.Hit);
            }
        }

        private void CycleWeapon()
        {
            if (animator2D == null)
                return;

            _weaponIndex = (_weaponIndex + 1) % 3;
            animator2D.Unequip(VisualEquipmentSlot.Weapon);

            if (_weaponIndex == 0 && sword != null)
                animator2D.Equip(sword);
            else if (_weaponIndex == 1 && alternateWeapon != null)
                animator2D.Equip(alternateWeapon);
        }

        private void ToggleArmor()
        {
            if (animator2D == null || armor == null)
                return;

            _armorOn = !_armorOn;

            if (_armorOn)
                animator2D.Equip(armor);
            else
                animator2D.Unequip(VisualEquipmentSlot.Armor);
        }

        private void ToggleCloak()
        {
            if (animator2D == null || cloak == null)
                return;

            _cloakOn = !_cloakOn;

            if (_cloakOn)
                animator2D.Equip(cloak);
            else
                animator2D.Unequip(VisualEquipmentSlot.Cloak);
        }

        private void OnGUI()
        {
            if (!showControls)
                return;

            GUI.Box(
                new Rect(16, 16, 440, 236),
                "Edward — high-resolution frame animation test");

            GUI.Label(
                new Rect(32, 48, 410, 22),
                "WASD move | Shift run | full 8-direction facing");

            GUI.Label(
                new Rect(32, 72, 410, 22),
                "J attack | K heavy | G guard | C cast | E interact");

            GUI.Label(
                new Rect(32, 96, 410, 22),
                "H hit | Y critical hit | Delete death | R reset");

            GUI.Label(
                new Rect(32, 120, 410, 22),
                "X weapon | V armor | B cloak");

            GUI.Label(
                new Rect(32, 154, 400, 58),
                "Every state is full painted frame animation. " +
                "Equipment is synchronized as full overlay frames, not rotating body parts.");
        }
    }
}
