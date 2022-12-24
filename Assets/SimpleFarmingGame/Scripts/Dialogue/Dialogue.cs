using System;
using UnityEngine;
using UnityEngine.Events;

namespace SFG.DialogueSystem
{
    [Serializable]
    public class Dialogue
    {
        [Header("对话详情")] 
        [Tooltip("头像图片")] public Sprite FaceImage;
        [Tooltip("头像是否在左边")] public bool IsOnLeft;
        [Tooltip("人物名字")] public string Name;
        [TextArea]
        [Tooltip("对话内容")] public string DialogContent;
        [Tooltip("对话是否需要暂停")] public bool NeedToPause;
        [HideInInspector, Tooltip("当前对话是否已经结束")] public bool IsFinished;
        // public UnityEvent OnAfterTalkEvent;
    }
}