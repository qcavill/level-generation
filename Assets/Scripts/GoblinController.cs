using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinController : MonoBehaviour
{   
  PatrolState patrolState;
  RunState runState;
  State state;
  GameObject player;
  InternalStatus internalStatus;
  int currentState = 1;
  //Based on the state machine design pattern found in 
  // /Gamma, E., Helm, R., Johnson, R. and Vlissides, J. 1994. Design Patterns: Elements of Reusable Object-Oriented Software. Pearson Education.
  void Start()
  {
    player = Player.returnPlayer();
    patrolState = new PatrolState(gameObject);
    runState = new RunState(gameObject);
    State state = new PatrolState(gameObject);
    internalStatus = gameObject.GetComponent<InternalStatus>();
    transform.position = player.transform.position + new Vector3(0,0,1);
  }

 
  

  void OnTriggerEnter(Collider other) {

      internalStatus.hitPoints -= 10;
    
  }
  
  void Update()
  {
    Debug.DrawLine(transform.position, transform.position + transform.forward * 5, Color.red);
    print(currentState);
   

    if (internalStatus.hitPoints <= 0){
      Destroy(gameObject);
    }

    if(Vector3.Distance(gameObject.transform.position, player.transform.position) < 10 && internalStatus.hitPoints >= 50) {
      state = new ChaseState(gameObject);
    }
    else if(internalStatus.hitPoints >= 50) {
      state = new PatrolState(gameObject);  
    }else {
      state = new RunState(gameObject);
    }

    state.Behaviour();
    print(Vector3.Distance (gameObject.transform.position, player.transform.position));
    print(state);
  }


}
