#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ProductionWorldPack01Builder
    {
        public const string ArtRoot =
            "Assets/KeeperFirstCovenant/Art/Runtime/World/ProductionPack01";

        public const string PrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/ProductionWorld/Pack01";

        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/ProductionWorldPack01_Test.unity";

        public const string WideArchPath =
            ArtRoot + "/RuinedArch_Wide.png";

        public const string TallRuneStonePath =
            ArtRoot + "/RuneStone_TallBlue.png";

        public const string BrightCirclePath =
            ArtRoot + "/CovenantCircle_BrightBlue.png";

        public const string BlueBrazierPath =
            ArtRoot + "/Brazier_Blue.png";

        public const string WideArchPrefabPath =
            PrefabRoot + "/RuinedArch_Wide.prefab";

        public const string TallRuneStonePrefabPath =
            PrefabRoot + "/RuneStone_TallBlue.prefab";

        public const string BrightCirclePrefabPath =
            PrefabRoot + "/CovenantCircle_BrightBlue.prefab";

        public const string BlueBrazierPrefabPath =
            PrefabRoot + "/Brazier_Blue.prefab";

        [MenuItem("Keeper First Covenant/Production Art/World Pack 01/BUILD PACK")]
        public static void BuildAll()
        {
            if (!SourcesPresent())
            {
                Debug.LogWarning(
                    "Production World Pack 01 is waiting for its four source PNG files in: " +
                    ArtRoot);
                return;
            }

            BuildPrefabs();
            BuildTestScene();

            Debug.Log(
                "Production World Pack 01 built: 4 painted gameplay prefabs + test scene.");
        }

        [MenuItem("Keeper First Covenant/Production Art/World Pack 01/Build Prefabs")]
        public static void BuildPrefabs()
        {
            if (!SourcesPresent())
                return;

            EnsureFolders();

            BuildWideArch();
            BuildTallRuneStone();
            BuildBrightCircle();
            BuildBlueBrazier();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static bool SourcesPresent()
        {
            return LoadSprite(WideArchPath) != null &&
                   LoadSprite(TallRuneStonePath) != null &&
                   LoadSprite(BrightCirclePath) != null &&
                   LoadSprite(BlueBrazierPath) != null;
        }

        public static bool PrefabsPresent()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(WideArchPrefabPath) != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(TallRuneStonePrefabPath) != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(BrightCirclePrefabPath) != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(BlueBrazierPrefabPath) != null;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void BuildWideArch()
        {
            GameObject root = CreateVerticalSpriteRoot(
                "RuinedArch_Wide",
                LoadSprite(WideArchPath),
                12);

            BoxCollider left = root.AddComponent<BoxCollider>();
            left.center = new Vector3(-0.82f, 0.88f, 0f);
            left.size = new Vector3(0.62f, 1.75f, 0.58f);

            BoxCollider right = root.AddComponent<BoxCollider>();
            right.center = new Vector3(0.82f, 0.88f, 0f);
            right.size = new Vector3(0.62f, 1.75f, 0.58f);

            BoxCollider lintel = root.AddComponent<BoxCollider>();
            lintel.center = new Vector3(0f, 1.72f, 0f);
            lintel.size = new Vector3(2.15f, 0.38f, 0.58f);

            SavePrefab(root, WideArchPrefabPath);
        }

        private static void BuildTallRuneStone()
        {
            GameObject root = CreateVerticalSpriteRoot(
                "RuneStone_TallBlue",
                LoadSprite(TallRuneStonePath),
                13);

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.72f, 0f);
            collider.size = new Vector3(0.92f, 1.45f, 0.62f);

            Light light = CreatePointLight(
                root.transform,
                "RuneGlow",
                new Color(0.24f, 0.47f, 1f),
                1.35f,
                4.2f,
                new Vector3(0f, 1.0f, -0.18f));

            CreateMagicParticles(
                root.transform,
                "RuneMotes",
                new Color(0.42f, 0.63f, 1f, 0.78f),
                new Vector3(0f, 0.85f, 0f),
                0.55f,
                8f,
                1.7f,
                0.045f);

            ProductionWorldPropFx fx =
                root.AddComponent<ProductionWorldPropFx>();

            fx.Configure(light, 1.35f, 0.22f, 1.55f);

            SavePrefab(root, TallRuneStonePrefabPath);
        }

        private static void BuildBrightCircle()
        {
            GameObject root =
                new GameObject("CovenantCircle_BrightBlue");

            GameObject visual =
                new GameObject("Visual");

            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = LoadSprite(BrightCirclePath);
            renderer.sortingOrder = -8;

            BoxCollider trigger =
                root.AddComponent<BoxCollider>();

            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.08f, 0f);
            trigger.size = new Vector3(3.2f, 0.18f, 2.25f);

            Light light = CreatePointLight(
                root.transform,
                "CircleGlow",
                new Color(0.22f, 0.43f, 1f),
                1.1f,
                5.5f,
                new Vector3(0f, 0.35f, 0f));

            CreateMagicParticles(
                root.transform,
                "CircleMotes",
                new Color(0.5f, 0.68f, 1f, 0.72f),
                new Vector3(0f, 0.12f, 0f),
                1.45f,
                11f,
                1.25f,
                0.035f);

            ProductionWorldPropFx fx =
                root.AddComponent<ProductionWorldPropFx>();

            fx.Configure(light, 1.1f, 0.3f, 1.2f);

            SavePrefab(root, BrightCirclePrefabPath);
        }

        private static void BuildBlueBrazier()
        {
            GameObject root = CreateVerticalSpriteRoot(
                "Brazier_Blue",
                LoadSprite(BlueBrazierPath),
                14);

            BoxCollider baseCollider =
                root.AddComponent<BoxCollider>();

            baseCollider.center =
                new Vector3(0f, 0.42f, 0f);

            baseCollider.size =
                new Vector3(0.72f, 0.84f, 0.68f);

            SphereCollider heatTrigger =
                root.AddComponent<SphereCollider>();

            heatTrigger.isTrigger = true;
            heatTrigger.center =
                new Vector3(0f, 1.18f, 0f);

            heatTrigger.radius = 0.62f;

            Light light = CreatePointLight(
                root.transform,
                "BlueFlameLight",
                new Color(0.26f, 0.48f, 1f),
                1.8f,
                5.2f,
                new Vector3(0f, 1.2f, -0.15f));

            CreateMagicParticles(
                root.transform,
                "BlueFlameSparks",
                new Color(0.42f, 0.62f, 1f, 0.9f),
                new Vector3(0f, 1.15f, 0f),
                0.28f,
                14f,
                0.95f,
                0.035f);

            ProductionWorldPropFx fx =
                root.AddComponent<ProductionWorldPropFx>();

            fx.Configure(light, 1.8f, 0.35f, 2.2f);

            SavePrefab(root, BlueBrazierPrefabPath);
        }

        private static GameObject CreateVerticalSpriteRoot(
            string name,
            Sprite sprite,
            int sortingOrder)
        {
            GameObject root = new GameObject(name);

            GameObject visual =
                new GameObject("Visual");

            visual.transform.SetParent(root.transform, false);

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;

            // Only the painted visual billboards toward the camera.
            // Gameplay collision remains stable in world space.
            visual.AddComponent<BillboardCharacter2D>();

            return root;
        }

        private static Light CreatePointLight(
            Transform parent,
            string name,
            Color color,
            float intensity,
            float range,
            Vector3 localPosition)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;

            Light light = child.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;

            return light;
        }

        private static void CreateMagicParticles(
            Transform parent,
            string name,
            Color color,
            Vector3 localPosition,
            float radius,
            float rate,
            float lifetime,
            float size)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;

            ParticleSystem system =
                child.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = lifetime;
            main.startSpeed = 0.12f;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = 96;
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission =
                system.emission;

            emission.rateOverTime = rate;

            ParticleSystem.ShapeModule shape =
                system.shape;

            shape.shapeType =
                ParticleSystemShapeType.Sphere;

            shape.radius = radius;

            ParticleSystem.VelocityOverLifetimeModule velocity =
                system.velocityOverLifetime;

            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(0.16f);

            ParticleSystem.ColorOverLifetimeModule colorOverLife =
                system.colorOverLifetime;

            colorOverLife.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(
                        new Color(color.r, color.g, color.b),
                        0f),
                    new GradientColorKey(
                        new Color(color.r, color.g, color.b),
                        1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(color.a, 0.18f),
                    new GradientAlphaKey(color.a * 0.65f, 0.72f),
                    new GradientAlphaKey(0f, 1f)
                });

            colorOverLife.color =
                new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystemRenderer particleRenderer =
                child.GetComponent<ParticleSystemRenderer>();

            Material material =
                GetOrCreateParticleMaterial();

            if (material != null)
                particleRenderer.sharedMaterial = material;
        }

        private static Material GetOrCreateParticleMaterial()
        {
            const string path =
                "Assets/KeeperFirstCovenant/Materials/ProductionPack01_Particles.mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null)
                return material;

            EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Materials");

            Shader shader =
                Shader.Find("Particles/Standard Unlit");

            if (shader == null)
                shader =
                    Shader.Find(
                        "Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
                shader =
                    Shader.Find("Sprites/Default");

            if (shader == null)
                return null;

            material = new Material(shader);
            material.name = "ProductionPack01_Particles";

            AssetDatabase.CreateAsset(
                material,
                path);

            return material;
        }

        private static void SavePrefab(
            GameObject root,
            string path)
        {
            PrefabUtility.SaveAsPrefabAsset(
                root,
                path);

            Object.DestroyImmediate(root);
        }

        [MenuItem("Keeper First Covenant/Production Art/World Pack 01/Build Test Scene")]
        public static void BuildTestScene()
        {
            if (!PrefabsPresent())
                BuildPrefabs();

            if (!PrefabsPresent())
                return;

            EnsureFolders();

            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);

            GameObject root =
                new GameObject(
                    "ProductionWorldPack01_Test");

            BuildGround(root.transform);

            Instantiate(
                WideArchPrefabPath,
                new Vector3(-3.2f, 0f, 1.9f),
                root.transform);

            Instantiate(
                TallRuneStonePrefabPath,
                new Vector3(2.9f, 0f, 1.7f),
                root.transform);

            Instantiate(
                BrightCirclePrefabPath,
                new Vector3(0.3f, 0.02f, -1.1f),
                root.transform);

            Instantiate(
                BlueBrazierPrefabPath,
                new Vector3(-2.4f, 0f, -2.5f),
                root.transform);

            BuildCamera(root.transform);
            BuildLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(
                scene,
                ScenePath);

            AddToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void AddShowcaseToScene(
            Transform parent)
        {
            if (parent == null)
                return;

            if (!PrefabsPresent())
                BuildPrefabs();

            Instantiate(
                WideArchPrefabPath,
                new Vector3(-7.3f, 0f, 3.1f),
                parent);

            Instantiate(
                TallRuneStonePrefabPath,
                new Vector3(6.7f, 0f, 2.7f),
                parent);

            Instantiate(
                BrightCirclePrefabPath,
                new Vector3(3.1f, 0.025f, -2.0f),
                parent);

            Instantiate(
                BlueBrazierPrefabPath,
                new Vector3(-4.8f, 0f, -2.6f),
                parent);
        }

        private static GameObject Instantiate(
            string prefabPath,
            Vector3 position,
            Transform parent)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    prefabPath);

            if (prefab == null)
                return null;

            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    prefab) as GameObject;

            if (instance == null)
                return null;

            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            return instance;
        }

        private static void BuildGround(
            Transform parent)
        {
            GameObject ground =
                GameObject.CreatePrimitive(
                    PrimitiveType.Plane);

            ground.name = "CollisionGround";
            ground.transform.SetParent(
                parent,
                false);

            ground.transform.localScale =
                new Vector3(2.4f, 1f, 1.9f);

            Renderer renderer =
                ground.GetComponent<Renderer>();

            if (renderer != null)
            {
                Material material =
                    GetOrCreateGroundMaterial();

                if (material != null)
                    renderer.sharedMaterial = material;
            }
        }

        private static Material GetOrCreateGroundMaterial()
        {
            const string path =
                "Assets/KeeperFirstCovenant/Materials/ProductionPack01_Ground.mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    path);

            if (material != null)
                return material;

            EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Materials");

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            if (shader == null)
                return null;

            material = new Material(shader);
            material.name = "ProductionPack01_Ground";
            material.color =
                new Color(0.035f, 0.045f, 0.07f, 1f);

            AssetDatabase.CreateAsset(
                material,
                path);

            return material;
        }

        private static void BuildCamera(
            Transform parent)
        {
            GameObject go =
                new GameObject("Main Camera");

            go.tag = "MainCamera";
            go.transform.SetParent(parent, false);
            go.transform.position =
                new Vector3(8.8f, 10.2f, -10.6f);

            Camera camera =
                go.AddComponent<Camera>();

            camera.orthographic = true;
            camera.orthographicSize = 6.0f;
            camera.clearFlags =
                CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(0.01f, 0.018f, 0.04f, 1f);

            go.transform.LookAt(
                new Vector3(0f, 0.8f, 0f));

            go.AddComponent<AudioListener>();
        }

        private static void BuildLighting(
            Transform parent)
        {
            RenderSettings.ambientMode =
                UnityEngine.Rendering.AmbientMode.Flat;

            RenderSettings.ambientLight =
                new Color(0.08f, 0.1f, 0.18f);

            GameObject moon =
                new GameObject("Moonlight");

            moon.transform.SetParent(parent, false);
            moon.transform.rotation =
                Quaternion.Euler(52f, -34f, 0f);

            Light light =
                moon.AddComponent<Light>();

            light.type = LightType.Directional;
            light.color =
                new Color(0.45f, 0.56f, 1f);
            light.intensity = 0.68f;
        }

        private static void AddToBuildSettings(
            string path)
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(
                    EditorBuildSettings.scenes);

            foreach (
                EditorBuildSettingsScene scene
                in scenes)
            {
                if (scene.path == path)
                    return;
            }

            scenes.Add(
                new EditorBuildSettingsScene(
                    path,
                    true));

            EditorBuildSettings.scenes =
                scenes.ToArray();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(
                "Assets/KeeperFirstCovenant/Prefabs",
                "ProductionWorld");

            EnsureFolder(
                "Assets/KeeperFirstCovenant/Prefabs/ProductionWorld",
                "Pack01");

            EnsureFolder(
                "Assets/KeeperFirstCovenant",
                "Scenes");
        }

        private static void EnsureFolder(
            string parent,
            string child)
        {
            string path =
                parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(
                    parent,
                    child);
        }
    }
}
#endif
