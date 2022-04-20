using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    /*
     * Serialized members.
     */
    [Header("Game")]
    [Tooltip("First level to load.")]
    [SerializeField] int m_CurrentLevelIndex = 0;

    [Tooltip("Reference to Level management script.")]
    [SerializeField] LevelLoader m_LevelLoader;

    [Tooltip("Reference to Player's character.")]
    [SerializeField] Character m_Theseus;

    [Tooltip("Reference to Enemy's character.")]
    [SerializeField] Character m_Minotaur;

    [Header("UI Panels")]
    [SerializeField] GameObject m_Gameplay;
    [SerializeField] GameObject m_GameOver;

    [Header("UI Elements")]
    [SerializeField] TMP_Text m_RoundText;
    [SerializeField] TMP_Text m_DescriptionText;
    [SerializeField] Button m_UndoButton;
    [SerializeField] Button m_ReloadButton;
    [SerializeField] Button m_WaitButton;

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
    static public GameManager Instance { get; private set; }

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
        m_UndoButton.onClick.AddListener(Undo);
        m_ReloadButton.onClick.AddListener(Reload);
        m_WaitButton.onClick.AddListener(Wait);
    }

    void Update()
    {
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
    void GameOver()
    {
        Log("Game Over");
        m_GameOver.SetActive(true);
        m_Gameplay.SetActive(false);
    }

    Vector2 MoveVectorFromMinotaurToTheseus()
    {
        return m_Theseus.transform.position - m_Minotaur.transform.position;
    }

    void MoveEnemy()
    {
        currentRound++;
        OnRoundChanged();
        StartCoroutine(CoMoveEnemy());
    }

    IEnumerator CoMoveEnemy()
    {
        SetAllButtonsInteractable(false);
        isAvailable = false;

        yield return new WaitUntil(() => m_Theseus.IsAvailable);

        Vector2 delta = MoveVectorFromMinotaurToTheseus();
        Log($"Moving Minotaur by {delta}.");
        m_Minotaur.TryMove(delta);
        yield return new WaitUntil(() => m_Minotaur.IsAvailable);

        // We only have to check Game Over conditions when Minotaur moves.
        if (MoveVectorFromMinotaurToTheseus().sqrMagnitude < 1f)
        {
            GameOver();
        }

        SetAllButtonsInteractable(true);
        isAvailable = true;
    }

    void SetAllButtonsInteractable(bool interactable)
    {
        m_UndoButton.interactable = interactable;
        m_ReloadButton.interactable = interactable;
        m_WaitButton.interactable = interactable;
    }

    /*
     * Public methods.
     */
    public void LoadLevel(int index)
    {
        m_CurrentLevelIndex = index;
        Reload();
    }

    public void OnLevelLoaded(Level level)
    {
        // Reset characters' position:
        m_Theseus.transform.position = level.GridToWorldPosition(level.playerPosition);
        m_Minotaur.transform.position = level.GridToWorldPosition(level.enemyPosition);

        // Reset round counter and update UI texts:
        currentRound = 0;
        OnRoundChanged();
        m_DescriptionText.text = level.description;

        // Disable undo feature:
        m_UndoButton.interactable = false;

        // Show gameplay UI:
        m_GameOver.SetActive(false);
        m_Gameplay.SetActive(true);

        // Update camera ortographic size:
        int size = (int)Mathf.Max(level.size.y, level.size.x / Camera.main.aspect);
        Camera.main.orthographicSize = size / 2f + 1f;
    }

    /*
     * Callbacks.
     */

    void OnRoundChanged()
    {
        m_RoundText.text = $"Level {m_CurrentLevelIndex} – Round {currentRound}";
    }

    void Undo()
    {
        if (m_UndoButton.interactable)
        {
            currentRound--;
            OnRoundChanged();
            if (!m_Theseus.Undo() | !m_Minotaur.Undo())
            {
                m_UndoButton.interactable = false;
            }
        }
    }

    void Reload()
    {
        m_LevelLoader.Load(m_CurrentLevelIndex);
    }

    void Wait()
    {
        MoveEnemy();
    }
}
