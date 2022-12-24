using System;
using System.Collections;
using System.Collections.Generic;
using SFG.InventorySystem;
using UnityEngine;

namespace SFG.Characters.Player
{
    internal enum PlayerActionEnum { None, Carry, Hoe, Break, Water, Collect, Chop, Reap }

    internal enum BodyPartNamesEnum { Body, Hair, Arm, Tool }

    [Serializable]
    internal class AnimatorType
    {
        public PlayerActionEnum PlayerActionEnum;
        public BodyPartNamesEnum BodyPartNamesEnum;
        public AnimatorOverrideController AnimatorOverrideController;
    }

    internal sealed class PlayerAnimatorController : MonoBehaviour
    {
        [SerializeField] private List<AnimatorType> AnimatorTypes;

        // component
        private Animator[] m_Animators;
        private SpriteRenderer m_HoldItemSprite;
        // key: Animator name, value: Animator component
        private Dictionary<string, Animator> m_AnimatorNameDic = new();

        private float m_MouseX;
        private float m_MouseY;
        private bool m_IsUsingTool;
        private const float PlayAPartOfAnimationTime = 0.45f;   // 播放一部分动画时间
        private const float PlayRestOfAnimationTime = 0.25f;    // 播放剩余动画时间
        private const float DisplayHarvestFruitSpriteTime = 1f; // 显示收割果实图片时间

        private WaitForSeconds m_WaitForPlayAPartOfAnimation;
        private WaitForSeconds m_WaitForPlayRestOfAnimation;
        private WaitForSeconds m_WaitForDisplayHarvestFruitSprite;

        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int UseTool = Animator.StringToHash("UseTool");
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        private static readonly int MouseX = Animator.StringToHash("MouseX");
        private static readonly int MouseY = Animator.StringToHash("MouseY");

        private void Awake()
        {
            m_Animators = GetComponentsInChildren<Animator>();
            m_HoldItemSprite = transform.Find("HoldItem").GetComponent<SpriteRenderer>();

            #region Initialize animatorName dictionary

            foreach (Animator animator in m_Animators)
            {
                m_AnimatorNameDic.Add(animator.name, animator);
            }

            #endregion
        }

        private void OnEnable()
        {
            EventSystem.ItemSelectedEvent += OnItemSelectedEvent; // InventorySystem
            TransitionSystem.EventSystem.BeforeSceneUnloadedEvent += OnBeforeSceneUnloadedEvent;
            CursorSystem.EventSystem.MouseClickedEvent += OnMouseClickedEvent;
            CropSystem.EventSystem.SpawnFruitAtPlayerPosition += DisplayHarvestFruitSprite;
        }

        private void OnDisable()
        {
            EventSystem.ItemSelectedEvent -= OnItemSelectedEvent; // InventorySystem
            TransitionSystem.EventSystem.BeforeSceneUnloadedEvent -= OnBeforeSceneUnloadedEvent;
            CursorSystem.EventSystem.MouseClickedEvent += OnMouseClickedEvent;
            CropSystem.EventSystem.SpawnFruitAtPlayerPosition -= DisplayHarvestFruitSprite;
        }
        
        private void Start()
        {
            m_WaitForPlayAPartOfAnimation = new WaitForSeconds(PlayAPartOfAnimationTime);
            m_WaitForPlayRestOfAnimation = new WaitForSeconds(PlayRestOfAnimationTime);
            m_WaitForDisplayHarvestFruitSprite = new WaitForSeconds(DisplayHarvestFruitSpriteTime);
        }

        private void Update()
        {
            #region SwitchAnimation

            foreach (Animator animator in m_Animators)
            {
                animator.SetBool(IsMoving, PlayerModel.Instance.GetIsMoving);
                animator.SetFloat(MouseX, m_MouseX);
                animator.SetFloat(MouseY, m_MouseY);

                if (PlayerModel.Instance.GetIsMoving == false) continue;
                animator.SetFloat(InputX, PlayerModel.Instance.GetInputX);
                animator.SetFloat(InputY, PlayerModel.Instance.GetInputY);
            }

            #endregion
        }

        #region Event

