using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.tag == "Player") 
    {
      Debug.Log("Player damaged");
      collision.GetComponent<Health>().Damage(1);
      collision.GetComponent<DamageImpact>().Damage(transform.position);
    }
  }
}
