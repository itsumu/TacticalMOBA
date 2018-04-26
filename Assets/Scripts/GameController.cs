using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	public List<Character> characters;
	public HexGrid grid;
	public Character characterPrefab;
	public Base blueBasePrefab;
	public Base redBasePrefab;
	public GameObject canvasCharacterInfo;
	public Text textName;
	public Text textHP;
	public GameObject canvasGameState;
	public Transform dynamicCanvas;
	public Button buttonActionPrefab;
	public Camera mainCamera;
	public string[] buttonActionsLabels;
	public List<Button> buttonActions;
	public Character characterSelected;

	void Awake() {
		this.buttonActionsLabels = new string[] {
			"移动",
			"死亡",
			"爆裂",
			"沉默",
			"后跳",
			"射击"
		};
	}

	// Use this for initialization
	void Start () {
		initializeScene ();
	}

	// Update is called once per frame
	void Update () {
		if (turnIsOver ()) {
			settleTurn ();
			readyForNewTurn (); // Set off turn is over
			StartCoroutine (startNewTurn ());
		}
	}
		
	// Set up characters & bases
	void initializeScene() {
		createCharacters ();
	}

	void createCharacters() {
		var blueCell = this.grid.getCellByCubeCoordinates (0, this.grid.height / 2);
		var redCell = this.grid.getCellByCubeCoordinates (this.grid.width - 1, this.grid.height / 2);
		blueCell.enableCell();
		createCharacter (blueCell.coordinates.X, blueCell.coordinates.Y, "Link");
		Instantiate (blueBasePrefab, blueCell.transform.position, Quaternion.identity, this.grid.transform);
		redCell.enableCell ();
		createCharacter (redCell.coordinates.X, redCell.coordinates.Y, "Ganon");
		Instantiate (redBasePrefab, redCell.transform.position, Quaternion.identity, this.grid.transform);
	}

	bool turnIsOver() {
		foreach (var character in this.characters) {
			if (character.moveable) return false;
		}
		return true;
	}

	// Calculate attack damage or other affects that characters create
	void settleTurn() {
		// Attack damage calculations
		for (int i = 0; i < this.characters.Count; i++) {
			for (int j = 0; j < this.characters.Count; j++) {
				if (i == j)
					continue;
				if (this.characters[i].ableToAttack (this.characters[j].coordinates)) {
					this.characters [i].attack (this.characters [j]);
				}
			}
		}
	}

	void readyForNewTurn() {
		// Enable characters
		foreach (var character in this.characters) {
			character.moveable = true;
			character.avatar.moveable = true;
		}
	}

	IEnumerator startNewTurn() {
		// Show 
		this.canvasGameState.SetActive (true);
		yield return new WaitForSeconds (2);
		this.canvasGameState.SetActive (false);
		// Enables cells
		foreach (var character in this.characters) {
			HexCell cell = character.getCurrentCell ();
			cell.enableCell ();
		}
	}

	Character createCharacter(int cubeX, int cubeY, string name) {
		Character character = Instantiate (characterPrefab, grid.transform, false);
		character.Initialize (cubeX, cubeY, this.grid, name, canvasCharacterInfo, textName, textHP);
		character.mainCanvas = this.canvasCharacterInfo;
		character.textName = this.textName;
		character.textHP = this.textHP;
		character.avatar.CharacterSelected += OnSelectCharacter; // Add event
		this.characters.Add (character);
		return character;
	}

	// Events

	void OnSelectCharacter(object sender, EventArgs args) {
		this.characterSelected = sender as Character;
		HexCoordinates characterCoordinates = this.characterSelected.coordinates;

		// Generate action buttons
		for (int i = 0; i < this.buttonActionsLabels.Length; i++) {
			Button button = Instantiate (this.buttonActionPrefab, this.dynamicCanvas, false);
			Vector3 tempVector = this.grid.getPositionByCoordinates(HexGrid.directions [i].X + characterCoordinates.X,
				HexGrid.directions [i].Y + characterCoordinates.Y);
			tempVector += this.grid.transformOffset;
			tempVector.z = button.transform.position.z;
			button.transform.position = this.mainCamera.WorldToScreenPoint (tempVector);
			button.GetComponentInChildren<Text> ().text = this.buttonActionsLabels[i];
			// Bind button events & store in gamecontroller
			TakeAction actionToTake = button.GetComponent<TakeAction> ();
			actionToTake.TriggerClickButtonAction += OnTriggerClickButtonAction;
			this.buttonActions.Add (button);
		}
	}

	void OnTriggerClickButtonAction(object sender, EventArgs args) { // The sender is button (parent of exact sender)
		// Assign avatar character to take action
		(this.characterSelected as AvatarCharacter).takeAction ((sender as Button).GetComponentInChildren<Text> ().text);

		// Destroy buttons
		foreach (var button in this.buttonActions) {
			Destroy (button.gameObject);
		}
		this.buttonActions.RemoveRange (0, buttonActions.Count);
	}
}
