using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Physics.Systems;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public class RaCastTest : MonoBehaviour
{
    private Entity RayCast(float3 fromPos, float3 toPos)
    {
        var buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem < BuildPhysicsWorld>();
        var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        var rayCastInput = new RaycastInput
        {
            Start = fromPos,
            End = toPos,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0,
            }
        };
        var rayCastHit = new RaycastHit();
        if (collisionWorld.CastRay(rayCastInput, out rayCastHit))
        {
            //Hit something
           var hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[rayCastHit.RigidBodyIndex].Entity;
            return hitEntity;
        }
        else
        {
            return Entity.Null;;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayDistance = 100f;
            Debug.Log(RayCast(ray.origin, ray.direction * rayDistance));
        }
    }
}
