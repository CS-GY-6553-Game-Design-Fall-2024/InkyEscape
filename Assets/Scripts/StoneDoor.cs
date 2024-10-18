using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StoneDoor : Interactable
{
  public float openSpeed = 1.0f;  // speed of opening
  public float closeSpeed = 0.5f;  // speed of closing
  public float minScaleY = 0.1f;  // to prevent negative length
  private bool isShrinking = false;
  private bool isRising = false;
  private BoxCollider2D boxCollider;
  private float originalScaleY;
  private bool isPlayerInContact = false; 

  void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
    originalScaleY = transform.localScale.y;
  }

  void Update()
  {
    //if (Input.GetKeyDown(KeyCode.Z))
    //{
    //  isShrinking = true;
    //  isRising = false;
    //}

    //if (Input.GetKeyDown(KeyCode.C))
    //{
    //  isRising = true;
    //  isShrinking = false;
    //}

    if (isShrinking && transform.localScale.y > minScaleY)
    {
      Shorten();
    }

    if (isRising && transform.localScale.y < originalScaleY)
    {
      Rise();
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      isPlayerInContact = true;
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      isPlayerInContact = false;
    }
  }

  void Shorten()
  {
    Vector3 currentScale = transform.localScale;
    Vector3 currentPosition = transform.position;

    float newScaleY = currentScale.y - openSpeed * Time.deltaTime;
    if (newScaleY <= minScaleY)
    {
      newScaleY = minScaleY;
      isShrinking = false;
    }
    float heightBefore = boxCollider.size.y * currentScale.y;
    float heightAfter = boxCollider.size.y * newScaleY;

    float offset = (heightBefore - heightAfter) / 2;
    transform.localScale = new Vector3(currentScale.x, newScaleY, currentScale.z);
    transform.position = new Vector3(currentPosition.x, currentPosition.y - offset, currentPosition.z);
  }

  void Rise()
  {
    Vector3 currentScale = transform.localScale;
    Vector3 currentPosition = transform.position;

    float newScaleY = currentScale.y + closeSpeed * Time.deltaTime;
    if (newScaleY >= originalScaleY)
    {
      newScaleY = originalScaleY;
      isRising = false;
    }
    float heightBefore = boxCollider.size.y * currentScale.y;
    float heightAfter = boxCollider.size.y * newScaleY;

    float offset = (heightAfter - heightBefore) / 2;
    transform.localScale = new Vector3(currentScale.x, newScaleY, currentScale.z);
    transform.position = new Vector3(currentPosition.x, currentPosition.y + offset, currentPosition.z);
  }

  // Open or Close
  public override void Activate()
  {
    if (isRising || transform.localScale.y >= originalScaleY)
    {
      isRising = false;
      isShrinking = true;
    }
    else if (isShrinking || transform.localScale.y <= minScaleY)
    {
      isShrinking = false;
      isRising = true;
    }
  }
}