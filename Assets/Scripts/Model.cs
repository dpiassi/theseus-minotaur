using UnityEngine;

namespace StrangeLoopGames.TheseusMinotaur
{
    public class Model : MonoBehaviour
    {
        /*
         * Serialized members.
         */
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

        /*
         * Private non-serialized members:
         */
        State state;
        int currentRound = 0;

        /*
         * Public properties:
         */
        public State State => state;

        public bool IsAvailable { get; private set; } = true;

        public bool IsUndoEnabled
        {
            get => m_View.IsUndoEnabled;
            set => m_View.IsUndoEnabled = value;
        }

        public GameObject Exit => m_LevelLoader.Exit;

        public Character Minotaur => m_Minotaur;

        public Character Theseus => m_Theseus;

        /*
         * Internal functions:
         */
        internal void LoadLevel()
        {
            m_LevelLoader.Load(m_CurrentLevelIndex);
        }

        internal void LoadLevel(int index)
        {
            m_CurrentLevelIndex = index;
            LoadLevel();
        }

        internal void ChangeState(State state)
        {
            if (state != this.state)
            {
                Controller.Log($"State changed to: {state}");
                this.state = state;
                m_View.OnGameStateChanged(state);
            }
        }

        internal void SetAvailable(bool available)
        {
            m_View.SetAllButtonsInteractable(available);
            IsAvailable = available;
        }

        internal void OnLevelLoaded(Level level)
        {
            currentRound = 0;
            OnRoundChanged();
            m_View.SetDescriptionText(level.description);
        }

        internal void IncrementRound()
        {
            currentRound++;
            OnRoundChanged();
        }

        internal void DecrementRound()
        {
            currentRound--;
            OnRoundChanged();
        }

        void OnRoundChanged()
        {
            //m_View.SetRoundText(m_LevelLoader.Current.name, currentRound);
            m_View.SetRoundText("TEST", currentRound);
        }
    }
}