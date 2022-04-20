using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeLoopGames.TheseusMinotaur
{
    /// <summary>
    /// Handles each Character movement in the level grid.
    /// </summary>
    [DisallowMultipleComponent]
    public class Character : MonoBehaviour
    {
        /*
         * Serialized members.
         */
        [Tooltip("Speed in unities per second.")]
        [SerializeField] float m_Speed = 2f;

        [Tooltip("Delay between each move in seconds.")]
        [SerializeField] float m_Delay = 0.25f;

        [Tooltip("Max allowed steps (moves) per round.")]
        [SerializeField] int m_MovesPerRound = 1;

        [Tooltip("Character can't collide with these layers.")]
        [SerializeField] LayerMask m_BlockMovementLayer;

        /*
         * Auxiliar members.
         */
        Vector3 targetPosition;
        bool isMoveDone = false;
        bool isAvailable = true;

        /*
         * Private readonly collections.
         */
        readonly Stack<Vector3> history = new();

        /*
         * Public getters.
         */

        /// <summary>
        /// Indicates whether move coroutine is available or not.
        /// </summary>
        public bool IsAvailable => isAvailable;

        /*
         * Unity Messages.
         */
        void Update()
        {
            if (isMoveDone)
            {
                // If we've already reached target position, there's no reason for us to proceed.
                return;
            }

            /*
             * Update transform position towards targetPosition.
             */
            if (Vector3.Distance(transform.position, targetPosition) > Controller.MOVE_PRECISION)
            { // Multiple iterations, progressive adjustments.
                transform.position = Vector3.MoveTowards(
                   transform.position,
                   targetPosition,
                   m_Speed * Time.deltaTime);
            }
            else
            { // Last frame iteration, small adjustment.
              // Set the exact integer position:
                transform.position = targetPosition;

                // Update flag:
                isMoveDone = true;
            }
        }

        /*
         * Public methods.
         */

        /// <summary>
        /// Character will move only if coroutine is available.
        /// </summary>
        /// <param name="moveVector">Target/desired position.</param>
        /// <returns>Whether move attempt was successful or not.</returns>
        public bool TryMove(Vector2 moveVector)
        {
            if (isAvailable) // Avoid threads overflow.
            {
                StartCoroutine(Move(Vector2Int.RoundToInt(moveVector)));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if there are any Undo operations left to perform.
        /// </summary>
        /// <returns>Is there any remaining step to undo?</returns>
        public bool CanUndo()
        {
            return history.Count > 0;
        }

        /// <summary>
        /// Safely performs Undo operation.
        /// </summary>
        /// <returns>Whether undo attempt was successful or not.</returns>
        public bool Undo()
        {
            if (history.TryPop(out Vector3 previousPosition))
            {
                Controller.Log($"Undo on Character \"{gameObject.name}\" to {previousPosition}");
                StartCoroutine(SafeUndo());
            }
            return CanUndo();

            IEnumerator SafeUndo()
            {
                yield return new WaitUntil(() => isAvailable);
                targetPosition = previousPosition;
                yield return StartCoroutine(WaitUntilMoveIsDone());
            }
        }

        /*
         * Callbacks
         */

        /// <summary>
        /// Reset targetPosition and clear history every time
        /// a level is loaded.
        /// </summary>
        /// <param name="_">Level is passed dynamically from custom UnityEvent.</param>
        public void OnLevelLoaded(Level _)
        {
            targetPosition = transform.position;
            history.Clear();
        }

        /*
         * Private methods.
         */
        bool CanMoveBy(Vector2 delta)
        {
            return !Physics.Raycast(transform.position, delta, 1f, m_BlockMovementLayer);
        }

        IEnumerator Move(Vector2Int moveVector)
        {
            // Update flag:
            isAvailable = false;

            // Store last position just before moving on:
            history.Push(targetPosition);

            // Try to move the max number of allowed steps per round:
            for (int counter = 0; counter < m_MovesPerRound; counter++)
            {
                Vector2 deltaPosition = Vector2.zero;
                bool validMove = false;

                // Check if movement is valid horizontally:
                if (Mathf.Abs(moveVector.x) > Controller.MOVE_PRECISION)
                {
                    deltaPosition = new Vector2(Mathf.Sign(moveVector.x), 0f);
                    validMove = CanMoveBy(deltaPosition);
                }

                // Check if movement is valid vertically:
                if (!validMove && Mathf.Abs(moveVector.y) > Controller.MOVE_PRECISION)
                {
                    deltaPosition = new Vector2(0f, Mathf.Sign(moveVector.y));
                    validMove = CanMoveBy(deltaPosition);
                }

                if (validMove)
                {
                    // Update move vector, avoiding unrequested duplicated moves:
                    moveVector -= Vector2Int.RoundToInt(deltaPosition);

                    // Set new target position:
                    targetPosition = transform.position + (Vector3)deltaPosition;

                    // Wait until Character reaches target position:
                    yield return StartCoroutine(WaitUntilMoveIsDone());
                }
                else
                {
                    // There isn't any valid movement towards target position. Skip round:
                    break;
                }
            }

            // Update flag:
            isAvailable = true;
        }

        IEnumerator WaitUntilMoveIsDone()
        {
            // Update flag:
            isMoveDone = false;

            // Wait until current move operation finishes:
            yield return new WaitUntil(() => isMoveDone);

            // Apply delay:
            yield return new WaitForSeconds(m_Delay);
        }
    }
}