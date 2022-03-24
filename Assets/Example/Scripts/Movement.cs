using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An example script to control player mouvement
/// </summary>
public class Movement : MonoBehaviour
{
    [Tooltip("The player controller")]
    public CharacterController controller;

    [Tooltip("The player's speed")]
    public float moveSpeed;

    [Tooltip("The player's jump height")]
    public float jumpHeight;

    [Tooltip("The player's gravity")]
    public float gravity;

    [Tooltip("The player's current velocity")]
    public Vector3 velocity;

    [Tooltip("The player's maximum velocity mag")]
    private float terminalVelocity = 20f;

    [Tooltip("The lowest point on collider")]
    public Vector3 lowestPoint = Vector3.zero;

    [Tooltip("The radius of the sphere cast")]
    public float groundDistance = 0.2f;

    [Tooltip("Is currently grounded")]
    public bool isGrounded = false;

    [Tooltip("The layer mask for the ground")]
    public LayerMask groundMask;


    [Tooltip("The spawn point of the player")]
    private Vector3 startpos;

    private void Start() {
        //get vals

        startpos = transform.position;

        controller = GetComponent<CharacterController>();

        //get lowest point of the player using collider
        Collider collider = GetComponent<Collider>();
        lowestPoint.y = collider.bounds.center.y - collider.bounds.extents.y - transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        //if r is pressed, respawn
        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
            return;
        }

        //ground check
        isGrounded = false;
        Collider[] hits = Physics.OverlapSphere(transform.position + lowestPoint, groundDistance, groundMask);
        foreach (Collider _hit in hits)
        {
            if (_hit.transform.root == gameObject.transform) continue;
            if (_hit.isTrigger) continue;

            isGrounded = true;
            break;
        }

        //keep moving downwards if grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        //get input and do movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = Vector3.ClampMagnitude((transform.right * x) + (transform.forward * z), 1.0f);

        controller.Move(move * Time.deltaTime * moveSpeed);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        if (velocity.magnitude > terminalVelocity)
        {
            velocity = velocity.normalized * terminalVelocity;
        }

        controller.Move(velocity * Time.deltaTime);


        //respawn if fallen off map
        if (transform.position.y < -20)
        {
            Respawn();
        }
    }

    /// <summary>
    /// Respawns the player at the spawn point
    /// </summary>
    public void Respawn(){
        Debug.Log("Respawn");
        transform.position = startpos;
    }
}