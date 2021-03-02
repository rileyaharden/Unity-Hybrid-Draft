using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UI;

public class PlayerGO : MonoBehaviour
{

    private enum PlayerState
    {
        Idle,
        Walking,
        Attacking,
        Dead,
    }

    
    [SerializeField] private ConvertedEntityHolder convertedEntityHolder;
    [SerializeField] private LayerMask hostileMask;
    [SerializeField] private float playerAttackRange;
    [SerializeField] private float playerAttackCooldown;
    [SerializeField] private float attackDamage;
    [SerializeField] private float basicSpellDamage;
    [SerializeField] private float playerHealth = 10f;
    [SerializeField] private float playerEnergy = 10f;
    [SerializeField] private float basicSpellCooldown = 5f;
    [SerializeField] private int spellOneEnergyCost;

    private float maxPlayerHealth;
    private float maxPlayerEnergy;

    private PlayerState playerState = PlayerState.Idle;
    private Animator animator;
    private PathfindingGridSetup pathfindingGridSetupScript;
    private Collider2D attackTarget;
    private Vector3 attackTargetCurrentPosition;
    private float cellSize;
    private float currentAttackCooldown;
    private SpriteRenderer basicSpellTargetCircleRenderer;
    private ParticleSystem basicSpellPartSys;
    private float basicSpellCurrentCooldown;

    // UI references, probably bad way to do this
    [SerializeField] private Image spellOneCDVisual;
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private Image playerEnergyBar;

    private float previousPlayerHealth;
    private float previousPlayerEnergy;
    


    private void Start()
    {
        pathfindingGridSetupScript = GameObject.Find("PathfindingGridSetup").GetComponent<PathfindingGridSetup>();
        animator = gameObject.GetComponent<Animator>();
        cellSize = pathfindingGridSetupScript.pathfindingGrid.GetCellSize();

        basicSpellTargetCircleRenderer = GameObject.Find("BasicSpellTargetCircle").GetComponent<SpriteRenderer>();
        basicSpellPartSys = basicSpellTargetCircleRenderer.GetComponentInChildren<ParticleSystem>();

        maxPlayerHealth = playerHealth;
        maxPlayerEnergy = playerEnergy;
        
    }

