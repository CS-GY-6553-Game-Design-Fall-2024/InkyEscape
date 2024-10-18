using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
  public enum PlayerInteraction { IgnorePlayer, Refill, Defill }

  [Header("=== References ===")]
  [SerializeField] private Collider2D m_bulletCollider;
  [SerializeField] private Rigidbody2D m_rb;

  [Header("=== Bullet Settings ===")]
  [SerializeField] private Vector3 m_direction;
  [SerializeField] private Vector3 m_targetPos;
  [SerializeField] private bool m_affectWhileMoving = false;
  [SerializeField] private bool m_alreadySplash = false;
  [SerializeField] private int m_damage = 1;
  [SerializeField] private PlayerInteraction m_playerInteraction = PlayerInteraction.IgnorePlayer;

  public void Shoot(Vector2 direction, float speed, Vector3 targetPos) {
    m_rb.velocity = direction * speed;
    m_direction = direction;
    m_targetPos = targetPos;

    m_damage = 1;
    m_playerInteraction = PlayerInteraction.IgnorePlayer;
    m_affectWhileMoving = false;
    m_rb.gravityScale = 0f;
  }

  public void Shoot(
      Vector2 direction, 
      float speed, 
      Vector3 targetPos, 
      bool affectWhileMoving = false, 
      float gravityScale = 0f, 
      int damage = 1, 
      PlayerInteraction playerInter = PlayerInteraction.IgnorePlayer
  ) {
    m_rb.velocity = direction * speed;
    m_direction = direction;
    m_targetPos = targetPos;
    m_damage = damage;
    m_playerInteraction = playerInter;
    m_affectWhileMoving = affectWhileMoving;
    m_rb.gravityScale = gravityScale;
  }

  private void Update() {
    // If we must affect the paint tilemap while moving, we do that here
    if (m_affectWhileMoving) {
      if (m_playerInteraction == PlayerInteraction.Defill) TilemapPainter.current.CleanBullet(transform.position, 1);
      else TilemapPainter.current.SplatBullet(transform.position, 1);
    }

    Vector3 vectorToTarget = (m_targetPos - transform.position).normalized;
    if ((Vector3.Distance(transform.position, m_targetPos) < 0.1f || Vector3.Dot(m_direction, vectorToTarget) < 0f) && !m_alreadySplash) {
      m_alreadySplash = true;
      TilemapPainter.current.SplatBullet(m_targetPos, 1);
      Destroy(gameObject);
    }
  }

  private void OnTriggerEnter2D(Collider2D other) {
    string otherTag = other.gameObject.tag;
    switch(otherTag) {
      case "WallButton":
        Debug.Log("Hitting wall button");
        other.gameObject.GetComponent<WallButton>().ChangeLightColor();
        Destroy(gameObject);
        return;
      case "StoneDoor":
        Debug.Log("Hit the door");
        Destroy(gameObject);
        break;
      case "CommonDoor":
        Debug.Log("Hit the door");
        Destroy(gameObject);
        break;
      case "Player":
        switch (m_playerInteraction) {
          case PlayerInteraction.Refill:
            Player.current.Refill(m_damage);
            break;
          case PlayerInteraction.Defill:
            Player.current.Defill(m_damage);
            break;
        }
        break;
      case "Environment":
        Debug.Log("Hit the environment");
        Destroy(gameObject);
        return;
      
      
    }
  }
}
