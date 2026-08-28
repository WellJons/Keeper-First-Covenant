#if UNITY_EDITOR
using System.Collections.Generic;
using KeeperFirstCovenant.Characters;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KeeperFirstCovenant.EditorTools
{
    public static class ProductionArtGameBuilder
    {
        public const string ScenePath =
            "Assets/KeeperFirstCovenant/Scenes/ProductionArt_Playground.unity";

        [MenuItem("Keeper First Covenant/Production Art/BUILD EVERYTHING INTO GAME")]
        public static void BuildEverything()
        {
            ProductionSheetCharacterBuilder.BuildAllCharacters();
            ProductionSheetWorldBuilder.BuildWorld();

            if (ProductionWorldPack01Builder.SourcesPresent())
                ProductionWorldPack01Builder.BuildPrefabs();

            BuildPlayableScene();
            ProductionArtValidator.Validate(true);

            Debug.Log(
                "Approved production art has been built into game assets and the playable scene.");
        }

        [MenuItem("Keeper First Covenant/Production Art/Build Playable Scene")]
        public static void BuildPlayableScene()
        {
            EnsureSceneFolder();

            if (AssetDatabase.LoadAssetAtPath<GameObject>(
                    ProductionSheetCharacterBuilder.EdwardPrefabPath) == null)
            {
                ProductionSheetCharacterBuilder.BuildAllCharacters();
            }

            if (ProductionSheetWorldBuilder.LoadPrefab("StoneFloor_A") == null)
            {
                ProductionSheetWorldBuilder.BuildWorld();
            }

            Scene scene =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);

            GameObject root =
                new GameObject("Keeper_ProductionArt_Playground");

            GameObject systems =
                new GameObject("Systems");

            systems.transform.SetParent(root.transform, false);
            systems.AddComponent<TurnCombatDirector>();

            BuildGround(root.transform);
            BuildRuins(root.transform);

            if (ProductionWorldPack01Builder.SourcesPresent())
                ProductionWorldPack01Builder.AddShowcaseToScene(root.transform);

            GameObject edward =
                InstantiateCharacter(
                    ProductionSheetCharacterBuilder.EdwardPrefabPath,
                    "Edward",
                    new Vector3(-1.2f, 0f, -1.5f),
                    root.transform);

            GameObject eleanor =
                InstantiateCharacter(
                    ProductionSheetCharacterBuilder.EleanorPrefabPath,
                    "Eleanor",
                    new Vector3(-2.4f, 0f, -1.7f),
                    root.transform);

            GameObject aelis =
                InstantiateCharacter(
                    ProductionSheetCharacterBuilder.AelisPrefabPath,
                    "Aelis",
                    new Vector3(-3.3f, 0f, -1.15f),
                    root.transform);

            if (edward != null)
            {
                if (eleanor != null)
                {
                    HighResPartyFollower follower =
                        eleanor.GetComponent<HighResPartyFollower>();

                    if (follower == null)
                        follower = eleanor.AddComponent<HighResPartyFollower>();

                    follower.Leader = edward.transform;
                }

                if (aelis != null)
                {
                    HighResPartyFollower follower =
                        aelis.GetComponent<HighResPartyFollower>();

                    if (follower == null)
                        follower = aelis.AddComponent<HighResPartyFollower>();

                    follower.Leader = edward.transform;
                }
            }

            BuildCamera(
                root.transform,
                edward != null
                    ? edward.transform
                    : null);

            BuildWorldLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = edward;

            Debug.Log(
                "Playable approved-art scene built: " +
                ScenePath +
                " | WASD move, Shift run, J/K attack, G guard, C cast, H/Y hit.");
        }

        private static void BuildGround(Transform parent)
        {
            GameObject collision =
                new GameObject("GroundCollision");

            collision.transform.SetParent(parent, false);
            collision.transform.position =
                new Vector3(0f, -0.13f, 0f);

            BoxCollider collider =
                collision.AddComponent<BoxCollider>();

            collider.size =
                new Vector3(24f, 0.25f, 18f);

            GameObject tiles =
                new GameObject("PaintedGround");

            tiles.transform.SetParent(parent, false);

            float spacingX = 1.65f;
            float spacingZ = 1.05f;

            for (int z = -5; z <= 5; z++)
            {
                for (int x = -7; x <= 7; x++)
                {
                    string id =
                        ((x + z) & 1) == 0
                            ? "StoneFloor_A"
                            : "StoneFloor_B";

                    if (x == 1 && z == 1)
                        id = "RuneFloor";

                    GameObject tile =
                        InstantiateWorld(
                            id,
                            new Vector3(
                                x * spacingX +
                                ((z & 1) == 0 ? 0f : spacingX * 0.5f),
                                0f,
                                z * spacingZ),
                            tiles.transform);

                    if (tile != null)
                        tile.name = id + "_" + x + "_" + z;
                }
            }

            InstantiateWorld(
                "CovenantCircle",
                new Vector3(3.2f, 0.02f, 1.1f),
                tiles.transform);
        }

        private static void BuildRuins(Transform parent)
        {
            GameObject props =
                new GameObject("IndependentWorldProps");

            props.transform.SetParent(parent, false);

            Place("RuinedArch", -5.8f, 4.3f);
            Place("RuinedArchWide", 5.4f, 4.5f);
            Place("BrokenWall", -6.4f, 1.6f);
            Place("WallCorner", 6.2f, 1.1f);
            Place("Pillar", -4.3f, 5.0f);
            Place("BrokenPillar", 4.0f, 5.0f);
            Place("ShrineAltar", 5.4f, -2.5f);
            Place("RuneStone", -5.4f, -2.8f);
            Place("RuneStoneMossy", 3.9f, -4.1f);
            Place("OldTree", -7.1f, -4.7f);
            Place("DeadTree", 7.0f, -4.4f);
            Place("BrazierOrange", 1.8f, 3.7f);
            Place("BrazierBlue", 4.9f, 2.1f);
            Place("Lantern", -2.7f, 4.8f);
            Place("Banner", -4.7f, 3.4f);
            Place("Campfire", -2.7f, -3.5f);
            Place("Crate", -5.7f, 0.0f);
            Place("Barrel", -5.0f, 0.25f);
            Place("Bench", 2.5f, -4.2f);
            Place("Wagon", 6.0f, -1.0f);
            Place("Tent", -6.1f, -3.0f);
            Place("Fence", 6.0f, -4.5f);
            Place("KeeperStatue", 0.5f, 5.4f);
            Place("CrystalPurple", 5.7f, -4.2f);
            Place("SmallShrine", -0.4f, 3.9f);
            Place("Puddle", 0.4f, -2.1f);
            Place("Rock", 3.6f, 3.8f);
            Place("Grass", -4.0f, -3.3f);
            Place("Flowers", 4.2f, -2.8f);
            Place("StoneStairs", 0.0f, 6.0f);
            Place("RoadStraight", -1.0f, 1.7f);
            Place("RoadWide", 1.0f, 0.1f);

            void Place(
                string id,
                float x,
                float z)
            {
                InstantiateWorld(
                    id,
                    new Vector3(x, 0f, z),
                    props.transform);
            }
        }

        private static GameObject InstantiateWorld(
            string id,
            Vector3 position,
            Transform parent)
        {
            GameObject prefab =
                ProductionSheetWorldBuilder.LoadPrefab(id);

            if (prefab == null)
                return null;

            GameObject instance =
                PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (instance == null)
                return null;

            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            return instance;
        }

        private static GameObject InstantiateCharacter(
            string prefabPath,
            string name,
            Vector3 position,
            Transform parent)
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    prefabPath);

            if (prefab == null)
                return null;

            GameObject instance =
                PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (instance == null)
                return null;

            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            return instance;
        }

        private static void BuildCamera(
            Transform parent,
            Transform followTarget)
        {
            GameObject cameraObject =
                new GameObject("Main Camera");

            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position =
                new Vector3(9.2f, 10.7f, -11.5f);

            Camera camera =
                cameraObject.AddComponent<Camera>();

            camera.orthographic = true;
            camera.orthographicSize = 6.1f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(0.012f, 0.022f, 0.045f, 1f);

            cameraObject.transform.LookAt(
                new Vector3(0f, 0.65f, 0.4f));

            cameraObject.AddComponent<AudioListener>();

            TopDownCameraFollow follow =
                cameraObject.AddComponent<TopDownCameraFollow>();

            if (followTarget != null)
            {
                follow.Configure(
                    followTarget,
                    new Vector3(
                        9.2f,
                        10.7f,
                        -11.5f),
                    new Vector3(
                        0f,
                        0.65f,
                        0.4f));
            }
        }

        private static void BuildWorldLighting(Transform parent)
        {
            RenderSettings.ambientMode =
                UnityEngine.Rendering.AmbientMode.Flat;

            RenderSettings.ambientLight =
                new Color(0.12f, 0.15f, 0.24f);

            GameObject moon =
                new GameObject("Moonlight");

            moon.transform.SetParent(parent, false);
            moon.transform.rotation =
                Quaternion.Euler(52f, -28f, 0f);

            Light light =
                moon.AddComponent<Light>();

            light.type = LightType.Directional;
            light.intensity = 0.65f;
            light.color =
                new Color(0.47f, 0.58f, 1f);

            GameObject warm =
                new GameObject("WarmFill");

            warm.transform.SetParent(parent, false);
            warm.transform.position =
                new Vector3(-2.7f, 2.0f, -3.5f);

            Light warmLight =
                warm.AddComponent<Light>();

            warmLight.type = LightType.Point;
            warmLight.range = 7f;
            warmLight.intensity = 2.2f;
            warmLight.color =
                new Color(1f, 0.28f, 0.07f);
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder(
                    "Assets/KeeperFirstCovenant/Scenes"))
            {
                AssetDatabase.CreateFolder(
                    "Assets/KeeperFirstCovenant",
                    "Scenes");
            }
        }

        private static void AddSceneToBuildSettings(
            string path)
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(
                    EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene scene in scenes)
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
    }
}
#endif
