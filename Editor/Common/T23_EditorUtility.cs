#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using VRC.Udon;
using UdonSharp;
using UdonSharpEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Trigger2to3
{
    public class T23_EditorUtility : Editor
    {
        private static bool commonBufferUpdateTask = false;

        public static void ShowTitle(int category)
        {
            string[] title = { "Broadcast", "Trigger", "Action", "Option" };
            ShowTitle(title[category]);
        }

        public static void ShowTitle(string title)
        {
            Color backColor;
            switch (title)
            {
                case "Master":    backColor = new Color(0.95f, 0.40f, 0.40f); break;
                case "Broadcast": backColor = new Color(0.35f, 0.85f, 0.45f); break;
                case "Trigger":   backColor = new Color(0.95f, 0.85f, 0.25f); break;
                case "Action":    backColor = new Color(0.25f, 0.75f, 0.95f); break;
                default:          backColor = new Color(0.88f, 0.88f, 0.88f); break;
            }

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
            };
            style.normal.textColor = Color.black;

            Color oldBack = GUI.backgroundColor;
            GUI.backgroundColor = backColor;
            GUILayout.Box("T23 / " + title, style, GUILayout.Height(22), GUILayout.ExpandWidth(true));
            GUI.backgroundColor = oldBack;
        }

        public static GUIStyle HeadlineStyle(bool isMaster = false)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = isMaster ? 20 : 14;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.85f, 0.85f, 0.85f)
                : new Color(0.15f, 0.15f, 0.15f);
            return style;
        }

        public static void ShowSwapButton(T23_Master master, string currentTitle)
        {
            List<string> titles = master.actionTitles;

            int c = titles.IndexOf(currentTitle);
            if (c == -1) { return; }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.currentViewWidth - 100);
            EditorGUI.BeginDisabledGroup(c == 0);
            if (GUILayout.Button("↑"))
            {
                string swapTitle = titles[c - 1];
                titles[c - 1] = currentTitle;
                titles[c] = swapTitle;
                master.OrderComponents();
                master.shouldMoveComponents = true;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(c == titles.Count - 1);
            if (GUILayout.Button("↓"))
            {
                string swapTitle = titles[c + 1];
                titles[c + 1] = currentTitle;
                titles[c] = swapTitle;
                master.OrderComponents();
                master.shouldMoveComponents = true;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static readonly Dictionary<string, string> s_TriggerCategories = new Dictionary<string, string>
        {
            { "T23_OnSpawn",                    "Lifecycle" },
            { "T23_OnDestroy",                  "Lifecycle" },
            { "T23_OnEnable",                   "Lifecycle" },
            { "T23_OnDisable",                  "Lifecycle" },
            { "T23_OnInteract",                 "Interaction" },
            { "T23_OnPickup",                   "Interaction" },
            { "T23_OnDrop",                     "Interaction" },
            { "T23_OnPickupUseDown",            "Interaction" },
            { "T23_OnPickupUseUp",              "Interaction" },
            { "T23_OnEnterTrigger",             "Interaction" },
            { "T23_OnExitTrigger",              "Interaction" },
            { "T23_OnEnterCollider",            "Interaction" },
            { "T23_OnExitCollider",             "Interaction" },
            { "T23_InputJump",                  "Input" },
            { "T23_InputUse",                   "Input" },
            { "T23_InputGrab",                  "Input" },
            { "T23_InputDrop",                  "Input" },
            { "T23_OnKeyDown",                  "Input" },
            { "T23_OnKeyUp",                    "Input" },
            { "T23_OnPlayerJoined",             "Player" },
            { "T23_OnPlayerLeft",               "Player" },
            { "T23_OnPlayerRespawn",            "Player" },
            { "T23_OnAvatarChanged",            "Player" },
            { "T23_OnAvatarEyeHeightChanged",   "Player" },
            { "T23_OnOwnershipTransfer",        "Player" },
            { "T23_OnNetworkReady",             "Network" },
            { "T23_OnStationEntered",           "Station" },
            { "T23_OnStationExited",            "Station" },
            { "T23_OnTimer",                    "Timer" },
            { "T23_OnVideoEnd",                 "Video" },
            { "T23_OnVideoPause",               "Video" },
            { "T23_OnVideoPlay",                "Video" },
            { "T23_OnVideoStart",               "Video" },
            { "T23_UIOnClickButton",            "UI" },
            { "T23_UIOnEndEdit",                "UI" },
            { "T23_UIOnValueChanged",           "UI" },
            { "T23_OnParticleCollision",        "Physics" },
            { "T23_MidiNoteOn",                 "MIDI" },
            { "T23_MidiNoteOff",                "MIDI" },
            { "T23_MidiControlChange",          "MIDI" },
            { "T23_CustomTrigger",              "Logic" },
            { "T23_ConditionalTrigger",         "Logic" },
        };

        private static readonly Dictionary<string, string> s_ActionCategories = new Dictionary<string, string>
        {
            { "T23_SetGameObjectActive",        "GameObject" },
            { "T23_SetChildrenActive",          "GameObject" },
            { "T23_SetNextChildActive",         "GameObject" },
            { "T23_SetRandomChildActive",       "GameObject" },
            { "T23_DestroyObject",              "GameObject" },
            { "T23_SpawnObject",                "GameObject" },
            { "T23_SetParent",                  "GameObject" },
            { "T23_SetLayer",                   "GameObject" },
            { "T23_SpawnObjectPool",            "Object Pool" },
            { "T23_ReturnObjectPool",           "Object Pool" },
            { "T23_ReturnObjectPoolAll",        "Object Pool" },
            { "T23_ShuffleObjectPool",          "Object Pool" },
            { "T23_SetRendererActive",          "Renderer" },
            { "T23_SetMaterial",                "Renderer" },
            { "T23_SetColliderActive",          "Renderer" },
            { "T23_SetAnimatorActive",          "Renderer" },
            { "T23_SetParticlePlaying",         "Renderer" },
            { "T23_AnimationBool",              "Animation" },
            { "T23_AnimationFloat",             "Animation" },
            { "T23_AnimationInt",               "Animation" },
            { "T23_AnimationIntAdd",            "Animation" },
            { "T23_AnimationIntSubtract",       "Animation" },
            { "T23_AnimationIntMultiply",       "Animation" },
            { "T23_AnimationIntDivide",         "Animation" },
            { "T23_AnimationTrigger",           "Animation" },
            { "T23_SetVelocity",                "Physics" },
            { "T23_AddVelocity",                "Physics" },
            { "T23_SetAngularVelocity",         "Physics" },
            { "T23_AddAngularVelocity",         "Physics" },
            { "T23_AddForce",                   "Physics" },
            { "T23_SetIsKinematic",             "Physics" },
            { "T23_SetGravityStrength",         "Physics" },
            { "T23_SetUseGravity",              "Physics" },
            { "T23_AudioPlay",                  "Audio" },
            { "T23_AudioPause",                 "Audio" },
            { "T23_AudioTrigger",               "Audio" },
            { "T23_UseAudioBank",               "Audio" },
            { "T23_SetAudioSourceActive",       "Audio" },
            { "T23_SetPlayerSpeed",             "Player" },
            { "T23_SetPlayerVelocity",          "Player" },
            { "T23_AddPlayerVelocity",          "Player" },
            { "T23_SetAvatarEyeHeight",         "Player" },
            { "T23_SetAvatarUse",               "Player" },
            { "T23_SetAvatarAudioParameters",   "Player" },
            { "T23_SetVoiceParameters",         "Player" },
            { "T23_SetJumpImpulse",             "Player" },
            { "T23_TeleportPlayer",             "Teleport" },
            { "T23_TeleportObject",             "Teleport" },
            { "T23_PickupDrop",                 "Pickup" },
            { "T23_PickupHaptic",               "Pickup" },
            { "T23_SetUIBool",                  "UI" },
            { "T23_SetUIFloat",                 "UI" },
            { "T23_SetUIInt",                   "UI" },
            { "T23_SetUIText",                  "UI" },
            { "T23_TakeOwnership",              "System" },
            { "T23_UseAttachedStation",         "System" },
            { "T23_SetPropertyBox",             "System" },
            { "T23_CallUdonMethod",             "System" },
            { "T23_ActiveConditionalTrigger",   "Logic" },
            { "T23_ActiveCustomTrigger",        "Logic" },
            { "T23_UseLegacyLocomotion",        "Deprecated" },
            { "T23_SetLtcgiState",             "Integration" },
        };

        public static Dictionary<string, Type> GetModuleClasses(Type baseType, bool initialSplit)
        {
            Dictionary<string, string> categoryMap = null;
            if (initialSplit)
            {
                if (baseType == typeof(T23_TriggerBase))     categoryMap = s_TriggerCategories;
                else if (baseType == typeof(T23_ActionBase)) categoryMap = s_ActionCategories;
            }

            var moduleList = new Dictionary<string, Type>();
            var modules = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract).ToArray();

            foreach (var module in modules)
            {
                string displayName = module.Name.Replace("T23_", "");
                string key;
                if (categoryMap != null)
                {
                    string category;
                    if (!categoryMap.TryGetValue(module.Name, out category))
                        category = "Other";
                    key = category + "/" + displayName;
                }
                else
                {
                    key = displayName;
                }
                if (!moduleList.ContainsKey(key))
                    moduleList.Add(key, module);
            }
            return moduleList;
        }

        public static List<T23_BroadcastGlobal> GetAllBroadcastGlobals()
        {
            List<T23_BroadcastGlobal> broadcastGlobals = new List<T23_BroadcastGlobal>();
            GameObject[] rootObjs = null;
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                rootObjs = new GameObject[1];
                rootObjs[0] = stage.prefabContentsRoot;
            }
            else
            {
                rootObjs = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            }
            if (rootObjs.Length > 0)
            {
                foreach (var rootObj in rootObjs)
                {
                    var udons = rootObj.GetComponentsInChildren<UdonBehaviour>(true);
                    foreach (var udon in udons)
                    {
                        var proxy = UdonSharpEditorUtility.GetProxyBehaviour(udon);
                        if (proxy == null) { continue; }

                        var broadcast = proxy as T23_BroadcastGlobal;
                        if (broadcast == null) { continue; }

                        broadcastGlobals.Add(broadcast);
                    }
                }
            }
            return broadcastGlobals;
        }

        public static List<T23_CommonBuffer> GetAllCommonBuffers()
        {
            var commonBuffers = new List<T23_CommonBuffer>();
            GameObject[] rootObjs = null;
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                rootObjs = new GameObject[1];
                rootObjs[0] = stage.prefabContentsRoot;
            }
            else
            {
                rootObjs = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            }
            if (rootObjs.Length > 0)
            {
                foreach (var rootObj in rootObjs)
                {
                    var udons = rootObj.GetComponentsInChildren<UdonBehaviour>(true);
                    foreach (var udon in udons)
                    {
                        var proxy = UdonSharpEditorUtility.GetProxyBehaviour(udon);
                        if (proxy == null) { continue; }

                        var commonBuffer = proxy as T23_CommonBuffer;
                        if (commonBuffer == null) { continue; }

                        commonBuffers.Add(commonBuffer);
                    }
                }
            }
            return commonBuffers;
        }

        public static T23_BroadcastGlobal[] TakeCommonBuffersRelate(T23_CommonBuffer commonBuffer)
        {
            List<T23_BroadcastGlobal> broadcastGlobals = new List<T23_BroadcastGlobal>();
            var allbroadcasts = GetAllBroadcastGlobals();
            foreach (var broadcast in allbroadcasts)
            {
                var param = broadcast.commonBuffer;
                if (param != null && param == commonBuffer)
                {
                    broadcastGlobals.Add(broadcast);
                }
            }
            return broadcastGlobals.ToArray();
        }

        public static void UpdateAllCommonBuffersRelate()
        {
            if (!commonBufferUpdateTask)
            {
                commonBufferUpdateTask = true;
                EditorApplication.delayCall += () => UpdateAllCommonBuffersRelate_Delayed();
            }
        }

        private static void UpdateAllCommonBuffersRelate_Delayed()
        {
            var commonBuffers = GetAllCommonBuffers();
            foreach (var commonBuffer in commonBuffers)
            {
                commonBuffer.broadcasts = TakeCommonBuffersRelate(commonBuffer);
                UdonSharpEditorUtility.CopyProxyToUdon(commonBuffer);
            }
            commonBufferUpdateTask = false;
        }

        public static void JoinAllBufferingBroadcasts(T23_CommonBuffer commonBuffer)
        {
            var broadcasts = GetAllBroadcastGlobals(); ;
            foreach (var broadcast in broadcasts)
            {
                if (broadcast.commonBuffer == null && broadcast.bufferType != 0)
                {
                    broadcast.commonBuffer = commonBuffer;
                    UdonSharpEditorUtility.CopyProxyToUdon(broadcast);
                }
            }
            commonBuffer.broadcasts = TakeCommonBuffersRelate(commonBuffer);
            UdonSharpEditorUtility.CopyProxyToUdon(commonBuffer);
        }

        public static T23_CommonBuffer GetAutoJoinCommonBuffer(T23_BroadcastGlobal broadcast)
        {
            var commonBuffers = GetAllCommonBuffers();
            foreach (var commonBuffer in commonBuffers)
            {
                if (commonBuffer.autoJoin)
                {
                    broadcast.commonBuffer = commonBuffer;
                    return commonBuffer;
                }
            }
            return null;
        }

        public static string ToUnityFieldName(string before)
        {
            var _array = before.ToCharArray();
            _array[0] = char.ToUpper(_array[0]);
            var _ins = new List<int>();
            for (int i = _array.Length - 1; i > 0; i--)
            {
                if (char.IsUpper(_array[i])) { _ins.Add(i); }
            }
            var _list = new List<char>(_array);
            for (int i = 0; i < _ins.Count; i++)
            {
                _list.Insert(_ins[i], ' ');
            }
            return new string(_list.ToArray());
        }

        public static T23_CommonBuffer AddCommonBuffer()
        {
            GameObject[] rootObjs = null;
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                rootObjs = new GameObject[1];
                rootObjs[0] = stage.prefabContentsRoot;
            }
            else
            {
                rootObjs = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            }
            int num = 1;
            string objName = "";
            while (true)
            {
                objName = "CommonBuffer" + (num == 1 ? "" : num.ToString());
                bool duplicate = false;
                foreach (var obj in rootObjs)
                {
                    if (obj.name == objName)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                {
                    break;
                }
                num++;
            }
            var commonBufferObj = new GameObject(objName);
            var commonBuffer = commonBufferObj.AddComponent<T23_CommonBuffer>();
            return commonBuffer;
        }
    }
}
#endif
