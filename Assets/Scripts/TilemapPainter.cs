using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TilemapPainter : MonoBehaviour
{
    public static TilemapPainter current;

    [Header("=== Tilemaps ===")]
    [SerializeField, Tooltip("Background wall tilemap")] private Tilemap m_backgroundTilemap;
    [SerializeField, Tooltip("Wall tilemap")] private Tilemap m_wallTilemap;
    [SerializeField, Tooltip("Paint tilemap")] private Tilemap m_paintTilemap;

    [Header("=== Other References ===")]
    [SerializeField] private Camera m_cam;
    [SerializeField] private Collider2D m_bulletCollider;

    [Header("=== Tiles ===")]
    [SerializeField] private TileBase m_paintableTile;
    [SerializeField] private TileBase m_inkedTile;
    [SerializeField] private TileBase m_usedInkTile;

    [Header("=== Debug Settings ===")]
    [SerializeField] private Vector2Int m_cachedTilemapMin;
    [SerializeField] private Vector2Int m_cachedTilemapMax;
    [SerializeField] private bool m_testBulletSplat = false;
    [SerializeField] private float m_bulletRadius = 0.5f;

    [Header("=== Outputs - READ ONLY ===")]
    [SerializeField] private Vector3Int m_minTilePos;
    [SerializeField] private Vector3Int m_maxTilePos;
    private Dictionary<Vector3Int, TileBase> m_cachedTiles;

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (!Application.isPlaying) return;
        if (m_cam == null || m_bulletCollider == null) return;
        Bounds m_bulletBounds = m_bulletCollider.bounds;
        Vector3 topLeft = m_bulletBounds.center + new Vector3(-m_bulletBounds.extents.x, m_bulletBounds.extents.y, 0f);
        Vector3 topRight = m_bulletBounds.center + m_bulletBounds.extents;
        Vector3 bottomLeft = m_bulletBounds.center - m_bulletBounds.extents;
        Vector3 bottomRight = m_bulletBounds.center + new Vector3(m_bulletBounds.extents.x, -m_bulletBounds.extents.y, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(topLeft, 0.1f);
        Gizmos.DrawSphere(topRight, 0.1f);
        Gizmos.DrawSphere(bottomLeft, 0.1f);
        Gizmos.DrawSphere(bottomRight, 0.1f);
    }
    #endif

    private void Awake() {
        current = this;
    }

    private void Start() {
        m_cachedTiles = new Dictionary<Vector3Int, TileBase>();

        for (int x = m_cachedTilemapMin.x; x <= m_cachedTilemapMax.x; x++) {
            for (int y = m_cachedTilemapMin.y; y <= m_cachedTilemapMax.y; y++) {
                Vector3Int queryPos = new Vector3Int(x,y,0);
                TileBase tile = m_paintTilemap.GetTile(queryPos);
                if (tile != null) m_cachedTiles.Add(queryPos, tile);
            }
        }      
    }

    public void ResetTilemap() {
        for (int x = m_cachedTilemapMin.x; x <= m_cachedTilemapMax.x; x++) {
            for (int y = m_cachedTilemapMin.y; y <= m_cachedTilemapMax.y; y++) {
                Vector3Int queryTilePos = new Vector3Int(x,y,0);
                if (m_cachedTiles.ContainsKey(queryTilePos)) m_paintTilemap.SetTile(queryTilePos, m_cachedTiles[queryTilePos]);
                else m_paintTilemap.SetTile(queryTilePos, null);
            }
        }  
    }

    public void ResetTilemap(Collider2D trigger, Collider2D other) {
        if (other.gameObject.tag == "Player") ResetTilemap();
    }

    private void Update() {        
        // Can't do anything if the camera or bullet collider are null
        if (m_cam == null || m_bulletCollider == null) return;

        // Extract the position of the mouse in the world, and move the bullet collider to that position.
        // This moves the bullet collider to follow the mouse.
        Vector3 mouseWorldPos = m_cam.ScreenToWorldPoint(Input.mousePosition);
        m_bulletCollider.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);

        if (!m_testBulletSplat) return;

        if (Input.GetMouseButtonDown(0)) {
            SplatBullet(mouseWorldPos, 3);
        }
    }

    // Given a position, let's splat the paint tilemap
    public void SplatBullet(Vector3 queryPosition, int splatRadius) {
        // Given the query position, what's it's tile position?
        Vector3Int queryTilemapPosition = m_paintTilemap.WorldToCell(queryPosition);
        // Given the radius of the splat, what are the min and max possible positions of the tiles on the tilemap that the bullet can reach?
        Vector2Int min = new Vector2Int(queryTilemapPosition.x - splatRadius, queryTilemapPosition.y - splatRadius);
        Vector2Int max = new Vector2Int(queryTilemapPosition.x + splatRadius, queryTilemapPosition.y + splatRadius);
        // Iterate through all tiles within the min and max extents
        for(int x = min.x; x <= max.x; x++) {
            for(int y = min.y; y <= max.y; y++) {
                // Get the position of the current tile in Vector3Int;
                Vector3Int queryPos = new Vector3Int(x,y,0);
                Vector3 queryWorldPos = m_paintTilemap.CellToWorld(queryPos);
                // In order to "paint" the tile, we need to determine if the tile on either the background OR the wall tilemaps have a paintable wall
                // This follows since we can only paint on a paintable wall.
                TileBase backgroundTile = m_backgroundTilemap.GetTile(queryPos);
                TileBase paintedTile = m_paintTilemap.GetTile(queryPos);
                if ((backgroundTile == m_paintableTile || paintedTile == m_usedInkTile) && Vector3.Distance(queryPosition, queryWorldPos) <= m_bulletRadius) {
                    m_paintTilemap.SetTile(queryPos, m_inkedTile);
                }
            }
        }
    }
    public void CleanBullet(Vector3 queryPosition, int splatRadius) {
        // Given the query position, what's it's tile position?
        Vector3Int queryTilemapPosition = m_paintTilemap.WorldToCell(queryPosition);
        // Given the radius of the splat, what are the min and max possible positions of the tiles on the tilemap that the bullet can reach?
        Vector2Int min = new Vector2Int(queryTilemapPosition.x - splatRadius, queryTilemapPosition.y - splatRadius);
        Vector2Int max = new Vector2Int(queryTilemapPosition.x + splatRadius, queryTilemapPosition.y + splatRadius);
        // Iterate through all tiles within the min and max extents
        for(int x = min.x; x <= max.x; x++) {
            for(int y = min.y; y <= max.y; y++) {
                // Get the position of the current tile in Vector3Int;
                Vector3Int queryPos = new Vector3Int(x,y,0);
                Vector3 queryWorldPos = m_paintTilemap.CellToWorld(queryPos);
                // In order to "paint" the tile, we need to determine if the tile on either the background OR the wall tilemaps have a paintable wall
                // This follows since we can only paint on a paintable wall.
                TileBase paintedTile = m_paintTilemap.GetTile(queryPos);
                if ((paintedTile == m_inkedTile || paintedTile == m_usedInkTile) && Vector3.Distance(queryPosition, queryWorldPos) <= m_bulletRadius) {
                    m_paintTilemap.SetTile(queryPos, null);
                }
            }
        }
    }

    public bool CheckIsInk(Vector3 queryPosition) {
        // Given the query position, what's it's tile position?
        Vector3Int queryTilemapPosition = m_paintTilemap.WorldToCell(queryPosition);
        // Given this tilemap position, check what kind of tile it is
        TileBase queryTile = m_paintTilemap.GetTile(queryTilemapPosition);
        // Return if the tile is either ink or used ink
        return (queryTile == m_inkedTile || queryTile == m_usedInkTile);
    }

    public int GetInkCapacityFromTile(Vector3 queryPosition) {
        // Given the query position, what's it's tile position?
        Vector3Int queryTilemapPosition = m_paintTilemap.WorldToCell(queryPosition);
        // Given this tilemap position, check what kind of tile it is
        TileBase queryTile = m_paintTilemap.GetTile(queryTilemapPosition);
        // If the query tile is painted, return 1, and set it to used ink
        if (queryTile == m_inkedTile) {
            m_paintTilemap.SetTile(queryTilemapPosition, m_usedInkTile);
            return 1;
        }
        // Otherwise, return 0
        return 0;
    }

    public int GetInkCapacityFromTile(Bounds queryBounds) {
        // Given the query position, what's it's tile position?
        Vector3Int queryMin = m_paintTilemap.WorldToCell(queryBounds.min);
        Vector3Int queryMax = m_paintTilemap.WorldToCell(queryBounds.max);
        // Iterate through the covered tiles. For each tile, check what kind of tile it is, and determine if any should be added
        int inkGained = 0;
        for(int x = queryMin.x; x <= queryMax.x; x++) {
            for(int y = queryMin.y; y <= queryMax.y; y++) {
                Vector3Int queryTilemapPosition = new Vector3Int(x,y,0);
                TileBase queryTile = m_paintTilemap.GetTile(queryTilemapPosition);
                if (queryTile == m_inkedTile) {
                    m_paintTilemap.SetTile(queryTilemapPosition, m_usedInkTile);
                    inkGained++;
                }
            }
        }
        // Return the aggregated ink gained
        return inkGained;
    }

    /*
    private Tilemap m_tilemap;
    [SerializeField] private Camera m_cam;
    [SerializeField] private Transform gunTip;

    [SerializeField] private Vector3 mouseWorldPos;
    [SerializeField] private Vector3 gunTipPosition;

    [SerializeField] private Vector3Int mouseTilePosition;
    [SerializeField] private Vector3Int gunTipTilePosition;

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (!Application.isPlaying) return;
        if (m_cam == null) return;



    }
    #endif

    private void Awake() {
        m_tilemap = GetComponent<Tilemap>();
    }

    // Input.GetMouseButtonDown(0) - left click
    // Input.GetMouseButtonDown(1) - right click
    // Input.GetMouseButtonDown(2) - middle click

    private void Update() {
        if (m_cam == null) return;
        mouseWorldPos = m_cam.ScreenToWorldPoint(Input.mousePosition);
        gunTipPosition = gunTip.position;

        mouseTilePosition = m_tilemap.WorldToCell(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f));
        gunTipTilePosition = m_tilemap.WorldToCell(gunTipPosition);
    }
    */

}
