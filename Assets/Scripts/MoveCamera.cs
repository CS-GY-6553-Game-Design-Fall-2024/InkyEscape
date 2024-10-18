using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraPosition {
        public Collider2D externalTrigger;
        public Transform targetTransform;
        public float camSize;
    }

    [SerializeField] private Camera m_cam;
    [SerializeField] private float m_transitionTime = 0.25f;
    [SerializeField] private List<CameraPosition> m_targetPositions;
    private Vector3 m_transformVelocity = Vector3.zero;
    private float m_sizeVelocity = 0f;
    private int m_currentTargetIndex = 0;
    private Dictionary<Collider2D, CameraPosition> m_targetPositionMap;
    private CameraPosition m_currentCamPos;

    private void Start() {
        m_targetPositionMap = new Dictionary<Collider2D, CameraPosition>();
        foreach(CameraPosition camPos in m_targetPositions) {
            if (camPos.externalTrigger != null) m_targetPositionMap.Add(camPos.externalTrigger, camPos);
        }
        m_currentCamPos = m_targetPositions[0];
    }

    public void SetTarget(Collider2D trigger, Collider2D other) {
        if (other.gameObject.tag == "Player" && m_targetPositionMap.ContainsKey(trigger)) {
            m_currentCamPos = m_targetPositionMap[trigger];
        }
    }
    public void SetTargetByIndex(int index) {
        m_currentCamPos = m_targetPositions[index];
    }

    private void Update() {
        m_cam.transform.position = Vector3.SmoothDamp(
            m_cam.transform.position, 
            m_currentCamPos.targetTransform.position, 
            ref m_transformVelocity, 
            m_transitionTime
        );
        m_cam.orthographicSize = Mathf.SmoothDamp(
            m_cam.orthographicSize,
            m_currentCamPos.camSize,
            ref m_sizeVelocity,
            m_transitionTime
        );
        if (Input.GetKeyDown(KeyCode.R)) {
            TilemapPainter.current.ResetTilemap();
            Player.current.transform.position = m_currentCamPos.externalTrigger.transform.position;
        }
    }
}
