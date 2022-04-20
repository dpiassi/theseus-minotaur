using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace StrangeLoopGames.TheseusMinotaur
{
    [DisallowMultipleComponent]
    public class LevelLoader : MonoBehaviour
    {
        /*
         * Serialized members.
         */
        [Tooltip("The Floor prefab.")]
        [FormerlySerializedAs("m_Floor")]
        [SerializeField] GameObject m_FloorPrefab;

        [Tooltip("The Wall prefab.")]
        [FormerlySerializedAs("m_Wall")]
        [SerializeField] GameObject m_WallPrefab;

        [Tooltip("The Exit prefab.")]
        [FormerlySerializedAs("m_Exit")]
        [SerializeField] GameObject m_ExitPrefab;

        [Tooltip("Levels data.")]
        [SerializeField] Level[] m_Levels;

        [Tooltip("Event triggered every time a level is loaded.")]
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

        /// <summary>
        /// Clean loading of game Level.
        /// </summary>
        /// <param name="levelIndex">Level index in serialized array.</param>
        public void Load(int levelIndex)
        {
            // Unload: Destroy all objects from previous level.
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // Debug:
            Controller.Log($"Loading Level {levelIndex}");

            // Update public property:
            Current = m_Levels[levelIndex];

            // Configure floor:
            InstantiateFloor().name = "Floor";

            // Configure exit:
            InstantiateExit().name = "Exit";

            // Load outer walls:
            InstantiateOuterWall(Vector2.up).name = "TWall";
            InstantiateOuterWall(Vector2.down).name = "BWall";
            InstantiateOuterWall(Vector2.right).name = "RWall";
            InstantiateOuterWall(Vector2.left).name = "LWall";

            // Load inner walls:
            foreach (var wall in Current.walls)
            {
                InstantiateInnerWall(wall);
            }

            // Finally, trigger custom UnityEvent:
            m_OnLevelLoaded.Invoke(Current);
        }

        /*
         * Private methods.
         */
        GameObject InstantiateExit()
        {
            Exit = Instantiate(m_ExitPrefab, transform);
            var position = Current.GridToWorldPosition(Current.exitPosition);
            position.z -= Exit.transform.lossyScale.z;
            Exit.transform.position = position;
            return Exit;
        }

        GameObject InstantiateFloor()
        {
            var floor = Instantiate(m_FloorPrefab, transform);
            floor.transform.localScale = new Vector3
            {
                x = Current.size.x,
                y = Current.size.y,
                z = 1f,
            };
            var meshRenderer = floor.GetComponent<MeshRenderer>();
            meshRenderer.material.mainTextureScale = Current.size;
            return floor;
        }

        GameObject InstantiateOuterWall(Vector2 normalizedPosition)
        {
            var mult = Current.size * normalizedPosition;
            var position = (Vector3)mult / 2f;
            position.z = -0.5f;
            var localScale = new Vector3(
                mult.y != 0f ? (Current.size.x) : 0.1f,
                mult.x != 0f ? (Current.size.y) : 0.1f,
                1f);
            var wall = Instantiate(m_WallPrefab, transform);
            wall.transform.position = position;
            wall.transform.localScale = localScale;
            return wall;
        }

        void InstantiateInnerWall(Wall data)
        {
            var wall = Instantiate(m_WallPrefab, transform).transform;
            var position = Current.GridToWorldPosition(data.position);

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
}