using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed;

    private Vector3 playerMove;
    private Vector3 moveDir;
    private Animator animator;

    private PlayerState playerState;


    private enum PlayerState
    {
        Idle,
        Walking,
        Attacking,
        
    }

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        playerMove = Vector3.zero;
        playerMove.x = Input.GetAxisRaw("Horizontal");
        playerMove.y = Input.GetAxisRaw("Vertical");
        if (playerMove != Vector3.zero)
        {
            playerState = PlayerState.Walking;
            
        }
        else
        {
            playerState = PlayerState.Idle;
        }

        if (Input.GetMouseButtonDown(0))
        {
            playerState = PlayerState.Attacking;
        }

        switch (playerState)
        {
            case PlayerState.Idle:
                animator.SetBool("isWalking", false);
                break;
            case PlayerState.Walking:
                animator.SetBool("isWalking", true);
                playerMove.Normalize();
                transform.position += playerMove * playerSpeed * Time.deltaTime;
                animator.SetFloat("moveX", playerMove.x);
                animator.SetFloat("moveY", playerMove.y);
                break;
            case PlayerState.Attacking:
                StartCoroutine(PlayerAttack());
                //playerState = PlayerState.Idle;
                break;

        }

    }

    private IEnumerator PlayerAttack()
    {
        animator.SetBool("isAttacking", true);
        yield return null;
        animator.SetBool("isAttacking", false);
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        //yield return new WaitForSeconds(0.5f);
        //playerState = PlayerState.Idle;


    }

}
