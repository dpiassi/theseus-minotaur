using UnityEngine;

[System.Serializable]
public class Wall
{
    public enum Orientation { Horizontal, Vertical }
    public Orientation orientation;
    public Vector2 position;
}