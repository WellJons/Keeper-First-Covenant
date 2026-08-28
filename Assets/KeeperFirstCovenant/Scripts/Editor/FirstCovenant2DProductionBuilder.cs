#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class FirstCovenant2DProductionBuilder
    {
        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/Edward_2D_Production_Test.unity";

        [MenuItem("Keeper First Covenant/2D Production/Build Edward + World Kit")]
        public static void BuildAll()
        {
            Edward2DProductionBuilder.BuildEdward();
            FirstCovenant2DWorldBuilder.BuildWorldKit();
            BuildTestScene();
            FirstCovenant2DProductionValidator.ValidateGeneratedAssets(true);
        }

        [MenuItem("Keeper First Covenant/2D Production/Build Test Scene")]
        public static void BuildTestScene()
        {
            EnsureScenesFolder();

            GameObject edwardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                Edward2DProductionBuilder.EdwardPrefabPath);

            if (edwardPrefab == null)
            {
                Edward2DProductionBuilder.BuildEdward();
                edwardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Edward2DProductionBuilder.EdwardPrefabPath);
            }

            FirstCovenant2DWorldBuilder.PropInfo[] props =
                FirstCovenant2DWorldBuilder.EnsureAndGetProps();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new GameObject("Keeper_2D_Production_Test");

            BuildGround(root.transform);
            BuildWorld(root.transform, props);

            if (edwardPrefab != null)
            {
                GameObject edward = PrefabUtility.InstantiatePrefab(edwardPrefab) as GameObject;
                if (edward != null)
                {
                    edward.name = "Edward";
                    edward.transform.SetParent(root.transform);
                    edward.transform.position = new Vector3(-1.2f, 0f, -1.6f);
                }
            }

            BuildCamera(root.transform);
            BuildSceneMarkers(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = root;
            Debug.Log(
                "Keeper 2D production test ready. Open " + ScenePath +
                " and press Play. WASD/Shift move; J/K attack; G guard; C cast; " +
                "H/Y hit; X weapon; V armor; B cloak; Delete death; R revive.");
        }

        private static void BuildGround(Transform parent)
        {
            GameObject collision = new GameObject("GroundCollision");
            collision.transform.SetParent(parent);
            collision.transform.position = new Vector3(0f, -0.14f, 0f);

            BoxCollider groundCollider = collision.AddComponent<BoxCollider>();
            groundCollider.size = new Vector3(30f, 0.25f, 24f);

            GameObject tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                FirstCovenant2DWorldBuilder.PrefabRoot + "/AncientStoneTile.prefab");

            if (tilePrefab == null)
                return;

            GameObject tiles = new GameObject("DrawnGroundTiles");
            tiles.transform.SetParent(parent);

            const float spacing = 3.35f;
            for (int z = -3; z <= 3; z++)
            {
                for (int x = -4; x <= 4; x++)
                {
                    GameObject tile = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
                    if (tile == null)
                        continue;

                    tile.name = "StoneTile_" + x + "_" + z;
                    tile.transform.SetParent(tiles.transform);
                    tile.transform.position = new Vector3(
                        x * spacing + (z % 2 == 0 ? 0f : 0.18f),
                        0f,
                        z * spacing);
                    tile.transform.rotation = Quaternion.Euler(
                        0f,
                        ((x + z) & 1) == 0 ? 0f : 180f,
                        0f);
                }
            }
        }

        private static void BuildWorld(
            Transform parent,
            FirstCovenant2DWorldBuilder.PropInfo[] props)
        {
            Dictionary<string, string> paths = new Dictionary<string, string>();
            foreach (FirstCovenant2DWorldBuilder.PropInfo prop in props)
                paths[prop.id] = prop.prefabPath;

            GameObject world = new GameObject("ReusableWorldProps");
            world.transform.SetParent(parent);

            Place(paths, world.transform, "BrokenWall", new Vector3(-6.7f, 0f, 2.8f), 8f);
            Place(paths, world.transform, "BrokenWall", new Vector3(6.2f, 0f, 4.4f), -7f);
            Place(paths, world.transform, "BrokenPillar", new Vector3(-3.9f, 0f, 5.2f), 0f);
            Place(paths, world.transform, "BrokenPillar", new Vector3(4.3f, 0f, -3.3f), 0f);
            Place(paths, world.transform, "AncientShrine", new Vector3(5.5f, 0f, 1.0f), 0f);
            Place(paths, world.transform, "RuneStone", new Vector3(-4.7f, 0f, -2.1f), 0f);
            Place(paths, world.transform, "OldTree", new Vector3(-7.2f, 0f, -4.7f), 0f);
            Place(paths, world.transform, "OldTree", new Vector3(7.0f, 0f, -4.2f), 0f);
            Place(paths, world.transform, "CovenantBanner", new Vector3(-1.4f, 0f, 5.8f), 0f);
            Place(paths, world.transform, "Brazier", new Vector3(2.4f, 0f, 3.2f), 0f);
            Place(paths, world.transform, "RockCluster", new Vector3(-6.1f, 0f, 5.9f), 0f);
            Place(paths, world.transform, "RockCluster", new Vector3(7.4f, 0f, 1.8f), 0f);

            Place(paths, world.transform, "StoneStairs", new Vector3(0f, 0.015f, 6.7f), 0f);
            Place(paths, world.transform, "Puddle", new Vector3(1.1f, 0.02f, -2.7f), 22f);

            for (int i = 0; i < 13; i++)
            {
                float angle = i * 37f;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 position = new Vector3(
                    Mathf.Cos(radians) * (4.5f + (i % 3) * 1.15f),
                    0f,
                    Mathf.Sin(radians) * (3.3f + (i % 4) * 0.8f));
                Place(paths, world.transform, "GrassClump", position, angle);
            }
        }

        private static void Place(
            Dictionary<string, string> paths,
            Transform parent,
            string id,
            Vector3 position,
            float yRotation)
        {
            if (!paths.TryGetValue(id, out string path))
                return;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return;

            instance.name = id;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private static void BuildCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(9.8f, 11.6f, -12.4f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.1f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.038f, 0.055f, 1f);

            cameraObject.transform.LookAt(new Vector3(0f, 0.8f, 0.5f));

            AudioListener listener = cameraObject.AddComponent<AudioListener>();
            EditorUtility.SetDirty(listener);
        }

        private static void BuildSceneMarkers(Transform parent)
        {
            GameObject markerRoot = new GameObject("ProductionNotes");
            markerRoot.transform.SetParent(parent);

            TextMesh note = markerRoot.AddComponent<TextMesh>();
            note.text =
                "EDWARD 2D PRODUCTION TEST\n" +
                "Every visible object is a separate 2D asset/prefab.\n" +
                "Press Play for movement + animation controls.";
            note.fontSize = 34;
            note.characterSize = 0.045f;
            note.anchor = TextAnchor.MiddleCenter;
            note.alignment = TextAlignment.Center;
            note.color = new Color(0.72f, 0.80f, 0.82f, 0.78f);

            markerRoot.transform.position = new Vector3(0f, 0.03f, -7.4f);
            markerRoot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/KeeperFirstCovenant/Scenes"))
                AssetDatabase.CreateFolder("Assets/KeeperFirstCovenant", "Scenes");
        }

        private static void AddToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            bool exists = false;
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
