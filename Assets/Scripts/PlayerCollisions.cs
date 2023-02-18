using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{ 
  InternalStatus internalStatus;


  void Start()
  {
    internalStatus = gameObject.GetComponent<InternalStatus>();
  }


  void Update()
  {
      
  }

  void OnTriggerEnter(Collider other) {
    if (other.gameObject.tag == "treasure") {
      internalStatus.points += 10;
    }
  }

}
