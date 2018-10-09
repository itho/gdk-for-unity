[//]: # (Doc of docs reference 5.1)

# (GameObject-MonoBehaviour) SpatialOS entities: How to link SpatialOS entities with GameObjects
_This document relates to the GameObject-MonoBehaviour workflow._

Before reading this document, make sure you are familiar with:
* The SpatialOS entity section of [GameObject-MonoBehaviour and ECS workflows: SpatialOS entities](../intro-workflows-spos-entities.md)
* [Workers in the GDK](../workers/workers-in-the-gdk.md)

As described in the [SpatialOS entity background documentation](../intro-workflows-spos-entities.md), you represent SpatialOS entities with GameObjects in a Scene by creating the SpatialOS entity first, then linking it to a GameObject in a Scene.

This document is a guide on how to link a SpatialOS entity with a GameObject using the GameObject Creation Feature Module.

You create SpatialOS entities by creating setting up entity templates. Find out how to do this in the [Entity templates](../entity-templates.md) documentation.

When you are using the GameObject and MonoBehaviour workflow, you interact with SpatialOS entities in the [Runtime](../glossary.md#spatialos-runtime) by using Readers and Writers for every SpatialOS entity’s component in [schema](../glossary.md#schema).  Find out how to use interact with SpatialOS entities in the [Reader and Writers](./readers-writers.md).

## The Creation Feature Module

### How to use the GameObject Creation Feature Module
The GameObject Creation Feature Module is a default implementation of how to link SpatialOS entities to GameObjects.
To enable the Creation Feature Module in your project:

1. Set up your project to use the Creation Feature Module.
	* In your Unity project’s `Packages` directory, locate the Unity Packages manifest `manifest.json`. </br>
 Add the Creation Feature Module to the manifest by adding: `"com.improbable.gdk.gameobjectcreation": "file:<path-to-gdk>/workers/unity/Packages/com.improbable.gdk.gameobjectcreation"`

1. Set up your workers to use the Creation Feature Module when the [WorkerConnector](gomb-creating-workers-with-workerconnector.md) initializes them.
To do this, when you use the Worker Connector,   
call `GameObjectCreationHelper.EnableStandardGameObjectCreation(Worker.World)`.

 In your project, create a prefab for any SpatialOS entity you want to represent as a GameObject. Where you store the prefab depends on which worker is going to create the GameObject.
*   For any worker, use the `Resources/Prefabs/Common/` directory.  
* For specific worker types, use the `Resources/Prefabs/<WorkerType>` directory where `<WorkerType>` is the type of worker which is going to make this prefab. (Using this directory makes the prefab available for only for a specific worker type.)

1. For the SpatialOS entity, you want to represent as a GameObject, in its [entity template](TODO: link to doc 22 - entity templates), set the value of its `Metadata` component to the name of the prefab you have just set up.

Note: Do not proactively destroy GameObjects representing SpatialOS entities. The GDK manages the lifecycle of these GameObjects so by not destroying these GameObjects, you ensure that your worker’s internal state is not corrupted.

### How to customize the GameObject Creation Feature Module
To customize the creation of GameObjects, implement the `IEntityGameObjectCreator` interface. This interface provides the following methods that the GDK calls:

### IEntityGameObjectCreator interface

```csharp
GameObject OnEntityCreated(SpatialOSEntity entity);
```

Fields:
  * `SpatialOSEntity entity`: A SpatialOS entity that entered your [worker’s view](TODO: link to glossary - worker’s view).
Returns:
  * The `GameObject` that should represent the entity. Return null to _not_ link a GameObject to the SpatialOS entity.

```csharp
void OnEntityRemoved(EntityId entityId, GameObject linkedGameObject);
```

Fields:
  * `EntityId entityId`: The entity ID of the SpatialOS entity that was just removed from your worker.
  * `GameObject linkedGameObject`: The GameObject that was linked to the SpatialOS entity or null if none was linked.

### SpatialOSEntity
This is a wrapper around an ECS entity which you can use to easily access its data.


```csharp
bool HasComponent<T>()
```
Returns:
  * True, if the ECS entity contains the specified ECS component representing a SpatialOS component, denoted as `T`.

```csharp
T GetComponent<T>()
```
Returns:
  * If the ECS entity has it, returns the ECS  component, denoted as `T`. If it doesn’t have the ECS component, it returns a struct of type `T` with all its fields initialized with their default values.

#### Example GameObject creator

This is an example of a simple implementation of a GameObject creator.

```csharp
 public class YourGameObjectCreator : IEntityGameObjectCreator
{
    public GameObject OnEntityCreated(SpatialOSEntity entity)
    {
        if (!entity.HasComponent<Metadata.Component>())
        {
            return null;
        }

        var prefabName = entity.GetComponent<Metadata.Component>().EntityType;
        var prefab = Resources.Load<GameObject>(Path.Combine("Prefabs", prefabName));
        if (prefab == null)
        {
            return null;
        }

        return Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    public void OnEntityRemoved(EntityId entityId, GameObject linkedGameObject)
    {
	     // The linkedGameObject is null here if the method above has returned null
        if (linkedGameObject != null)
        {
            Object.Destroy(linkedGameObject);
        }
    }
}
```

To use your own `IEntityGameObjectCreator` implementation in the GameObject Creation Feature Module, you must call `EnableStandardGameObjectCreation(Worker.World, new YourEntityGameObjectCreator())` when initializing your worker.