using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  //Based on the state machine design pattern found in 
  // /Gamma, E., Helm, R., Johnson, R. and Vlissides, J. 1994. Design Patterns: Elements of Reusable Object-Oriented Software. Pearson Education.
public class RunState : State
{   
  public RunState(GameObject enemyObject) : base(enemyObject) {
  }

  public override void Behaviour() {
    enemyTransform.LookAt(playerTransform);
    rb.AddForce(enemyTransform.forward * -0.5f);
  }
}
