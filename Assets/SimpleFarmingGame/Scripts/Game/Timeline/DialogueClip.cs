using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SFG.TimelineSystem
{
    /*
     * PlayableAsset: 一个基类，适用于可在运行时用于实例化 Playable 的资源
     * ITimelineClipAsset: 实现此接口可以支持 Timeline 剪辑的高级功能
     */
    public class DialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;
        public DialogueBehaviour DialogueBehaviour = new();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<DialogueBehaviour> playable = 
                ScriptPlayable<DialogueBehaviour>.Create(graph, DialogueBehaviour);
            return playable;
        }
    }
}