using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  //Based on the state machine design pattern found in 
  // /Gamma, E., Helm, R., Johnson, R. and Vlissides, J. 1994. Design Patterns: Elements of Reusable Object-Oriented Software. Pearson Education.
public class PatrolState : State
{

  public PatrolState(GameObject enemy) : base(enemy){

  }

  public override void Behaviour(){
    
    rb.AddForce(enemyTransform.forward * 0.5f);

    if((lastMovement+2.5f) < Time.time){
      rb.velocity = new Vector3(0,0,0);
      rb.angularVelocity = new Vector3(0,0,0);
      enemyTransform.eulerAngles  = new Vector3(0,Random.Range(0,360),0);
      lastMovement = Time.time;
    }
  }
}
