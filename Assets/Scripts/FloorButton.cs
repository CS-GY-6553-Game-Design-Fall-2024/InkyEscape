using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorButton : MonoBehaviour
{
  [Header("=== References ===")]
  [SerializeField] private Transform m_buttonComponent;
  [SerializeField] private Interactable activatedObject;

  [Header("=== Button Settings ===")]
  [SerializeField] private bool m_keepActivated = false;
  [SerializeField] private float m_originalYPosition;
  [SerializeField] private float m_activatedYPosition;
  [SerializeField] private float m_transitionTime = 0.1f;

  private bool isSinglePress = false;
  private bool isPlayerOn = false;
  private bool isDown = false;
  private Vector3 m_startYPos, m_targetYPos;
  private Vector3 m_velocity = Vector3.zero;
  //private float originalScaleY;
  //private BoxCollider2D boxCollider;
  //private float pressedScaleY;

  private void Awake() {
    if (gameObject.tag == "SinglePressButton") {
      isSinglePress = true;
    }
    ResetButton();
  }

  private void Update() {
    if (isPlayerOn && !isDown) {
      PressDownButton();
      Debug.Log("Floor Button Pressed");
      activatedObject.Activate();
      isDown = true;
    }
    else if (!isPlayerOn && isDown && !m_keepActivated) {
      ResetButton();
      Debug.Log("Floor Button Reset");
      if (!isSinglePress) {
        activatedObject.Activate();
      }
      isDown = false;
    }

    m_buttonComponent.localPosition = Vector3.SmoothDamp(m_buttonComponent.localPosition, m_targetYPos, ref m_velocity, m_transitionTime);
  }

  private void Interact() {
    activatedObject.Activate();
    // TODO else if other elements
  }

  private void OnTriggerEnter2D(Collider2D other) {
    if (other.CompareTag("Player") && !Player.current.isTopDown) {
      isPlayerOn = true;
    }
  }

  private void OnTriggerExit2D(Collider2D other) {
    if (other.CompareTag("Player")) {
      isPlayerOn = false;
    }
  }

  private void PressDownButton() {
    //AdjustScale(pressedScaleY);
    m_startYPos = new Vector3(0f, m_originalYPosition, 0f);
    m_targetYPos = new Vector3(0f, m_activatedYPosition, 0f);
  }

  private void ResetButton() {
    //AdjustScale(originalScaleY);
     m_startYPos = new Vector3(0f, m_activatedYPosition, 0f);
    m_targetYPos = new Vector3(0f, m_originalYPosition, 0f);
  }

  /*
  private void AdjustScale(float newScaleY)
  {
    // Keep the button border still without changing with the body
    List<Transform> childTransforms = new List<Transform>();
    foreach (Transform child in transform)
    {
      childTransforms.Add(child);
    }
    List<Vector3> childWorldPositions = new List<Vector3>();
    foreach (Transform child in childTransforms)
    {
      childWorldPositions.Add(child.position);
    }

    Vector3 currentScale = transform.localScale;
    Vector3 currentPosition = transform.position;
    float heightBefore = boxCollider.size.y * currentScale.y;
    float heightAfter = boxCollider.size.y * newScaleY;
    float offset = (heightBefore - heightAfter) / 2;
    transform.localScale = new Vector3(currentScale.x, newScaleY, currentScale.z);
    transform.position = new Vector3(currentPosition.x, currentPosition.y - offset, currentPosition.z);


    // Keep the button border still without changing with the body
    for (int i = 0; i < childTransforms.Count; i++)
    {
      childTransforms[i].position = childWorldPositions[i];
    }
  }
  */

  void print(string str)
  {
    Debug.Log(str);
  }
}
