using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

namespace BigheadRunRun
{
    public enum GameState
    {
        Prepare,
        Playing,
        Paused,
        PreGameOver,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event System.Action<Transform> PlayerHasBeenSpawned;

        public static event System.Action<GameState, GameState> GameStateChanged;

        private static bool isRestart;

        public enum PointCountingMode
        {
            ByDistance,
            ByJumping
        }

        public GameState GameState
        {
            get
            {
                return _gameState;
            }
            private set
            {
                if (value != _gameState)
                {
                    GameState oldState = _gameState;
                    _gameState = value;

                    if (GameStateChanged != null)
                        GameStateChanged(_gameState, oldState);
                }
            }
        }

        public static int GameCount
        {
            get { return _gameCount; }
            private set { _gameCount = value; }
        }

        private static int _gameCount = 0;

        [Header("Set the target frame rate for this game")]
        [Tooltip("Use 60 for games requiring smooth quick motion, set -1 to use platform default frame rate")]
        public int targetFrameRate = 30;

        [Header("Current game state")]
        [SerializeField]
        private GameState _gameState = GameState.Prepare;

        // List of public variable for gameplay tweaking
        [Header("Gameplay Config")]
        public float splashLifeTime = 3f;
        public float minSplashScale = 0.2f;
        public float maxSplashScale = 1.4f;
        public int minNumberOfSplash = 4;
        public int maxNumberOfSplash = 6;
        [Range(0f, 1f)]
        public float coinFrequency = 0.1f;
        public int numberOfMovingWall = 50;
        public float fftVisualizationMagnitute = 8;
        public float fftOffsetFactor = 4f;
        public float fftDropDownTime = 1f;
        public float fftThreshold = 0.1f;
        public float jumpTime = 1;
        public float playerSpeed = 30;
        [HideInInspector]
        public bool lockCameraMovementX = true;
        [HideInInspector]
        public bool lockCameraMovementY = true;
        public float jumpHeight = 50;
        public PointCountingMode pointCountingMode = PointCountingMode.ByJumping;
        public float distancePerPoint = 500;
        public int pointPerjump = 1;
        [HideInInspector]
        public bool enableMovingWall = false;
        // List of public variables referencing other objects
        [Header("Object References")]
        public Transform lCenter;
        public Transform rCenter;
        public PlayerController playerController;
        public GameObject playerPrefab;
        public Material blobMaterial;
        public List<GameObject> InstantiatedWall = new List<GameObject>();
        public GameObject originalWall;
        public GameObject splashOnGroundPrefab;
        public GameObject coinPrefab;
        public CameraController cameraController;
        public GameObject originalPlane;
        public LayerMask groundLayerMask;
        public GameObject splashPrefab;
        [HideInInspector]
        public List<GameObject> coinList = new List<GameObject>();
        void OnEnable()
        {
            PlayerController.PlayerDied += PlayerController_PlayerDied;
            GameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            PlayerController.PlayerDied -= PlayerController_PlayerDied;
            GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState arg1, GameState arg2)
        {
            if (arg1.Equals(GameState.Playing))
            {
                if (CharacterScroller.Instance.playerTemp)
                    DestroyImmediate(CharacterScroller.Instance.playerTemp);
                GameObject currentCharacter = CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex];
                if (currentCharacter != null)
                {
                    playerPrefab = currentCharacter;
                }
                GameObject player = Instantiate(playerPrefab) as GameObject;
                StartCoroutine(WaitToGetPlayerController(player));
            }
        }

        private IEnumerator WaitToGetPlayerController(GameObject player)
        {
            yield return new WaitForEndOfFrame();
            playerController = player.GetComponentInChildren<PlayerController>();
            if (PlayerHasBeenSpawned != null)
                PlayerHasBeenSpawned(playerController.transform);
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            // Initial setup
            Application.targetFrameRate = targetFrameRate;
            ScoreManager.Instance.Reset();
            PrepareGame();
            SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
            cameraController = Camera.main.GetComponent<CameraController>();
        }

        // Update is called once per frame

        // Listens to the event when player dies and call GameOver
        void PlayerController_PlayerDied()
        {
            GameOver();
            AdManager.admanagerInstance.ShowInterstitial();
        }

        // Make initial setup and preparations before the game can be played
        public void PrepareGame()
        {
            GameState = GameState.Prepare;

            // Automatically start the game if this is a restart.
            if (isRestart)
            {
                isRestart = false;
                StartGame();
            }
        }

        // A new game official starts
        public void StartGame()
        {
            GameState = GameState.Playing;
            //if (SoundManager.Instance.background != null)
            //{
            //    SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
            //}
        }

        // Called when the player died
        public void GameOver()
        {
            //if (SoundManager.Instance.background != null)
            //{
            //    SoundManager.Instance.StopMusic();
            //}

            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
            GameState = GameState.GameOver;
            GameCount++;

            // Add other game over actions here if necessary
        }

        // Start a new game
        public void RestartGame(float delay = 0, bool characterSelectionCall = false)
        {
            isRestart = !characterSelectionCall;
            StartCoroutine(CRRestartGame(delay));
        }

        IEnumerator CRRestartGame(float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void HidePlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(false);
        }

        public void ShowPlayer()
        {
            if (playerController != null)
                playerController.gameObject.SetActive(true);
        }

        public void CleanScene()
        {
            foreach (var coin in coinList)
            {
                if (coin)
                    Destroy(coin);
            }
            WallManager.Instance.wallList = new List<GameObject>();
            for (int i = 0; i < PlaneManager.Instance.transform.childCount; i++)
            {
                Destroy(PlaneManager.Instance.transform.GetChild(i).gameObject);
            }
            if (playerController != null)
                Destroy(playerController.transform.parent.gameObject);
            if (CharacterScroller.Instance.playerTemp)
                DestroyImmediate(CharacterScroller.Instance.playerTemp);
            cameraController.ResetPos();
            foreach (var item in InstantiatedWall)
            {
                DestroyImmediate(item);
            }
            InstantiatedWall = new List<GameObject>();
            originalWall.SetActive(true);
            originalWall.GetComponent<Wall>().spawned = false;
            originalPlane.GetComponent<PlaneScript>().spawned = false;
            ScoreManager.Instance.Reset();
            GameState = GameState.Prepare;
        }
        //----------------------------------//
    }
}
