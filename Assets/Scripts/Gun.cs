using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Interactable
{
    [System.Serializable]
    public class GunOutput {
        public Transform outputRef;
        public float cooldown = 0.25f; 
        public float bulletSpeed = 0.1f;
        public int bulletDamage = 10;
        public float bulletGravity = 1f;
        public bool bulletAffectBackground = true;
        public Bullet.PlayerInteraction playerInteraction;
        public float timeLastFired = 0f;
    }

    [Header("=== References ===")]
    [SerializeField] private Bullet m_bulletPrefab;
    [SerializeField] private List<GunOutput> m_outlets; 

    public override void Activate() {
        m_activated = true;
    }
    public override void Deactivate() {
        m_activated = false;
    }
    public void ActivateByPlayer(Collider2D trigger, Collider2D other) {
        if (other.gameObject.tag == "Player") m_activated = true;
    }
    public void DeactivateByPlayer(Collider2D trigger, Collider2D other) {
        if (other.gameObject.tag == "Player") m_activated = false;
    }

    private void Update() {
        if (!m_activated) return;
        foreach(GunOutput output in m_outlets) {
            if (Time.time - output.timeLastFired >= output.cooldown) FireGunOutlet(output);
        }
    }

    public void FireGunOutlet(GunOutput toFire) {
        // Get bullet direction according to mouse position and player position
        Vector3 startPosition = toFire.outputRef.position;
        startPosition.z = 0f;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y-1000f, startPosition.z);

        // Determine the direction the bullet should move in
        Vector3 shootDirection = Vector3.down;

        // Generating bullet with appropriate velocity
        Bullet projectile = Instantiate(m_bulletPrefab, startPosition, Quaternion.identity) as Bullet;
        projectile.Shoot(shootDirection, toFire.bulletSpeed, endPosition, toFire.bulletAffectBackground, toFire.bulletGravity, toFire.bulletDamage, toFire.playerInteraction);

        // Set the time last shot
        toFire.timeLastFired = Time.time;
    }
}
