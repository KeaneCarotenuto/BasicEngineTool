using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    public float mouseSensitivity;
    public GameObject bodyToTurn;
    public GameObject headToTurn;

    private float xRoation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None);
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
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
