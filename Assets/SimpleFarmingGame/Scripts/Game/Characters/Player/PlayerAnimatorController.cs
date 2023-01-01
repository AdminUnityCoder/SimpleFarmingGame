using System;
using System.Collections;
using System.Collections.Generic;
using SFG.InventorySystem;
using UnityEngine;

namespace SimpleFarmingGame.Game
{
    public sealed class PlayerAnimatorController : MonoBehaviour
    {
        private enum PlayerActionEnum { None, Carry, Hoe, Break, Water, Collect, Chop, Reap }

        private enum BodyPartNamesEnum { Body, Hair, Arm, Tool }

        [Serializable]
        private class AnimatorType
        {
            /// <summary>
            /// 玩家动作
            /// </summary>
            public PlayerActionEnum PlayerActionEnum;

            /// <summary>
            /// 身体部位名字
            /// </summary>
            public BodyPartNamesEnum BodyPartNamesEnum;

            /// <summary>
            /// Animator Override Controller
            /// </summary>
            public AnimatorOverrideController AnimatorOverrideController;
        }

        [SerializeField] private List<AnimatorType> AnimatorTypes;

        private Dictionary<string, Animator> m_AnimatorComponentDict = new();
        private Animator[] m_Animators;
        private SpriteRenderer m_HoldItemSprite;

        private bool m_IsUsingTool;
        private float m_MouseX;
        private float m_MouseY;
        private WaitForSeconds m_WaitForPlayPartialAnimation;
        private WaitForSeconds m_WaitForPlayRemainingAnimation;
        private WaitForSeconds m_WaitForShowHarvestFruitSprite;
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int UseTool = Animator.StringToHash("UseTool");
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        private static readonly int MouseX = Animator.StringToHash("MouseX");
        private static readonly int MouseY = Animator.StringToHash("MouseY");

        private void Awake()
        {
            m_Animators = GetComponentsInChildren<Animator>();
            foreach (Animator animator in m_Animators)
            {
                m_AnimatorComponentDict.Add(animator.name, animator);
            }

            m_HoldItemSprite = transform.Find("HoldItem").GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            m_WaitForPlayPartialAnimation = new WaitForSeconds(Player.PlayPartialAnimationTime);
            m_WaitForPlayRemainingAnimation = new WaitForSeconds(Player.PlayRemainingAnimationTime);
            m_WaitForShowHarvestFruitSprite = new WaitForSeconds(Player.ShowHarvestFruitSpriteTime);
        }

        private void Update()
        {
            foreach (Animator animator in m_Animators)
            {
                animator.SetBool(IsMoving, Player.Instance.IsMoving);
                animator.SetFloat(MouseX, m_MouseX);
                animator.SetFloat(MouseY, m_MouseY);

                if (!Player.Instance.IsMoving) continue;
                animator.SetFloat(InputX, Player.Instance.InputX);
                animator.SetFloat(InputY, Player.Instance.InputY);
            }
        }

        private void OnEnable()
        {
            EventSystem.ItemSelectedEvent += SwitchPlayerActionAndSetupHoldItemSprite; // InventorySystem
            SFG.TransitionSystem.EventSystem.BeforeSceneUnloadedEvent += ResetPlayerState;
            SFG.CursorSystem.EventSystem.MouseClickedEvent += OnMouseClickedEvent;
            SFG.CropSystem.EventSystem.SpawnFruitAtPlayerPosition += ShowHarvestFruitSprite;
        }

        private void OnDisable()
        {
            EventSystem.ItemSelectedEvent -= SwitchPlayerActionAndSetupHoldItemSprite; // InventorySystem
            SFG.TransitionSystem.EventSystem.BeforeSceneUnloadedEvent -= ResetPlayerState;
            SFG.CursorSystem.EventSystem.MouseClickedEvent -= OnMouseClickedEvent;
            SFG.CropSystem.EventSystem.SpawnFruitAtPlayerPosition -= ShowHarvestFruitSprite;
        }

        /// <summary>
        /// 通过传入的玩家动作(<paramref name="playerAction"/>)切换对应的 AnimatorOverrideController。<br/>
        /// </summary>
        /// <param name="playerAction">玩家动作</param>
        private void SwitchAnimatorOverrideController(PlayerActionEnum playerAction)
        {
            foreach (var animatorType in AnimatorTypes)
            {
                if (animatorType.PlayerActionEnum == playerAction)
                {
                    m_AnimatorComponentDict[animatorType.BodyPartNamesEnum.ToString()].runtimeAnimatorController
                        = animatorType.AnimatorOverrideController;
                }
                else if (animatorType.PlayerActionEnum == PlayerActionEnum.None)
                {
                    m_AnimatorComponentDict[animatorType.BodyPartNamesEnum.ToString()].runtimeAnimatorController
                        = animatorType.AnimatorOverrideController;
                }
            }
        }

