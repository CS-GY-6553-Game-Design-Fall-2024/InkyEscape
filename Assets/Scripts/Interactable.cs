using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] protected bool m_activated;

    public virtual void Activate() {}
    public virtual void Deactivate() {}
    public virtual void Activate(Collider2D trigger, Collider2D other) {}
    public virtual void Deactivate(Collider2D trigger, Collider2D other) {}
    public virtual void Toggle(bool setTo) {}
}
