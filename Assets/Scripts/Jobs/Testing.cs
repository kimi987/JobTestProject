using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class Testing : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfBox;
    
    private List<Box> pfBoxs = new List<Box>();

    public class Box
    {
        public Transform transform;
        public float moveY;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            var box = Instantiate(pfBox,
                new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f)), Quaternion.identity);
            
            pfBoxs.Add(new Box()
            {
                transform = box.transform,
                moveY =  Random.Range(1f, 2f)
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
        var startTime = Time.realtimeSinceStartup;
        if (useJobs)
        { 
//            var positionArray = new NativeArray<float3>(pfBoxs.Count, Allocator.TempJob);
            var moveYArray = new NativeArray<float>(pfBoxs.Count(), Allocator.TempJob);
            var transformAccessArray = new TransformAccessArray(pfBoxs.Count);
            for (int i = 0; i < pfBoxs.Count; i++)
            {
//                positionArray[i] = pfBoxs[i].transform.position;
                transformAccessArray.Add(pfBoxs[i].transform);
                moveYArray[i] = pfBoxs[i].moveY;
            }
            
//            var reallyToughParallelJob = new ReallyToughParalleJob()
//            {
//                deltaTime = Time.deltaTime,
//                positionArray = positionArray,
//                moveYArray = moveYArray,
//            };
            
//            var jobHandler = reallyToughParallelJob.Schedule(pfBoxs.Count, 100);
//            jobHandler.Complete();
            
            var reallyToughParalleIJobTransform = new ReallyToughParalleIJobTransform()
            {
                deltaTime = Time.deltaTime,
                moveYArray =  moveYArray,
            };

            var jobHandler = reallyToughParalleIJobTransform.Schedule(transformAccessArray);
            jobHandler.Complete();
//            
            for (int i = 0; i < pfBoxs.Count; i++)
            {
//                pfBoxs[i].transform.position =  positionArray[i];
                pfBoxs[i].moveY = moveYArray[i];
            }

//            positionArray.Dispose();
            moveYArray.Dispose();
            transformAccessArray.Dispose();
        }
        else
        {
            foreach (var b in pfBoxs)
            {
                b.transform.position += new Vector3(0, b.moveY * Time.deltaTime);

                if (b.transform.position.y > 5f)
                {
                    b.moveY = -math.abs(b.moveY);
                }
                if (b.transform.position.y < -5f)
                {
                    b.moveY = +math.abs(b.moveY);
                }

                float value = 0;
                for (int i = 0; i < 10000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }

//        
//        ReallyToughTask();
//
//        if (useJobs)
//        {
//            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
//            for (int i = 0; i < 10; i++)
//            {
//                jobHandleList.Add(ReallyToughTaskJob());
//            }
//        
//            JobHandle.CompleteAll(jobHandleList);
//            jobHandleList.Dispose();
//
//        }
//        else
//        {
//            for (int i = 0; i < 10; i++)
//            {
//                ReallyToughTask();    
//            }
//        }
//            
//                
//        
        Debug.Log((Time.realtimeSinceStartup - startTime) * 1000 + "ms");
        
    }

    private void ReallyToughTask()
    {
        var value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }


    private JobHandle ReallyToughTaskJob()
    {
        var job = new ReallyToughTaskJob();

        return job.Schedule();
    }
}
[BurstCompile]
public struct ReallyToughTaskJob : IJob
{
    public void Execute()
    {
        var value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
    
}
[BurstCompile]
public struct ReallyToughParalleJob : IJobParallelFor
{
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;
    
    public void Execute(int index)
    {
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0);

        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0;
        for (int i = 0; i < 10000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
[BurstCompile]
public struct ReallyToughParalleIJobTransform : IJobParallelForTransform
{
    
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0);

        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0;
        for (int i = 0; i < 10000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
