using UnityEngine;
using System.Collections;

namespace BigheadRunRun
{
    public class PlayerController : MonoBehaviour
    {
        public static event System.Action PlayerDied;

        void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        void Start()
        {
            // Setup       
        }

        // Update is called once per frame
        void Update()
        {
            // Activities that take place every frame
        }

        // Listens to changes in game state
        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                // Do whatever necessary when a new game starts
            }
        }

        // Calls this when the player dies and game over
        public void Die()
        {
            // Fire event
            if (PlayerDied != null)
                PlayerDied();
        }
    }
}
