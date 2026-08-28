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
        [SerializeField] private PaperDollAppearanceDefinition outfitAppearance;
        [SerializeField] private EquipmentVisualDefinition weaponVisual;

        [Header("Layers")]
        [SerializeField] private PaperDollLayer[] layers;

        [Header("Weapon stays separate from the character")]
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private Transform weaponSocket;

        [Header("Direction")]
        [SerializeField] private FacingDirection4 facing = FacingDirection4.SouthEast;

        private readonly Dictionary<PaperDollSlot, SpriteRenderer> _renderers =
            new Dictionary<PaperDollSlot, SpriteRenderer>();

        public FacingDirection4 Facing => facing;
        public Transform WeaponSocket => weaponSocket;

        private void Awake()
        {
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

        public void SetOutfit(PaperDollAppearanceDefinition appearance)
        {
            outfitAppearance = appearance;
            Refresh();
        }

        public void SetWeapon(EquipmentVisualDefinition visual)
        {
            weaponVisual = visual;
            RefreshWeapon();
        }

        public void SetFacing(FacingDirection4 direction)
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

            // Character moves on XZ. South means closer to the isometric camera.
            bool east = movement.x >= 0f;
            bool south = movement.z <= 0f;

            FacingDirection4 next;
            if (south)
                next = east ? FacingDirection4.SouthEast : FacingDirection4.SouthWest;
            else
                next = east ? FacingDirection4.NorthEast : FacingDirection4.NorthWest;

            SetFacing(next);
        }

        public void Refresh()
        {
            if (_renderers.Count == 0)
                RebuildLayerCache();

            foreach (KeyValuePair<PaperDollSlot, SpriteRenderer> pair in _renderers)
            {
                Sprite sprite = null;
                bool flip = false;

                PaperDollSlotSprites outfit = outfitAppearance != null
                    ? outfitAppearance.Find(pair.Key)
                    : null;

                if (outfit != null)
                {
                    sprite = outfit.sprites.Get(facing, out flip);
                }
                else if (baseAppearance != null)
                {
                    PaperDollSlotSprites baseSlot = baseAppearance.Find(pair.Key);
                    if (baseSlot != null)
                        sprite = baseSlot.sprites.Get(facing, out flip);
                }

                pair.Value.sprite = sprite;
                pair.Value.flipX = flip;
                pair.Value.enabled = sprite != null;
            }

            RefreshWeapon();
        }

        private void RefreshWeapon()
        {
            if (weaponRenderer == null)
                return;

            if (weaponVisual == null || !weaponVisual.hasWeaponVisual)
            {
                weaponRenderer.sprite = null;
                weaponRenderer.enabled = false;
                return;
            }

            Sprite sprite = weaponVisual.weaponSprites.Get(facing, out bool flip);
            weaponRenderer.sprite = sprite;
            weaponRenderer.flipX = flip;
            weaponRenderer.enabled = sprite != null;
        }
    }
}
