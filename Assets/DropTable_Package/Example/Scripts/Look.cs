using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An example script to control player mouse looking
/// </summary>
public class Look : MonoBehaviour
{
    [Tooltip("The mouse sensitivity")]
    public float mouseSensitivity;

    [Tooltip("The object to rotate on Y axis (body)")]
    public GameObject bodyToTurn;

    [Tooltip("The object to rotate on X axis (head)")]
    public GameObject headToTurn;

    private float xRoation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //mouse locking
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None);
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            //get input and rotate objects
            Cursor.visible = false;
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRoation -= mouseY;
            xRoation = Mathf.Clamp(xRoation, -90f, 90f);

            headToTurn.transform.localRotation = Quaternion.Euler(xRoation, 0f, 0f);

            bodyToTurn.transform.Rotate(bodyToTurn.transform.up * mouseX);
        }
        else
        {
            Cursor.visible = true;
        }
    }
}
