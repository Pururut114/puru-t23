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

        public static Dictionary<string, Type> GetModuleClasses(Type baseType, bool initialSplit)
        {
            Dictionary<string, Type> moduleList = new Dictionary<string, Type>();
            var modules = Assembly.GetAssembly(baseType).GetTypes().Where(t => { return t.IsSubclassOf(baseType); }).ToArray();
            foreach (var module in modules)
            {
                string split = initialSplit ? (module.Name.Substring(4, 1) + "/") : "";
                string key = split + module.Name.Replace("T23_", "");
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
