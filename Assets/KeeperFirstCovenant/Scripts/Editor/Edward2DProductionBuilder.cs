#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class Edward2DProductionBuilder
    {
        public const string Root = "Assets/KeeperFirstCovenant/Generated2D";
        public const string SpriteRoot = Root + "/Sprites/Edward";
        public const string DataRoot = Root + "/Data";
        public const string PrefabRoot = Root + "/Prefabs";
        public const string EdwardPrefabPath = PrefabRoot + "/Edward_2D_Production.prefab";

        private static readonly Color Outline = new Color(0.055f, 0.06f, 0.075f, 1f);
        private static readonly Color Skin = new Color(0.78f, 0.67f, 0.60f, 1f);
        private static readonly Color SkinLight = new Color(0.91f, 0.80f, 0.71f, 1f);
        private static readonly Color SkinShadow = new Color(0.53f, 0.40f, 0.36f, 1f);
        private static readonly Color Hair = new Color(0.055f, 0.06f, 0.075f, 1f);
        private static readonly Color HairLight = new Color(0.13f, 0.14f, 0.17f, 1f);
        private static readonly Color Cloth = new Color(0.105f, 0.105f, 0.125f, 1f);
        private static readonly Color ClothLight = new Color(0.19f, 0.18f, 0.205f, 1f);
        private static readonly Color ClothShadow = new Color(0.055f, 0.055f, 0.068f, 1f);
        private static readonly Color Leather = new Color(0.20f, 0.125f, 0.085f, 1f);
        private static readonly Color LeatherLight = new Color(0.34f, 0.22f, 0.13f, 1f);
        private static readonly Color Metal = new Color(0.53f, 0.57f, 0.60f, 1f);
        private static readonly Color MetalLight = new Color(0.86f, 0.89f, 0.88f, 1f);
        private static readonly Color Eye = new Color(0.60f, 0.50f, 0.37f, 1f);

        private static readonly FacingDirection8[] AuthoredDirections =
        {
            FacingDirection8.North,
            FacingDirection8.NorthEast,
            FacingDirection8.East,
            FacingDirection8.SouthEast,
            FacingDirection8.South
        };

        private sealed class RigRefs
        {
            public readonly List<PaperDollLayer> layers = new List<PaperDollLayer>();
            public Transform rigRoot;
            public Transform torso;
            public Transform head;
            public SpriteRenderer eyes;
            public Transform upperArmLeft;
            public Transform upperArmRight;
            public Transform forearmLeft;
            public Transform forearmRight;
            public Transform thighLeft;
            public Transform thighRight;
            public Transform shinLeft;
            public Transform shinRight;
            public Transform cloakLeft;
            public Transform cloakCenter;
            public Transform cloakRight;
            public Transform weaponSocket;
            public SpriteRenderer weaponRenderer;
            public Transform castingHand;
        }

        [MenuItem("Keeper First Covenant/2D Production/Rebuild Edward")]
        public static void BuildEdward()
        {
            EnsureFolder("Assets", "KeeperFirstCovenant");
            EnsureFolder("Assets/KeeperFirstCovenant", "Generated2D");
            EnsureFolder(Root, "Sprites");
            EnsureFolder(Root + "/Sprites", "Edward");
            EnsureFolder(Root, "Data");
            EnsureFolder(Root, "Prefabs");

            PaperDollAppearanceDefinition baseAppearance = BuildBaseAppearance();
            EquipmentVisualDefinition cloak = BuildCloak();
            EquipmentVisualDefinition armor = BuildArmor();
            EquipmentVisualDefinition sword = BuildWeapon(false);
            EquipmentVisualDefinition greatsword = BuildWeapon(true);
            CharacterDefinition definition = BuildEdwardDefinition();

            GameObject prefabSource = BuildPrefabObject(baseAppearance, cloak, armor, sword, greatsword, definition);
            PrefabUtility.SaveAsPrefabAsset(prefabSource, EdwardPrefabPath);
            UnityEngine.Object.DestroyImmediate(prefabSource);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Edward 2D production prefab rebuilt: " + EdwardPrefabPath);
        }

        private static PaperDollAppearanceDefinition BuildBaseAppearance()
        {
            PaperDollAppearanceDefinition appearance = GetOrCreate<PaperDollAppearanceDefinition>(
                DataRoot + "/Edward_BaseAppearance.asset");

            appearance.appearanceId = "edward_base";
            appearance.displayName = "Edward — base travel body";

            appearance.slots = new[]
            {
                Slot(PaperDollSlot.Head, "head", DrawHead),
                Slot(PaperDollSlot.Eyes, "eyes", DrawEyes),
                Slot(PaperDollSlot.HairBack, "hair_back", DrawHairBack),
                Slot(PaperDollSlot.HairFront, "hair_front", DrawHairFront),
                Slot(PaperDollSlot.Torso, "torso", DrawTorso),
                Slot(PaperDollSlot.UpperArmLeft, "upper_arm_l", (c,d) => DrawUpperArm(c,d,false,false)),
                Slot(PaperDollSlot.ForearmLeft, "forearm_l", (c,d) => DrawForearm(c,d,false,false)),
                Slot(PaperDollSlot.HandLeft, "hand_l", DrawHand),
                Slot(PaperDollSlot.UpperArmRight, "upper_arm_r", (c,d) => DrawUpperArm(c,d,true,false)),
                Slot(PaperDollSlot.ForearmRight, "forearm_r", (c,d) => DrawForearm(c,d,true,false)),
                Slot(PaperDollSlot.HandRight, "hand_r", DrawHand),
                Slot(PaperDollSlot.Pelvis, "pelvis", DrawPelvis),
                Slot(PaperDollSlot.ThighLeft, "thigh_l", DrawThigh),
                Slot(PaperDollSlot.ShinLeft, "shin_l", DrawShin),
                Slot(PaperDollSlot.BootLeft, "boot_l", DrawBoot),
                Slot(PaperDollSlot.ThighRight, "thigh_r", DrawThigh),
                Slot(PaperDollSlot.ShinRight, "shin_r", DrawShin),
                Slot(PaperDollSlot.BootRight, "boot_r", DrawBoot),
                Slot(PaperDollSlot.BeltAccessory, "belt", DrawBelt)
            };

            EditorUtility.SetDirty(appearance);
            return appearance;
        }

        private static EquipmentVisualDefinition BuildCloak()
        {
            EquipmentVisualDefinition item = GetOrCreate<EquipmentVisualDefinition>(
                DataRoot + "/Edward_TravelerCloak.asset");

            item.visualId = "edward_traveler_cloak";
            item.displayName = "Worn Traveler Cloak";
            item.equipSlot = EquipmentVisualSlot.Cloak;
            item.hasWeaponVisual = false;
            item.hiddenSlots = Array.Empty<PaperDollSlot>();
            item.slotOverrides = new[]
            {
                Slot(PaperDollSlot.CloakBackLeft, "cloak_back_l", (c,d) => DrawCloakPanel(c,d,-1)),
                Slot(PaperDollSlot.CloakBackCenter, "cloak_back_c", (c,d) => DrawCloakPanel(c,d,0)),
                Slot(PaperDollSlot.CloakBackRight, "cloak_back_r", (c,d) => DrawCloakPanel(c,d,1)),
                Slot(PaperDollSlot.CloakFrontLeft, "cloak_front_l", (c,d) => DrawCloakFront(c,d,false)),
                Slot(PaperDollSlot.CloakFrontRight, "cloak_front_r", (c,d) => DrawCloakFront(c,d,true)),
                Slot(PaperDollSlot.ShoulderAccessory, "cloak_clasp", DrawCloakClasp)
            };

            EditorUtility.SetDirty(item);
            return item;
        }

        private static EquipmentVisualDefinition BuildArmor()
        {
            EquipmentVisualDefinition item = GetOrCreate<EquipmentVisualDefinition>(
                DataRoot + "/Edward_LeatherArmor.asset");

            item.visualId = "edward_leather_armor";
            item.displayName = "Reinforced Travel Leather";
            item.equipSlot = EquipmentVisualSlot.Torso;
            item.hasWeaponVisual = false;
            item.hiddenSlots = Array.Empty<PaperDollSlot>();
            item.slotOverrides = new[]
            {
                Slot(PaperDollSlot.Torso, "armor_torso", DrawArmorTorso),
                Slot(PaperDollSlot.UpperArmLeft, "armor_upper_l", (c,d) => DrawUpperArm(c,d,false,true)),
                Slot(PaperDollSlot.UpperArmRight, "armor_upper_r", (c,d) => DrawUpperArm(c,d,true,true))
            };

            EditorUtility.SetDirty(item);
            return item;
        }

        private static EquipmentVisualDefinition BuildWeapon(bool heavy)
        {
            string id = heavy ? "edward_greatsword" : "edward_travel_sword";
            EquipmentVisualDefinition item = GetOrCreate<EquipmentVisualDefinition>(
                DataRoot + "/" + id + ".asset");

            item.visualId = id;
            item.displayName = heavy ? "Weathered Greatsword" : "Travel Sword";
            item.equipSlot = EquipmentVisualSlot.Weapon;
            item.slotOverrides = Array.Empty<PaperDollSlotSprites>();
            item.hiddenSlots = Array.Empty<PaperDollSlot>();
            item.hasWeaponVisual = true;
            item.weaponSprites = BuildDirectionalSet(
                heavy ? "greatsword" : "sword",
                (c, d) => DrawSword(c, d, heavy));

            EditorUtility.SetDirty(item);
            return item;
        }

        private static CharacterDefinition BuildEdwardDefinition()
        {
            CharacterDefinition definition = GetOrCreate<CharacterDefinition>(
                DataRoot + "/Edward_VisualTest.asset");

            definition.characterId = "edward";
            definition.displayName = "Edward";
            definition.faction = CombatFaction.Player;
            definition.maxHealth = 100;
            definition.maxMana = 50;
            definition.strength = 13;
            definition.finesse = 12;
            definition.intellect = 12;
            definition.willpower = 11;
            definition.perception = 12;
            definition.actionPoints = 2;
            definition.movementMeters = 9f;
            definition.startingActions = Array.Empty<CombatActionDefinition>();

            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject BuildPrefabObject(
            PaperDollAppearanceDefinition baseAppearance,
            EquipmentVisualDefinition cloak,
            EquipmentVisualDefinition armor,
            EquipmentVisualDefinition sword,
            EquipmentVisualDefinition greatsword,
            CharacterDefinition definition)
        {
            GameObject root = new GameObject("Edward_2D_Production");
            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.85f;
            cc.radius = 0.28f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.22f;
            cc.skinWidth = 0.035f;

            CombatantRuntime combatant = root.AddComponent<CombatantRuntime>();
            combatant.SetDefinition(definition);

            GameObject billboard = new GameObject("BillboardRoot");
            billboard.transform.SetParent(root.transform, false);
            billboard.AddComponent<BillboardCharacter2D>();

            GameObject rigObject = new GameObject("RigRoot");
            rigObject.transform.SetParent(billboard.transform, false);
            rigObject.transform.localPosition = new Vector3(0f, 0.93f, 0f);

            RigRefs rig = BuildRig(rigObject.transform);
            PaperDollCharacterVisual visual = rigObject.AddComponent<PaperDollCharacterVisual>();
            visual.Configure(baseAppearance, rig.layers.ToArray(), rig.weaponRenderer, rig.weaponSocket);
            visual.EquipVisual(cloak);
            visual.EquipVisual(sword);

            PaperDollMotionAnimator motion = rigObject.AddComponent<PaperDollMotionAnimator>();
            motion.Configure(
                rig.rigRoot,
                rig.torso,
                rig.head,
                rig.eyes,
                rig.upperArmLeft,
                rig.upperArmRight,
                rig.forearmLeft,
                rig.forearmRight,
                rig.thighLeft,
                rig.thighRight,
                rig.shinLeft,
                rig.shinRight,
                rig.cloakLeft,
                rig.cloakCenter,
                rig.cloakRight,
                rig.weaponSocket);

            root.AddComponent<EdwardExplorationController>();
            root.AddComponent<CombatVisualBridge>();
            root.AddComponent<PaperDollBloodVisual>();

            EdwardFireVisual fire = root.AddComponent<EdwardFireVisual>();
            fire.Configure(motion, rig.weaponSocket, rig.castingHand);

            EdwardVisualTestDriver test = root.AddComponent<EdwardVisualTestDriver>();
            test.Configure(visual, motion, combatant, sword, greatsword, armor, cloak);

            return root;
        }

        private static RigRefs BuildRig(Transform parent)
        {
            RigRefs r = new RigRefs();
            r.rigRoot = parent;

            r.cloakLeft = Bone(parent, "CloakBackLeft", new Vector3(-0.16f, -0.10f, 0f),
                PaperDollSlot.CloakBackLeft, r.layers).transform;
            r.cloakCenter = Bone(parent, "CloakBackCenter", new Vector3(0f, -0.12f, 0f),
                PaperDollSlot.CloakBackCenter, r.layers).transform;
            r.cloakRight = Bone(parent, "CloakBackRight", new Vector3(0.16f, -0.10f, 0f),
                PaperDollSlot.CloakBackRight, r.layers).transform;

            GameObject pelvis = Bone(parent, "Pelvis", new Vector3(0f, -0.12f, 0f),
                PaperDollSlot.Pelvis, r.layers);

            r.thighLeft = Bone(parent, "ThighLeft", new Vector3(-0.13f, -0.23f, 0f),
                PaperDollSlot.ThighLeft, r.layers).transform;
            r.shinLeft = Bone(r.thighLeft, "ShinLeft", new Vector3(0f, -0.28f, 0f),
                PaperDollSlot.ShinLeft, r.layers).transform;
            Bone(r.shinLeft, "BootLeft", new Vector3(0f, -0.23f, 0f),
                PaperDollSlot.BootLeft, r.layers);

            r.thighRight = Bone(parent, "ThighRight", new Vector3(0.13f, -0.23f, 0f),
                PaperDollSlot.ThighRight, r.layers).transform;
            r.shinRight = Bone(r.thighRight, "ShinRight", new Vector3(0f, -0.28f, 0f),
                PaperDollSlot.ShinRight, r.layers).transform;
            Bone(r.shinRight, "BootRight", new Vector3(0f, -0.23f, 0f),
                PaperDollSlot.BootRight, r.layers);

            r.torso = Bone(parent, "Torso", new Vector3(0f, 0.22f, 0f),
                PaperDollSlot.Torso, r.layers).transform;

            r.upperArmLeft = Bone(r.torso, "UpperArmLeft", new Vector3(-0.27f, 0.11f, 0f),
                PaperDollSlot.UpperArmLeft, r.layers).transform;
            r.forearmLeft = Bone(r.upperArmLeft, "ForearmLeft", new Vector3(-0.04f, -0.25f, 0f),
                PaperDollSlot.ForearmLeft, r.layers).transform;
            GameObject leftHand = Bone(r.forearmLeft, "HandLeft", new Vector3(0f, -0.20f, 0f),
                PaperDollSlot.HandLeft, r.layers);
            r.castingHand = leftHand.transform;

            r.upperArmRight = Bone(r.torso, "UpperArmRight", new Vector3(0.27f, 0.11f, 0f),
                PaperDollSlot.UpperArmRight, r.layers).transform;
            r.forearmRight = Bone(r.upperArmRight, "ForearmRight", new Vector3(0.04f, -0.25f, 0f),
                PaperDollSlot.ForearmRight, r.layers).transform;
            Transform rightHand = Bone(r.forearmRight, "HandRight", new Vector3(0f, -0.20f, 0f),
                PaperDollSlot.HandRight, r.layers).transform;

            GameObject weapon = new GameObject("WeaponSocket");
            weapon.transform.SetParent(rightHand, false);
            weapon.transform.localPosition = new Vector3(0.02f, -0.03f, 0f);
            r.weaponSocket = weapon.transform;

            GameObject weaponVisual = new GameObject("WeaponVisual");
            weaponVisual.transform.SetParent(weapon.transform, false);
            // The generated weapon art is centered in its source texture, while the
            // actual rotation pivot must sit on the grip in Edward's hand.
            weaponVisual.transform.localPosition = new Vector3(0f, 0.34f, 0f);
            r.weaponRenderer = weaponVisual.AddComponent<SpriteRenderer>();
            r.weaponRenderer.sortingOrder = 35;

            r.head = Bone(r.torso, "Head", new Vector3(0f, 0.42f, 0f),
                PaperDollSlot.Head, r.layers).transform;
            Bone(r.head, "HairBack", Vector3.zero, PaperDollSlot.HairBack, r.layers);
            GameObject eyes = Bone(r.head, "Eyes", Vector3.zero, PaperDollSlot.Eyes, r.layers);
            r.eyes = eyes.GetComponent<SpriteRenderer>();
            Bone(r.head, "HairFront", Vector3.zero, PaperDollSlot.HairFront, r.layers);

            Bone(r.torso, "Belt", new Vector3(0f, -0.20f, 0f),
                PaperDollSlot.BeltAccessory, r.layers);
            Bone(r.torso, "CloakClasp", new Vector3(0f, 0.20f, 0f),
                PaperDollSlot.ShoulderAccessory, r.layers);
            Bone(parent, "CloakFrontLeft", new Vector3(-0.14f, -0.05f, 0f),
                PaperDollSlot.CloakFrontLeft, r.layers);
            Bone(parent, "CloakFrontRight", new Vector3(0.14f, -0.05f, 0f),
                PaperDollSlot.CloakFrontRight, r.layers);

            return r;
        }

        private static GameObject Bone(
            Transform parent,
            string name,
            Vector3 localPosition,
            PaperDollSlot slot,
            List<PaperDollLayer> layers)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();

            layers.Add(new PaperDollLayer
            {
                slot = slot,
                renderer = renderer
            });

            return go;
        }

        private static PaperDollSlotSprites Slot(
            PaperDollSlot slot,
            string id,
            Action<CelSpritePainter, FacingDirection8> draw)
        {
            return new PaperDollSlotSprites
            {
                slot = slot,
                sprites = BuildDirectionalSet(id, draw)
            };
        }

        private static DirectionalSpriteSet8 BuildDirectionalSet(
            string id,
            Action<CelSpritePainter, FacingDirection8> draw)
        {
            DirectionalSpriteSet8 set = new DirectionalSpriteSet8();
            foreach (FacingDirection8 direction in AuthoredDirections)
            {
                CelSpritePainter canvas = new CelSpritePainter(192, 192);
                draw(canvas, direction);

                string path = SpriteRoot + "/" + id + "_" + direction.ToString().ToLowerInvariant() + ".png";
                Sprite sprite = canvas.SaveAsSprite(path, 128f, new Vector2(0.5f, 0.5f));

                switch (direction)
                {
                    case FacingDirection8.North: set.north = sprite; break;
                    case FacingDirection8.NorthEast: set.northEast = sprite; break;
                    case FacingDirection8.East: set.east = sprite; break;
                    case FacingDirection8.SouthEast: set.southEast = sprite; break;
                    case FacingDirection8.South: set.south = sprite; break;
                }
            }

            set.mirrorMissingWestDirections = true;
            return set;
        }

        private static void DrawHead(CelSpritePainter c, FacingDirection8 d)
        {
            float x = d == FacingDirection8.East ? 105f :
                d == FacingDirection8.NorthEast || d == FacingDirection8.SouthEast ? 101f : 96f;

            c.Ellipse(new Vector2(x, 93f), new Vector2(31f, 36f), Outline);
            c.Ellipse(new Vector2(x, 94f), new Vector2(27f, 32f), Skin);
            if (d != FacingDirection8.North)
                c.Ellipse(new Vector2(x - 8f, 102f), new Vector2(15f, 12f), SkinLight);
            c.Polygon(SkinShadow,
                new Vector2(x + 17f, 73f), new Vector2(x + 26f, 88f),
                new Vector2(x + 21f, 109f), new Vector2(x + 11f, 94f));
        }

        private static void DrawEyes(CelSpritePainter c, FacingDirection8 d)
        {
            if (d == FacingDirection8.North)
                return;

            float shift = d == FacingDirection8.East ? 10f :
                d == FacingDirection8.NorthEast || d == FacingDirection8.SouthEast ? 5f : 0f;

            if (d != FacingDirection8.East)
            {
                c.Line(new Vector2(78f + shift, 101f), new Vector2(88f + shift, 99f), 2.2f, Outline);
                c.Ellipse(new Vector2(84f + shift, 99f), new Vector2(2.5f, 3f), Eye);
            }

            c.Line(new Vector2(100f + shift, 99f), new Vector2(111f + shift, 101f), 2.2f, Outline);
            c.Ellipse(new Vector2(104f + shift, 99f), new Vector2(2.5f, 3f), Eye);
        }

        private static void DrawHairBack(CelSpritePainter c, FacingDirection8 d)
        {
            c.Ellipse(new Vector2(96f, 101f), new Vector2(35f, 38f), Outline);
            c.Ellipse(new Vector2(96f, 104f), new Vector2(31f, 34f), Hair);
            c.Polygon(Hair,
                new Vector2(66f, 98f), new Vector2(73f, 58f), new Vector2(84f, 78f),
                new Vector2(94f, 52f), new Vector2(104f, 78f), new Vector2(119f, 55f),
                new Vector2(126f, 103f));
            c.Polygon(HairLight,
                new Vector2(72f, 114f), new Vector2(89f, 130f),
                new Vector2(82f, 103f), new Vector2(69f, 92f));
        }

        private static void DrawHairFront(CelSpritePainter c, FacingDirection8 d)
        {
            c.Polygon(Outline,
                new Vector2(63f, 116f), new Vector2(76f, 139f), new Vector2(92f, 128f),
                new Vector2(103f, 143f), new Vector2(114f, 126f), new Vector2(131f, 117f),
                new Vector2(118f, 104f), new Vector2(70f, 103f));
            c.Polygon(Hair,
                new Vector2(67f, 117f), new Vector2(78f, 134f), new Vector2(92f, 124f),
                new Vector2(103f, 138f), new Vector2(113f, 122f), new Vector2(126f, 116f),
                new Vector2(115f, 108f), new Vector2(72f, 107f));
            c.Line(new Vector2(83f, 128f), new Vector2(75f, 108f), 2f, HairLight);
            c.Line(new Vector2(105f, 133f), new Vector2(98f, 108f), 2f, HairLight);
        }

        private static void DrawTorso(CelSpritePainter c, FacingDirection8 d)
        {
            c.Polygon(Outline,
                new Vector2(56f, 55f), new Vector2(72f, 139f),
                new Vector2(120f, 139f), new Vector2(136f, 55f),
                new Vector2(118f, 38f), new Vector2(74f, 38f));
            c.Polygon(Cloth,
                new Vector2(61f, 57f), new Vector2(76f, 134f),
                new Vector2(116f, 134f), new Vector2(131f, 57f),
                new Vector2(115f, 43f), new Vector2(77f, 43f));
            c.Polygon(ClothLight,
                new Vector2(67f, 65f), new Vector2(78f, 129f),
                new Vector2(89f, 129f), new Vector2(83f, 58f));
            c.Polygon(ClothShadow,
                new Vector2(111f, 43f), new Vector2(127f, 59f),
                new Vector2(115f, 133f), new Vector2(103f, 133f));
            c.Line(new Vector2(75f, 111f), new Vector2(118f, 62f), 5f, Leather);
        }

        private static void DrawArmorTorso(CelSpritePainter c, FacingDirection8 d)
        {
            DrawTorso(c, d);
            c.Polygon(Outline,
                new Vector2(66f, 72f), new Vector2(77f, 128f),
                new Vector2(116f, 128f), new Vector2(126f, 72f),
                new Vector2(113f, 56f), new Vector2(80f, 56f));
            c.Polygon(Leather,
                new Vector2(71f, 74f), new Vector2(81f, 123f),
                new Vector2(112f, 123f), new Vector2(121f, 74f),
                new Vector2(110f, 62f), new Vector2(83f, 62f));
            c.Line(new Vector2(79f, 111f), new Vector2(115f, 78f), 5f, LeatherLight);
            c.Line(new Vector2(80f, 88f), new Vector2(116f, 88f), 3f, Metal);
        }

        private static void DrawUpperArm(CelSpritePainter c, FacingDirection8 d, bool right, bool armored)
        {
            Color main = armored ? Leather : Cloth;
            Color light = armored ? LeatherLight : ClothLight;
            c.Polygon(Outline,
                new Vector2(76f, 58f), new Vector2(113f, 57f),
                new Vector2(119f, 135f), new Vector2(73f, 135f));
            c.Polygon(main,
                new Vector2(81f, 63f), new Vector2(108f, 62f),
                new Vector2(113f, 130f), new Vector2(78f, 130f));
            c.Polygon(light,
                new Vector2(82f, 66f), new Vector2(91f, 65f),
                new Vector2(91f, 126f), new Vector2(80f, 126f));
        }

        private static void DrawForearm(CelSpritePainter c, FacingDirection8 d, bool right, bool armored)
        {
            c.Polygon(Outline,
                new Vector2(79f, 58f), new Vector2(111f, 58f),
                new Vector2(116f, 136f), new Vector2(75f, 136f));
            c.Polygon(Cloth,
                new Vector2(84f, 63f), new Vector2(106f, 63f),
                new Vector2(110f, 131f), new Vector2(81f, 131f));
            c.Rect(80, 93, 31, 16, Leather);
            c.Line(new Vector2(82f, 98f), new Vector2(109f, 104f), 2f, LeatherLight);
        }

        private static void DrawHand(CelSpritePainter c, FacingDirection8 d)
        {
            c.Ellipse(new Vector2(96f, 96f), new Vector2(20f, 24f), Outline);
            c.Ellipse(new Vector2(96f, 96f), new Vector2(16f, 20f), Leather);
            c.Line(new Vector2(84f, 101f), new Vector2(106f, 88f), 2f, LeatherLight);
        }

        private static void DrawPelvis(CelSpritePainter c, FacingDirection8 d)
        {
            c.Polygon(Outline,
                new Vector2(67f, 75f), new Vector2(125f, 75f),
                new Vector2(117f, 119f), new Vector2(75f, 119f));
            c.Polygon(ClothShadow,
                new Vector2(73f, 80f), new Vector2(119f, 80f),
                new Vector2(112f, 114f), new Vector2(80f, 114f));
        }

        private static void DrawThigh(CelSpritePainter c, FacingDirection8 d)
        {
            c.Polygon(Outline,
                new Vector2(75f, 52f), new Vector2(113f, 52f),
                new Vector2(118f, 140f), new Vector2(70f, 140f));
            c.Polygon(Cloth,
                new Vector2(80f, 57f), new Vector2(108f, 57f),
                new Vector2(112f, 135f), new Vector2(76f, 135f));
            c.Polygon(ClothLight,
                new Vector2(80f, 62f), new Vector2(89f, 60f),
                new Vector2(88f, 132f), new Vector2(77f, 132f));
        }

        private static void DrawShin(CelSpritePainter c, FacingDirection8 d)
        {
            c.Polygon(Outline,
                new Vector2(78f, 49f), new Vector2(111f, 49f),
                new Vector2(114f, 142f), new Vector2(73f, 142f));
            c.Polygon(ClothShadow,
                new Vector2(83f, 54f), new Vector2(106f, 54f),
                new Vector2(109f, 137f), new Vector2(78f, 137f));
        }

        private static void DrawBoot(CelSpritePainter c, FacingDirection8 d)
        {
            c.Polygon(Outline,
                new Vector2(78f, 61f), new Vector2(110f, 61f),
                new Vector2(118f, 115f), new Vector2(133f, 128f),
                new Vector2(126f, 140f), new Vector2(72f, 137f));
            c.Polygon(Leather,
                new Vector2(83f, 66f), new Vector2(105f, 66f),
                new Vector2(112f, 118f), new Vector2(125f, 129f),
                new Vector2(121f, 134f), new Vector2(78f, 132f));
            c.Line(new Vector2(84f, 105f), new Vector2(111f, 105f), 3f, LeatherLight);
        }

        private static void DrawBelt(CelSpritePainter c, FacingDirection8 d)
        {
            c.Rect(56, 89, 80, 15, Outline);
            c.Rect(61, 93, 70, 8, Leather);
            c.Rect(91, 88, 12, 17, Metal);
            c.Rect(94, 91, 6, 11, Outline);
        }

        private static void DrawCloakPanel(CelSpritePainter c, FacingDirection8 d, int panel)
        {
            float center = 96f + panel * 11f;
            c.Polygon(Outline,
                new Vector2(center - 22f, 139f), new Vector2(center + 22f, 139f),
                new Vector2(center + 29f, 52f), new Vector2(center + 9f, 36f),
                new Vector2(center - 3f, 47f), new Vector2(center - 20f, 35f),
                new Vector2(center - 29f, 62f));
            c.Polygon(ClothShadow,
                new Vector2(center - 17f, 134f), new Vector2(center + 17f, 134f),
                new Vector2(center + 23f, 58f), new Vector2(center + 7f, 43f),
                new Vector2(center - 2f, 53f), new Vector2(center - 17f, 43f),
                new Vector2(center - 23f, 64f));
            c.Line(new Vector2(center - 11f, 126f), new Vector2(center - 3f, 55f), 3f, ClothLight);
        }

        private static void DrawCloakFront(CelSpritePainter c, FacingDirection8 d, bool right)
        {
            float sign = right ? 1f : -1f;
            float x = 96f + sign * 12f;
            c.Polygon(Outline,
                new Vector2(x - 18f, 137f), new Vector2(x + 17f, 137f),
                new Vector2(x + sign * 25f, 54f), new Vector2(x - sign * 3f, 65f));
            c.Polygon(Cloth,
                new Vector2(x - 13f, 132f), new Vector2(x + 12f, 132f),
                new Vector2(x + sign * 19f, 61f), new Vector2(x - sign * 1f, 69f));
        }

        private static void DrawCloakClasp(CelSpritePainter c, FacingDirection8 d)
        {
            c.Ring(new Vector2(96f, 96f), new Vector2(17f, 17f), 5f, Metal);
            c.Ellipse(new Vector2(96f, 96f), new Vector2(5f, 5f), MetalLight);
        }

        private static void DrawSword(CelSpritePainter c, FacingDirection8 d, bool heavy)
        {
            float halfWidth = heavy ? 8f : 5f;
            float tipY = heavy ? 171f : 163f;
            float guardY = heavy ? 58f : 64f;

            c.Polygon(Outline,
                new Vector2(96f - halfWidth - 3f, guardY),
                new Vector2(96f - halfWidth - 2f, tipY - 10f),
                new Vector2(96f, tipY),
                new Vector2(96f + halfWidth + 2f, tipY - 10f),
                new Vector2(96f + halfWidth + 3f, guardY));
            c.Polygon(Metal,
                new Vector2(96f - halfWidth, guardY + 4f),
                new Vector2(96f - halfWidth + 1f, tipY - 12f),
                new Vector2(96f, tipY - 4f),
                new Vector2(96f + halfWidth - 1f, tipY - 12f),
                new Vector2(96f + halfWidth, guardY + 4f));
            c.Line(new Vector2(96f, guardY + 8f), new Vector2(96f, tipY - 10f), 2f, MetalLight);
            c.Line(new Vector2(70f, guardY), new Vector2(122f, guardY), heavy ? 9f : 7f, Outline);
            c.Line(new Vector2(74f, guardY), new Vector2(118f, guardY), heavy ? 4f : 3f, Metal);
            c.Rect(91, 26, 10, (int)(guardY - 26f), Outline);
            c.Rect(94, 29, 4, (int)(guardY - 32f), Leather);
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
