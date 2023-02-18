using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballMovement : MonoBehaviour
{    
  Rigidbody rb;
  float mouseX;
  float mouseY;

  void Start()
  {
    rb = gameObject.GetComponent<Rigidbody>();
    mouseX = Input.mousePosition.x - 960;
    mouseY = Input.mousePosition.y - 540;

  }

  void Update()
  {
    rb.AddForce(new Vector3(mouseX,0,mouseY));
  }
}
