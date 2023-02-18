using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{     
  public GameObject fireballPrefab;
  GameObject fireball;
  GameObject player;

  void Start()
  {
    player = Player.returnPlayer();
  }

  void Update()
  {
    if (Input.GetKey("x")) {
      fireball = Instantiate(fireballPrefab, player.transform.position , player.transform.rotation);
    }
  }
}
