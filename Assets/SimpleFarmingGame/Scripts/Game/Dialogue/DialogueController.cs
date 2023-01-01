using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleFarmingGame.Game
{
    [RequireComponent(typeof(NPC))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        public UnityEvent OnDialogueFinishedEvent;
        public List<Dialogue> DialogueList;
        private NPC NPC => GetComponent<NPC>();

        private Stack<Dialogue> m_DialogueStack;
        private bool m_CanTalk;
        private bool m_IsTalking;
        private GameObject m_Sign;

        private void Awake()
        {
            m_Sign = transform.GetChild(1).gameObject;
            CreateDialogueStack();
        }

        private void Update()
        {
            m_Sign.SetActive(m_CanTalk);
            if (m_CanTalk && Input.GetKeyDown(KeyCode.Space) && m_IsTalking == false)
            {
                StartCoroutine(DialogueCoroutine());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_CanTalk = NPC.IsMoving == false && NPC.CanInteractable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_CanTalk = false;
            }
        }

        private void CreateDialogueStack()
        {
            m_DialogueStack = new Stack<Dialogue>();

            // 倒序 Push 进 Stack 里面，然后 Stack 先进后出，所以会按顺序执行
            for (int i = DialogueList.Count - 1; i > -1; --i)
            {
                DialogueList[i].IsFinished = false;
                m_DialogueStack.Push(DialogueList[i]);
            }
        }

        private IEnumerator DialogueCoroutine()
        {
            m_IsTalking = true;
            if (m_DialogueStack.TryPop(out Dialogue result))
            {
                EventSystem.CallShowDialogueBoxEvent(result);
                EventSystem.CallUpdateGameStateEvent(GameState.Pause);
                yield return new WaitUntil(() => result.IsFinished == true);
                m_IsTalking = false;
            }
            else
            {
                // FIXME: 必须聊天了才能使用数字键进行快捷操作，需要修改为游戏一开始是Gameplay
                EventSystem.CallUpdateGameStateEvent(GameState.Gameplay);
                EventSystem.CallShowDialogueBoxEvent(null);
                CreateDialogueStack();
                m_IsTalking = false;
                if (OnDialogueFinishedEvent != null)
                {
                    OnDialogueFinishedEvent.Invoke();
                    m_CanTalk = false;
                }
            }
        }
    }
}