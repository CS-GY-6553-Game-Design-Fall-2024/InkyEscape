using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExternalTrigger : MonoBehaviour
{
    public Collider2D _collider;
    public UnityEvent<Collider2D, Collider2D> onTriggerEnterEvent;
    public UnityEvent<Collider2D, Collider2D> onTriggerExitEvent;

    private void Awake() {
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        onTriggerEnterEvent?.Invoke(_collider, other);
    }

    private void OnTriggerExit2D(Collider2D other) {
        onTriggerExitEvent?.Invoke(_collider, other);
    }
}
