using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [Serializable]
    public sealed class PaperDollLayer
    {
        public PaperDollSlot slot;
        public SpriteRenderer renderer;
    }

    public sealed class PaperDollCharacterVisual : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private PaperDollAppearanceDefinition baseAppearance;
        [SerializeField] private List<EquipmentVisualDefinition> equippedVisuals = new List<EquipmentVisualDefinition>();

        [Header("Layers")]
        [SerializeField] private PaperDollLayer[] layers;

        [Header("Weapon stays separate")]
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private Transform weaponSocket;

        [Header("Direction")]
        [SerializeField] private FacingDirection8 facing = FacingDirection8.South;

        private readonly Dictionary<PaperDollSlot, SpriteRenderer> _renderers =
            new Dictionary<PaperDollSlot, SpriteRenderer>();

        public FacingDirection8 Facing => facing;
        public Transform WeaponSocket => weaponSocket;
        public PaperDollAppearanceDefinition BaseAppearance => baseAppearance;
        public IReadOnlyList<EquipmentVisualDefinition> EquippedVisuals => equippedVisuals;

        private void Awake()
        {
            RebuildLayerCache();
            Refresh();
        }

        public void Configure(
            PaperDollAppearanceDefinition appearance,
            PaperDollLayer[] configuredLayers,
            SpriteRenderer configuredWeaponRenderer,
            Transform configuredWeaponSocket)
        {
            baseAppearance = appearance;
            layers = configuredLayers;
            weaponRenderer = configuredWeaponRenderer;
            weaponSocket = configuredWeaponSocket;
            RebuildLayerCache();
            Refresh();
        }

        public void RebuildLayerCache()
        {
            _renderers.Clear();

            if (layers == null)
                return;

            foreach (PaperDollLayer layer in layers)
            {
                if (layer == null || layer.renderer == null)
                    continue;

                _renderers[layer.slot] = layer.renderer;
            }
        }

        public void SetBaseAppearance(PaperDollAppearanceDefinition appearance)
        {
            baseAppearance = appearance;
            Refresh();
        }

        public void EquipVisual(EquipmentVisualDefinition visual)
        {
            if (visual == null)
                return;

            for (int i = equippedVisuals.Count - 1; i >= 0; i--)
            {
                EquipmentVisualDefinition current = equippedVisuals[i];
                if (current == null || current.equipSlot == visual.equipSlot)
                    equippedVisuals.RemoveAt(i);
            }

            equippedVisuals.Add(visual);
            Refresh();
        }

        public void UnequipVisual(EquipmentVisualSlot slot)
        {
            for (int i = equippedVisuals.Count - 1; i >= 0; i--)
            {
                EquipmentVisualDefinition current = equippedVisuals[i];
                if (current == null || current.equipSlot == slot)
                    equippedVisuals.RemoveAt(i);
            }

            Refresh();
        }

        public void ClearEquipmentVisuals()
        {
            equippedVisuals.Clear();
            Refresh();
        }

        public EquipmentVisualDefinition GetEquipped(EquipmentVisualSlot slot)
        {
            for (int i = equippedVisuals.Count - 1; i >= 0; i--)
            {
                EquipmentVisualDefinition current = equippedVisuals[i];
                if (current != null && current.equipSlot == slot)
                    return current;
            }

            return null;
        }

        public void SetFacing(FacingDirection8 direction)
        {
            if (facing == direction)
                return;

            facing = direction;
            Refresh();
        }

        public void FaceWorldDirection(Vector3 movement)
        {
            if (movement.sqrMagnitude < 0.0001f)
                return;

            SetFacing(DirectionalSpriteSet8.FromWorldDirection(movement));
        }

        public void Refresh()
        {
            if (_renderers.Count == 0)
                RebuildLayerCache();

            foreach (KeyValuePair<PaperDollSlot, SpriteRenderer> pair in _renderers)
            {
                ResolveSlot(pair.Key, out Sprite sprite, out bool flip, out bool hidden);
                pair.Value.sprite = hidden ? null : sprite;
                pair.Value.flipX = flip;
                pair.Value.enabled = !hidden && sprite != null;
            }

            RefreshWeapon();
            ApplyDirectionalSorting();
        }

        public void SetTint(Color color)
        {
            foreach (SpriteRenderer renderer in _renderers.Values)
            {
                if (renderer != null)
                    renderer.color = color;
            }

            if (weaponRenderer != null)
                weaponRenderer.color = color;
        }

        private void ResolveSlot(PaperDollSlot slot, out Sprite sprite, out bool flipX, out bool hidden)
        {
            sprite = null;
            flipX = false;
            hidden = false;

            for (int i = equippedVisuals.Count - 1; i >= 0; i--)
            {
                EquipmentVisualDefinition equipment = equippedVisuals[i];
                if (equipment == null)
                    continue;

                if (equipment.Hides(slot))
                {
                    hidden = true;
                    return;
                }

                PaperDollSlotSprites overrideSlot = equipment.Find(slot);
                if (overrideSlot != null)
                {
                    sprite = overrideSlot.sprites.Get(facing, out flipX);
                    return;
                }
            }

            PaperDollSlotSprites baseSlot = baseAppearance != null ? baseAppearance.Find(slot) : null;
            if (baseSlot != null)
                sprite = baseSlot.sprites.Get(facing, out flipX);
        }

        private void ApplyDirectionalSorting()
        {
            bool facingNorth = facing == FacingDirection8.North ||
                               facing == FacingDirection8.NorthEast ||
                               facing == FacingDirection8.NorthWest;

            foreach (KeyValuePair<PaperDollSlot, SpriteRenderer> pair in _renderers)
            {
                SpriteRenderer renderer = pair.Value;
                if (renderer == null)
                    continue;

                int order;
                switch (pair.Key)
                {
                    case PaperDollSlot.CloakBack:
                    case PaperDollSlot.CloakBackLeft:
                    case PaperDollSlot.CloakBackCenter:
                    case PaperDollSlot.CloakBackRight:
                        order = facingNorth ? 34 : 3;
                        break;
                    case PaperDollSlot.ThighLeft:
                    case PaperDollSlot.ShinLeft:
                    case PaperDollSlot.BootLeft:
                        order = 10;
                        break;
                    case PaperDollSlot.ThighRight:
                    case PaperDollSlot.ShinRight:
                    case PaperDollSlot.BootRight:
                        order = 12;
                        break;
                    case PaperDollSlot.Pelvis:
                        order = 17;
                        break;
                    case PaperDollSlot.Torso:
                        order = 20;
                        break;
                    case PaperDollSlot.UpperArmLeft:
                    case PaperDollSlot.ForearmLeft:
                    case PaperDollSlot.HandLeft:
                        order = facingNorth ? 23 : 21;
                        break;
                    case PaperDollSlot.UpperArmRight:
                    case PaperDollSlot.ForearmRight:
                    case PaperDollSlot.HandRight:
                        order = facingNorth ? 21 : 24;
                        break;
                    case PaperDollSlot.BeltAccessory:
                    case PaperDollSlot.ShoulderAccessory:
                        order = 27;
                        break;
                    case PaperDollSlot.CloakFront:
                    case PaperDollSlot.CloakFrontLeft:
                    case PaperDollSlot.CloakFrontRight:
                        order = facingNorth ? 6 : 31;
                        break;
                    case PaperDollSlot.HairBack:
                        order = 40;
                        break;
                    case PaperDollSlot.Head:
                        order = 41;
                        break;
                    case PaperDollSlot.Eyes:
                    case PaperDollSlot.Mouth:
                        order = 42;
                        break;
                    case PaperDollSlot.Hair:
                    case PaperDollSlot.HairFront:
                        order = 43;
                        break;
                    default:
                        order = 20;
                        break;
                }

                renderer.sortingOrder = order;
            }

            if (weaponRenderer != null)
                weaponRenderer.sortingOrder = facingNorth ? 16 : 35;
        }

        private void RefreshWeapon()
        {
            if (weaponRenderer == null)
                return;

            EquipmentVisualDefinition weapon = GetEquipped(EquipmentVisualSlot.Weapon);
            if (weapon == null || !weapon.hasWeaponVisual)
            {
                weaponRenderer.sprite = null;
                weaponRenderer.enabled = false;
                return;
            }

            Sprite sprite = weapon.weaponSprites.Get(facing, out bool flip);
            weaponRenderer.sprite = sprite;
            weaponRenderer.flipX = flip;
            weaponRenderer.enabled = sprite != null;
        }
    }
}
