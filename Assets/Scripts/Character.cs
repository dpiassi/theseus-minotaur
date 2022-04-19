using System.Collections;
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
     * Public getters.
     */
    public bool IsAvailable => isAvailable;

    /*
     * Unity Messages.
     */
    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (isMoveDone)
        {
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
            StartCoroutine(Move(moveVector));
            return true;
        }
        return false;
    }

    /*
     * Private methods.
     */
    IEnumerator Move(Vector2 moveVector)
    {
        isAvailable = false;

        for (int counter = 0; counter < m_MovesPerRound; counter++)
        {
            var deltaPosition = Vector2.zero;
            if (Mathf.Abs(moveVector.x) > GameManager.MOVE_PRECISION)
            {
                deltaPosition.x += Mathf.Sign(moveVector.x);
            }
            else if (Mathf.Abs(moveVector.y) > GameManager.MOVE_PRECISION)
            {
                deltaPosition.y += Mathf.Sign(moveVector.y);
            }
            else
            {
                break;
            }

            if (!Physics.Raycast(transform.position, deltaPosition, 1f, m_BlockMovementLayer))
            {
                moveVector -= deltaPosition;
                targetPosition = transform.position + (Vector3)deltaPosition;
                yield return StartCoroutine(WaitUntilMoveIsDone());
            }
        }

        isAvailable = true;
    }

    IEnumerator WaitUntilMoveIsDone()
    {
        isMoveDone = false;
        yield return new WaitUntil(() => isMoveDone);
        yield return new WaitForSeconds(m_Delay);
    }
}
