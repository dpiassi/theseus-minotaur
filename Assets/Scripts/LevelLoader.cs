using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LevelLoader : MonoBehaviour
{
    /*
     * Serialized members.
     */
    [Tooltip("The Floor prefab.")]
    [SerializeField] GameObject m_Floor;

    [Tooltip("The Wall prefab.")]
    [SerializeField] GameObject m_Wall;

    [Tooltip("The Exit prefab.")]
    [SerializeField] GameObject m_Exit;

    [Tooltip("Levels data.")]
    [SerializeField] Level[] m_Levels;

    [SerializeField] LevelLoadedEvent m_OnLevelLoaded;

    /*
     * Custom events.
     */
    [System.Serializable]
    public class LevelLoadedEvent : UnityEvent<Level> { }

    /*
     * Unity Messages.
     */
    void Start()
    {
        Load(0);
    }

    /*
     * Public methods.
     */
    public void Load(int levelIndex)
    {
        // Destroy all level elements.
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        GameManager.Log($"Loading Level {levelIndex}");

        var level = m_Levels[levelIndex];
        InstantiateFloor(level).name = "Floor";
        InstantiateExit(level).name = "Exit";
        InstantiateOuterWall(level, Vector2.up).name = "TWall";
        InstantiateOuterWall(level, Vector2.down).name = "BWall";
        InstantiateOuterWall(level, Vector2.right).name = "RWall";
        InstantiateOuterWall(level, Vector2.left).name = "LWall";

        foreach (var wall in level.walls)
        {
            InstantiateInnerWall(level, wall);
        }

        m_OnLevelLoaded.Invoke(level);
    }

    /*
     * Private methods.
     */
    GameObject InstantiateExit(Level level)
    {
        var exit = Instantiate(m_Exit, transform);
        var position = level.GridToWorldPosition(level.exitPosition);
        position.z -= exit.transform.lossyScale.z;
        exit.transform.position = position;
        return exit;
    }

    GameObject InstantiateFloor(Level level)
    {
        var floor = Instantiate(m_Floor, transform);
        floor.transform.localScale = new Vector3
        {
            x = level.size.x,
            y = level.size.y,
            z = 1f,
        };
        var meshRenderer = floor.GetComponent<MeshRenderer>();
        meshRenderer.material.mainTextureScale = level.size;
        return floor;
    }

    GameObject InstantiateOuterWall(Level level, Vector2 normalizedPosition)
    {
        var mult = level.size * normalizedPosition;
        var position = (Vector3) mult / 2f;
        position.z = -0.5f;
        var localScale = new Vector3(
            mult.y != 0f ? (level.size.x): 0.1f,
            mult.x != 0f ? (level.size.y) : 0.1f,
            1f);
        var wall = Instantiate(m_Wall, transform);
        wall.transform.position = position;
        wall.transform.localScale = localScale;
        return wall;
    }

    void InstantiateInnerWall(Level level, Wall data)
    {
        var wall = Instantiate(m_Wall, transform).transform;
        var position = level.GridToWorldPosition(data.position);

        switch (data.orientation)
        {
            case Wall.Orientation.Horizontal:
                position.y += 0.5f;
                wall.localScale = new Vector3(1f, 0.1f, 1f);
                break;

            case Wall.Orientation.Vertical:
                position.x += 0.5f;
                wall.localScale = new Vector3(0.1f, 1f, 1f);
                break;
        }
        wall.position = position;
    }
}
