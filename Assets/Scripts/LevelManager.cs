using UnityEngine;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [Tooltip("The Floor prefab.")]
    [SerializeField] GameObject m_Floor;

    [Tooltip("The Wall prefab.")]
    [SerializeField] GameObject m_Wall;

    [Tooltip("Current level size.")]
    [SerializeField] Vector2Int m_GridSize = Vector2Int.one;

    // Start is called before the first frame update
    void Start()
    {
        InstantiateFloor().name = "Floor";
        InstantiateOuterWall(new Vector2Int(0, 1)).name = "TWall";
        InstantiateOuterWall(new Vector2Int(0, -1)).name = "BWall";
        InstantiateOuterWall(new Vector2Int(1, 0)).name = "RWall";
        InstantiateOuterWall(new Vector2Int(-1, 0)).name = "LWall";

        Camera.main.orthographicSize = m_GridSize.y / 2f + 1f;
    }

    GameObject InstantiateFloor()
    {
        var floor = Instantiate(m_Floor, transform);
        floor.transform.localScale = new Vector3
        {
            x = m_GridSize.x,
            y = m_GridSize.y,
            z = 1f,
        };
        var meshRenderer = floor.GetComponent<MeshRenderer>();
        meshRenderer.material.mainTextureScale = m_GridSize;
        return floor;
    }

    GameObject InstantiateOuterWall(Vector2Int coords01) // (0, 1)
    {
        var mult = (Vector2)(m_GridSize * coords01);
        var position = (Vector3) mult / 2f;
        position.z = -0.5f;
        var localScale = new Vector3(mult.y, mult.x, 1f);
        if (localScale.x == 0f)
            localScale.x = 0.1f;
        if (localScale.y == 0f)
            localScale.y = 0.1f;

        var wall = Instantiate(m_Wall, transform);
        wall.transform.position = position;
        wall.transform.localScale = localScale;
        return wall;
    }
}
