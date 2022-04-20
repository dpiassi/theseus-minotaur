using UnityEngine;


namespace StrangeLoopGames.TheseusMinotaur
{
    /// <summary>
    /// Structure to handle data serialization of each Wall.
    /// A Wall is a barrier inside the game map (level).
    /// </summary>
    [System.Serializable]
    public struct Wall
    {
        public enum Orientation { Horizontal, Vertical }
        public Orientation orientation;
        public Vector2 position;
    }
}