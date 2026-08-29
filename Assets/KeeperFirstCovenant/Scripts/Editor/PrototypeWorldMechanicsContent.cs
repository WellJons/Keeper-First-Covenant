#if UNITY_EDITOR
using System.Linq;
using KeeperFirstCovenant.Combat;
using KeeperFirstCovenant.World;
using UnityEditor;
using UnityEngine;

namespace KeeperFirstCovenant.EditorTools
{
    public static class PrototypeWorldMechanicsContent
    {
        private const string RootName =
            "DEV_WorldMechanics";

        public static void Build()
        {
            GameObject existing =
                GameObject.Find(RootName);

            if (existing != null)
            {
                Object.DestroyImmediate(
                    existing);
            }

            CombatantRuntime[] combatants =
                Object.FindObjectsByType<
                    CombatantRuntime>(
                    FindObjectsSortMode.None);

            CombatantRuntime leader =
                combatants
                    .Where(x =>
                        x != null &&
                        x.Definition != null &&
                        x.Definition.characterId ==
                            "edward")
                    .FirstOrDefault();

            Vector3 anchor =
                leader != null
                    ? leader.transform.position
                    : Vector3.zero;

            GameObject root =
                new GameObject(RootName);

            Undo.RegisterCreatedObjectUndo(
                root,
                "Create DEV world mechanics");

            CreateLockedDoor(
                root.transform,
                anchor +
                new Vector3(
                    -4.5f,
                    0f,
                    5.5f));

            CreateBreakableCrate(
                root.transform,
                anchor +
                new Vector3(
                    -2.5f,
                    0.55f,
                    5.5f));

            CreateLooseProp(
                root.transform,
                anchor +
                new Vector3(
                    -1.1f,
                    0.35f,
                    5.2f));
        }

        private static void CreateLockedDoor(
            Transform parent,
            Vector3 position)
        {
            GameObject hinge =
                new GameObject(
                    "DEV_LockedDoor");

            hinge.transform.SetParent(
                parent,
                false);

            hinge.transform.position =
                position;

            GameObject door =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            door.name = "DoorBody";

            door.transform.SetParent(
                hinge.transform,
                false);

            door.transform.localPosition =
                new Vector3(
                    0f,
                    1.1f,
                    0f);

            door.transform.localScale =
                new Vector3(
                    0.28f,
                    2.2f,
                    1.8f);

            LockableDoor lockable =
                hinge.AddComponent<
                    LockableDoor>();

            lockable.ConfigurePrototype(
                true,
                "dev_road_key",
                "dev_lockpick",
                12,
                14);

            EnvironmentalDestructible destructible =
                hinge.AddComponent<
                    EnvironmentalDestructible>();

            destructible.ConfigurePrototype(
                34f,
                ImpactTier.Devastating,
                4f);
        }

        private static void CreateBreakableCrate(
            Transform parent,
            Vector3 position)
        {
            GameObject crate =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            crate.name =
                "DEV_BreakableCrate";

            crate.transform.SetParent(
                parent,
                false);

            crate.transform.position =
                position;

            crate.transform.localScale =
                new Vector3(
                    1.05f,
                    1.05f,
                    1.05f);

            Rigidbody body =
                crate.AddComponent<Rigidbody>();

            body.isKinematic = true;
            body.mass = 2f;

            EnvironmentalDestructible destructible =
                crate.AddComponent<
                    EnvironmentalDestructible>();

            destructible.ConfigurePrototype(
                16f,
                ImpactTier.Heavy,
                5f);
        }

        private static void CreateLooseProp(
            Transform parent,
            Vector3 position)
        {
            GameObject prop =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere);

            prop.name =
                "DEV_LoosePhysicsProp";

            prop.transform.SetParent(
                parent,
                false);

            prop.transform.position =
                position;

            prop.transform.localScale =
                Vector3.one * 0.65f;

            Rigidbody body =
                prop.AddComponent<Rigidbody>();

            body.mass = 0.7f;
        }
    }
}
#endif
