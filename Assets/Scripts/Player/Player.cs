using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.4f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private Vector2 lastDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


  
void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (movement.sqrMagnitude > 0f)
        {
            lastDir = movement;
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            animator.speed = 0.6f;           
        }
        else
        {
            
            animator.SetFloat("MoveX", lastDir.x);
            animator.SetFloat("MoveY", lastDir.y);

            
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            
            animator.Play(
                stateInfo.fullPathHash,   
                0,                        
                1f                        
            );

            
            animator.speed = 0f;
        }
    }
    void FixedUpdate()
    {
        
        rb.velocity = movement * moveSpeed;
    }
}
