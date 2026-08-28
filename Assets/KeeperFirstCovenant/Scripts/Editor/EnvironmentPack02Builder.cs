#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using KeeperFirstCovenant.Visual;
using KeeperFirstCovenant.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class EnvironmentPack02Builder
    {
        public const string ArtRoot =
            "Assets/KeeperFirstCovenant/Art/Runtime/World/EnvironmentPack02";

        public const string PrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/ProductionWorld/EnvironmentPack02";

        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/EnvironmentPack02_Test.unity";

        private const string GroundMatPath =
            "Assets/KeeperFirstCovenant/Materials/EnvironmentPack02_Ground.mat";

        private const string FoliageMatPath =
            "Assets/KeeperFirstCovenant/Materials/EnvironmentPack02_Foliage.mat";

        private static readonly string[] RequiredSprites =
        {
            "Ground/Ground_Dirt_A.png",
            "Ground/Ground_Dirt_B.png",
            "Ground/Ground_Stone_A.png",
            "Ground/Ground_Stone_B.png",
            "Ground/Ground_Grass_A.png",
            "Ground/Path_Stone_A.png",
            "Ground/Transition_GrassToStone_A.png",
            "Ground/Puddle_A.png",
            "Nature/Grass_Small_A.png",
            "Nature/Grass_Tall_A.png",
            "Nature/Flower_Blue_A.png",
            "Trees/Tree_Living_A.png",
            "Trees/Tree_Living_B.png",
            "Trees/Tree_Twisted_A.png",
            "Trees/Tree_Dead_A.png",
            "Trees/Log_A.png"
        };

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 02/BUILD EVERYTHING")]
        public static void BuildAll()
        {
            if (!SourcesPresent())
            {
                Debug.LogWarning(
                    "Environment Pack 02 source PNGs are incomplete. " +
                    "Copy the archive into the project first: " + ArtRoot);
                return;
            }

            EnsureFolders();
            BuildPrefabs();
            BuildTestScene();
            EnvironmentPack02Validator.Validate(true);

            Debug.Log(
                "Environment Pack 02 is ready: ground, grass, flowers, animated trees and log prefabs.");
        }

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 02/Build Prefabs")]
        public static void BuildPrefabs()
        {
            EnsureFolders();

            Material groundMaterial = GetOrCreateGroundMaterial();
            Material foliageMaterial = GetOrCreateFoliageMaterial();

            BuildGround("Ground_Dirt_A", EnvironmentSurfaceType.Dirt, false, groundMaterial);
            BuildGround("Ground_Dirt_B", EnvironmentSurfaceType.Dirt, false, groundMaterial);
            BuildGround("Ground_Stone_A", EnvironmentSurfaceType.Stone, false, groundMaterial);
            BuildGround("Ground_Stone_B", EnvironmentSurfaceType.Stone, false, groundMaterial);
            BuildGround("Ground_Grass_A", EnvironmentSurfaceType.Grass, false, groundMaterial);
            BuildGround("Path_Stone_A", EnvironmentSurfaceType.Stone, false, groundMaterial);
            BuildGround("Transition_GrassToStone_A", EnvironmentSurfaceType.Grass, false, groundMaterial);
            BuildGround("Puddle_A", EnvironmentSurfaceType.Water, true, groundMaterial, true);

            BuildFoliage("Grass_Small_A", "Nature", foliageMaterial, 0.075f, 1.45f, 2.7f, 0.24f, 12);
            BuildFoliage("Grass_Tall_A", "Nature", foliageMaterial, 0.105f, 1.25f, 3.0f, 0.33f, 13);
            BuildFoliage("Flower_Blue_A", "Nature", foliageMaterial, 0.045f, 1.1f, 3.3f, 0.17f, 13);

            BuildTree("Tree_Living_A", foliageMaterial, 0.035f, 0.72f, 4.6f, 0.16f, 0.52f);
            BuildTree("Tree_Living_B", foliageMaterial, 0.032f, 0.66f, 4.8f, 0.14f, 0.46f);
            BuildTree("Tree_Twisted_A", foliageMaterial, 0.022f, 0.58f, 5.2f, 0.09f, 0.50f);
            BuildTree("Tree_Dead_A", foliageMaterial, 0.015f, 0.48f, 5.6f, 0.06f, 0.42f);
            BuildLog("Log_A", groundMaterial);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static bool SourcesPresent()
        {
            foreach (string relative in RequiredSprites)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(
                        ArtRoot + "/" + relative) == null)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool PrefabsPresent()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                       PrefabRoot + "/Ground/Ground_Dirt_A.prefab") != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(
                       PrefabRoot + "/Nature/Grass_Small_A.prefab") != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(
                       PrefabRoot + "/Trees/Tree_Living_A.prefab") != null;
        }

        private static Sprite LoadSprite(string category, string id)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                ArtRoot + "/" + category + "/" + id + ".png");
        }

        private static void BuildGround(
            string id,
            EnvironmentSurfaceType surface,
            bool wet,
            Material material,
            bool trigger = false)
        {
            Sprite sprite = LoadSprite("Ground", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -20;

            EnvironmentSurfaceMarker marker =
                root.AddComponent<EnvironmentSurfaceMarker>();

            marker.Configure(surface, wet, wet ? 0.75f : 1f);

            if (trigger)
            {
                BoxCollider volume = root.AddComponent<BoxCollider>();
                volume.isTrigger = true;
                volume.center = new Vector3(0f, 0.04f, 0f);
                volume.size = new Vector3(1.95f, 0.08f, 1.45f);
            }

            SavePrefab(root, PrefabRoot + "/Ground/" + id + ".prefab");
        }

        private static void BuildFoliage(
            string id,
            string category,
            Material material,
            float strength,
            float speed,
            float stiffness,
            float gust,
            int sortingOrder)
        {
            Sprite sprite = LoadSprite(category, id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(root.transform, sprite, material, sortingOrder);

            WindReactiveProp wind = root.AddComponent<WindReactiveProp>();
            wind.Configure(strength, speed, 0.8f, stiffness, gust);

            visual.AddComponent<BillboardCharacter2D>();

            SavePrefab(root, PrefabRoot + "/" + category + "/" + id + ".prefab");
        }

        private static void BuildTree(
            string id,
            Material material,
            float strength,
            float speed,
            float stiffness,
            float gust,
            float trunkRadius)
        {
            Sprite sprite = LoadSprite("Trees", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(root.transform, sprite, material, 22);
            visual.AddComponent<BillboardCharacter2D>();

            WindReactiveProp wind = root.AddComponent<WindReactiveProp>();
            wind.Configure(strength, speed, 0.42f, stiffness, gust);

            CapsuleCollider trunk = root.AddComponent<CapsuleCollider>();
            trunk.direction = 1;
            trunk.radius = trunkRadius;
            trunk.height = Mathf.Max(1.3f, sprite.bounds.size.y * 0.48f);
            trunk.center = new Vector3(0f, trunk.height * 0.5f, 0f);

            SavePrefab(root, PrefabRoot + "/Trees/" + id + ".prefab");
        }

        private static void BuildLog(string id, Material material)
        {
            Sprite sprite = LoadSprite("Trees", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(root.transform, sprite, material, 16);
            visual.AddComponent<BillboardCharacter2D>();

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.28f, 0f);
            collider.size = new Vector3(1.55f, 0.56f, 0.62f);

            SavePrefab(root, PrefabRoot + "/Trees/" + id + ".prefab");
        }

        private static GameObject CreateVerticalVisual(
            Transform parent,
            Sprite sprite,
            Material material,
            int sortingOrder)
        {
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;

            // Imported runtime sprites use a centered pivot. Raise the visual so its bottom sits on Y=0.
            visual.transform.localPosition =
                Vector3.up * sprite.bounds.extents.y;

            return visual;
        }

        private static Material GetOrCreateGroundMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(GroundMatPath);
            if (material != null)
                return material;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                throw new InvalidOperationException("Sprites/Default shader is unavailable.");

            material = new Material(shader)
            {
                name = "EnvironmentPack02_Ground"
            };

            AssetDatabase.CreateAsset(material, GroundMatPath);
            return material;
        }

        private static Material GetOrCreateFoliageMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(FoliageMatPath);
            if (material != null)
                return material;

            Shader shader = Shader.Find("Keeper First Covenant/Foliage Wind");
            if (shader == null)
                throw new InvalidOperationException(
                    "Foliage wind shader was not imported.");

            material = new Material(shader)
            {
                name = "EnvironmentPack02_Foliage"
            };

            AssetDatabase.CreateAsset(material, FoliageMatPath);
            return material;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 02/Build Test Scene")]
        public static void BuildTestScene()
        {
            if (!PrefabsPresent())
                BuildPrefabs();

            if (!PrefabsPresent())
                return;

            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new GameObject("EnvironmentPack02_Test");

            BuildCollisionGround(root.transform);
            BuildTerrainPatch(root.transform);
            BuildNatureShowcase(root.transform);
            BuildCamera(root.transform);
            BuildLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BuildCollisionGround(Transform parent)
        {
            GameObject ground = new GameObject("GroundCollision");
            ground.transform.SetParent(parent, false);
            ground.transform.position = new Vector3(0f, -0.08f, 0f);

            BoxCollider collider = ground.AddComponent<BoxCollider>();
            collider.size = new Vector3(24f, 0.15f, 18f);
        }

        private static void BuildTerrainPatch(Transform parent)
        {
            Transform terrain = new GameObject("TerrainTiles").transform;
            terrain.SetParent(parent, false);

            string[] ids =
            {
                "Ground_Dirt_A",
                "Ground_Dirt_B",
                "Ground_Grass_A",
                "Ground_Stone_A",
                "Ground_Stone_B",
                "Path_Stone_A"
            };

            int index = 0;
            for (int z = -3; z <= 3; z++)
            {
                for (int x = -4; x <= 4; x++)
                {
                    string id = ids[index++ % ids.Length];

                    Spawn(
                        PrefabRoot + "/Ground/" + id + ".prefab",
                        new Vector3(x * 1.82f, 0f, z * 1.28f),
                        terrain);
                }
            }

            Spawn(
                PrefabRoot + "/Ground/Puddle_A.prefab",
                new Vector3(2.1f, 0.02f, -0.6f),
                terrain);

            Spawn(
                PrefabRoot + "/Ground/Transition_GrassToStone_A.prefab",
                new Vector3(-2.2f, 0.025f, 1.4f),
                terrain);
        }

        private static void BuildNatureShowcase(Transform parent)
        {
            Transform nature = new GameObject("AnimatedNature").transform;
            nature.SetParent(parent, false);

            string grass = PrefabRoot + "/Nature/Grass_Small_A.prefab";
            string tall = PrefabRoot + "/Nature/Grass_Tall_A.prefab";
            string flowers = PrefabRoot + "/Nature/Flower_Blue_A.prefab";

            for (int i = 0; i < 24; i++)
            {
                float x = -7.2f + (i % 8) * 1.8f;
                float z = -4.2f + (i / 8) * 1.3f;
                string path = i % 5 == 0 ? flowers : (i % 2 == 0 ? tall : grass);

                GameObject instance = Spawn(path, new Vector3(x, 0f, z), nature);
                if (instance != null)
                {
                    float scale = 0.82f + (i % 4) * 0.08f;
                    instance.transform.localScale = Vector3.one * scale;
                }
            }

            Spawn(PrefabRoot + "/Trees/Tree_Living_A.prefab", new Vector3(-6.6f, 0f, 3.6f), nature);
            Spawn(PrefabRoot + "/Trees/Tree_Living_B.prefab", new Vector3(6.4f, 0f, 3.7f), nature);
            Spawn(PrefabRoot + "/Trees/Tree_Twisted_A.prefab", new Vector3(-6.8f, 0f, -2.6f), nature);
            Spawn(PrefabRoot + "/Trees/Tree_Dead_A.prefab", new Vector3(6.6f, 0f, -2.8f), nature);
            Spawn(PrefabRoot + "/Trees/Log_A.prefab", new Vector3(4.5f, 0f, -4.1f), nature);
        }

        private static GameObject Spawn(
            string prefabPath,
            Vector3 position,
            Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return null;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return null;

            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            return instance;
        }

        public static void AddShowcaseToScene(Transform parent)
        {
            if (parent == null || !PrefabsPresent())
                return;

            Transform nature = new GameObject("EnvironmentPack02").transform;
            nature.SetParent(parent, false);

            Spawn(PrefabRoot + "/Trees/Tree_Living_A.prefab", new Vector3(-7.4f, 0f, 5.1f), nature);
            Spawn(PrefabRoot + "/Trees/Tree_Living_B.prefab", new Vector3(7.2f, 0f, 4.8f), nature);
            Spawn(PrefabRoot + "/Trees/Tree_Twisted_A.prefab", new Vector3(-7.6f, 0f, -4.5f), nature);
            Spawn(PrefabRoot + "/Trees/Tree_Dead_A.prefab", new Vector3(7.4f, 0f, -4.4f), nature);

            for (int i = 0; i < 12; i++)
            {
                float x = -5.5f + (i % 6) * 2.0f;
                float z = -4.7f + (i / 6) * 8.8f;
                string path = i % 3 == 0
                    ? PrefabRoot + "/Nature/Grass_Tall_A.prefab"
                    : PrefabRoot + "/Nature/Grass_Small_A.prefab";

                Spawn(path, new Vector3(x, 0f, z), nature);
            }

            Spawn(PrefabRoot + "/Nature/Flower_Blue_A.prefab", new Vector3(2.7f, 0f, 3.6f), nature);
            Spawn(PrefabRoot + "/Trees/Log_A.prefab", new Vector3(-4.4f, 0f, -3.7f), nature);
            Spawn(PrefabRoot + "/Ground/Puddle_A.prefab", new Vector3(3.8f, 0.02f, -2.8f), nature);
        }

        private static void BuildCamera(Transform parent)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(10.6f, 12.6f, -13.8f);

            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.014f, 0.03f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;

            go.transform.LookAt(new Vector3(0f, 0.8f, 0.3f));
            go.AddComponent<AudioListener>();
        }

        private static void BuildLighting(Transform parent)
        {
            RenderSettings.ambientMode =
                UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight =
                new Color(0.09f, 0.12f, 0.2f);

            GameObject moon = new GameObject("Moonlight");
            moon.transform.SetParent(parent, false);
            moon.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            Light light = moon.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.46f, 0.57f, 1f);
            light.intensity = 0.7f;
        }

        private static void AddToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene existing in scenes)
            {
                if (existing.path == path)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/KeeperFirstCovenant", "Materials");
            EnsureFolder("Assets/KeeperFirstCovenant/Prefabs/ProductionWorld", "EnvironmentPack02");
            EnsureFolder(PrefabRoot, "Ground");
            EnsureFolder(PrefabRoot, "Nature");
            EnsureFolder(PrefabRoot, "Trees");
            EnsureFolder("Assets/KeeperFirstCovenant", "Scenes");
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
