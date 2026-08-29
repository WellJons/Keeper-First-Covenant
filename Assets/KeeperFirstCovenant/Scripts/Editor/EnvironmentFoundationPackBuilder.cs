#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Environment;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class EnvironmentFoundationPackBuilder
    {
        public const string ArtRoot =
            "Assets/KeeperFirstCovenant/Art/Environment/Foundation";

        public const string PrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/Environment/Foundation";

        public const string MaterialRoot =
            "Assets/KeeperFirstCovenant/Materials/Environment/Foundation";

        public const string ProfileRoot =
            "Assets/KeeperFirstCovenant/Data/Environment/Foundation/Profiles";

        public const string PhysicsMaterialRoot =
            "Assets/KeeperFirstCovenant/Data/Environment/Foundation/PhysicsMaterials";

        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/EnvironmentFoundation_Test.unity";

        public const string ParticleMaterialPath =
            MaterialRoot + "/Environment_SoftParticle.mat";

        private const float TileSize = 4f;
        private const float PathHalfWidth = 0.24f;

        private const string MeadowTexture =
            ArtRoot + "/Ground/Ground_MeadowGrass_A.png";
        private const string DirtTexture =
            ArtRoot + "/Ground/Ground_WoodlandDirt_A.png";
        private const string StoneTexture =
            ArtRoot + "/Ground/Ground_NaturalStone_A.png";
        private const string DirtRoadTexture =
            ArtRoot + "/Ground/Road_PackedDirt_A.png";
        private const string CobbleRoadTexture =
            ArtRoot + "/Ground/Road_OldCobble_A.png";

        private const string LowGrassTexture =
            ArtRoot + "/Foliage/Grass_LowMeadow_A.png";
        private const string TallGrassTexture =
            ArtRoot + "/Foliage/Grass_TallMeadow_A.png";
        private const string WildflowersTexture =
            ArtRoot + "/Foliage/Grass_Wildflowers_A.png";

        private sealed class Profiles
        {
            public EnvironmentSurfaceProfile Meadow;
            public EnvironmentSurfaceProfile Dirt;
            public EnvironmentSurfaceProfile Stone;
            public EnvironmentSurfaceProfile DirtRoad;
            public EnvironmentSurfaceProfile CobbleRoad;
        }

        private readonly struct GroundSpec
        {
            public readonly string Id;
            public readonly string MaterialName;
            public readonly Texture2D PrimaryTexture;
            public readonly Texture2D SecondaryTexture;
            public readonly EnvironmentSurfaceProfile PrimaryProfile;
            public readonly EnvironmentSurfaceProfile SecondaryProfile;
            public readonly EnvironmentSurfacePattern Pattern;
            public readonly float ShaderMode;

            public GroundSpec(
                string id,
                string materialName,
                Texture2D primaryTexture,
                Texture2D secondaryTexture,
                EnvironmentSurfaceProfile primaryProfile,
                EnvironmentSurfaceProfile secondaryProfile,
                EnvironmentSurfacePattern pattern,
                float shaderMode)
            {
                Id = id;
                MaterialName = materialName;
                PrimaryTexture = primaryTexture;
                SecondaryTexture = secondaryTexture;
                PrimaryProfile = primaryProfile;
                SecondaryProfile = secondaryProfile;
                Pattern = pattern;
                ShaderMode = shaderMode;
            }
        }

        [MenuItem("Keeper First Covenant/Environment Foundation/BUILD COMPLETE PACK")]
        public static void BuildCompletePack()
        {
            BuildCompletePackInternal(true, true);
        }

        [MenuItem("Keeper First Covenant/Environment Foundation/Rebuild Assets Only")]
        public static void BuildAssetsOnly()
        {
            BuildCompletePackInternal(false, true);
        }

        public static void EnsureBuilt(bool buildTestScene)
        {
            if (LoadPrefab("Ground_MeadowGrass_4m") != null &&
                LoadPrefab("Grass_TallMeadow_A") != null)
            {
                if (buildTestScene &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                {
                    BuildDemoScene();
                }

                return;
            }

            BuildCompletePackInternal(buildTestScene, false);
        }

        public static GameObject LoadPrefab(string id)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                PrefabRoot + "/" + id + ".prefab");
        }

        public static Material LoadParticleMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);
        }

        public static void AttachSurfaceEffects(GameObject actor)
        {
            if (actor == null)
                return;

            SurfaceFootstepEmitter emitter = actor.GetComponent<SurfaceFootstepEmitter>();
            if (emitter == null)
                emitter = actor.AddComponent<SurfaceFootstepEmitter>();

            emitter.Configure(LoadParticleMaterial(), ~0);
        }

        public static void BuildFoundationGround(Transform parent)
        {
            GameObject ground = new GameObject("EnvironmentFoundationGround");
            ground.transform.SetParent(parent, false);

            for (int z = -2; z <= 2; z++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    string id = ResolveGroundId(x, z);
                    Quaternion rotation = Quaternion.identity;

                    if (id == "Road_OldCobble_Straight_4m")
                        rotation = Quaternion.Euler(0f, 90f, 0f);

                    InstantiatePrefab(
                        id,
                        new Vector3(x * TileSize, 0f, z * TileSize),
                        rotation,
                        ground.transform);
                }
            }

            GameObject foliage = new GameObject("InteractiveFoliage");
            foliage.transform.SetParent(parent, false);

            PlaceFoliage("Grass_LowMeadow_A", -5.4f, -4.9f, 0.92f);
            PlaceFoliage("Grass_LowMeadow_A", 5.2f, -5.5f, 1.06f);
            PlaceFoliage("Grass_LowMeadow_A", -7.1f, 4.8f, 1.12f);
            PlaceFoliage("Grass_TallMeadow_A", -3.8f, 5.7f, 0.95f);
            PlaceFoliage("Grass_TallMeadow_A", 5.7f, 4.5f, 1.08f);
            PlaceFoliage("Grass_Wildflowers_A", -7.2f, -5.7f, 0.94f);
            PlaceFoliage("Grass_Wildflowers_A", 6.8f, 6.0f, 1.03f);
            PlaceFoliage("Grass_Wildflowers_A", 4.8f, -6.2f, 0.82f);

            void PlaceFoliage(string id, float x, float z, float scale)
            {
                GameObject instance = InstantiatePrefab(
                    id,
                    new Vector3(x, 0f, z),
                    Quaternion.identity,
                    foliage.transform);

                if (instance != null)
                    instance.transform.localScale *= scale;
            }
        }

        private static void BuildCompletePackInternal(
            bool buildTestScene,
            bool logResult)
        {
            EnsureFolders();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ReimportSourceTextures();

            Profiles profiles = BuildProfiles();
            BuildParticleMaterial();
            BuildGroundAssets(profiles);
            BuildFoliageAssets();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (buildTestScene)
                BuildDemoScene();

            bool valid =
                !buildTestScene ||
                EnvironmentFoundationValidator.Validate(false);

            if (logResult)
            {
                Debug.Log(
                    "Environment Foundation pack built: walkable ground, modular roads, " +
                    "interactive foliage, wind, surface physics and test scene. Validation=" +
                    valid);
            }
        }

        private static Profiles BuildProfiles()
        {
            Profiles profiles = new Profiles
            {
                Meadow = GetOrCreateProfile(
                    "Surface_MeadowGrass",
                    "meadow-grass",
                    EnvironmentSurfaceKind.MeadowGrass,
                    0.98f,
                    0.82f,
                    0.64f,
                    0.72f,
                    new Color(0.42f, 0.58f, 0.18f, 0.88f),
                    4,
                    7,
                    new Vector2(0.022f, 0.052f),
                    new Vector2(0.12f, 0.34f)),

                Dirt = GetOrCreateProfile(
                    "Surface_WoodlandDirt",
                    "woodland-dirt",
                    EnvironmentSurfaceKind.WoodlandDirt,
                    0.96f,
                    0.76f,
                    0.58f,
                    0.76f,
                    new Color(0.37f, 0.24f, 0.12f, 0.82f),
                    3,
                    6,
                    new Vector2(0.02f, 0.06f),
                    new Vector2(0.13f, 0.38f)),

                Stone = GetOrCreateProfile(
                    "Surface_NaturalStone",
                    "natural-stone",
                    EnvironmentSurfaceKind.NaturalStone,
                    0.99f,
                    0.88f,
                    0.74f,
                    0.82f,
                    new Color(0.42f, 0.44f, 0.46f, 0.72f),
                    2,
                    4,
                    new Vector2(0.015f, 0.04f),
                    new Vector2(0.08f, 0.24f)),

                DirtRoad = GetOrCreateProfile(
                    "Surface_PackedDirtRoad",
                    "packed-dirt-road",
                    EnvironmentSurfaceKind.PackedDirtRoad,
                    1.04f,
                    0.79f,
                    0.63f,
                    0.86f,
                    new Color(0.62f, 0.46f, 0.27f, 0.78f),
                    3,
                    5,
                    new Vector2(0.018f, 0.052f),
                    new Vector2(0.11f, 0.31f)),

                CobbleRoad = GetOrCreateProfile(
                    "Surface_OldCobblestone",
                    "old-cobblestone",
                    EnvironmentSurfaceKind.OldCobblestone,
                    0.99f,
                    0.91f,
                    0.78f,
                    0.78f,
                    new Color(0.52f, 0.5f, 0.56f, 0.7f),
                    2,
                    4,
                    new Vector2(0.014f, 0.036f),
                    new Vector2(0.07f, 0.2f))
            };

            return profiles;
        }

        private static EnvironmentSurfaceProfile GetOrCreateProfile(
            string assetName,
            string stableId,
            EnvironmentSurfaceKind kind,
            float movementMultiplier,
            float staticFriction,
            float dynamicFriction,
            float stepDistance,
            Color particleColor,
            int minParticles,
            int maxParticles,
            Vector2 particleSize,
            Vector2 particleSpeed)
        {
            string path = ProfileRoot + "/" + assetName + ".asset";
            EnvironmentSurfaceProfile profile =
                AssetDatabase.LoadAssetAtPath<EnvironmentSurfaceProfile>(path);

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EnvironmentSurfaceProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.Configure(
                stableId,
                kind,
                movementMultiplier,
                staticFriction,
                dynamicFriction,
                stepDistance,
                particleColor,
                minParticles,
                maxParticles,
                particleSize,
                particleSpeed);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void BuildGroundAssets(Profiles profiles)
        {
            Texture2D meadow = LoadTexture(MeadowTexture);
            Texture2D dirt = LoadTexture(DirtTexture);
            Texture2D stone = LoadTexture(StoneTexture);
            Texture2D dirtRoad = LoadTexture(DirtRoadTexture);
            Texture2D cobbleRoad = LoadTexture(CobbleRoadTexture);

            List<GroundSpec> specs = new List<GroundSpec>
            {
                new GroundSpec(
                    "Ground_MeadowGrass_4m",
                    "Ground_MeadowGrass",
                    meadow,
                    meadow,
                    profiles.Meadow,
                    null,
                    EnvironmentSurfacePattern.FullPrimary,
                    0f),

                new GroundSpec(
                    "Ground_WoodlandDirt_4m",
                    "Ground_WoodlandDirt",
                    dirt,
                    dirt,
                    profiles.Dirt,
                    null,
                    EnvironmentSurfacePattern.FullPrimary,
                    0f),

                new GroundSpec(
                    "Ground_NaturalStone_4m",
                    "Ground_NaturalStone",
                    stone,
                    stone,
                    profiles.Stone,
                    null,
                    EnvironmentSurfacePattern.FullPrimary,
                    0f),

                Road("Road_PackedDirt_Straight_4m", "Road_PackedDirt_Straight", meadow, dirtRoad, profiles.Meadow, profiles.DirtRoad, EnvironmentSurfacePattern.CenterBand, 1f),
                Road("Road_PackedDirt_Corner_4m", "Road_PackedDirt_Corner", meadow, dirtRoad, profiles.Meadow, profiles.DirtRoad, EnvironmentSurfacePattern.Corner, 2f),
                Road("Road_PackedDirt_Cross_4m", "Road_PackedDirt_Cross", meadow, dirtRoad, profiles.Meadow, profiles.DirtRoad, EnvironmentSurfacePattern.Cross, 3f),
                Road("Road_PackedDirt_TJunction_4m", "Road_PackedDirt_TJunction", meadow, dirtRoad, profiles.Meadow, profiles.DirtRoad, EnvironmentSurfacePattern.TJunction, 6f),
                Road("Road_OldCobble_Straight_4m", "Road_OldCobble_Straight", meadow, cobbleRoad, profiles.Meadow, profiles.CobbleRoad, EnvironmentSurfacePattern.CenterBand, 1f),
                Road("Road_OldCobble_Corner_4m", "Road_OldCobble_Corner", meadow, cobbleRoad, profiles.Meadow, profiles.CobbleRoad, EnvironmentSurfacePattern.Corner, 2f),
                Road("Road_OldCobble_Cross_4m", "Road_OldCobble_Cross", meadow, cobbleRoad, profiles.Meadow, profiles.CobbleRoad, EnvironmentSurfacePattern.Cross, 3f),
                Road("Road_OldCobble_TJunction_4m", "Road_OldCobble_TJunction", meadow, cobbleRoad, profiles.Meadow, profiles.CobbleRoad, EnvironmentSurfacePattern.TJunction, 6f),
                Road("Transition_GrassToDirt_4m", "Transition_GrassToDirt", meadow, dirt, profiles.Meadow, profiles.Dirt, EnvironmentSurfacePattern.EdgeTransition, 5f),
                Road("Transition_GrassToStone_4m", "Transition_GrassToStone", meadow, stone, profiles.Meadow, profiles.Stone, EnvironmentSurfacePattern.EdgeTransition, 5f)
            };

            foreach (GroundSpec spec in specs)
            {
                Material material = BuildGroundMaterial(spec);
                BuildGroundPrefab(spec, material);
            }
        }

        private static GroundSpec Road(
            string id,
            string material,
            Texture2D primaryTexture,
            Texture2D secondaryTexture,
            EnvironmentSurfaceProfile primary,
            EnvironmentSurfaceProfile secondary,
            EnvironmentSurfacePattern pattern,
            float shaderMode)
        {
            return new GroundSpec(
                id,
                material,
                primaryTexture,
                secondaryTexture,
                primary,
                secondary,
                pattern,
                shaderMode);
        }

        private static Material BuildGroundMaterial(GroundSpec spec)
        {
            Shader shader = Shader.Find("Keeper First Covenant/Environment/Painterly Ground");
            string path = MaterialRoot + "/" + spec.MaterialName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_SurfaceA", spec.PrimaryTexture);
            material.SetTexture("_SurfaceB", spec.SecondaryTexture ?? spec.PrimaryTexture);
            material.SetColor("_Tint", Color.white);
            material.SetFloat("_BlendMode", spec.ShaderMode);
            material.SetFloat("_PathHalfWidth", PathHalfWidth);
            material.SetFloat("_EdgeFeather", 0.055f);
            material.SetFloat("_EdgeNoise", spec.ShaderMode > 0f ? 0.026f : 0f);
            material.SetFloat("_NoiseScale", 12f);
            material.SetFloat("_ColorVariation", 0.026f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildGroundPrefab(GroundSpec spec, Material material)
        {
            GameObject root = new GameObject(spec.Id);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.transform.localScale = new Vector3(TileSize, TileSize, 1f);

            Collider generatedCollider = visual.GetComponent<Collider>();
            if (generatedCollider != null)
                Object.DestroyImmediate(generatedCollider);

            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = -40;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(TileSize, 0.2f, TileSize);
            collider.center = new Vector3(0f, -0.085f, 0f);

            EnvironmentSurfaceProfile physicsProfile =
                spec.SecondaryProfile != null
                    ? spec.SecondaryProfile
                    : spec.PrimaryProfile;
            collider.sharedMaterial = GetOrCreatePhysicsMaterial(physicsProfile);

            EnvironmentSurface surface = root.AddComponent<EnvironmentSurface>();
            surface.Configure(
                spec.PrimaryProfile,
                spec.SecondaryProfile,
                spec.Pattern,
                new Vector2(TileSize, TileSize),
                PathHalfWidth);

            PrefabUtility.SaveAsPrefabAsset(
                root,
                PrefabRoot + "/" + spec.Id + ".prefab");
            Object.DestroyImmediate(root);
        }

        private static PhysicsMaterial GetOrCreatePhysicsMaterial(
            EnvironmentSurfaceProfile profile)
        {
            string path = PhysicsMaterialRoot + "/Physics_" + profile.Kind + ".physicMaterial";
            PhysicsMaterial material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);

            if (material == null)
            {
                material = new PhysicsMaterial("Physics_" + profile.Kind);
                AssetDatabase.CreateAsset(material, path);
            }

            material.staticFriction = profile.StaticFriction;
            material.dynamicFriction = profile.DynamicFriction;
            material.bounciness = 0f;
            material.frictionCombine = PhysicsMaterialCombine.Average;
            material.bounceCombine = PhysicsMaterialCombine.Minimum;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildFoliageAssets()
        {
            Shader foliageShader = Shader.Find(
                "Keeper First Covenant/Environment/Interactive Foliage");

            BuildFoliage(
                "Grass_LowMeadow_A",
                LowGrassTexture,
                foliageShader,
                new Vector2(1.35f, 0.92f),
                0.066f,
                1.6f,
                0.42f,
                0.97f,
                0.92f,
                false);

            BuildFoliage(
                "Grass_TallMeadow_A",
                TallGrassTexture,
                foliageShader,
                new Vector2(1.48f, 1.72f),
                0.095f,
                1.28f,
                0.68f,
                0.91f,
                0.95f,
                false);

            BuildFoliage(
                "Grass_Wildflowers_A",
                WildflowersTexture,
                foliageShader,
                new Vector2(1.82f, 1.08f),
                0.058f,
                1.42f,
                0.34f,
                0.96f,
                0.88f,
                true);
        }

        private static void BuildFoliage(
            string id,
            string texturePath,
            Shader shader,
            Vector2 worldSize,
            float windStrength,
            float windSpeed,
            float concealment,
            float movementMultiplier,
            float flammability,
            bool addPollen)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            string materialPath = MaterialRoot + "/" + id + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetFloat("_WindStrength", windStrength);
            material.SetFloat("_WindSpeed", windSpeed);
            material.SetFloat("_WindScale", 1.65f);
            material.SetFloat("_AnchorHeight", 0.12f);
            material.SetFloat("_AlphaCutoff", 0.012f);
            EditorUtility.SetDirty(material);

            GameObject root = new GameObject(id);
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            visual.AddComponent<BillboardCharacter2D>();

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 4;

            float sourceWidth = sprite != null ? sprite.bounds.size.x : 1f;
            float sourceHeight = sprite != null ? sprite.bounds.size.y : 1f;
            visual.transform.localScale = new Vector3(
                worldSize.x / Mathf.Max(0.01f, sourceWidth),
                worldSize.y / Mathf.Max(0.01f, sourceHeight),
                1f);

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(
                worldSize.x * 0.78f,
                worldSize.y * 0.82f,
                worldSize.x * 0.72f);
            trigger.center = new Vector3(0f, worldSize.y * 0.42f, 0f);

            InteractiveFoliage interaction = root.AddComponent<InteractiveFoliage>();
            interaction.Configure(
                renderer,
                9f,
                3.8f,
                id.Contains("Tall") ? 0.68f : 0.5f,
                0.045f,
                0.075f);

            FoliageGameplayVolume gameplay = root.AddComponent<FoliageGameplayVolume>();
            gameplay.Configure(concealment, movementMultiplier, flammability);

            if (addPollen)
                AddPollen(root.transform, worldSize);

            PrefabUtility.SaveAsPrefabAsset(
                root,
                PrefabRoot + "/" + id + ".prefab");
            Object.DestroyImmediate(root);
        }

        private static void AddPollen(Transform parent, Vector2 worldSize)
        {
            GameObject pollenObject = new GameObject("PollenVFX");
            pollenObject.transform.SetParent(parent, false);
            pollenObject.transform.localPosition = new Vector3(0f, worldSize.y * 0.35f, 0f);

            ParticleSystem particles = pollenObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 4f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 3.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.025f, 0.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.026f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.74f, 0.87f, 1f, 0.42f),
                new Color(1f, 0.91f, 0.56f, 0.34f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 36;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 1.6f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(worldSize.x * 0.72f, worldSize.y * 0.25f, 0.3f);

            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(0.025f, 0.085f);
            velocity.x = new ParticleSystem.MinMaxCurve(-0.025f, 0.035f);

            ParticleSystemRenderer renderer =
                pollenObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = LoadParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 8;
        }

        private static void BuildParticleMaterial()
        {
            Shader shader = Shader.Find(
                "Keeper First Covenant/Environment/Soft World Particle");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, ParticleMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetFloat("_Softness", 0.18f);
            EditorUtility.SetDirty(material);
        }

        private static Texture2D LoadTexture(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void ReimportSourceTextures()
        {
            string[] paths =
            {
                MeadowTexture,
                DirtTexture,
                StoneTexture,
                DirtRoadTexture,
                CobbleRoadTexture,
                LowGrassTexture,
                TallGrassTexture,
                WildflowersTexture
            };

            foreach (string path in paths)
            {
                AssetDatabase.ImportAsset(
                    path,
                    ImportAssetOptions.ForceSynchronousImport |
                    ImportAssetOptions.ForceUpdate);
            }
        }

        private static void BuildDemoScene()
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new GameObject("EnvironmentFoundation_Test");
            root.AddComponent<EnvironmentWindController>();
            BuildFoundationGround(root.transform);

            GameObject actor = BuildTestActor(root.transform);
            BuildCamera(root.transform, actor.transform);
            BuildLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            Selection.activeGameObject = actor;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject BuildTestActor(Transform parent)
        {
            GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actor.name = "Environment Test Walker";
            actor.transform.SetParent(parent, false);
            actor.transform.position = new Vector3(-4f, 1f, -4f);

            Collider primitiveCollider = actor.GetComponent<Collider>();
            if (primitiveCollider != null)
                Object.DestroyImmediate(primitiveCollider);

            CharacterController controller = actor.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.36f;
            controller.center = new Vector3(0f, 0f, 0f);
            controller.stepOffset = 0.34f;

            MeshRenderer renderer = actor.GetComponent<MeshRenderer>();
            Material actorMaterial = GetOrCreateActorMaterial();
            renderer.sharedMaterial = actorMaterial;

            actor.AddComponent<EnvironmentFoundationTestWalker>();
            AttachSurfaceEffects(actor);
            return actor;
        }

        private static Material GetOrCreateActorMaterial()
        {
            string path = MaterialRoot + "/Environment_TestWalker.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_BaseColor", new Color(0.22f, 0.34f, 0.62f, 1f));
            material.SetFloat("_Smoothness", 0.28f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildCamera(Transform parent, Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(10f, 12f, -12f);
            cameraObject.transform.LookAt(new Vector3(0f, 0.6f, 0f));

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.4f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.27f, 0.38f, 0.49f, 1f);

            cameraObject.AddComponent<AudioListener>();
            TopDownCameraFollow follow = cameraObject.AddComponent<TopDownCameraFollow>();
            follow.Configure(
                target,
                new Vector3(10f, 12f, -12f),
                new Vector3(0f, 0.6f, 0f));
        }

        private static void BuildLighting(Transform parent)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.68f, 0.74f, 1f);

            GameObject sun = new GameObject("Soft Daylight");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.93f, 0.82f, 1f);
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
        }

        private static string ResolveGroundId(int x, int z)
        {
            if (x == 0)
            {
                if (z == 0)
                    return "Road_PackedDirt_Cross_4m";

                return "Road_PackedDirt_Straight_4m";
            }

            if (z == 1 && x > 0)
                return "Road_OldCobble_Straight_4m";

            if ((x == -3 && z == 2) || (x == -2 && z == 2))
                return "Ground_NaturalStone_4m";

            if ((x == 2 && z == -2) || (x == 3 && z == -2))
                return "Ground_WoodlandDirt_4m";

            return "Ground_MeadowGrass_4m";
        }

        private static GameObject InstantiatePrefab(
            string id,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            GameObject prefab = LoadPrefab(id);
            if (prefab == null)
                return null;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return null;

            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            return instance;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/KeeperFirstCovenant", "Materials");
            EnsureFolder("Assets/KeeperFirstCovenant/Materials", "Environment");
            EnsureFolder("Assets/KeeperFirstCovenant/Materials/Environment", "Foundation");

            EnsureFolder("Assets/KeeperFirstCovenant", "Prefabs");
            EnsureFolder("Assets/KeeperFirstCovenant/Prefabs", "Environment");
            EnsureFolder("Assets/KeeperFirstCovenant/Prefabs/Environment", "Foundation");

            EnsureFolder("Assets/KeeperFirstCovenant", "Data");
            EnsureFolder("Assets/KeeperFirstCovenant/Data", "Environment");
            EnsureFolder("Assets/KeeperFirstCovenant/Data/Environment", "Foundation");
            EnsureFolder("Assets/KeeperFirstCovenant/Data/Environment/Foundation", "Profiles");
            EnsureFolder("Assets/KeeperFirstCovenant/Data/Environment/Foundation", "PhysicsMaterials");

            EnsureFolder("Assets/KeeperFirstCovenant", "Scenes");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == path)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
