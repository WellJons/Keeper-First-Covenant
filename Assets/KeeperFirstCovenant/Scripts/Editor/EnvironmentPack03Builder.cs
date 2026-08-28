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
    public static class EnvironmentPack03Builder
    {
        public const string ArtRoot =
            "Assets/KeeperFirstCovenant/Art/Runtime/World/EnvironmentPack03";

        public const string PrefabRoot =
            "Assets/KeeperFirstCovenant/Prefabs/ProductionWorld/EnvironmentPack03";

        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/EnvironmentPack03_Test.unity";

        private const string GroundMaterialPath =
            "Assets/KeeperFirstCovenant/Materials/EnvironmentPack02_Ground.mat";

        private const string FoliageMaterialPath =
            "Assets/KeeperFirstCovenant/Materials/EnvironmentPack02_Foliage.mat";

        private static readonly string[] RequiredSprites =
        {
            "Rocks/Rock_SmallPile_A.png",
            "Rocks/Rock_Cluster_Medium_A.png",
            "Rocks/Rock_Shard_Tall_A.png",
            "Rocks/Rock_Boulder_Mossy_A.png",
            "Rocks/Rock_Rubble_Flat_A.png",

            "Nature/Grass_Clump_B.png",
            "Nature/Grass_Dense_A.png",
            "Nature/Flower_Blue_B.png",
            "Nature/Flower_Pale_A.png",

            "Trees/Tree_Leafy_C.png",
            "Trees/Sapling_A.png",
            "Trees/Stump_B.png",

            "Fillers/Puddle_MuddyCluster_A.png",
            "Fillers/BrokenSignpost_A.png",
            "Fillers/Debris_Small_A.png"
        };

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 03/BUILD EVERYTHING")]
        public static void BuildAll()
        {
            if (!SourcesPresent())
            {
                Debug.LogWarning(
                    "Environment Pack 03 source PNGs are incomplete: " +
                    ArtRoot);
                return;
            }

            EnsureFolders();
            BuildPrefabs();
            BuildTestScene();
            EnvironmentPack03Validator.Validate(true);

            Debug.Log(
                "Environment Pack 03 ready: rocks, animated plants, new tree silhouettes and environmental fillers.");
        }

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 03/Build Prefabs")]
        public static void BuildPrefabs()
        {
            EnsureFolders();

            Material groundMaterial = LoadOrCreateGroundMaterial();
            Material foliageMaterial = LoadOrCreateFoliageMaterial();

            BuildRock(
                "Rock_SmallPile_A",
                groundMaterial,
                new Vector3(0f, 0.30f, 0f),
                new Vector3(1.25f, 0.60f, 0.85f));

            BuildRock(
                "Rock_Cluster_Medium_A",
                groundMaterial,
                new Vector3(0f, 0.48f, 0f),
                new Vector3(1.50f, 0.95f, 0.95f));

            BuildRock(
                "Rock_Shard_Tall_A",
                groundMaterial,
                new Vector3(0f, 0.76f, 0f),
                new Vector3(0.92f, 1.52f, 0.78f));

            BuildRock(
                "Rock_Boulder_Mossy_A",
                groundMaterial,
                new Vector3(0f, 0.56f, 0f),
                new Vector3(1.45f, 1.12f, 1.00f));

            BuildFlatRubble(
                "Rock_Rubble_Flat_A",
                groundMaterial);

            BuildFoliage(
                "Grass_Clump_B",
                foliageMaterial,
                0.082f,
                1.40f,
                2.7f,
                0.25f,
                new Vector2(0.86f, 1.14f));

            BuildFoliage(
                "Grass_Dense_A",
                foliageMaterial,
                0.105f,
                1.22f,
                3.0f,
                0.32f,
                new Vector2(0.90f, 1.16f));

            BuildFoliage(
                "Flower_Blue_B",
                foliageMaterial,
                0.050f,
                1.10f,
                3.4f,
                0.17f,
                new Vector2(0.90f, 1.08f));

            BuildFoliage(
                "Flower_Pale_A",
                foliageMaterial,
                0.045f,
                1.05f,
                3.5f,
                0.16f,
                new Vector2(0.92f, 1.08f));

            BuildTree(
                "Tree_Leafy_C",
                foliageMaterial,
                0.034f,
                0.70f,
                4.7f,
                0.15f,
                0.50f);

            BuildSapling(
                "Sapling_A",
                foliageMaterial);

            BuildStump(
                "Stump_B",
                groundMaterial);

            BuildMuddyPuddle(
                "Puddle_MuddyCluster_A",
                groundMaterial);

            BuildBrokenSignpost(
                "BrokenSignpost_A",
                groundMaterial);

            BuildDebris(
                "Debris_Small_A",
                groundMaterial);

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
                       PrefabRoot + "/Rocks/Rock_Cluster_Medium_A.prefab") != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(
                       PrefabRoot + "/Nature/Grass_Dense_A.prefab") != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(
                       PrefabRoot + "/Trees/Tree_Leafy_C.prefab") != null &&
                   AssetDatabase.LoadAssetAtPath<GameObject>(
                       PrefabRoot + "/Fillers/Puddle_MuddyCluster_A.prefab") != null;
        }

        private static Sprite LoadSprite(
            string category,
            string id)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                ArtRoot + "/" + category + "/" + id + ".png");
        }

        private static void BuildRock(
            string id,
            Material material,
            Vector3 colliderCenter,
            Vector3 colliderSize)
        {
            Sprite sprite = LoadSprite("Rocks", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(
                root.transform,
                sprite,
                material,
                15);

            visual.AddComponent<BillboardCharacter2D>();

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center = colliderCenter;
            collider.size = colliderSize;

            SavePrefab(
                root,
                PrefabRoot + "/Rocks/" + id + ".prefab");
        }

        private static void BuildFlatRubble(
            string id,
            Material material)
        {
            Sprite sprite = LoadSprite("Rocks", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -7;

            SavePrefab(
                root,
                PrefabRoot + "/Rocks/" + id + ".prefab");
        }

        private static void BuildFoliage(
            string id,
            Material material,
            float strength,
            float speed,
            float stiffness,
            float gust,
            Vector2 scaleRange)
        {
            Sprite sprite = LoadSprite("Nature", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(
                root.transform,
                sprite,
                material,
                13);

            visual.AddComponent<BillboardCharacter2D>();

            WindReactiveProp wind =
                root.AddComponent<WindReactiveProp>();

            wind.Configure(
                strength,
                speed,
                0.80f,
                stiffness,
                gust);

            EnvironmentVisualVariation variation =
                root.AddComponent<EnvironmentVisualVariation>();

            variation.Configure(
                visual.transform,
                scaleRange,
                true,
                0.015f);

            SavePrefab(
                root,
                PrefabRoot + "/Nature/" + id + ".prefab");
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
            GameObject visual = CreateVerticalVisual(
                root.transform,
                sprite,
                material,
                23);

            visual.AddComponent<BillboardCharacter2D>();

            WindReactiveProp wind =
                root.AddComponent<WindReactiveProp>();

            wind.Configure(
                strength,
                speed,
                0.42f,
                stiffness,
                gust);

            CapsuleCollider trunk =
                root.AddComponent<CapsuleCollider>();

            trunk.direction = 1;
            trunk.radius = trunkRadius;
            trunk.height = Mathf.Max(
                1.6f,
                sprite.bounds.size.y * 0.48f);
            trunk.center =
                Vector3.up * (trunk.height * 0.50f);

            SavePrefab(
                root,
                PrefabRoot + "/Trees/" + id + ".prefab");
        }

        private static void BuildSapling(
            string id,
            Material material)
        {
            Sprite sprite = LoadSprite("Trees", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(
                root.transform,
                sprite,
                material,
                17);

            visual.AddComponent<BillboardCharacter2D>();

            WindReactiveProp wind =
                root.AddComponent<WindReactiveProp>();

            wind.Configure(
                0.055f,
                0.90f,
                0.55f,
                3.9f,
                0.18f);

            EnvironmentVisualVariation variation =
                root.AddComponent<EnvironmentVisualVariation>();

            variation.Configure(
                visual.transform,
                new Vector2(0.88f, 1.12f),
                true,
                0.015f);

            // Saplings intentionally do not block movement.
            SavePrefab(
                root,
                PrefabRoot + "/Trees/" + id + ".prefab");
        }

        private static void BuildStump(
            string id,
            Material material)
        {
            Sprite sprite = LoadSprite("Trees", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(
                root.transform,
                sprite,
                material,
                16);

            visual.AddComponent<BillboardCharacter2D>();

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center =
                new Vector3(0f, 0.26f, 0f);

            collider.size =
                new Vector3(0.78f, 0.52f, 0.68f);

            SavePrefab(
                root,
                PrefabRoot + "/Trees/" + id + ".prefab");
        }

        private static void BuildMuddyPuddle(
            string id,
            Material material)
        {
            Sprite sprite = LoadSprite("Fillers", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -9;

            BoxCollider trigger =
                root.AddComponent<BoxCollider>();

            trigger.isTrigger = true;
            trigger.center =
                new Vector3(0f, 0.04f, 0f);
            trigger.size =
                new Vector3(3.6f, 0.08f, 1.6f);

            EnvironmentSurfaceMarker surface =
                root.AddComponent<EnvironmentSurfaceMarker>();

            surface.Configure(
                EnvironmentSurfaceType.Water,
                true,
                0.68f);

            SavePrefab(
                root,
                PrefabRoot + "/Fillers/" + id + ".prefab");
        }

        private static void BuildBrokenSignpost(
            string id,
            Material material)
        {
            Sprite sprite = LoadSprite("Fillers", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = CreateVerticalVisual(
                root.transform,
                sprite,
                material,
                16);

            visual.AddComponent<BillboardCharacter2D>();

            BoxCollider collider =
                root.AddComponent<BoxCollider>();

            collider.center =
                new Vector3(0f, 0.36f, 0f);

            collider.size =
                new Vector3(1.35f, 0.72f, 0.58f);

            SavePrefab(
                root,
                PrefabRoot + "/Fillers/" + id + ".prefab");
        }

        private static void BuildDebris(
            string id,
            Material material)
        {
            Sprite sprite = LoadSprite("Fillers", id);
            if (sprite == null)
                return;

            GameObject root = new GameObject(id);
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localRotation =
                Quaternion.Euler(90f, 0f, 0f);

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -6;

            // Small debris is visual-only so movement does not snag on clutter.
            SavePrefab(
                root,
                PrefabRoot + "/Fillers/" + id + ".prefab");
        }

        private static GameObject CreateVerticalVisual(
            Transform parent,
            Sprite sprite,
            Material material,
            int sortingOrder)
        {
            GameObject visual =
                new GameObject("Visual");

            visual.transform.SetParent(parent, false);

            SpriteRenderer renderer =
                visual.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;

            visual.transform.localPosition =
                Vector3.up * sprite.bounds.extents.y;

            return visual;
        }

        private static Material LoadOrCreateGroundMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    GroundMaterialPath);

            if (material != null)
                return material;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                throw new InvalidOperationException(
                    "Sprites/Default shader is unavailable.");

            material = new Material(shader)
            {
                name = "EnvironmentPack02_Ground"
            };

            AssetDatabase.CreateAsset(
                material,
                GroundMaterialPath);

            return material;
        }

        private static Material LoadOrCreateFoliageMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    FoliageMaterialPath);

            if (material != null)
                return material;

            Shader shader =
                Shader.Find(
                    "Keeper First Covenant/Foliage Wind");

            if (shader == null)
                throw new InvalidOperationException(
                    "FoliageWind shader is unavailable. Environment Pack 02 must be present.");

            material = new Material(shader)
            {
                name = "EnvironmentPack02_Foliage"
            };

            AssetDatabase.CreateAsset(
                material,
                FoliageMaterialPath);

            return material;
        }

        private static void SavePrefab(
            GameObject root,
            string path)
        {
            PrefabUtility.SaveAsPrefabAsset(
                root,
                path);

            UnityEngine.Object.DestroyImmediate(root);
        }

        [MenuItem("Keeper First Covenant/Production Art/Environment Pack 03/Build Test Scene")]
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
                    "EnvironmentPack03_Test");

            BuildCollisionGround(root.transform);
            BuildShowcase(root.transform);
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
            if (parent == null || !PrefabsPresent())
                return;

            Transform root =
                new GameObject(
                    "EnvironmentPack03").transform;

            root.SetParent(parent, false);

            Spawn(
                PrefabRoot + "/Rocks/Rock_Cluster_Medium_A.prefab",
                new Vector3(-6.7f, 0f, 0.6f),
                root);

            Spawn(
                PrefabRoot + "/Rocks/Rock_Boulder_Mossy_A.prefab",
                new Vector3(6.5f, 0f, -0.4f),
                root);

            Spawn(
                PrefabRoot + "/Trees/Tree_Leafy_C.prefab",
                new Vector3(0f, 0f, 6.3f),
                root);

            Spawn(
                PrefabRoot + "/Trees/Stump_B.prefab",
                new Vector3(4.7f, 0f, 4.7f),
                root);

            Spawn(
                PrefabRoot + "/Fillers/BrokenSignpost_A.prefab",
                new Vector3(-3.8f, 0f, -4.8f),
                root);

            Spawn(
                PrefabRoot + "/Fillers/Puddle_MuddyCluster_A.prefab",
                new Vector3(3.4f, 0.02f, -4.2f),
                root);

            for (int i = 0; i < 14; i++)
            {
                string path =
                    i % 4 == 0
                        ? PrefabRoot + "/Nature/Flower_Blue_B.prefab"
                        : i % 3 == 0
                            ? PrefabRoot + "/Nature/Flower_Pale_A.prefab"
                            : i % 2 == 0
                                ? PrefabRoot + "/Nature/Grass_Dense_A.prefab"
                                : PrefabRoot + "/Nature/Grass_Clump_B.prefab";

                float x = -5.5f + (i % 7) * 1.8f;
                float z = -5.8f + (i / 7) * 11.5f;

                Spawn(
                    path,
                    new Vector3(x, 0f, z),
                    root);
            }
        }

        private static void BuildCollisionGround(
            Transform parent)
        {
            GameObject ground =
                new GameObject("GroundCollision");

            ground.transform.SetParent(parent, false);
            ground.transform.position =
                new Vector3(0f, -0.08f, 0f);

            BoxCollider collider =
                ground.AddComponent<BoxCollider>();

            collider.size =
                new Vector3(24f, 0.15f, 18f);
        }

        private static void BuildShowcase(
            Transform parent)
        {
            Transform root =
                new GameObject(
                    "Pack03_Assets").transform;

            root.SetParent(parent, false);

            string[] rockIds =
            {
                "Rock_SmallPile_A",
                "Rock_Cluster_Medium_A",
                "Rock_Shard_Tall_A",
                "Rock_Boulder_Mossy_A",
                "Rock_Rubble_Flat_A"
            };

            for (int i = 0; i < rockIds.Length; i++)
            {
                Spawn(
                    PrefabRoot +
                    "/Rocks/" +
                    rockIds[i] +
                    ".prefab",
                    new Vector3(
                        -6.6f + i * 3.2f,
                        0f,
                        3.8f),
                    root);
            }

            string[] natureIds =
            {
                "Grass_Clump_B",
                "Grass_Dense_A",
                "Flower_Blue_B",
                "Flower_Pale_A"
            };

            for (int i = 0; i < natureIds.Length; i++)
            {
                Spawn(
                    PrefabRoot +
                    "/Nature/" +
                    natureIds[i] +
                    ".prefab",
                    new Vector3(
                        -4.6f + i * 3.0f,
                        0f,
                        0.4f),
                    root);
            }

            Spawn(
                PrefabRoot + "/Trees/Tree_Leafy_C.prefab",
                new Vector3(-5.5f, 0f, -3.5f),
                root);

            Spawn(
                PrefabRoot + "/Trees/Sapling_A.prefab",
                new Vector3(-2.2f, 0f, -3.6f),
                root);

            Spawn(
                PrefabRoot + "/Trees/Stump_B.prefab",
                new Vector3(0.4f, 0f, -3.7f),
                root);

            Spawn(
                PrefabRoot + "/Fillers/Puddle_MuddyCluster_A.prefab",
                new Vector3(4.3f, 0.02f, -3.6f),
                root);

            Spawn(
                PrefabRoot + "/Fillers/BrokenSignpost_A.prefab",
                new Vector3(6.1f, 0f, -1.3f),
                root);

            Spawn(
                PrefabRoot + "/Fillers/Debris_Small_A.prefab",
                new Vector3(5.4f, 0.01f, 1.6f),
                root);
        }

        private static GameObject Spawn(
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

            instance.transform.SetParent(
                parent,
                true);

            instance.transform.position =
                position;

            return instance;
        }

        private static void BuildCamera(
            Transform parent)
        {
            GameObject go =
                new GameObject("Main Camera");

            go.tag = "MainCamera";
            go.transform.SetParent(parent, false);
            go.transform.position =
                new Vector3(10.2f, 11.8f, -13.0f);

            Camera camera =
                go.AddComponent<Camera>();

            camera.orthographic = true;
            camera.orthographicSize = 7.0f;
            camera.clearFlags =
                CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(0.008f, 0.014f, 0.03f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;

            go.transform.LookAt(
                new Vector3(0f, 0.75f, 0f));

            go.AddComponent<AudioListener>();
        }

        private static void BuildLighting(
            Transform parent)
        {
            RenderSettings.ambientMode =
                UnityEngine.Rendering.AmbientMode.Flat;

            RenderSettings.ambientLight =
                new Color(0.09f, 0.12f, 0.20f);

            GameObject moon =
                new GameObject("Moonlight");

            moon.transform.SetParent(parent, false);
            moon.transform.rotation =
                Quaternion.Euler(52f, -35f, 0f);

            Light light =
                moon.AddComponent<Light>();

            light.type =
                LightType.Directional;

            light.color =
                new Color(0.46f, 0.57f, 1f);

            light.intensity = 0.70f;
        }

        private static void AddToBuildSettings(
            string path)
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(
                    EditorBuildSettings.scenes);

            foreach (
                EditorBuildSettingsScene existing
                in scenes)
            {
                if (existing.path == path)
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
                "Assets/KeeperFirstCovenant/Prefabs/ProductionWorld",
                "EnvironmentPack03");

            EnsureFolder(PrefabRoot, "Rocks");
            EnsureFolder(PrefabRoot, "Nature");
            EnsureFolder(PrefabRoot, "Trees");
            EnsureFolder(PrefabRoot, "Fillers");

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
            {
                AssetDatabase.CreateFolder(
                    parent,
                    child);
            }
        }
    }
}
#endif
