using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class HostileBasic : MonoBehaviour
{
    [SerializeField] private ConvertedEntityHolder convertedEntityHolder;

    public float hostileHealth;
    public float hostileSpeed;
    public float hostileAttackRange;
    public float hostileAttackCooldown;
    public float hostileAttackDamage;

    private PathfindingGridSetup pathfindingGridSetupScript;
    public Vector3 attackTargetCurrentPosition;
    private float currentAttackCooldown;
    private float cellSize;

    private Animator daAnimator;

    private Collider2D lastAttackTarget;

    private PlayerGO playerGOScript;



    private void Start()
    {
        //pathfindingGridSetupScript = GameObject.Find("PathfindingGridSetup").GetComponent<PathfindingGridSetup>();
        attackTargetCurrentPosition = Vector3.zero;
        //animator = gameObject.GetComponent<Animator>();

    }

    public void DamageHostile(float damageAmount)
    {
        hostileHealth -= damageAmount;
        if (hostileHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void AttackTarget(Collider2D attackTarget, Animator animator)
    {



        pathfindingGridSetupScript = GameObject.Find("PathfindingGridSetup").GetComponent<PathfindingGridSetup>();
        cellSize = pathfindingGridSetupScript.pathfindingGrid.GetCellSize();

        daAnimator = animator;
        Entity entity = convertedEntityHolder.GetEntity();
        EntityManager entityManager = convertedEntityHolder.GetEntityManager();


        PathFollowComp pathFollowComp = entityManager.GetComponentData<PathFollowComp>(entity);
        DynamicBuffer<PathPositionBuffer> pathPositionBuffer = entityManager.GetBuffer<PathPositionBuffer>(entity);

        if (DidColliderChange(attackTarget))
        {
            lastAttackTarget = attackTarget;
            attackTargetCurrentPosition = attackTarget.transform.position;
            playerGOScript = attackTarget.GetComponent<PlayerGO>();
            // Add pathfinding params
            pathfindingGridSetupScript.pathfindingGrid.GetXY(attackTargetCurrentPosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
            ValidateGridPosition(ref endX, ref endY);
            pathfindingGridSetupScript.pathfindingGrid.GetXY(transform.position + new Vector3(1, 1) * cellSize * 0.5f, out int startX, out int startY);
            ValidateGridPosition(ref startX, ref startY);

            entityManager.AddComponentData(entity, new PathfindingParams
            {
                startPosition = new int2(startX, startY),
                endPosition = new int2(endX, endY)
            });
            return;
        }

        if (currentAttackCooldown > 0)
        {
            currentAttackCooldown -= Time.deltaTime;
        }
        if (attackTarget.gameObject.activeSelf)
        {
            //attackTargetCurrentPosition = attackTarget.transform.position;

            // if target hasn't moved 0.25f length away from last pathfinding target position then run
            //      attack code, else, set new pathfinding params 
            if (Vector3.Distance(attackTarget.transform.position, attackTargetCurrentPosition) < 1f)
            {
                // if player is within attack range then attack, else move towards target
                // later on need to factor in size of target in this check
                if (Vector3.Distance(transform.position, attackTarget.transform.position) < hostileAttackRange)
                {
                    if (currentAttackCooldown <= 0)
                    {
                        //animator.SetBool("isWalking", false);
                        currentAttackCooldown = hostileAttackCooldown;
                        // attack
                        StartCoroutine(HostileAttack());
                    }

                }
                else // move towards target
                {
                    if (pathFollowComp.pathIndex >= 0)
                    {

                        int2 pathPosition = pathPositionBuffer[pathFollowComp.pathIndex].position;

                        float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                        float3 moveDir = math.normalizesafe(targetPosition - (float3)transform.position);

                        animator.SetFloat("moveX", moveDir.x);
                        animator.SetFloat("moveY", moveDir.y);

                        transform.position += (Vector3)(moveDir * hostileSpeed * Time.deltaTime);

                        if (math.distance(transform.position, targetPosition) < 0.1f)
                        {
                            // next waypoint
                            pathFollowComp.pathIndex--;
                            entityManager.SetComponentData(entity, pathFollowComp);
                        }


                    }
                    else
                    {
                        Vector3 moveDir = (attackTarget.transform.position - transform.position).normalized;
                        transform.position += moveDir * hostileSpeed * Time.deltaTime;
                    }
                }
            }
            else
            {
                attackTargetCurrentPosition = attackTarget.transform.position;
                // Add pathfinding params
                pathfindingGridSetupScript.pathfindingGrid.GetXY(attackTargetCurrentPosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
                ValidateGridPosition(ref endX, ref endY);
                pathfindingGridSetupScript.pathfindingGrid.GetXY(transform.position + new Vector3(1, 1) * cellSize * 0.5f, out int startX, out int startY);
                ValidateGridPosition(ref startX, ref startY);

                entityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
            }
        }
        else // if target was destroyed
        {
            // for now do nothing, later find new target
        }
    }

    private void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);

    }

    private IEnumerator HostileAttack()
    {
        daAnimator.SetBool("isAttacking", true);
        yield return null;
        daAnimator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(0.25f);
        playerGOScript.DamagePlayer(hostileAttackDamage);

    }

    private bool DidColliderChange(Collider2D newCollider)
    {
        if (newCollider == lastAttackTarget)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}
