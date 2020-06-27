using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class SpawnMonkeySystem : ComponentSystem
{
    public int SpwanNum = 300;
    public Grid<GridNode> gird;
    public Random random = new Random(56);

    protected override void OnCreate()
    {
        base.OnCreate();
        
    }

    protected override void OnUpdate()
    {
        var jobHandlerArray = new NativeList<JobHandle>(Allocator.TempJob);
        
        gird = PathFindingSetup.Instance.pathfindingGrid;
            
        Entities.ForEach((Entity entity, ref MonkeyPrefab monkeyPrefab, ref Translation translation) =>
        {    
            for (var i = 0; i < SpwanNum; i++)
            {
                // var job = new SpawnMonkeyJob
                // {
                //     monkeyEntity = monkeyPrefab.monkeyEntity,
                //     entityManager = EntityManager,
                // };
                //
                // jobHandlerArray.Add(job.Schedule());
                var newEntity = EntityManager.Instantiate(monkeyPrefab.monkeyEntity);
                gird.GetXY(translation.Value, out var startX, out var startY);
                EntityManager.AddComponentData(newEntity, new PathFindingParam
                {
                    startPos = new int2(startX, startY),
                    endPos = new int2(random.NextInt(0, 30), random.NextInt(0, 15)),
                });
            }
        });
        
        JobHandle.CompleteAll(jobHandlerArray);
        jobHandlerArray.Dispose();
        SpwanNum = 0;
    }
    
    
    
    public struct SpawnMonkeyJob: IJob
    {
        public Entity monkeyEntity;
        public EntityManager entityManager;
        public void Execute()
        {
            entityManager.Instantiate(monkeyEntity);
        }
    }
}
