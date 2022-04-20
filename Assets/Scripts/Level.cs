using UnityEngine;

namespace StrangeLoopGames.TheseusMinotaur
{
    /// <summary>
    /// ScriptableObject to handle data serialization of each Level.
    /// It's better to store them in a ScriptableObject to avoid data losses and
    /// improve modularization as well, contributing to future maintenances.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level", order = 1)]
    public class Level : ScriptableObject
    {
        /*
         * Public (serialized) members.
         */
        public Vector2 size;
        public Vector2 playerPosition;
        public Vector2 enemyPosition;
        public Vector2 exitPosition;
        [Multiline] public string description;
        public Wall[] walls;

        /// <summary>
        /// Converts from grid (2D positive integers) to World space.
        /// </summary>
        /// <param name="position">Position in level grid. From (1, 1) to (size).</param>
        /// <returns>Position in world space.</returns>
        internal Vector3 GridToWorldPosition(Vector2 position)
        {
            return (Vector3)position + (Vector3)(size + Vector2.one) / -2f; ;
        }
    }
}
