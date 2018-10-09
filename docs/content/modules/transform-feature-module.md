[//]: # (Doc of docs reference 35)
[//]: # (TODO - tech writer review - doc 35)
[//]: # (TODO - Add links as noted - doc 35)
[//]: # (TODO - ongoing updates in this doc https://docs.google.com/document/d/1fX6CP1OGBx281dAQmsNpp7bfnnXUmnyLwHCWOx3wz8E/edit#)
# Transform Synchronization Feature Module

## What is transform synchronization?
_This document relates to both [GameObject-MonoBehaviour and ECS workflows](../intro-workflows-spos-entities.md)._

Transform synchronisation is making sure that when the authoritative worker moves an entity, that it also moves on all other workers.

This feature module does this whenever a GameObject or Rigidbody, linked to a SpatialOS (link) entity, moves.

There are two different concepts of locations associated with a SpatialOS entity:

* The *transform*. This is used by workers to, for example, run physics simulation or render a player in a client.

[//]: # (TODO - Add link below to `Improbable.Position`, Load Balancer, as noted - doc 35)
* The *position*. This is defined by the standard component `Improbable.Position`  and is used by the SpatialOS Load Balancer to determine which worker should have authority over a given entity component (link).

This feature module manages both types of location automatically; primarily updating the transform and, less frequently, updating the position to match.

## How to use this Feature Module

### How to enable automatic transform synchronization for an entity
[//]: # (TODO - Add link below to feature module helper function as noted - doc 35)
When [creating a SpatialOS entity]((../gameobject/create-delete-spos-entities.md)) use the feature module helper functions to add the SpatialOS components necessary to use the Transform Synchronization feature module.

```csharp

var entityBuilder = EntityBuilder.Begin()

    .SetReadAcl("ServerAttribute", "ClientAttribute")

    .SetEntityAclComponentWriteAccess("ServerAttribute")

    .SetPersistence(false)

    .AddPosition(0, 0, 0, "ServerAttribute")

    .AddMetadata("EntityThatCanMove", "ServerAttribute")

    .AddTransformSynchronizationComponents("ServerAttribute");

```

Add the [`TransformSynchronization`] MonoBehaviour (link) to the prefab that will be linked to the entity.

![](../../assets/image-transform-feature-module-md-0.png)

Add the appropriate [transform strategies](link to header) to the `TransformSynchronization` MonoBehaviour. If needed, create the transform strategy `ScriptableObject` (link to header), using the menu items.

![](../../assets/image-transform-feature-module-md-0.png)

After this the entity’s location will be sent to all interested workers whenever the `Transform` or the `Rigidbody`, if one exists, moves.

**N.B.** Writing to the feature module SpatialOS components directly may cause unexpected behaviour.

### How to tell if a given worker has write access to the transform components

[This is a little trickier right now although we can just tell them what the components to look out for are. Need to decide what to actually tell users here. I’d like this to be a helper function on the feature module.]

## The `TransformSynchronization` MonoBehaviour

This component can be added to any GameObject intended to be linked to a SpatialOS entity. When enabled it configures the underlying ECS entity to use the specified strategies.

### Specifying strategies

An arbitrary number of strategies can be specified, but only one per worker type, for either send or receive, should be added. The recommended setup for receive strategies is to have client-workers use the `InterpolationReceiveStrategy` and for server-workers to use the `DirectReceiveStrategy`. The only available send strategy for client- and server-workers is the `RateLimitedSendStrategy`.

### Other configuration

#### Set kinematic when not authoritative

If this option is selected and there is a `Rigidbody` on the `GameObject`, the `Rigidbody` will become [kinematic] (TODO link to unity docs) when the given worker has no write accessnot authoritative. When write accessauthority is gained the Rigidbody will return to the state it was in before.

## What strategies are available?

This feature module offers different strategies for when to send transform and position updates to SpatialOS, and what to do with the updates received.

Strategies are all scriptable objects that can be referenced by the TrasnformSynchrnoization MonoBehaviour. Each one requires a string specifiying the worker type that it applies to, as well as strategy specific data.

### Sending updates

#### `RateLimitedSendStrategy`

This will send a transform and positions updates whenever respective periods of time have elapsed, provided the entity has moved.

Created from the menu item

`Assets/Create/SpatialOS/Transforms/Send Strategies/RateLimited`.

### Handling received transform updates

#### `InterpolationReceiveStrategy`

We recommend using this strategy on Client Workers.

This strategy store a buffer of received updates and creates virtual transform values that smoothly moves between them; sacrifices some latency to visual smoothness.

Created from the menu item

`Assets/Create/SpatialOS/Transforms/Receive Strategies/Interpolation`.

#### `DirectReceiveStrategy`

We recommend using this on Server Workers.

This applies updates as they are received.

Created from the menu item

`Assets/Create/SpatialOS/Transforms/Receive Strategies/Direct`.

## How to configure the strategies

### `RateLimitedSending`

#### `MaxTransformSendRateHz`

This is the frequency at which trasnform updates will be sent, provided the entity has moved since the last one.

Increasing this can result in better looking movement on the client but will also cause increased load, both from sending and receiving more updates, and increase the network bandwidth used.

#### `MaxPositionSendRateHz`

This is the frequency at which position updates will be sent, provided the entity has moved since the last one.

### `InterpolationReceiveStrategy`

#### `TargetBufferSize`

Reducing this will decrease latency between the authoritative worker and the client, but potentially cause movement to hitch.

This should be increased when reducing the number of updates sent per second.

#### `MaxBufferSize`

Increasing this will increase how large the latency can be between the authoritative worker and client before the buffer is reset.

This should always be larger than the target buffer size. Reducing this will limit how large the latency between the authoritative client can be, but potentially cause movement to stutter more frequently.

The buffer size tends to increase when the authoritative worker is overloaded, so the maximum should be increased if the server tick rate is unstable.