using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    /*
     * Serialized members.
     */
    [Tooltip("Reference to Player's character.")]
    [SerializeField] Character m_Theseus;

    [Tooltip("Reference to Enemy's character.")]
    [SerializeField] Character m_Minotaur;

    [Tooltip("Whether to print logs or not.")]
    [SerializeField] bool m_IsDebugEnabled = true;

    /*
     * Constants.
     */
    public const float MOVE_PRECISION = 0.001f;

    /*
     * Static members.
     */
    static public GameManager Instance { get; private set; }

    /*
     * Auxiliar members.
     */
    Vector2 deltaPosition = Vector2.zero;
    bool isAvailable = true;

    /*
     * Unity Messages.
     */
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    void Update()
    {
        if (!isAvailable)
        {
            return;
        }

        /*
         * Input:
         */
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != deltaPosition.x)
        {
            deltaPosition.x = horizontal;
        }
        if (Mathf.Abs(deltaPosition.x) > MOVE_PRECISION)
        {
            deltaPosition.y = 0f;
        }
        else if (vertical != deltaPosition.y)
        {
            deltaPosition.y = vertical;
        }

        /*
         * Movement:
         */
        if (deltaPosition.sqrMagnitude > MOVE_PRECISION)
        {
            Log($"Moving Theseus by {deltaPosition}.");
            if (m_Theseus.TryMove(new Vector2[] { deltaPosition }))
            {
                Log($"Moving Minotaur...");
                StartCoroutine(MoveEnemy(deltaPosition));
            }
        }
    }

    /*
     * Private methods.
     */
    IEnumerator MoveEnemy(Vector2 delta)
    {
        isAvailable = false;
        yield return new WaitUntil(() => m_Theseus.IsAvailable);
        Log($"Theseus finished moving...");
        Log($"Moving Minotaur by {delta}.");
        m_Minotaur.TryMove(new Vector2[] { delta, delta });
        yield return new WaitUntil(() => m_Minotaur.IsAvailable);
        isAvailable = true;
    }


    void Log(object message)
    {
        if (m_IsDebugEnabled)
        {
            Debug.Log(message);
        }
    }


    /*
     * Public methods.
     */
    public void OnLevelLoaded(Level level)
    {
        m_Theseus.transform.position = level.GridToWorldPosition(level.playerPosition);
        m_Minotaur.transform.position = level.GridToWorldPosition(level.enemyPosition);
    }
}
