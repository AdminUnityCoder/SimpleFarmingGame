using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleFarmingGame.Game
{
    public class DialogueUI : MonoBehaviour
    {
        [Tooltip("对话框")] public GameObject DialoguePanel;
        public Text DialogueContent;
        public Image LeftFaceImage, RightFaceImage;
        public Text LeftNameText, RightNameText;
        [Tooltip("提示框，是否按空格键继续")] public GameObject PromptDialogBox;

        private void Awake()
        {
            PromptDialogBox.SetActive(false);
        }

        private void OnEnable()
        {
            EventSystem.ShowDialogueBoxEvent += OnShowDialogueBoxEvent;
        }

        private void OnDisable()
        {
            EventSystem.ShowDialogueBoxEvent -= OnShowDialogueBoxEvent;
        }

        private void OnShowDialogueBoxEvent(Dialogue dialogue)
        {
            StartCoroutine(ShowDialogueCoroutine(dialogue));
        }

        private IEnumerator ShowDialogueCoroutine(Dialogue dialogue)
        {
            if (dialogue != null)
            {
                dialogue.IsFinished = false;
                DialoguePanel.SetActive(true);
                PromptDialogBox.SetActive(false);
                DialogueContent.text = string.Empty;

                if (dialogue.Name != string.Empty)
                {
                    ShowSpriteAndNameOfDialogueFigure(dialogue);
                }
                else // dialogue.Name == string.Empty
                {
                    /*
                    LeftFaceImage.gameObject.SetActive(false);
                    LeftNameText.gameObject.SetActive(false);
                    RightFaceImage.gameObject.SetActive(false);
                    RightNameText.gameObject.SetActive(false);
                    */
                    DialoguePanel.SetActive(false);
                }

                yield return DialogueContent.DOText(dialogue.DialogContent, 1f).WaitForCompletion();

                dialogue.IsFinished = true;

                if (dialogue.NeedToPause && dialogue.IsFinished)
                {
                    PromptDialogBox.SetActive(true);
                }
            }
            else // dialogue == null
            {
                DialoguePanel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示对话人物的头像和名字
        /// </summary>
        /// <param name="dialogue">dialogue data</param>
        private void ShowSpriteAndNameOfDialogueFigure(Dialogue dialogue)
        {
            if (dialogue.IsOnLeft)
            {
                RightFaceImage.gameObject.SetActive(false);
                LeftFaceImage.gameObject.SetActive(true);
                LeftFaceImage.sprite = dialogue.FaceImage;
                LeftNameText.text = dialogue.Name;
            }
            else // On right
            {
                LeftFaceImage.gameObject.SetActive(false);
                RightFaceImage.gameObject.SetActive(true);
                RightFaceImage.sprite = dialogue.FaceImage;
                RightNameText.text = dialogue.Name;
            }
        }
    }
}