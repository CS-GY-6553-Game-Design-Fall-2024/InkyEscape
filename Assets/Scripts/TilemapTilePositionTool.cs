using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
public class TilemapTilePositionTool : MonoBehaviour
{
    [SerializeField] private Tilemap m_queryTilemap;
    [SerializeField] private Vector3 m_mousePosition;
    [Space]
    [SerializeField] private Vector3Int m_mouseTilemapPosition;

    private void OnEnable() {
        if (!Application.isEditor){
            Destroy(this);
        }
        SceneView.onSceneGUIDelegate += OnScene;
    }

    private void OnDisable() {
        SceneView.onSceneGUIDelegate -= OnScene;
    }

    private void OnScene(SceneView scene) {
        Event e = Event.current;
        Vector3 mousePos = e.mousePosition;
        float ppp = EditorGUIUtility.pixelsPerPoint;
        mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
        mousePos.x *= ppp;

        m_mousePosition = scene.camera.ScreenToWorldPoint(mousePos);
        m_mouseTilemapPosition = m_queryTilemap.WorldToCell(m_mousePosition);
        
        e.Use();
    }

}
#endif
