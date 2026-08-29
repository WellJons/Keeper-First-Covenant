# Persistent World Objects

Interactive scene state now participates in normal save/load.

The generic IPersistentWorldObject interface is used by:
- SearchableLoot
- LockableDoor
- EnvironmentalDestructible
- HiddenDiscoverable
- TrapMechanism

Each object exposes a stable PersistenceId. If a designer does not set an explicit ID,
WorldPersistenceUtility derives one from:
scene + hierarchy path + sibling indices + component type.

Save payload stores:
- persistence ID;
- component type;
- position;
- rotation;
- component-specific JSON state.

Restored examples:
- searched caches stay searched;
- doors keep locked/open state;
- discovered secrets stay visible;
- spent/disarmed traps stay spent;
- destroyed obstacles stay destroyed.

For important authored story objects, an explicit persistenceId is still recommended so hierarchy refactors cannot invalidate old saves.
