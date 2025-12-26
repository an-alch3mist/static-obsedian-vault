using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PythonInterpreter
{
    /// <summary>
    /// Manages the console UI for displaying script output
    /// Supports rich text formatting for errors and normal output
    /// </summary>
    public class ConsoleManager : MonoBehaviour
    {
        #region Public Fields
        [Header("UI References")]
        public TextMeshProUGUI ConsoleText;
        public ScrollRect ScrollRect;
        public int MaxLines = 1000;
        
        [Header("Settings")]
        public bool AutoScroll = true;
        public bool TimestampEnabled = false;
        #endregion

        #region Private Fields
        private List<string> logLines;
        private bool needsScrollUpdate;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            logLines = new List<string>();
            
            if (ConsoleText == null)
            {
                Debug.LogError("ConsoleText reference is missing in ConsoleManager!");
            }
        }

        private void LateUpdate()
        {
            // Auto-scroll to bottom if needed
            if (needsScrollUpdate && AutoScroll && ScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                ScrollRect.verticalNormalizedPosition = 0f;
                needsScrollUpdate = false;
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Logs a normal message
        /// </summary>
        public void Log(string message)
        {
            AddLine(message, "white");
        }

        /// <summary>
        /// Logs an error message in red
        /// </summary>
        public void LogError(string message)
        {
            AddLine(message, "red");
        }

        /// <summary>
        /// Logs a warning message in yellow
        /// </summary>
        public void LogWarning(string message)
        {
            AddLine(message, "yellow");
        }

        /// <summary>
        /// Clears the console
        /// </summary>
        public void Clear()
        {
            logLines.Clear();
            UpdateDisplay();
        }
        #endregion

        #region Private Methods
        private void AddLine(string message, string color)
        {
            string timestamp = "";
            if (TimestampEnabled)
            {
                timestamp = string.Format("[{0:HH:mm:ss}] ", DateTime.Now);
            }

            string formattedLine;
            if (color != "white")
            {
                formattedLine = string.Format("{0}<color={1}>{2}</color>", timestamp, color, message);
            }
            else
            {
                formattedLine = timestamp + message;
            }

            logLines.Add(formattedLine);

            // Limit number of lines
            while (logLines.Count > MaxLines)
            {
                logLines.RemoveAt(0);
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (ConsoleText == null)
                return;

            ConsoleText.text = string.Join("\n", logLines.ToArray());
            needsScrollUpdate = true;
        }
        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Test Log Messages")]
        private void TestLogMessages()
        {
            Log("Normal log message");
            LogWarning("This is a warning");
            LogError("This is an error message");
            Log("Testing <b>bold</b> and <i>italic</i> text");
        }

        [ContextMenu("Clear Console")]
        private void EditorClear()
        {
            Clear();
        }
#endif
        #endregion
    }
}
