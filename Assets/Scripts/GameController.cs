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
	public GameObject treePrefab;
	public GameObject canvasCharacterInfo;
	public Text textName;
	public Text textHP;
	public GameObject canvasGameState;
	public Text textGameState;
	public Transform dynamicCanvas;
	public Button buttonActionPrefab;
	public Camera mainCamera;
	public string[] buttonActionsLabels;
	public List<Button> buttonActions;
	public Character characterSelected;
	public ActionExecutor actionCalculator;
	public bool isExecutingActions;

	void Awake() {
		this.buttonActionsLabels = new string[] {
			"移动",
			"死亡",
			"爆裂",
			"沉默",
			"后跳",
			"射击"
		};
		this.actionCalculator = new ActionExecutor ();
		this.isExecutingActions = false;
	}

	// Use this for initialization
	void Start () {
		initializeScene ();
	}

	// Update is called once per frame
	void Update () {
		if (checkTurnIsOver ()) {
			StartCoroutine(settleTurn ());
		}
	}
		
	// Set up items over grid
	void initializeScene() {
		createCharacters ();
		createBases ();
		createBarriers ();
	}

	void createCharacters() {
		var blueCell = this.grid.getCellByCubeCoordinates (0, this.grid.height / 2);
		var redCell = this.grid.getCellByCubeCoordinates (this.grid.width - 1, this.grid.height / 2);
		blueCell.enableCell();
		createCharacter (blueCell.coordinates.X, blueCell.coordinates.Y, "Link");
		redCell.enableCell ();
		createCharacter (redCell.coordinates.X, redCell.coordinates.Y, "Ganon");
	}

	void createBases() {
		var blueCell = this.grid.getCellByCubeCoordinates (0, this.grid.height / 2);
		var redCell = this.grid.getCellByCubeCoordinates (this.grid.width - 1, this.grid.height / 2);
		blueCell.enableCell();
		Instantiate (blueBasePrefab, blueCell.transform.position, Quaternion.identity, this.grid.transform);
		redCell.enableCell ();
		Instantiate (redBasePrefab, redCell.transform.position, Quaternion.identity, this.grid.transform);
	}

	void createBarriers() {
		HexCell[] cells = this.grid.getCells ();
		int randomIndex = UnityEngine.Random.Range (1, cells.Length - 1);
		while (!cells [randomIndex].isActiveAndEnabled) {
			randomIndex = UnityEngine.Random.Range (1, cells.Length - 1);
		}
		Instantiate (treePrefab, cells [randomIndex].transform.position, Quaternion.identity, this.grid.transform);
		cells [randomIndex].blockCell ();
	}

	bool checkTurnIsOver() {
		foreach (var character in this.characters) {
			if (character.moveable || this.isExecutingActions) return false;
		}
		return true;
	}

	// Calculate attack damage or other affects that characters create
	IEnumerator settleTurn() {
		this.isExecutingActions = true;
		for (int i = 0; this.actionCalculator.executeActionsByPhase (i); i++) {
			this.textGameState.text = "Phase " + (i + 1);
			this.canvasGameState.SetActive (true);
			yield return new WaitForSeconds (2);
		}
		this.canvasGameState.SetActive (false);
		startNewTurn ();
		this.isExecutingActions = false;
	}

	void startNewTurn() {
		// Enables cells & characters
		foreach (var character in this.characters) {
			HexCell cell = character.getCurrentCell ();
			cell.enableCell ();
			character.moveable = true;
			character.avatar.moveable = true;
			character.actionPoint = 3;

		}
	}

	Character createCharacter(int cubeX, int cubeY, string name) {
		Character character = Instantiate (characterPrefab, grid.transform, false);
		character.Initialize (cubeX, cubeY, this.grid, name, canvasCharacterInfo, textName, textHP,
			this.actionCalculator);
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
		(this.characterSelected as AvatarCharacter).receiveAction ((sender as Button).GetComponentInChildren<Text> ().text);

		// Destroy buttons
		foreach (var button in this.buttonActions) {
			Destroy (button.gameObject);
		}
		this.buttonActions.RemoveRange (0, buttonActions.Count);
	}
}
