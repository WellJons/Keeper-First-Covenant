#if UNITY_EDITOR
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Inventory;
using KeeperFirstCovenant.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class FirstCovenantPrototypeBuilder
    {
        private const string GeneratedRoot = "Assets/KeeperFirstCovenant/Generated";
        private const string ScenePath = "Assets/KeeperFirstCovenant/Scenes/Prototype_Road.unity";

        [MenuItem("Keeper First Covenant/Build Prototype Road Scene")]
        public static void Build()
        {
            EnsureProjectFolders();

            Material grass = GetMaterial("Grass", new Color(0.16f, 0.23f, 0.13f), 0f, 0.15f);
            Material road = GetMaterial("Road", new Color(0.28f, 0.22f, 0.16f), 0f, 0.1f);
            Material bark = GetMaterial("Bark", new Color(0.20f, 0.12f, 0.07f), 0f, 0.1f);
            Material leaves = GetMaterial("Leaves", new Color(0.12f, 0.25f, 0.12f), 0f, 0.12f);
            Material stone = GetMaterial("OldStone", new Color(0.31f, 0.33f, 0.34f), 0f, 0.2f);
            Material silver = GetMaterial("AncientSilver", new Color(0.60f, 0.72f, 0.78f), 0.65f, 0.75f);
            Material black = GetMaterial("RestraintBlack", new Color(0.035f, 0.04f, 0.045f), 0.45f, 0.35f);
            Material white = GetMaterial("White", new Color(0.90f, 0.90f, 0.84f), 0f, 0.25f);
            Material edCloth = GetMaterial("EdwardTravelCloth", new Color(0.24f, 0.16f, 0.12f), 0f, 0.18f);
            Material lucianCloth = GetMaterial("LucianTravelCloth", new Color(0.14f, 0.19f, 0.26f), 0f, 0.28f);
            Material banditCloth = GetMaterial("BanditCloth", new Color(0.22f, 0.20f, 0.18f), 0f, 0.12f);
            Material fire = GetMaterial("FireProxy", new Color(0.95f, 0.32f, 0.08f), 0f, 0.35f);

            CombatActionDefinition swordSlash = GetAction(
                "SwordSlash",
                "Sword Slash",
                CombatActionCategory.Melee,
                TargetKind.Enemy,
                1,
                0,
                1.9f,
                new DiceFormula(1, 8, 0),
                DamageType.Physical,
                AbilityAttribute.Strength);

            CombatActionDefinition fireSpark = GetAction(
                "FireSpark",
                "Fire Spark",
                CombatActionCategory.Spell,
                TargetKind.Enemy,
                1,
                2,
                10f,
                new DiceFormula(2, 4, 0),
                DamageType.Fire,
                AbilityAttribute.Intellect);

            fireSpark.createsSurface = SurfaceType.Fire;
            fireSpark.surfaceRadius = 1.5f;
            fireSpark.surfaceDurationTurns = 2;
            EditorUtility.SetDirty(fireSpark);

            CombatActionDefinition sealBolt = GetAction(
                "SealBolt",
                "Seal Bolt",
                CombatActionCategory.Spell,
                TargetKind.Enemy,
                1,
                2,
                11f,
                new DiceFormula(1, 6, 1),
                DamageType.Arcane,
                AbilityAttribute.Intellect);

            CharacterDefinition edDef = GetCharacter(
                "edward",
                "Edward",
                CombatFaction.Player,
                58,
                16,
                13,
                11,
                11,
                10,
                12,
                new[] { swordSlash, fireSpark });

            CharacterDefinition lucianDef = GetCharacter(
                "lucian",
                "Lucian",
                CombatFaction.Ally,
                42,
                30,
                9,
                12,
                16,
                13,
                15,
                new[] { sealBolt });

            CharacterDefinition banditDef = GetCharacter(
                "road_bandit",
                "Road Bandit",
                CombatFaction.Enemy,
                36,
                0,
                12,
                11,
                8,
                9,
                10,
                new[] { swordSlash });

            WeaponDefinition travelSword = GetWeapon(
                "travel_sword",
                "Travel Sword",
                WeaponClass.Sword,
                new DiceFormula(1, 8),
                18,
                2.4f);

            ItemDefinition silverCoins = GetItem(
                "silver_coins",
                "Silver Coins",
                ItemCategory.Treasure,
                true,
                50,
                1,
                0.01f);

            ItemDefinition blackFragment = GetItem(
                "black_ring_fragment",
                "Black Ring Fragment",
                ItemCategory.Miscellaneous,
                false,
                1,
                0,
                0.2f);

            blackFragment.illegalOrSuspicious = true;
            blackFragment.description = "A cold black fragment with unnaturally clean edges. Its purpose is unknown.";
            EditorUtility.SetDirty(blackFragment);

            LootTableDefinition roadsideLoot = GetOrCreateAsset<LootTableDefinition>(
                GeneratedRoot + "/Data/RoadsideLoot.asset");

            roadsideLoot.entries = new[]
            {
                new LootEntry
                {
                    item = silverCoins,
                    chance = 1f,
                    minAmount = 4,
                    maxAmount = 12,
                    hidden = false,
                    requiredPerception = 0
                },
                new LootEntry
                {
                    item = blackFragment,
                    chance = 1f,
                    minAmount = 1,
                    maxAmount = 1,
                    hidden = true,
                    requiredPerception = 13
                }
            };
            EditorUtility.SetDirty(roadsideLoot);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject("Keeper_First_Covenant_Prototype");
            GameObject environment = new GameObject("Environment");
            environment.transform.SetParent(root.transform);

            CreatePrimitive("Ground", PrimitiveType.Plane, Vector3.zero, new Vector3(4f, 1f, 4f), grass, environment.transform);
            CreatePrimitive("Road", PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(3.4f, 0.12f, 34f), road, environment.transform);

            for (int i = 0; i < 8; i++)
            {
                float z = -13f + i * 4f;
                CreateTree(new Vector3(-6.5f - (i % 2) * 1.4f, 0f, z), bark, leaves, environment.transform);
                CreateTree(new Vector3(6.4f + ((i + 1) % 2) * 1.2f, 0f, z + 1.4f), bark, leaves, environment.transform);
            }

            CreateCamp(new Vector3(-5.2f, 0f, -3.5f), bark, fire, environment.transform);
            CreateAncientShrine(new Vector3(6.2f, 0f, 7.0f), stone, silver, black, environment.transform);

            GameObject actors = new GameObject("Actors");
            actors.transform.SetParent(root.transform);

            GameObject ed = CreateCharacter("Edward", new Vector3(-1.5f, 1f, -5f), edCloth, silver, actors.transform);
            CombatantRuntime edRuntime = ed.AddComponent<CombatantRuntime>();
            edRuntime.SetDefinition(edDef);
            InventoryComponent edInventory = ed.AddComponent<InventoryComponent>();
            edInventory.Add(travelSword, 1);
            EditorUtility.SetDirty(edRuntime);
            EditorUtility.SetDirty(edInventory);

            GameObject lucian = CreateCharacter("Lucian", new Vector3(1.2f, 1f, -4.4f), lucianCloth, silver, actors.transform);
            CombatantRuntime lucianRuntime = lucian.AddComponent<CombatantRuntime>();
            lucianRuntime.SetDefinition(lucianDef);
            CreateStaff(lucian.transform, silver);
            EditorUtility.SetDirty(lucianRuntime);

            GameObject whiteGoat = CreateGoat(new Vector3(-0.2f, 0f, -3.0f), white, black, actors.transform);
            CreateLabel(whiteGoat.transform, "White — unnamed", new Vector3(0f, 1.8f, 0f));

            GameObject bandit = CreateCharacter("Road Bandit", new Vector3(1.8f, 1f, 6f), banditCloth, stone, actors.transform);
            CombatantRuntime banditRuntime = bandit.AddComponent<CombatantRuntime>();
            banditRuntime.SetDefinition(banditDef);
            EditorUtility.SetDirty(banditRuntime);

            GameObject hiddenCache = CreatePrimitive(
                "Hidden Roadside Cache",
                PrimitiveType.Cube,
                new Vector3(7.7f, 0.45f, 8.5f),
                new Vector3(1.2f, 0.9f, 0.9f),
                bark,
                environment.transform);

            SearchableLoot searchable = hiddenCache.AddComponent<SearchableLoot>();
            searchable.Configure(roadsideLoot, "Search cache", true);
            EditorUtility.SetDirty(searchable);
            CreateLabel(hiddenCache.transform, "Perception can reveal hidden loot", new Vector3(0f, 1.1f, 0f));

            GameObject systems = new GameObject("GameSystems");
            systems.transform.SetParent(root.transform);
            systems.AddComponent<WorldState>();
            systems.AddComponent<TurnCombatDirector>();

            GameObject gridObject = new GameObject("TacticalGrid3D");
            gridObject.transform.SetParent(systems.transform);
            gridObject.AddComponent<TacticalGrid3D>();

            CreateLighting(root.transform);
            CreateCamera(root.transform);

            CreateLabel(ed.transform, "Edward", new Vector3(0f, 1.6f, 0f));
            CreateLabel(lucian.transform, "Lucian", new Vector3(0f, 1.6f, 0f));
            CreateLabel(bandit.transform, "Road Bandit", new Vector3(0f, 1.6f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = root;
            Debug.Log("Keeper: First Covenant prototype scene built at " + ScenePath);
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "KeeperFirstCovenant");
            EnsureFolder("Assets/KeeperFirstCovenant", "Scenes");
            EnsureFolder("Assets/KeeperFirstCovenant", "Generated");
            EnsureFolder(GeneratedRoot, "Materials");
            EnsureFolder(GeneratedRoot, "Data");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static Material GetMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = GeneratedRoot + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);

            EditorUtility.SetDirty(material);
            return material;
        }

        private static CombatActionDefinition GetAction(
            string id,
            string displayName,
            CombatActionCategory category,
            TargetKind target,
            int ap,
            int mana,
            float range,
            DiceFormula damage,
            DamageType damageType,
            AbilityAttribute scaling)
        {
            CombatActionDefinition action = GetOrCreateAsset<CombatActionDefinition>(
                GeneratedRoot + "/Data/Action_" + id + ".asset");

            action.actionId = id;
            action.displayName = displayName;
            action.category = category;
            action.targetKind = target;
            action.actionPointCost = ap;
            action.manaCost = mana;
            action.rangeMeters = range;
            action.damage = damage;
            action.damageType = damageType;
            action.scalingAttribute = scaling;
            action.scalingMultiplier = 1f;
            action.baseHitChance = 78;
            action.requiresAttackRoll = true;
            EditorUtility.SetDirty(action);
            return action;
        }

        private static CharacterDefinition GetCharacter(
            string id,
            string displayName,
            CombatFaction faction,
            int health,
            int mana,
            int strength,
            int finesse,
            int intellect,
            int willpower,
            int perception,
            CombatActionDefinition[] actions)
        {
            CharacterDefinition character = GetOrCreateAsset<CharacterDefinition>(
                GeneratedRoot + "/Data/Character_" + id + ".asset");

            character.characterId = id;
            character.displayName = displayName;
            character.faction = faction;
            character.maxHealth = health;
            character.maxMana = mana;
            character.strength = strength;
            character.finesse = finesse;
            character.intellect = intellect;
            character.willpower = willpower;
            character.perception = perception;
            character.actionPoints = 2;
            character.movementMeters = 9f;
            character.startingActions = actions;
            EditorUtility.SetDirty(character);
            return character;
        }

        private static WeaponDefinition GetWeapon(
            string id,
            string displayName,
            WeaponClass weaponClass,
            DiceFormula damage,
            int valueSilver,
            float weight)
        {
            WeaponDefinition weapon = GetOrCreateAsset<WeaponDefinition>(
                GeneratedRoot + "/Data/Weapon_" + id + ".asset");

            weapon.itemId = id;
            weapon.displayName = displayName;
            weapon.category = ItemCategory.Weapon;
            weapon.weaponClass = weaponClass;
            weapon.damage = damage;
            weapon.damageType = DamageType.Physical;
            weapon.scalingAttribute = AbilityAttribute.Strength;
            weapon.valueSilver = valueSilver;
            weapon.weight = weight;
            weapon.stackable = false;
            weapon.maxStack = 1;
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static ItemDefinition GetItem(
            string id,
            string displayName,
            ItemCategory category,
            bool stackable,
            int maxStack,
            int valueSilver,
            float weight)
        {
            ItemDefinition item = GetOrCreateAsset<ItemDefinition>(
                GeneratedRoot + "/Data/Item_" + id + ".asset");

            item.itemId = id;
            item.displayName = displayName;
            item.category = category;
            item.stackable = stackable;
            item.maxStack = Mathf.Max(1, maxStack);
            item.valueSilver = valueSilver;
            item.weight = weight;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static GameObject CreatePrimitive(
            string name,
            PrimitiveType type,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            return go;
        }

        private static void CreateTree(Vector3 position, Material bark, Material leaves, Transform parent)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(parent);
            tree.transform.position = position;

            CreatePrimitive("Trunk", PrimitiveType.Cylinder, position + Vector3.up * 1.4f, new Vector3(0.35f, 1.4f, 0.35f), bark, tree.transform);
            CreatePrimitive("CrownA", PrimitiveType.Sphere, position + Vector3.up * 3.3f, new Vector3(1.5f, 1.3f, 1.5f), leaves, tree.transform);
            CreatePrimitive("CrownB", PrimitiveType.Sphere, position + new Vector3(0.6f, 3.0f, 0.3f), new Vector3(1.0f, 1.0f, 1.0f), leaves, tree.transform);
        }

        private static void CreateCamp(Vector3 position, Material bark, Material fire, Transform parent)
        {
            GameObject camp = new GameObject("Roadside Camp");
            camp.transform.SetParent(parent);

            GameObject logA = CreatePrimitive("LogA", PrimitiveType.Cylinder, position + Vector3.up * 0.18f, new Vector3(0.16f, 0.9f, 0.16f), bark, camp.transform);
            logA.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            GameObject logB = CreatePrimitive("LogB", PrimitiveType.Cylinder, position + Vector3.up * 0.20f, new Vector3(0.16f, 0.9f, 0.16f), bark, camp.transform);
            logB.transform.rotation = Quaternion.Euler(90f, 0f, 90f);

            CreatePrimitive("Fire", PrimitiveType.Sphere, position + Vector3.up * 0.42f, new Vector3(0.38f, 0.55f, 0.38f), fire, camp.transform);

            GameObject lightObject = new GameObject("Campfire Light");
            lightObject.transform.SetParent(camp.transform);
            lightObject.transform.position = position + Vector3.up * 1.2f;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 3.2f;
            light.color = new Color(1f, 0.45f, 0.18f);
        }

        private static void CreateAncientShrine(
            Vector3 position,
            Material stone,
            Material silver,
            Material black,
            Transform parent)
        {
            GameObject shrine = new GameObject("Ancient Road Shrine");
            shrine.transform.SetParent(parent);

            CreatePrimitive("Base", PrimitiveType.Cube, position + Vector3.up * 0.35f, new Vector3(3.2f, 0.7f, 2.4f), stone, shrine.transform);
            CreatePrimitive("PillarLeft", PrimitiveType.Cube, position + new Vector3(-1.15f, 1.8f, 0f), new Vector3(0.55f, 3.0f, 0.7f), stone, shrine.transform);
            CreatePrimitive("PillarRight", PrimitiveType.Cube, position + new Vector3(1.15f, 1.8f, 0f), new Vector3(0.55f, 3.0f, 0.7f), stone, shrine.transform);
            CreatePrimitive("Lintel", PrimitiveType.Cube, position + new Vector3(0f, 3.25f, 0f), new Vector3(2.9f, 0.45f, 0.75f), stone, shrine.transform);

            CreateSegmentRing("Silver Covenant Ring", position + new Vector3(0f, 2.0f, -0.42f), 0.82f, 14, silver, shrine.transform, 360f);
            CreateSegmentRing("Broken Black Ring", position + new Vector3(0f, 2.0f, -0.52f), 1.12f, 10, black, shrine.transform, 275f);

            CreateLabel(shrine.transform, "Ancient geometry: silver bond / broken black restraint", new Vector3(0f, 4.1f, 0f));
        }

        private static void CreateSegmentRing(
            string name,
            Vector3 center,
            float radius,
            int segments,
            Material material,
            Transform parent,
            float arcDegrees)
        {
            GameObject ring = new GameObject(name);
            ring.transform.SetParent(parent);

            for (int i = 0; i < segments; i++)
            {
                float t = segments <= 1 ? 0f : i / (float)segments;
                float angle = t * arcDegrees;
                float radians = angle * Mathf.Deg2Rad;

                Vector3 local = new Vector3(
                    Mathf.Cos(radians) * radius,
                    Mathf.Sin(radians) * radius,
                    0f);

                GameObject segment = CreatePrimitive(
                    "Segment",
                    PrimitiveType.Cube,
                    center + local,
                    new Vector3(0.30f, 0.10f, 0.10f),
                    material,
                    ring.transform);

                segment.transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
            }
        }

        private static GameObject CreateCharacter(
            string name,
            Vector3 position,
            Material clothing,
            Material metal,
            Transform parent)
        {
            GameObject actor = new GameObject(name);
            actor.transform.SetParent(parent);
            actor.transform.position = position;

            CreatePrimitive("Body", PrimitiveType.Capsule, position, new Vector3(0.72f, 1.0f, 0.72f), clothing, actor.transform);
            CreatePrimitive("Head", PrimitiveType.Sphere, position + Vector3.up * 1.35f, new Vector3(0.48f, 0.48f, 0.48f), clothing, actor.transform);

            if (name == "Edward")
            {
                GameObject sword = CreatePrimitive(
                    "Travel Sword",
                    PrimitiveType.Cube,
                    position + new Vector3(0.62f, 0.35f, 0f),
                    new Vector3(0.10f, 1.25f, 0.08f),
                    metal,
                    actor.transform);
                sword.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
            }

            return actor;
        }

        private static void CreateStaff(Transform owner, Material material)
        {
            GameObject staff = CreatePrimitive(
                "Lucian Staff",
                PrimitiveType.Cylinder,
                owner.position + new Vector3(0.65f, 0.15f, 0f),
                new Vector3(0.08f, 1.3f, 0.08f),
                material,
                owner);

            staff.transform.rotation = Quaternion.Euler(0f, 0f, 6f);
        }

        private static GameObject CreateGoat(
            Vector3 position,
            Material white,
            Material black,
            Transform parent)
        {
            GameObject goat = new GameObject("White Goat");
            goat.transform.SetParent(parent);
            goat.transform.position = position;

            CreatePrimitive("Body", PrimitiveType.Capsule, position + new Vector3(0f, 0.78f, 0f), new Vector3(0.55f, 0.75f, 0.75f), white, goat.transform);
            CreatePrimitive("Head", PrimitiveType.Sphere, position + new Vector3(0f, 1.08f, 0.72f), new Vector3(0.48f, 0.48f, 0.56f), white, goat.transform);

            Vector3[] legOffsets =
            {
                new Vector3(-0.33f, 0.35f, -0.38f),
                new Vector3(0.33f, 0.35f, -0.38f),
                new Vector3(-0.33f, 0.35f, 0.38f),
                new Vector3(0.33f, 0.35f, 0.38f)
            };

            foreach (Vector3 offset in legOffsets)
                CreatePrimitive("Leg", PrimitiveType.Cylinder, position + offset, new Vector3(0.09f, 0.35f, 0.09f), white, goat.transform);

            for (int i = 0; i < 10; i++)
            {
                float angle = i * 36f;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 collarPoint = position + new Vector3(
                    Mathf.Cos(radians) * 0.37f,
                    0.92f,
                    0.38f + Mathf.Sin(radians) * 0.22f);

                GameObject segment = CreatePrimitive(
                    "Ancient Collar Segment",
                    PrimitiveType.Cube,
                    collarPoint,
                    new Vector3(0.18f, 0.07f, 0.08f),
                    black,
                    goat.transform);

                segment.transform.rotation = Quaternion.Euler(0f, -angle, 0f);
            }

            return goat;
        }

        private static void CreateLighting(Transform parent)
        {
            GameObject sunObject = new GameObject("Sun");
            sunObject.transform.SetParent(parent);
            sunObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.25f;
            sun.color = new Color(1f, 0.88f, 0.72f);

            RenderSettings.ambientLight = new Color(0.30f, 0.34f, 0.38f);
        }

        private static void CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(12f, 14f, -16f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 48f;
            cameraObject.transform.LookAt(new Vector3(0f, 0.7f, 0f));
        }

        private static void CreateLabel(Transform parent, string text, Vector3 localOffset)
        {
            GameObject labelObject = new GameObject("Label_" + text);
            labelObject.transform.SetParent(parent);
            labelObject.transform.localPosition = localOffset;

            TextMesh mesh = labelObject.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 32;
            mesh.characterSize = 0.07f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.92f, 0.92f, 0.88f);

            labelObject.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
        }
    }
}
#endif