        private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
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

        private void OnBeforeSceneUnloadedEvent()
        {
            m_HoldItemSprite.enabled = false;
            SwitchAnimatorOverrideController(PlayerActionEnum.None);
        }

        private void OnMouseClickedEvent(Vector3 mouseWorldPosition, ItemDetails itemDetails)
        {
            if (m_IsUsingTool) return;
            if (itemDetails.ItemType is ItemType.Seed or ItemType.Commodity or ItemType.Furniture)
            {
                CursorSystem.EventSystem.CallExecuteActionAfterAnimation(mouseWorldPosition, itemDetails);
            }
            else // if use tool
            {
                m_MouseX = mouseWorldPosition.x - PlayerModel.Instance.GetPosition.x;
                m_MouseY = mouseWorldPosition.y - (PlayerModel.Instance.GetPosition.y + 0.85f);

                if (Mathf.Abs(m_MouseX) > Mathf.Abs(m_MouseY)) { m_MouseY = 0; }
                else { m_MouseX = 0; }

                StartCoroutine(UseToolCoroutine(mouseWorldPosition, itemDetails));
            }
        }

        /// <summary>
        /// 显示收获果实的图片，实际生成果实数据是在InventoryManger中
        /// </summary>
        /// <param name="harvestFruitID">收获果实的ID</param>
        private void DisplayHarvestFruitSprite(int harvestFruitID)
        {
            Sprite harvestFruitSprite = InventoryManager.Instance.GetItemDetails(harvestFruitID).ItemIconOnWorld;
            if (m_HoldItemSprite.enabled == false)
            {
                StartCoroutine(DisplayHarvestFruitSpriteCoroutine(harvestFruitSprite));
            }
        }

        #endregion

        /// <summary>
        /// 通过玩家动作切换 animatorOverrideController。<br/>
        /// Switch animatorOverrideController by player actions.
        /// </summary>
        /// <param name="playerAction">玩家动作</param>
        private void SwitchAnimatorOverrideController(PlayerActionEnum playerAction)
        {
            foreach (var animatorType in AnimatorTypes)
            {
                if (animatorType.PlayerActionEnum == playerAction)
                {
                    m_AnimatorNameDic[animatorType.BodyPartNamesEnum.ToString()].runtimeAnimatorController
                        = animatorType.AnimatorOverrideController;
                }
                else if (animatorType.PlayerActionEnum == PlayerActionEnum.None)
                {
                    m_AnimatorNameDic[animatorType.BodyPartNamesEnum.ToString()].runtimeAnimatorController
                        = animatorType.AnimatorOverrideController;
                }
            }
        }

        /// <summary>
        /// 根据传入的物品详情的物品类型匹配玩家动作
        /// </summary>
        /// <param name="itemDetails">物品详情</param>
        /// <returns>返回玩家动作</returns>
        private static PlayerActionEnum MatchPlayerAction(ItemDetails itemDetails)
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

        #region Coroutine

        private IEnumerator UseToolCoroutine(Vector3 mouseWorldPosition, ItemDetails itemDetails)
        {
            m_IsUsingTool = true;
            PlayerModel.Instance.DisableInput();
            yield return null;

            // Modify the rotation direction of the player
            foreach (var animator in m_Animators)
            {
                animator.SetTrigger(UseTool);
                animator.SetFloat(InputX, m_MouseX);
                animator.SetFloat(InputY, m_MouseY);
            }

            yield return m_WaitForPlayAPartOfAnimation;
            CursorSystem.EventSystem.CallExecuteActionAfterAnimation(mouseWorldPosition, itemDetails);
            yield return m_WaitForPlayRestOfAnimation;
            m_IsUsingTool = false;
            PlayerModel.Instance.EnableInput();
        }

        // 显示收获果实图片协程
        private IEnumerator DisplayHarvestFruitSpriteCoroutine(Sprite sprite)
        {
            m_HoldItemSprite.sprite = sprite;
            m_HoldItemSprite.enabled = true;
            yield return m_WaitForDisplayHarvestFruitSprite;
            m_HoldItemSprite.enabled = false;
        }

        #endregion
    }
}