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
    public const float MOVE_PRECISION = 0.01f;

    /*
     * Static members.
     */
    static public GameManager Instance { get; private set; }

    /*
     * Auxiliar members.
     */
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
            if (m_Theseus.TryMove(deltaPosition))
            {
                Log($"Moving Minotaur...");
                StartCoroutine(MoveEnemy());
            }
        }
    }

    /*
     * Private methods.
     */
    Vector2 MoveVectorFromMinotaurToTheseus()
    {
        return m_Theseus.transform.position - m_Minotaur.transform.position;
    }

    IEnumerator MoveEnemy()
    {
        isAvailable = false;
        yield return new WaitUntil(() => m_Theseus.IsAvailable);
        Log($"Theseus finished moving...");
        Vector2 delta = MoveVectorFromMinotaurToTheseus();
        Log($"Moving Minotaur by {delta}.");
        m_Minotaur.TryMove(delta);
        yield return new WaitUntil(() => m_Minotaur.IsAvailable);

        if (MoveVectorFromMinotaurToTheseus().sqrMagnitude < 1f)
        {
            Debug.Log("GAME OVER!");
        }

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
