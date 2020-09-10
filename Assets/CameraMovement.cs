using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    Rigidbody rigidBody;
    Vector3 moveTo;
    Vector3 rotateTo;
    Vector2Int gridSize;

    float maxHeight;
    float minHeight;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        gridSize = GameState.Instance.GetGridSize();
        maxHeight = 140f;
        minHeight = 5f;
    }

    private void FixedUpdate()
    {

        moveTo.x = Mathf.Clamp(transform.position.x + Input.GetAxis("Horizontal") * 15f * Time.deltaTime, gridSize.x * - 7f, gridSize.x * 7f);
        moveTo.y = Mathf.Clamp(transform.position.y + Input.GetAxis("CameraScale") * 30f * Time.deltaTime, 5f, 140f);
        moveTo.z = Mathf.Clamp(transform.position.z + Input.GetAxis("Vertical") * 15f * Time.deltaTime + Input.GetAxis("CameraScale") * 10f * Time.deltaTime, gridSize.y * -8f, gridSize.y * 6f); //y is actually z here
        //rotateTo = new Vector3 (transform.rotation.eulerAngles.y + Input.GetAxis("CameraRotation") * 15f * Time.deltaTime);

        //x rotation 0-90
        rotateTo.x = Mathf.Lerp(0, 90f, (transform.position.y - minHeight) / maxHeight);
        rotateTo.y = (transform.localEulerAngles.y + Input.GetAxis("CameraRotation") * 100f * Time.deltaTime);  //change to manual control
        rotateTo.z = 0; // (transform.localEulerAngles.z + Input.GetAxis("CameraRotation") * 100f * Time.deltaTime);


        rigidBody.MovePosition(moveTo);
        rigidBody.MoveRotation(Quaternion.Euler(rotateTo));

        if (Input.GetButtonDown("Jump")) CenterCamera();
    }


    public void CenterCamera()
    {
        rigidBody.MovePosition(new Vector3(0, maxHeight, 0));
        rigidBody.MoveRotation(Quaternion.Euler(90, 0, 0));
    }
}
