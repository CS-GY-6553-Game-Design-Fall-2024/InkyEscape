using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonDoor : Interactable
{
  public float minScaleY = 0.1f;
  private bool isOpen = false;
  private float originalScaleY;
  private BoxCollider2D boxCollider;

  void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
    originalScaleY = transform.localScale.y;
  }

  // Open or Close the door instantly
  public override void Activate()
  {
    if (isOpen)
    {
      CloseDoor();
    }
    else
    {
      OpenDoor();
    }

    isOpen = !isOpen;
  }

  private void CloseDoor()
  {
    Vector3 currentScale = transform.localScale;
    Vector3 currentPosition = transform.position;

    float heightBefore = boxCollider.size.y * currentScale.y;
    float heightAfter = boxCollider.size.y * originalScaleY;

    float offset = (heightAfter - heightBefore) / 2;
    transform.localScale = new Vector3(currentScale.x, originalScaleY, currentScale.z);
    transform.position = new Vector3(currentPosition.x, currentPosition.y + offset, currentPosition.z);
    Debug.Log("Door closed");
  }

  private void OpenDoor()
  {
    Vector3 currentScale = transform.localScale;
    Vector3 currentPosition = transform.position;

    float heightBefore = boxCollider.size.y * currentScale.y;
    float heightAfter = boxCollider.size.y * minScaleY;

    float offset = (heightAfter - heightBefore) / 2;
    transform.localScale = new Vector3(currentScale.x, minScaleY, currentScale.z);
    transform.position = new Vector3(currentPosition.x, currentPosition.y + offset, currentPosition.z);
    Debug.Log("Door closed");
  }

}
