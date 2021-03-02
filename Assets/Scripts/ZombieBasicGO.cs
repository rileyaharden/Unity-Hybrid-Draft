using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ZombieBasicGO : HostileBasic
{
    //[SerializeField] private ConvertedEntityHolder convertedEntityHolder;
    [SerializeField] private LayerMask citizenMask;
    [SerializeField] private float chaseRadius = 5f;

    private Vector3 commandCenterPosition;
    private Animator animator;


    private void Start()
    {
        //Debug.Log(convertedEntityHolder.GetEntity());
        commandCenterPosition = GameObject.FindGameObjectWithTag("Building").transform.position;
        animator = gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        //Entity entity = convertedEntityHolder.GetEntity();
        //EntityManager entityManager = convertedEntityHolder.GetEntityManager();


        //PathFollowComp pathFollowComp = entityManager.GetComponentData<PathFollowComp>(entity);
        ////PathfindingParams pathfindingParams = entityManager.GetComponentData<PathfindingParams>(entity);
        //DynamicBuffer<PathPositionBuffer> pathPositionBuffer = entityManager.GetBuffer<PathPositionBuffer>(entity);
        Collider2D citizenCollider = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y), chaseRadius, citizenMask);

        if (citizenCollider != null)
        {
            //attackTargetCurrentPosition = citizenCollider.transform.position;
            AttackTarget(citizenCollider, animator);
            //entityManager.AddComponentData(entity, new PathfindingParams 
            //{ 
            //    startPosition = new int2(),

            //});
        }

        //// stop within 2f of command center position
        //if (math.distance(transform.position, commandCenterPosition) < 2f)
        //{
        //    pathFollowComp.pathIndex = -1;
        //    entityManager.SetComponentData(entity, pathFollowComp);
        //    animator.SetBool("isAttacking", true);
        //}

        //if (pathFollowComp.pathIndex >= 0)
        //{
            


        //    int2 pathPosition = pathPositionBuffer[pathFollowComp.pathIndex].position;

        //    float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
        //    float3 moveDir = math.normalizesafe(targetPosition - (float3)transform.position);
        //    float moveSpeed = 3f;

        //    transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

        //    if (math.distance(transform.position, targetPosition) < 0.1f)
        //    {
        //        // next waypoint
        //        pathFollowComp.pathIndex--;
        //        entityManager.SetComponentData(entity, pathFollowComp);
        //    }

            
        //}

    }

}
