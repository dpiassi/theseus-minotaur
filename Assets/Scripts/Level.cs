using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Custom/Level", order = 1)]
public class Level : ScriptableObject
{
    public Vector2 size;
    public Vector2 playerPosition;
    public Vector2 enemyPosition;
    public Vector2 exitPosition;
    [Multiline] public string description;
    public Wall[] walls;

    public Vector3 Offset => (size + Vector2.one) / -2f;

    internal Vector3 GridToWorldPosition(Vector2 position)
    {
        return (Vector3)position + Offset;
    }
}