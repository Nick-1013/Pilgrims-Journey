using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    public Rigidbody2D theRB;

    public float jumpForce;

    public float runSpeed;

    public float activeSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        activeSpeed = moveSpeed;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            activeSpeed = runSpeed;
        }
        theRB.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * activeSpeed, theRB.linearVelocity.y);

        if (Input.GetButtonDown("Jump"))
        {
            theRB.linearVelocity = new Vector2(theRB.linearVelocity.x, jumpForce);
        }
    }
}
