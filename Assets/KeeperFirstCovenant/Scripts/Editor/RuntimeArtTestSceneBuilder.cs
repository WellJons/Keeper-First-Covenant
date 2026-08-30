#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class RuntimeArtTestSceneBuilder
    {
        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/HighRes_RuntimeArt_Test.unity";

        private const string WorldRoot =
            "Assets/KeeperFirstCovenant/Prefabs/RuntimeWorld/";

        [MenuItem("Keeper First Covenant/High-Res 2D/Build Painted Runtime Test Scene")]
        public static void Build()
        {
            RuntimeAtlasBootstrapBuilder.HydrateAndBuild();

            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);

            GameObject root =
                new GameObject(
                    "HighRes_RuntimeArt_Test");

            BuildGround(root.transform);
            BuildRuins(root.transform);
            SpawnEdward(root.transform);
            BuildCamera(root.transform);
            BuildLight(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(
                scene,
                ScenePath);

            AddToBuildSettings(ScenePath);

            Selection.activeGameObject = root;

            Debug.Log(
                "Painted runtime test scene built: " +
                ScenePath +
                ". Press Play: WASD / Shift. " +
                "The visible Edward and world props come from packed painted art.");
        }

        private static void BuildGround(
            Transform parent)
        {
            GameObject collision =
                new GameObject("GroundCollision");

            collision.transform.SetParent(
                parent,
                false);

            collision.transform.position =
                new Vector3(0f, -0.12f, 0f);

            BoxCollider collider =
                collision.AddComponent<BoxCollider>();

            collider.size =
                new Vector3(28f, 0.2f, 22f);

            GameObject floor =
                Load("StoneFloor");

            if (floor == null)
                return;

            GameObject floorRoot =
                new GameObject("PaintedFloor");

            floorRoot.transform.SetParent(
                parent,
                false);

            for (int z = -3; z <= 3; z++)
            {
                for (int x = -4; x <= 4; x++)
                {
                    GameObject tile =
                        PrefabUtility.InstantiatePrefab(
                            floor) as GameObject;

                    if (tile == null)
                        continue;

                    tile.transform.SetParent(
                        floorRoot.transform,
                        false);

                    tile.transform.position =
                        new Vector3(
                            x * 2.15f,
                            0.005f,
                            z * 2.15f);
                }
            }
        }

        private static void BuildRuins(
            Transform parent)
        {
            GameObject props =
                new GameObject("PaintedWorldProps");

            props.transform.SetParent(
                parent,
                false);

            Place(
                props.transform,
                "RuinedArch",
                new Vector3(-5.5f, 0f, 3.7f));

            Place(
                props.transform,
                "RuinedArch",
                new Vector3(5.6f, 0f, 4.0f));

            Place(
                props.transform,
                "BrokenWall",
                new Vector3(-6.2f, 0f, -1.5f));

            Place(
                props.transform,
                "RockMonolith",
                new Vector3(6.0f, 0f, -2.8f));

            Place(
                props.transform,
                "ShrineAltar",
                new Vector3(3.4f, 0f, 2.4f));

            Place(
                props.transform,
                "CovenantRuneCircle",
                new Vector3(0.7f, 0.015f, 1.1f));

            Place(
                props.transform,
                "Campfire",
                new Vector3(-3.6f, 0f, -3.2f));

            Place(
                props.transform,
                "Brazier",
                new Vector3(-1.2f, 0f, 4.5f));

            Place(
                props.transform,
                "Wagon",
                new Vector3(5.0f, 0f, -4.4f));

            Place(
                props.transform,
                "CovenantCrystal",
                new Vector3(-5.2f, 0f, 5.0f));
        }

        private static void SpawnEdward(
            Transform parent)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    HighResEdwardPrefabBuilder.PrefabPath);

            if (prefab == null)
                return;

            GameObject edward =
                PrefabUtility.InstantiatePrefab(
                    prefab) as GameObject;

            if (edward == null)
                return;

            edward.name = "Edward";
            edward.transform.SetParent(
                parent,
                false);

            edward.transform.position =
                new Vector3(-1.5f, 0f, -1.8f);
        }

        private static void BuildCamera(
            Transform parent)
        {
            GameObject go =
                new GameObject("Main Camera");

            go.tag = "MainCamera";
            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                new Vector3(
                    10.2f,
                    12.0f,
                    -13.2f);

            Camera camera =
                go.AddComponent<Camera>();

            camera.orthographic = true;
            camera.orthographicSize = 7.2f;
            camera.backgroundColor =
                new Color(
                    0.018f,
                    0.025f,
                    0.05f,
                    1f);

            camera.clearFlags =
                CameraClearFlags.SolidColor;

            go.transform.LookAt(
                new Vector3(
                    0f,
                    0.8f,
                    0.5f));

            go.AddComponent<AudioListener>();
        }

        private static void BuildLight(
            Transform parent)
        {
            GameObject lightObject =
                new GameObject("MoonKeyLight");

            lightObject.transform.SetParent(
                parent,
                false);

            lightObject.transform.rotation =
                Quaternion.Euler(
                    52f,
                    -38f,
                    0f);

            Light light =
                lightObject.AddComponent<Light>();

            light.type =
                LightType.Directional;

            light.intensity = 0.75f;
            light.color =
                new Color(
                    0.56f,
                    0.67f,
                    1f,
                    1f);
        }

        private static void Place(
            Transform parent,
            string id,
            Vector3 position)
        {
            GameObject prefab = Load(id);

            if (prefab == null)
                return;

            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    prefab) as GameObject;

            if (instance == null)
                return;

            instance.transform.SetParent(
                parent,
                false);

            instance.transform.position =
                position;
        }

        private static GameObject Load(
            string id)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(
                WorldRoot +
                id +
                ".prefab");
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
    }
}
#endif
