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

	void Awake() {
	}

	// Use this for initialization
	void Start () {
		initializeScene ();
	}

	// Update is called once per frame
	void Update () {
		if (turnIsOver ()) {
			settleActions ();
			readyForNewTurn (); // Set off turn is over
			StartCoroutine (startNewTurn ());
		}
	}
		
	// Set up characters & bases
	void initializeScene() {
		HexCell[] cells = grid.getCells ();
		cells [0].enableCell();
		createCharacter (cells[0].coordinates.X, cells[0].coordinates.Y, "Link");
		Instantiate (blueBasePrefab, cells[0].transform.position, Quaternion.identity, this.grid.transform);
		cells [cells.Length - 1].enableCell ();
		createCharacter (cells[cells.Length - 1].coordinates.X, cells[cells.Length - 1].coordinates.Y, "Ganon");
		Instantiate (redBasePrefab, cells[cells.Length - 1].transform.position, Quaternion.identity, this.grid.transform);
	}

	bool turnIsOver() {
		foreach (var character in this.characters) {
			if (character.moveable) return false;
		}
		return true;
	}

	void readyForNewTurn() {
		foreach (var character in this.characters) {
			character.moveable = true;
		}
	}

	// Calculate attack damage or other affects that characters create
	void settleActions() {
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

	IEnumerator startNewTurn() {
		this.canvasGameState.SetActive (true);
		yield return new WaitForSeconds (2);
		this.canvasGameState.SetActive (false);
		foreach (var character in this.characters) {
			HexCell cell = this.grid.getCellByCubeCoordinates (character.coordinates.X, character.coordinates.Y);
			cell.enableCell ();
		}
	}

	Character createCharacter(int cubeX, int cubeY, string name) {
		Character character = Instantiate (characterPrefab, grid.transform, false);
		character.Initialize (cubeX, cubeY, this.grid, name);
		character.mainCanvas = this.canvasCharacterInfo;
		character.textName = this.textName;
		character.textHP = this.textHP;
		this.characters.Add (character);
		return character;
	}

}
