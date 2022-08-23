using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	private float score; //The score of the player ball.
	public float movementSpeed; //The movement speed of the player ball.
	private float rotationSpeed; //The rotation speed of the player ball.
	private bool movingLeft; //This will make the player ball move left.
	public GUIStyle GameScoreGUI = new GUIStyle(); //This is the GUI style of the score normal.
	public AudioClip PlayerDieSound; //The sound that will play when the player dies and game is lost.
	public AudioClip SlideSwitchSound; //The sound that will play when the player dies and game is lost.
	bool showGUI = true; //The GUI that will be displayed 
	bool showGUI2 = false; //The GUI that will be displayed 
	public Texture GameOverFullScreen; //The GUI that will display a big texture on the full screen when you lose.
	//public Texture GameOverScoreBoard; //The GUI that will display scoreboard texture on the middle of the screen when you lose.
	public GUIStyle EndGameScoreGUI = new GUIStyle(); //This is the GUI style of the score normal.
	public Texture ExitGameButtonHUD; //The exit HUD button that will display when the game ends.
	public Texture RestartGameButtonHUD; //The restart HUD button that will display when the game ends.
	float m_Timer = 1.0f; //How much time will pass till you get an increase in the score distance. 1.0f for 1 second.
	bool TimerStop = false; //Bool to calculate timer.
	bool PlaySoundOnce = true; //Is sliding sound enabled?

	void Start () {
		score = 0; //Score will start at 0 always on start.
		movingLeft = true; //Moving left is true at start.
		//rotationSpeed = 1.5f; //Set the rotation speed at start so it will start to rotate at the begging.
		TimerStop = true; //Set the timer to true to calculate the time distance.
		PlaySoundOnce = true; //The play sound once will be true at start.
	}

	void StartTimer () {
		if (TimerStop) {
			m_Timer -= Time.deltaTime;
			if(m_Timer <= 0.00f) {
				score++;
				m_Timer = 0.01f;
			}
		}
	}

	void Update () {
		transform.Translate(Vector3.down * Time.deltaTime * movementSpeed);

		if (TimerStop) {
			m_Timer -= Time.deltaTime;
			if(m_Timer <= 0.00f) {
				score++;
				m_Timer = 1.00f;
			}	
		}
			
		if (Input.GetMouseButtonDown(0)) {
			if (PlaySoundOnce) {
			GetComponent<AudioSource>().PlayOneShot(SlideSwitchSound, 2.7F);
				PlaySoundOnce = false;
				PlaySoundOnce = true;
			}
			rotationSpeed = .5f;
			movingLeft = !movingLeft;
			
		}

		if (Input.GetMouseButton(0)) {
			rotationSpeed += 1.5f * Time.deltaTime;
		}

		if(movingLeft)
			transform.Rotate(0, 0, rotationSpeed);
		else
			transform.Rotate(0, 0, -rotationSpeed);
		}

		void OnTriggerEnter2D(Collider2D other) {
		if(other.tag == "KillPlayer")
			Die();
	}	



	void Die() {
		TimerStop = false;
		PlaySoundOnce = false;
		rotationSpeed = 0f;
		movementSpeed = 0.1f;
		this.gameObject.GetComponentInChildren<Renderer> ().enabled = false;
		this.gameObject.GetComponentInChildren<Collider2D> ().enabled = false;
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		r.enabled = false;
		Destroy (GameObject.FindWithTag("BackgroundMusic"));
		GetComponent<AudioSource>().PlayOneShot(PlayerDieSound, 2.7F);
		showGUI = false;
		showGUI2 = true;

        AdManager.admanagerInstance.ShowInterstitial();
    }

	void OnGUI() {
		if (showGUI) {
			GUI.Label (new Rect (7, 17, 110, 110), score + "<b></b>" , GameScoreGUI); 
		}

		if (showGUI2) {
			showGUI = false;
			GUI.DrawTexture(new Rect(((Screen.width / 3) - 868f), ((Screen.height / 5) - 741.5f), 5600, 2975), GameOverFullScreen, ScaleMode.ScaleToFit, true, 0.0F); //Display the end game full screen.
			//GUI.DrawTexture(new Rect(((Screen.width / 2) - 240f), ((Screen.height / 2) - 405.5f), 475, 800), GameOverScoreBoard, ScaleMode.ScaleToFit, true, 0.0F); //Display the end game score board.
			GUI.Label (new Rect(Screen.width / 2 - 48f, ((Screen.height / 2) - 34.5f), 40, -210), "<b>Score</b>: " + score + "s", EndGameScoreGUI);

			if ( GUI.Button(new Rect(Screen.width / 2 - (320f / 2), Screen.height / 2 - -150, 110, 110), ExitGameButtonHUD)) {
				Application.LoadLevel(0);
			}
				
			if ( GUI.Button(new Rect(Screen.width / 2 - (-100f / 2), Screen.height / 2 - -150, 110, 110), RestartGameButtonHUD)) {
				Application.LoadLevel(Application.loadedLevel);
			}

		} 

	}
}
