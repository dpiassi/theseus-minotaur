using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class LevelLoader : MonoBehaviour
{
    /*
     * Serialized members.
     */
    [Tooltip("The Floor prefab.")]
    [FormerlySerializedAs("m_Floor")]
    [SerializeField] GameObject m_Floor;

    [Tooltip("The Wall prefab.")]
    [FormerlySerializedAs("m_Wall")]
    [SerializeField] GameObject m_Wall;

    [Tooltip("The Exit prefab.")]
    [FormerlySerializedAs("m_Exit")]
    [SerializeField] GameObject m_Exit;

    [Tooltip("Levels data.")]
    [SerializeField] Level[] m_Levels;

    [SerializeField] LevelLoadedEvent m_OnLevelLoaded;

    /*
     * Public getters.
     */
    public GameObject Exit { get; private set; }
    public Level Current { get; private set; }

    /*
     * Custom events.
     */
    [System.Serializable]
    public class LevelLoadedEvent : UnityEvent<Level> { }
    
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

        Controller.Log($"Loading Level {levelIndex}");

        Current = m_Levels[levelIndex];
        InstantiateFloor(Current).name = "Floor";
        InstantiateExit(Current);
        InstantiateOuterWall(Current, Vector2.up).name = "TWall";
        InstantiateOuterWall(Current, Vector2.down).name = "BWall";
        InstantiateOuterWall(Current, Vector2.right).name = "RWall";
        InstantiateOuterWall(Current, Vector2.left).name = "LWall";

        foreach (var wall in Current.walls)
        {
            InstantiateInnerWall(Current, wall);
        }

        m_OnLevelLoaded.Invoke(Current);
    }

    /*
     * Private methods.
     */
    void InstantiateExit(Level level)
    {
        Exit = Instantiate(m_Exit, transform);
        var position = level.GridToWorldPosition(level.exitPosition);
        position.z -= Exit.transform.lossyScale.z;
        Exit.transform.position = position;
        Exit.name = "Exit";
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
        var position = (Vector3)mult / 2f;
        position.z = -0.5f;
        var localScale = new Vector3(
            mult.y != 0f ? (level.size.x) : 0.1f,
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
