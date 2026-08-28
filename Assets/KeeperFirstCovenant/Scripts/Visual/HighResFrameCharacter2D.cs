using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeeperFirstCovenant.Visual
{
    [DisallowMultipleComponent]
    public sealed class HighResFrameCharacter2D : MonoBehaviour
    {
        [Header("Base character")]
        [SerializeField] private FrameAnimationLibrary baseLibrary;

        [Header("Synchronized render layers")]
        [SerializeField] private SpriteRenderer baseRenderer;
        [SerializeField] private SpriteRenderer armorRenderer;
        [SerializeField] private SpriteRenderer cloakRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private SpriteRenderer headgearRenderer;
        [SerializeField] private SpriteRenderer accessoryRenderer;

        [Header("State")]
        [SerializeField] private CharacterFrameState state = CharacterFrameState.Idle;
        [SerializeField] private SpriteFacing8 facing = SpriteFacing8.South;

        private readonly Dictionary<VisualEquipmentSlot, FrameEquipmentLayerDefinition> _equipment =
            new Dictionary<VisualEquipmentSlot, FrameEquipmentLayerDefinition>();

        private FrameAnimationClip8 _activeBaseClip;
        private float _frameAccumulator;
        private int _frameIndex;
        private bool _impactEmitted;
        private bool _finishedEmitted;

        public CharacterFrameState State => state;
        public SpriteFacing8 Facing => facing;
        public int FrameIndex => _frameIndex;
        public bool IsOneShotPlaying => _activeBaseClip != null && !_activeBaseClip.loop;

        public event Action<CharacterFrameState> StateChanged;
        public event Action<CharacterFrameState> Impact;
        public event Action<CharacterFrameState> AnimationFinished;

        private void Awake()
        {
            ResolveRenderers();
            RestartState(state, true);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Configure(
            FrameAnimationLibrary library,
            SpriteRenderer baseLayer,
            SpriteRenderer armorLayer,
            SpriteRenderer cloakLayer,
            SpriteRenderer weaponLayer,
            SpriteRenderer headgearLayer = null,
            SpriteRenderer accessoryLayer = null)
        {
            baseLibrary = library;
            baseRenderer = baseLayer;
            armorRenderer = armorLayer;
            cloakRenderer = cloakLayer;
            weaponRenderer = weaponLayer;
            headgearRenderer = headgearLayer;
            accessoryRenderer = accessoryLayer;

            ResolveRenderers();
            RestartState(state, true);
        }

        public void SetFacing(SpriteFacing8 nextFacing)
        {
            if (facing == nextFacing)
                return;

            facing = nextFacing;
            _frameIndex = 0;
            _frameAccumulator = 0f;
            _impactEmitted = false;
            _finishedEmitted = false;
            RefreshFrame();
        }

        public void FaceWorldDirection(Vector3 worldDirection)
        {
            if (worldDirection.sqrMagnitude < 0.0001f)
                return;

            SetFacing(SpriteFacing8Utility.FromWorldDirection(worldDirection));
        }

        public void PlayLoop(CharacterFrameState nextState)
        {
            FrameAnimationClip8 clip = baseLibrary != null ? baseLibrary.Find(nextState) : null;
            if (clip == null)
                return;

            if (!clip.loop)
                Debug.LogWarning($"{name}: {nextState} is configured as one-shot but PlayLoop was requested.");

            RestartState(nextState, state == nextState);
        }

        public void PlayOneShot(CharacterFrameState nextState, bool restartIfSame = true)
        {
            FrameAnimationClip8 clip = baseLibrary != null ? baseLibrary.Find(nextState) : null;
            if (clip == null)
                return;

            if (!restartIfSame && state == nextState && IsOneShotPlaying)
                return;

            RestartState(nextState, true);
        }

        public void Equip(FrameEquipmentLayerDefinition visual)
        {
            if (visual == null)
                return;

            _equipment[visual.slot] = visual;
            RefreshFrame();
        }

        public void Unequip(VisualEquipmentSlot slot)
        {
            if (_equipment.Remove(slot))
                RefreshFrame();
        }

        public FrameEquipmentLayerDefinition GetEquipped(VisualEquipmentSlot slot)
        {
            return _equipment.TryGetValue(slot, out FrameEquipmentLayerDefinition value)
                ? value
                : null;
        }

        public void ClearEquipment()
        {
            _equipment.Clear();
            RefreshFrame();
        }

        private void Tick(float deltaTime)
        {
            if (_activeBaseClip == null)
            {
                RestartState(CharacterFrameState.Idle, true);
                return;
            }

            int frameCount = _activeBaseClip.GetFrameCount(facing);
            if (frameCount <= 0)
                return;

            float fps = Mathf.Max(1f, _activeBaseClip.framesPerSecond);
            _frameAccumulator += deltaTime * fps;

            while (_frameAccumulator >= 1f)
            {
                _frameAccumulator -= 1f;
                AdvanceFrame(frameCount);

                if (_activeBaseClip == null)
                    return;

                frameCount = _activeBaseClip.GetFrameCount(facing);
                if (frameCount <= 0)
                    return;
            }
        }

        private void AdvanceFrame(int frameCount)
        {
            if (_activeBaseClip == null)
                return;

            int nextIndex = _frameIndex + 1;

            if (_activeBaseClip.loop)
            {
                _frameIndex = nextIndex % frameCount;
            }
            else if (nextIndex >= frameCount)
            {
                _frameIndex = Mathf.Max(0, frameCount - 1);
                RefreshFrame();

                if (!_finishedEmitted)
                {
                    _finishedEmitted = true;
                    AnimationFinished?.Invoke(state);
                }

                return;
            }
            else
            {
                _frameIndex = nextIndex;
            }

            EmitImpactIfNeeded();
            RefreshFrame();
        }

        private void EmitImpactIfNeeded()
        {
            if (_activeBaseClip == null ||
                _impactEmitted ||
                _activeBaseClip.impactFrame < 0 ||
                _frameIndex < _activeBaseClip.impactFrame)
            {
                return;
            }

            _impactEmitted = true;
            Impact?.Invoke(state);
        }

        private void RestartState(CharacterFrameState nextState, bool force)
        {
            if (!force && state == nextState)
                return;

            FrameAnimationClip8 nextClip = baseLibrary != null ? baseLibrary.Find(nextState) : null;
            if (nextClip == null)
            {
                if (nextState != CharacterFrameState.Idle)
                {
                    nextState = CharacterFrameState.Idle;
                    nextClip = baseLibrary != null ? baseLibrary.Find(nextState) : null;
                }
            }

            state = nextState;
            _activeBaseClip = nextClip;
            _frameIndex = 0;
            _frameAccumulator = 0f;
            _impactEmitted = false;
            _finishedEmitted = false;

            RefreshFrame();
            StateChanged?.Invoke(state);
        }

        private void RefreshFrame()
        {
            ApplyLayer(baseRenderer, baseLibrary, state, facing, _frameIndex);

            ApplyEquipmentLayer(VisualEquipmentSlot.Armor, armorRenderer);
            ApplyEquipmentLayer(VisualEquipmentSlot.Cloak, cloakRenderer);
            ApplyEquipmentLayer(VisualEquipmentSlot.Weapon, weaponRenderer);
            ApplyEquipmentLayer(VisualEquipmentSlot.Headgear, headgearRenderer);
            ApplyEquipmentLayer(VisualEquipmentSlot.Accessory, accessoryRenderer);
        }

        private void ApplyEquipmentLayer(
            VisualEquipmentSlot slot,
            SpriteRenderer renderer)
        {
            if (renderer == null)
                return;

            FrameEquipmentLayerDefinition equipment = GetEquipped(slot);
            FrameAnimationLibrary library = equipment != null
                ? equipment.animationLibrary
                : null;

            ApplyLayer(renderer, library, state, facing, _frameIndex);
        }

        private static void ApplyLayer(
            SpriteRenderer renderer,
            FrameAnimationLibrary library,
            CharacterFrameState requestedState,
            SpriteFacing8 requestedFacing,
            int synchronizedFrameIndex)
        {
            if (renderer == null)
                return;

            FrameAnimationClip8 clip = library != null
                ? library.Find(requestedState)
                : null;

            if (clip == null && library != null)
                clip = library.Find(CharacterFrameState.Idle);

            if (clip == null)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                return;
            }

            int count = clip.GetFrameCount(requestedFacing);
            if (count <= 0)
            {
                renderer.sprite = null;
                renderer.enabled = false;
                return;
            }

            int index = synchronizedFrameIndex % count;
            Sprite sprite = clip.GetFrame(requestedFacing, index, out bool flipX);
            renderer.sprite = sprite;
            renderer.flipX = flipX;
            renderer.enabled = sprite != null;
        }

        private void ResolveRenderers()
        {
            if (baseRenderer == null)
            {
                Transform child = transform.Find("Base");
                if (child != null)
                    baseRenderer = child.GetComponent<SpriteRenderer>();
            }

            if (armorRenderer == null)
                armorRenderer = FindRenderer("Armor");
            if (cloakRenderer == null)
                cloakRenderer = FindRenderer("Cloak");
            if (weaponRenderer == null)
                weaponRenderer = FindRenderer("Weapon");
            if (headgearRenderer == null)
                headgearRenderer = FindRenderer("Headgear");
            if (accessoryRenderer == null)
                accessoryRenderer = FindRenderer("Accessory");
        }

        private SpriteRenderer FindRenderer(string childName)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<SpriteRenderer>() : null;
        }
    }
}
