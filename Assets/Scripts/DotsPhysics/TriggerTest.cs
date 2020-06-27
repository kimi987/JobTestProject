using System.Net;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;


public class TriggerTest : JobComponentSystem
{

    [BurstCompile]
    private struct TriggerJob : ITriggerEventsJob
    {

        public ComponentDataFromEntity<PhysicsVelocity> physicsVelocityEntities;
        public void Execute(TriggerEvent triggerEvent)
        {
           if  (physicsVelocityEntities.HasComponent(triggerEvent.Entities.EntityA))
           {
               var physicsVelocity = physicsVelocityEntities[triggerEvent.Entities.EntityA];
               physicsVelocity.Linear.y = 5f;
               physicsVelocityEntities[triggerEvent.Entities.EntityA] = physicsVelocity;
           }

            if (!physicsVelocityEntities.HasComponent(triggerEvent.Entities.EntityB))
            {
                var physicsVelocity = physicsVelocityEntities[triggerEvent.Entities.EntityB];
                
                physicsVelocity.Linear.y = 5f;
                physicsVelocityEntities[triggerEvent.Entities.EntityB] = physicsVelocity;
            }
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    
    protected override void OnCreate()
    {
//        base.OnCreate();

        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();

        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var triggleJob = new TriggerJob
        {
            physicsVelocityEntities = GetComponentDataFromEntity<PhysicsVelocity>()
        };

        return triggleJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
    }
}
