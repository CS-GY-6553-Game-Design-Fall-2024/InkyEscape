using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallButton : MonoBehaviour
{
  public Interactable activatedObject;
  public GameObject[] lights; // Lights
  private int currentLightIndex = 0; // Light index

  private void Start()
  {
    lights = new GameObject[transform.childCount];
    for (int i = 0; i < transform.childCount; i++)
    {
      lights[i] = transform.GetChild(i).gameObject;
    }
  }

  // �ı��Ӷ������ɫ
  public void ChangeLightColor()
  {
    if (currentLightIndex < lights.Length)
    {
      Debug.Log("Light up one");
      SpriteRenderer lightSprite = lights[currentLightIndex].GetComponent<SpriteRenderer>();
      if (lightSprite != null)
      {
        lightSprite.color = Color.yellow;
        currentLightIndex++;
      }
    }

    if (currentLightIndex >= lights.Length)
    {
      Activate();
    }
  }

  private void Activate()
  {
    activatedObject.Activate();
  }
}