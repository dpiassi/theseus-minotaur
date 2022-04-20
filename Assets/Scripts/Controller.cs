using System.Collections;
using UnityEngine;

namespace StrangeLoopGames.TheseusMinotaur
{
    [DisallowMultipleComponent]
    public class Controller : MonoBehaviour
    {
        /*
         * Serialized members.
         */
        [SerializeField] Model m_Model;

        [Tooltip("Whether to print logs or not.")]
        [SerializeField] bool m_IsDebugEnabled = true;

        /*
         * Constants.
         */
        public const float MOVE_PRECISION = 0.001f;

        /*
         * Static members.
         */
        static Controller instance = null;

        static public void Log(object message)
        {
            if (instance.m_IsDebugEnabled)
            {
                Debug.Log(message);
            }
        }

        /*
         * Unity Messages.
         */
        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }

        void Start()
        {
            m_Model.LoadLevel();
        }

        void Update()
        {
            if (m_Model.State != State.Playing)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Undo();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Wait();
            }

            if (!m_Model.IsAvailable)
            {
                return;
            }

            /* Input: */
            var deltaPosition = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };

            /* Movement */
            if (deltaPosition.sqrMagnitude > MOVE_PRECISION)
            {
                Log($"Moving Theseus by {deltaPosition}.");
                if (m_Model.Theseus.TryMove(deltaPosition))
                {
                    MoveEnemy();
                }
            }
        }

        /*
         * Private methods.
         */
        Vector2 MoveVectorFromMinotaurToTheseus()
        {
            return m_Model.Theseus.transform.position - m_Model.Minotaur.transform.position;
        }

        Vector2 MoveVectorFromTheseusToExit()
        {
            return m_Model.Exit.transform.position - m_Model.Theseus.transform.position;
        }

        void MoveEnemy()
        {
            m_Model.IncrementRound();
            StartCoroutine(CoMoveEnemy());
        }

        IEnumerator CoMoveEnemy()
        {
            // Prevent thread overflow on main coroutine (CoMoveEnemy).
            m_Model.SetAvailable(false);

            yield return new WaitUntil(() => m_Model.Theseus.IsAvailable);

            Vector2 delta = MoveVectorFromMinotaurToTheseus();
            Log($"Moving Minotaur by {delta}.");
            m_Model.Minotaur.TryMove(delta);
            yield return new WaitUntil(() => m_Model.Minotaur.IsAvailable);

            // We only have to check Game Over conditions when Minotaur moves.
            if (MoveVectorFromMinotaurToTheseus().sqrMagnitude < 1f)
            {
                m_Model.ChangeState(State.GameOver);
            }

            // We only have to check Win conditions after Minotaur moves.
            else if (MoveVectorFromTheseusToExit().sqrMagnitude < 1f)
            {
                m_Model.ChangeState(State.Escaped);

            }

            // Main coroutine (CoMoveEnemy) is now available.
            m_Model.SetAvailable(true);
        }

        /*
         * Public methods.
         */
        public void ResumeGame()
        {
            m_Model.ChangeState(State.Playing);
        }

        public void PauseGame()
        {
            m_Model.ChangeState(State.Paused);
        }

        public void LoadLevel(int index)
        {
            m_Model.LoadLevel(index);
            Reload();
        }

        /*
         * Callbacks.
         */
        public void OnLevelLoaded(Level level)
        {
            // Reset characters' position:
            m_Model.Theseus.transform.position = level.GridToWorldPosition(level.playerPosition);
            m_Model.Minotaur.transform.position = level.GridToWorldPosition(level.enemyPosition);

            // Notify Model about level loading:
            m_Model.OnLevelLoaded(level);

            // Disable undo feature:
            m_Model.IsUndoEnabled = false;

            // Show gameplay UI:
            m_Model.ChangeState(State.Playing);

            // Update camera ortographic size:
            int size = (int)Mathf.Max(level.size.y, level.size.x / Camera.main.aspect);
            Camera.main.orthographicSize = size / 2f + 1f;
        }

        public void Undo()
        {
            if (m_Model.IsUndoEnabled)
            {
                m_Model.DecrementRound();

                if (!m_Model.Theseus.Undo() | !m_Model.Minotaur.Undo())
                {
                    m_Model.IsUndoEnabled = false;
                }
            }
        }

        public void Reload()
        {
            m_Model.LoadLevel();
        }

        public void Wait()
        {
            MoveEnemy();
        }
    }
}