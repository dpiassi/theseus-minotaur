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
    public bool TryMove(Vector2[] moves)
    {
        if (isAvailable)
        {
            StartCoroutine(Move(moves));
            return true;
        }
        return false;
    }

    /*
     * Private methods.
     */
    IEnumerator Move(Vector2[] moves)
    {
        isAvailable = false;
        foreach (var move in moves)
        {
            var deltaPosition = new Vector3(move.x, move.y);
            targetPosition = transform.position + deltaPosition;
            yield return StartCoroutine(WaitUntilMoveIsDone());
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
