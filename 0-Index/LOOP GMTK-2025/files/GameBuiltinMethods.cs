using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PythonInterpreter
{
    /// <summary>
    /// Registers game-specific builtin methods that can yield to Unity
    /// Examples: move(), harvest(), sleep(), get_pos(), etc.
    /// </summary>
    public class GameBuiltinMethods : MonoBehaviour
    {
        #region Public Fields
        [Header("References")]
        public CoroutineRunner CoroutineRunner;
        public Transform PlayerTransform;
        public GameObject SpeechBubble;
        
        [Header("Game State")]
        public int GridWidth = 10;
        public int GridHeight = 10;
        #endregion

        #region Private Fields
        private Vector2Int playerPosition;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (PlayerTransform != null)
            {
                playerPosition = new Vector2Int(
                    Mathf.RoundToInt(PlayerTransform.position.x),
                    Mathf.RoundToInt(PlayerTransform.position.y)
                );
            }
        }

        private void Start()
        {
            RegisterGameBuiltins();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Registers all game-specific builtin functions
        /// </summary>
        public void RegisterGameBuiltins()
        {
            if (CoroutineRunner == null)
            {
                Debug.LogError("CoroutineRunner reference is missing!");
                return;
            }

            PythonInterpreter interpreter = CoroutineRunner.GetInterpreter();
            if (interpreter == null)
            {
                Debug.LogError("Failed to get interpreter from CoroutineRunner!");
                return;
            }

            // Movement
            interpreter.RegisterBuiltin("move", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "move() requires 1 argument");
                
                string direction = args[0].ToString().ToLower();
                return Move(direction);
            });

            // Harvest
            interpreter.RegisterBuiltin("harvest", (args) =>
            {
                return Harvest();
            });

            // Position getters
            interpreter.RegisterBuiltin("get_pos", (args) =>
            {
                List<object> pos = new List<object>();
                pos.Add((double)playerPosition.x);
                pos.Add((double)playerPosition.y);
                return pos;
            });

            interpreter.RegisterBuiltin("get_pos_x", (args) =>
            {
                return (double)playerPosition.x;
            });

            interpreter.RegisterBuiltin("get_pos_y", (args) =>
            {
                return (double)playerPosition.y;
            });

            // Grid size
            interpreter.RegisterBuiltin("get_grid_size", (args) =>
            {
                List<object> size = new List<object>();
                size.Add((double)GridWidth);
                size.Add((double)GridHeight);
                return size;
            });

            interpreter.RegisterBuiltin("get_grid_width", (args) =>
            {
                return (double)GridWidth;
            });

            interpreter.RegisterBuiltin("get_grid_height", (args) =>
            {
                return (double)GridHeight;
            });

            // Pathfinding helpers
            interpreter.RegisterBuiltin("is_passable", (args) =>
            {
                if (args.Count < 2)
                    throw new RuntimeError(0, "is_passable() requires 2 arguments");
                
                int x = (int)Math.Round((double)args[0]);
                int y = (int)Math.Round((double)args[1]);
                
                return IsPassable(x, y);
            });

            interpreter.RegisterBuiltin("is_block", (args) =>
            {
                if (args.Count < 2)
                    throw new RuntimeError(0, "is_block() requires 2 arguments");
                
                int x = (int)Math.Round((double)args[0]);
                int y = (int)Math.Round((double)args[1]);
                
                return !IsPassable(x, y);
            });

            interpreter.RegisterBuiltin("is_goal", (args) =>
            {
                if (args.Count < 2)
                    throw new RuntimeError(0, "is_goal() requires 2 arguments");
                
                int x = (int)Math.Round((double)args[0]);
                int y = (int)Math.Round((double)args[1]);
                
                // Example goal position
                return x == GridWidth - 1 && y == GridHeight - 1;
            });

            interpreter.RegisterBuiltin("can_move", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "can_move() requires 1 argument");
                
                string direction = args[0].ToString().ToLower();
                Vector2Int newPos = GetDirectionOffset(direction);
                newPos += playerPosition;
                
                return IsPassable(newPos.x, newPos.y);
            });

            // UI and timing
            interpreter.RegisterBuiltin("say", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "say() requires 1 argument");
                
                string text = args[0].ToString();
                Say(text);
                return null;
            });

            interpreter.RegisterBuiltin("sleep", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "sleep() requires 1 argument");
                
                float seconds = (float)(double)args[0];
                return new WaitForSeconds(seconds);
            });

            // Submission (for puzzle verification)
            interpreter.RegisterBuiltin("submit", (args) =>
            {
                if (args.Count < 1)
                    throw new RuntimeError(0, "submit() requires 1 argument");
                
                string answer = args[0].ToString();
                Debug.Log("Submitted: " + answer);
                return null;
            });
        }
        #endregion

        #region Private Methods - Game Actions
        private YieldInstruction Move(string direction)
        {
            Vector2Int offset = GetDirectionOffset(direction);
            Vector2Int newPos = playerPosition + offset;

            // Check bounds
            if (newPos.x < 0 || newPos.x >= GridWidth || newPos.y < 0 || newPos.y >= GridHeight)
            {
                Debug.LogWarning("Cannot move outside grid bounds");
                return new WaitForSeconds(0.1f);
            }

            // Check if passable
            if (!IsPassable(newPos.x, newPos.y))
            {
                Debug.LogWarning("Cannot move to blocked position");
                return new WaitForSeconds(0.1f);
            }

            // Update position
            playerPosition = newPos;

            // Move player transform
            if (PlayerTransform != null)
            {
                PlayerTransform.position = new Vector3(newPos.x, newPos.y, PlayerTransform.position.z);
            }

            return new WaitForSeconds(0.2f);
        }

        private YieldInstruction Harvest()
        {
            Debug.Log("Harvesting at position: " + playerPosition);
            return new WaitForSeconds(0.3f);
        }

        private void Say(string text)
        {
            Debug.Log("[Player Says]: " + text);
            
            // Show speech bubble if available
            if (SpeechBubble != null)
            {
                SpeechBubble.SetActive(true);
                // You can add TMPro text update here
            }
        }

        private Vector2Int GetDirectionOffset(string direction)
        {
            switch (direction)
            {
                case "up":
                case "u":
                    return new Vector2Int(0, 1);
                case "down":
                case "d":
                    return new Vector2Int(0, -1);
                case "left":
                case "l":
                    return new Vector2Int(-1, 0);
                case "right":
                case "r":
                    return new Vector2Int(1, 0);
                default:
                    throw new RuntimeError(0, "Invalid direction: " + direction);
            }
        }

        private bool IsPassable(int x, int y)
        {
            // Check bounds
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
                return false;

            // Example: block certain positions
            // You can implement your own collision detection here
            // For now, all positions are passable
            return true;
        }
        #endregion
    }
}