        /// <summary>
        /// 根据传入的物品详情的物品类型匹配玩家动作
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        /// <returns>返回玩家动作</returns>
        private PlayerActionEnum MatchPlayerAction(ItemDetails itemDetails)
        {
            PlayerActionEnum retAction = itemDetails.ItemType switch
            {
                ItemType.Seed => PlayerActionEnum.Carry
              , ItemType.Commodity => PlayerActionEnum.Carry
              , ItemType.HoeTool => PlayerActionEnum.Hoe
              , ItemType.WaterTool => PlayerActionEnum.Water
              , ItemType.CollectTool => PlayerActionEnum.Collect
              , ItemType.AxeTool => PlayerActionEnum.Chop
              , ItemType.PickAxeTool => PlayerActionEnum.Break
              , ItemType.SickleTool => PlayerActionEnum.Reap
              , ItemType.Furniture => PlayerActionEnum.None
              , _ => PlayerActionEnum.None
            };
            return retAction;
        }

        #region Event

        /// <summary>
        /// 通过传入的 <paramref name="itemDetails"/> 的 ItemType 切换玩家动作并且设置 HoldItemSprite 的显示与隐藏
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        /// <param name="isSelected">是否点击选择</param>
        private void SwitchPlayerActionAndSetupHoldItemSprite(ItemDetails itemDetails, bool isSelected)
        {
            PlayerActionEnum currentPlayerAction = MatchPlayerAction(itemDetails);

            if (isSelected == false)
            {
                currentPlayerAction = PlayerActionEnum.None;
                m_HoldItemSprite.enabled = false;
            }
            else if (currentPlayerAction == PlayerActionEnum.Carry)
            {
                m_HoldItemSprite.sprite = itemDetails.ItemIconOnWorld;
                m_HoldItemSprite.enabled = true;
            }
            else
            {
                m_HoldItemSprite.enabled = false;
            }

            SwitchAnimatorOverrideController(currentPlayerAction);
        }

        /// <summary>
        /// 关闭 HoldItemSprite 和切换玩家动作为 None状态
        /// </summary>
        private void ResetPlayerState()
        {
            m_HoldItemSprite.enabled = false;
            SwitchAnimatorOverrideController(PlayerActionEnum.None);
        }

        private void OnMouseClickedEvent(Vector3 mouseWorldPosition, ItemDetails itemDetails)
        {
            if (m_IsUsingTool) return;

            if (itemDetails.ItemType is ItemType.Seed or ItemType.Commodity or ItemType.Furniture)
            {
                SFG.CursorSystem.EventSystem.CallExecuteActionAfterAnimation(mouseWorldPosition, itemDetails);
            }
            else // if use tool
            {
                m_MouseX = mouseWorldPosition.x - Player.Instance.Position.x;
                m_MouseY = mouseWorldPosition.y - (Player.Instance.Position.y + 0.85f);

                if (Mathf.Abs(m_MouseX) > Mathf.Abs(m_MouseY))
                    m_MouseY = 0;
                else
                    m_MouseX = 0;

                StartCoroutine(UseToolCoroutine(mouseWorldPosition, itemDetails));
            }
        }

        /// <summary>
        /// 显示收获果实的图片，实际生成果实数据是在InventoryManger中
        /// </summary>
        /// <param name="harvestFruitID">收获果实的ID</param>
        private void ShowHarvestFruitSprite(int harvestFruitID)
        {
            Sprite harvestFruitSprite = InventoryManager.Instance.GetItemDetails(harvestFruitID).ItemIconOnWorld;
            if (m_HoldItemSprite.enabled == false)
            {
                StartCoroutine(ShowHarvestFruitSpriteCoroutine(harvestFruitSprite));
            }
        }

        #endregion

        #region Coroutine

        /// <summary>
        /// 使用工具协程
        /// </summary>
        /// <param name="mouseWorldPosition">鼠标世界坐标</param>
        /// <param name="itemDetails">物品详情</param>
        /// <returns></returns>
        private IEnumerator UseToolCoroutine(Vector3 mouseWorldPosition, ItemDetails itemDetails)
        {
            m_IsUsingTool = true;
            Player.Instance.DisableInput();
            yield return null;

            // Modify the rotation direction of the player
            foreach (var animator in m_Animators)
            {
                animator.SetTrigger(UseTool);
                animator.SetFloat(InputX, m_MouseX);
                animator.SetFloat(InputY, m_MouseY);
            }

            yield return m_WaitForPlayPartialAnimation; // 先播放部分动画
            SFG.CursorSystem.EventSystem.CallExecuteActionAfterAnimation(mouseWorldPosition, itemDetails);
            yield return m_WaitForPlayRemainingAnimation; // 再将剩余动画部分播放完
            m_IsUsingTool = false;
            Player.Instance.EnableInput();
        }

        /// <summary>
        /// 显示收获果实图片协程
        /// </summary>
        /// <param name="sprite">收获果实图片</param>
        /// <returns></returns>
        private IEnumerator ShowHarvestFruitSpriteCoroutine(Sprite sprite)
        {
            m_HoldItemSprite.sprite = sprite;
            m_HoldItemSprite.enabled = true;
            yield return m_WaitForShowHarvestFruitSprite;
            m_HoldItemSprite.enabled = false;
        }

        #endregion
    }
}