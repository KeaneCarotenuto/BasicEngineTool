using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public CharacterController controller;

    public float moveSpeed;
    public float jumpHeight;

    public float gravity;

    public Vector3 velocity;
    private float terminalVelocity = 20f;

    public Vector3 lowestPoint = Vector3.zero;
    public float groundDistance = 0.2f;
    public bool isGrounded = false;
    public LayerMask groundMask;



    private Vector3 startpos;

    private void Start() {
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

        isGrounded = false;

        Collider[] hits = Physics.OverlapSphere(transform.position + lowestPoint, groundDistance, groundMask);

        foreach (Collider _hit in hits)
        {
            if (_hit.transform.root == gameObject.transform) continue;
            if (_hit.isTrigger) continue;

            isGrounded = true;
            break;
        }


        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

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

        if (transform.position.y < -20)
        {
            Respawn();
        }
    }

    public void Respawn(){
        Debug.Log("Respawn");
        transform.position = startpos;
    }
}