    private void Update()
    {
        if (currentAttackCooldown > 0) currentAttackCooldown -= Time.deltaTime;
        if (basicSpellCurrentCooldown > 0)
        {
            basicSpellCurrentCooldown -= Time.deltaTime;
            spellOneCDVisual.fillAmount = basicSpellCurrentCooldown / basicSpellCooldown;
        }

        //// goes up 0.5 per second 
        //if (playerHealth < 100)
        //{
        //    playerHealth = Mathf.Clamp(playerHealth + 0.5f *Time.deltaTime, 0, 100);
        //}
        // goes up one per second
        if (playerEnergy < 100)
        {
            playerEnergy = Mathf.Clamp(playerEnergy + Time.deltaTime, 0, 100);
        }


        playerHealthBar.fillAmount = playerHealth / maxPlayerHealth;
        playerEnergyBar.fillAmount = playerEnergy / maxPlayerEnergy;





        Entity entity = convertedEntityHolder.GetEntity();
        EntityManager entityManager = convertedEntityHolder.GetEntityManager();


        PathFollowComp pathFollowComp = entityManager.GetComponentData<PathFollowComp>(entity);
        DynamicBuffer<PathPositionBuffer> pathPositionBuffer = entityManager.GetBuffer<PathPositionBuffer>(entity);

        switch (playerState)
        {
            case PlayerState.Idle:

                break;
            case PlayerState.Walking: // if still have a path then make GO walk, else change state to idle
                if (pathFollowComp.pathIndex >= 0)
                {

                    int2 pathPosition = pathPositionBuffer[pathFollowComp.pathIndex].position;

                    float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                    float3 moveDir = math.normalizesafe(targetPosition - (float3)transform.position);
                    float moveSpeed = 3f;

                    animator.SetFloat("moveX", moveDir.x);
                    animator.SetFloat("moveY", moveDir.y);
                    
                    transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

                    if (math.distance(transform.position, targetPosition) < 0.1f)
                    {
                        // next waypoint
                        pathFollowComp.pathIndex--;
                        entityManager.SetComponentData(entity, pathFollowComp);
                    }


                }
                else
                {
                    playerState = PlayerState.Idle;
                    animator.SetBool("isWalking", false);
                }
                break;
            case PlayerState.Attacking:
                // if target has not been destroyed yet, run attack code
                if (attackTarget.gameObject.activeSelf)
                {
                    // if target hasn't moved a cell length away from last pathfinding target position then run
                    //      attack code, else, set new pathfinding params 
                    if (Vector3.Distance(attackTarget.transform.position, attackTargetCurrentPosition) < 1f)
                    {

                        // if player is within attack range then attack, else move towards target
                        // later on need to factor in size of target in this check
                        if (Vector3.Distance(transform.position, attackTarget.transform.position) < playerAttackRange)
                        {
                            if (currentAttackCooldown <= 0)
                            {
                                Vector3 targetDirection = (attackTarget.transform.position - transform.position).normalized;
                                animator.SetBool("isWalking", false);
                                animator.SetFloat("moveX", targetDirection.x);
                                animator.SetFloat("moveY", targetDirection.y);
                                currentAttackCooldown = playerAttackCooldown;
                                // attack
                                StartCoroutine(PlayerAttack());
                            }

                        }
                        else // move towards target
                        {
                            if (pathFollowComp.pathIndex >= 0)
                            {

                                int2 pathPosition = pathPositionBuffer[pathFollowComp.pathIndex].position;

                                float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                                float3 moveDir = math.normalizesafe(targetPosition - (float3)transform.position);
                                float moveSpeed = 3f;

                                animator.SetFloat("moveX", moveDir.x);
                                animator.SetFloat("moveY", moveDir.y);

                                transform.position += (Vector3)(moveDir * moveSpeed * Time.deltaTime);

                                if (math.distance(transform.position, targetPosition) < 0.1f)
                                {
                                    // next waypoint
                                    pathFollowComp.pathIndex--;
                                    entityManager.SetComponentData(entity, pathFollowComp);
                                }


                            }
                            else
                            {

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
                    Collider2D nearbyEnemyCollider = Physics2D.OverlapCircle(transform.position, 5f, hostileMask);
                    if (nearbyEnemyCollider != null)
                    {
                        attackTarget = nearbyEnemyCollider;
                        attackTargetCurrentPosition = attackTarget.transform.position;

                        // all of this to add initial pathfinding params
                        pathfindingGridSetupScript.pathfindingGrid.GetXY(attackTargetCurrentPosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
                        ValidateGridPosition(ref endX, ref endY);
                        pathfindingGridSetupScript.pathfindingGrid.GetXY(transform.position + new Vector3(1, 1) * cellSize * 0.5f, out int startX, out int startY);
                        ValidateGridPosition(ref startX, ref startY);
                        // Add pathfinding params
                        entityManager.AddComponentData(entity, new PathfindingParams
                        {
                            startPosition = new int2(startX, startY),
                            endPosition = new int2(endX, endY)
                        });
                    }
                }
                
                
                break;

            case PlayerState.Dead:

                break;
        }


        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = RAHUtility.GetMouseWorldPosition(true);
            RaycastHit2D rayCastHit = Physics2D.Raycast(mousePosition, Vector2.zero);

            // if clicked collider then attack, if didn't click anything then walk. Will add more options later
            if (rayCastHit) 
            {
                playerState = PlayerState.Attacking;
                attackTarget = rayCastHit.collider;
                attackTargetCurrentPosition = attackTarget.transform.position;

                // all of this to add initial pathfinding params
                pathfindingGridSetupScript.pathfindingGrid.GetXY(attackTargetCurrentPosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
                ValidateGridPosition(ref endX, ref endY);
                pathfindingGridSetupScript.pathfindingGrid.GetXY(transform.position + new Vector3(1, 1) * cellSize * 0.5f, out int startX, out int startY);
                ValidateGridPosition(ref startX, ref startY);
                // Add pathfinding params
                entityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });

            }
            else
            {

                pathfindingGridSetupScript.pathfindingGrid.GetXY(mousePosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
                ValidateGridPosition(ref endX, ref endY);


                pathfindingGridSetupScript.pathfindingGrid.GetXY(transform.position + new Vector3(1, 1) * cellSize * 0.5f, out int startX, out int startY);
                ValidateGridPosition(ref startX, ref startY);

                // Add pathfinding params
                entityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });

                playerState = PlayerState.Walking;
                animator.SetBool("isWalking", true);
            }

            


        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (basicSpellCurrentCooldown <= 0 &&
                playerEnergy >= spellOneEnergyCost)
            {
                StartCoroutine(CastBasicSpell(RAHUtility.GetMouseWorldPosition(true)));
                basicSpellCurrentCooldown = basicSpellCooldown;
            }
        }
        

    }

    private void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);

    }

    public void DamagePlayer(float damageAmount)
    {
        playerHealth -= damageAmount;
        //playerHealthBar.fillAmount = playerHealth / maxPlayerHealth;
        if (playerHealth <= 0)
        {
            if (playerState != PlayerState.Dead)
            {
                StartCoroutine(PlayerDeath());
                playerState = PlayerState.Dead;
            }
            
        }
    }

    private IEnumerator PlayerDeath()
    {
        animator.SetBool("isDead", true);
        yield return null;
        animator.SetBool("isDead", false);
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }

    private IEnumerator PlayerAttack()
    {
        animator.SetBool("isAttacking", true);
        yield return null;
        animator.SetBool("isAttacking", false);
        yield return new WaitForSeconds(0.5f); // need to make this the animation time
        if (attackTarget.gameObject.activeSelf)
        {
            attackTarget.GetComponent<HostileBasic>().DamageHostile(attackDamage);
        }


    }

    private IEnumerator CastBasicSpell(Vector3 position)
    {
        basicSpellTargetCircleRenderer.transform.position = position;
        basicSpellTargetCircleRenderer.color = new Color(1f, 1f, 1f, 1f);
        spellOneCDVisual.fillAmount = 1;
        playerEnergy -= spellOneEnergyCost;
        //playerEnergyBar.fillAmount = playerEnergy / maxPlayerEnergy;
        yield return new WaitForSeconds(1);
        // Run Destruction function
        basicSpellTargetCircleRenderer.color = new Color(1f, 1f, 1f, 0);
        basicSpellPartSys.Play();
        //basicSpellTargetCircleRenderer.transform.position = new Vector3(-2, -2);
        Collider2D[] hitColliderArray = Physics2D.OverlapCircleAll(new Vector2(position.x, position.y), 1f, hostileMask);
        foreach (Collider2D hitCol in hitColliderArray)
        {
            hitCol.GetComponent<HostileBasic>().DamageHostile(basicSpellDamage);
        }
        yield return new WaitForSeconds(1);
        basicSpellPartSys.Stop();
    }

}
