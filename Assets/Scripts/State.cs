using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  //Based on the state machine design pattern found in 
  // /Gamma, E., Helm, R., Johnson, R. and Vlissides, J. 1994. Design Patterns: Elements of Reusable Object-Oriented Software. Pearson Education.
abstract public class  State
{
  GameObject player;
  GameObject enemy;
  public Rigidbody rb;
  public int nextDirection = 0;
  public int currentDirection =0;
  public float lastMovement = Time.time;
  public Transform playerTransform;
  public Transform enemyTransform;

  public State(GameObject enemyObject) {
    enemy = enemyObject;
    player = Player.returnPlayer();
    rb = enemy.GetComponent<Rigidbody>();
    playerTransform = player.GetComponent<Transform>();
    enemyTransform = enemy.GetComponent<Transform>();
  }

  abstract public void Behaviour();
  
}
