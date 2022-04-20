using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StrangeLoopGames.TheseusMinotaur
{
    [DisallowMultipleComponent]
    public class View : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] GameObject m_Gameplay;
        [SerializeField] GameObject m_ClosePauseMenu;
        [SerializeField] GameObject m_PauseMenu;
        [SerializeField] TMP_Text m_PauseTitle;

        [Header("UI Elements")]
        [SerializeField] TMP_Text m_RoundText;
        [SerializeField] TMP_Text m_DescriptionText;
        [SerializeField] Button m_UndoButton;
        [SerializeField] Button m_ReloadButton;
        [SerializeField] Button m_WaitButton;

        public bool IsUndoEnabled
        {
            get => m_UndoButton.interactable;
            set => m_UndoButton.interactable = value;
        }

        internal void SetRoundText(string level, int round)
        {
            m_RoundText.text = $"{level} – Round {round}";
        }

        internal void SetDescriptionText(string text)
        {
            m_DescriptionText.text = text;
        }

        internal void SetAllButtonsInteractable(bool interactable)
        {
            m_UndoButton.interactable = interactable;
            m_ReloadButton.interactable = interactable;
            m_WaitButton.interactable = interactable;
        }

        internal void OnGameStateChanged(State state)
        {
            m_ClosePauseMenu.SetActive(state == State.Paused);
            m_PauseMenu.SetActive(state != State.Playing);
            m_Gameplay.SetActive(state == State.Playing);

            switch (state)
            {
                case State.Playing:
                    break;
                case State.Paused:
                    m_PauseTitle.text = "Change Level";
                    break;
                case State.Escaped:
                    m_PauseTitle.text = $"You Escaped -- {m_RoundText.text}";
                    break;
                case State.GameOver:
                    m_PauseTitle.text = $"Game Over -- {m_RoundText.text}";
                    break;

            }
        }
    }
}