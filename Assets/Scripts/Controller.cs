using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class Controller : MonoBehaviour
{
    /*
     * Enums.
     */
    public enum State { None, Playing, Paused, Escaped, GameOver };
    public State GameState { get; private set; }

    /*
     * Serialized members.
     */
    [Header("MVC")]
    [SerializeField] View m_View;

    [Header("Game")]
    [Tooltip("First level to load.")]
    [SerializeField] int m_CurrentLevelIndex = 0;

    [Tooltip("Reference to Level management script.")]
    [SerializeField] LevelLoader m_LevelLoader;

    [Tooltip("Reference to Player's character.")]
    [SerializeField] Character m_Theseus;

    [Tooltip("Reference to Enemy's character.")]
    [SerializeField] Character m_Minotaur;

    [Header("Debug")]
    [Tooltip("Whether to print logs or not.")]
    [SerializeField] bool m_IsDebugEnabled = true;

    /*
     * Constants.
     */
    public const float MOVE_PRECISION = 0.001f;

    /*
     * Static members.
     */
    static public Controller Instance { get; private set; }

    static public void Log(object message)
    {
        if (Instance.m_IsDebugEnabled)
        {
            Debug.Log(message);
        }
    }

    /*
     * Auxiliar members.
     */
    bool isAvailable = true;
    int currentRound = 0;

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

    void Start()
    {
        m_LevelLoader.Load(m_CurrentLevelIndex);
    }

    void Update()
    {
        if (GameState != State.Playing)
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
                MoveEnemy();
            }
        }
    }

    /*
     * Private methods.
     */
    void ChangeState(State state)
    {
        if (state != GameState)
        {
            Controller.Log($"State changed to: {state.ToString()}");
            GameState = state;
            m_View.OnGameStateChanged(state);
        }
    }

    Vector2 MoveVectorFromMinotaurToTheseus()
    {
        return m_Theseus.transform.position - m_Minotaur.transform.position;
    }

    Vector2 MoveVectorFromTheseusToExit()
    {
        return m_LevelLoader.Exit.transform.position - m_Theseus.transform.position;
    }

    void MoveEnemy()
    {
        currentRound++;
        m_View.SetRoundText(m_LevelLoader.Current.name, currentRound);
        StartCoroutine(CoMoveEnemy());
    }

    IEnumerator CoMoveEnemy()
    {
        m_View.SetAllButtonsInteractable(false);
        isAvailable = false;

        yield return new WaitUntil(() => m_Theseus.IsAvailable);

        Vector2 delta = MoveVectorFromMinotaurToTheseus();
        Log($"Moving Minotaur by {delta}.");
        m_Minotaur.TryMove(delta);
        yield return new WaitUntil(() => m_Minotaur.IsAvailable);

        // We only have to check Game Over conditions when Minotaur moves.
        if (MoveVectorFromMinotaurToTheseus().sqrMagnitude < 1f)
        {
            ChangeState(State.GameOver);
        }

        // We only have to check Win conditions after Minotaur moves.
        else if (MoveVectorFromTheseusToExit().sqrMagnitude < 1f)
        {
            ChangeState(State.GameOver);

        }

        m_View.SetAllButtonsInteractable(true);
        isAvailable = true;
    }

    /*
     * Public methods.
     */
    public void ChangeLevel()
    {
        ChangeState(State.Paused);
    }

    public void LoadLevel(int index)
    {
        m_CurrentLevelIndex = index;
        Reload();
    }

    /*
     * Callbacks.
     */
    public void OnLevelLoaded(Level level)
    {
        // Reset characters' position:
        m_Theseus.transform.position = level.GridToWorldPosition(level.playerPosition);
        m_Minotaur.transform.position = level.GridToWorldPosition(level.enemyPosition);

        // Reset round counter and update UI texts:
        currentRound = 0;
        m_View.SetRoundText(m_LevelLoader.Current.name, currentRound);
        m_View.SetDescriptionText(level.description);

        // Disable undo feature:
        m_View.IsUndoEnabled = false;

        // Show gameplay UI:
        ChangeState(State.Playing);

        // Update camera ortographic size:
        int size = (int)Mathf.Max(level.size.y, level.size.x / Camera.main.aspect);
        Camera.main.orthographicSize = size / 2f + 1f;
    }

    public void Undo()
    {
        if (m_View.IsUndoEnabled)
        {
            currentRound--;
            m_View.SetRoundText(m_LevelLoader.Current.name, currentRound);

            if (!m_Theseus.Undo() | !m_Minotaur.Undo())
            {
                m_View.IsUndoEnabled = false;
            }
        }
    }

    public void Reload()
    {
        m_LevelLoader.Load(m_CurrentLevelIndex);
    }

    public void Wait()
    {
        MoveEnemy();
    }
}
