using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
  Rigidbody rigidbody;
  public float movementSpeed;
  
  void Start()
  {
    rigidbody = gameObject.GetComponent<Rigidbody>();
  }

  void Update()
  {
    if (Input.GetKey("w")) {
      rigidbody.MovePosition(rigidbody.position + new Vector3(0,0,0.05f*movementSpeed));
      // rb.velocity = new Vector3(0,0,0.05f*movementSpeed);
    }
    if (Input.GetKey("s")) {
      rigidbody.MovePosition(rigidbody.position + new Vector3(0,0,-0.05f*movementSpeed));
      // rb.velocity = new Vector3(0,0,-0.05f*movementSpeed);

    }
    if (Input.GetKey("a")) {
      rigidbody.MovePosition(rigidbody.position + new Vector3(-0.05f*movementSpeed,0,0));
      // rb.velocity = new Vector3(-0.05f*movementSpeed,0,0);
    }
    if (Input.GetKey("d")) {
      rigidbody.MovePosition(rigidbody.position + new Vector3(0.05f*movementSpeed,0,0));
      // rb.velocity = new Vector3(0.05f*movementSpeed,0,0);
    }
  }
}
