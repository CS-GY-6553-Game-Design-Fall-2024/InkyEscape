using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
  private bool isPlayerInRange = false;
  public Interactable activatedObject;
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    // Player in the range AND Press E
    if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
    {
      Interact();
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      isPlayerInRange = true;
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      isPlayerInRange = false;
    }
  }

  private void Interact()
  {
    Debug.Log("Interact with the rope!");
    activatedObject.Activate();
    // TODO else if other elements
  }
}
