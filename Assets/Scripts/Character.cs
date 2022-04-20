using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Tooltip("Max moves per round.")]
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

        if (Vector3.Distance(transform.position, targetPosition) > GameManager.MOVE_PRECISION)
        {
            transform.position = Vector3.MoveTowards(
               transform.position,
               targetPosition,
               m_Speed * Time.deltaTime);
        }
        else
        {
            // Set the exact integer position:
            transform.position = targetPosition;

            // Update flag:
            isMoveDone = true;
        }
    }

    /*
     * Public methods.
     */
    public bool TryMove(Vector2 moveVector)
    {
        if (isAvailable)
        {
            StartCoroutine(Move(Vector2Int.RoundToInt(moveVector)));
            return true;
        }
        return false;
    }

    public bool CanUndo()
    {
        // Is there any remaining step to undo?
        return history.Count > 0;
    }

    public bool Undo()
    {
        if (history.TryPop(out Vector3 previousPosition))
        {
            GameManager.Log($"Undo on Character \"{gameObject.name}\" to {previousPosition}");
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

        for (int counter = 0; counter < m_MovesPerRound; counter++)
        {
            Vector2 deltaPosition = Vector2.zero;
            bool validMove = false;

            if (Mathf.Abs(moveVector.x) > GameManager.MOVE_PRECISION)
            {
                deltaPosition = new Vector2(Mathf.Sign(moveVector.x), 0f);
                validMove = CanMoveBy(deltaPosition);
            }

            if (!validMove && Mathf.Abs(moveVector.y) > GameManager.MOVE_PRECISION)
            {
                deltaPosition = new Vector2(0f, Mathf.Sign(moveVector.y));
                validMove = CanMoveBy(deltaPosition);
            }

            if (validMove)
            {
                // Update move vector:
                moveVector -= Vector2Int.RoundToInt(deltaPosition);

                targetPosition = transform.position + (Vector3)deltaPosition;

                yield return StartCoroutine(WaitUntilMoveIsDone());
            }
            else
            {
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
        yield return new WaitUntil(() => isMoveDone);
        yield return new WaitForSeconds(m_Delay);
    }
}
