using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour {
	public HexCoordinates coordinates;
	public HexGrid grid;
	public AvatarCharacter avatarPrefab;
	public AvatarCharacter avatar;
	public Character originalCharacter;
	public int movingRangeWidth;
	public bool isReadyToMove;
	public bool isReadyToAttack;
	public int healthPoint;
	public int attackDamage;
	public int actionPoint;
	public double attackWidth;
	public string characterName;
	public bool moveable;
	public Dictionary<string, Action> availableActions;
	public Action currentAction;
	public GameObject mainCanvas;
	public Text textName;
	public Text textHP;
	public bool isSelected;
	public ActionExecutor actionExecutor;

	public void Initialize(int cubeX, int cubeY, HexGrid grid, string name,
		GameObject mainCanvas,Text textName,Text textHP, ActionExecutor actionExecutor) {
		this.coordinates = new HexCoordinates (cubeX, cubeY);
		this.grid = grid;
		this.grid.SelectCell += OnCellSelected; // Bind event
		ownTheCell (getCurrentCell ());
		this.moveToCoordinate (this.coordinates.X, this.coordinates.Y);
		this.movingRangeWidth = 2; // Default moving range
		this.isReadyToMove = false;
		this.isReadyToAttack = false;
		this.healthPoint = 5;
		this.actionPoint = 3;
		this.attackWidth = 1;
		this.attackDamage = 1;
		this.characterName = name;
		this.moveable = true;
		this.isSelected = false;
		this.availableActions = new Dictionary<string, Action> ();
		this.availableActions.Add ("移动", new Movement());
		this.availableActions.Add("射击", new InteractiveSkill ());
		this.actionExecutor = actionExecutor;

		this.originalCharacter = this;
		if (this.avatarPrefab != null) {
			this.avatar = Instantiate (this.avatarPrefab, this.grid.transform, false);
		}
		if (this.avatar != null) {
			this.avatar.Initialize (cubeX, cubeY, grid, name, this, mainCanvas, textName, 
				textHP, actionExecutor);
		}
	}

	// Cube coordinate
	public void moveToCoordinate(int x, int y) {
		this.transform.position = getCurrentCell ().transform.position;
		this.coordinates = new HexCoordinates (x, y);
	}

	public void ownTheCell(HexCell cell) {
		cell.owner = this;
	}

	public void dropTheCell(HexCell cell) {
		cell.owner = null;
	}

	void displayUIState() {
		this.textName.text = this.characterName;
		this.textHP.text = "HP: " + Convert.ToString (this.healthPoint);
		this.mainCanvas.SetActive(true);
	}

	void hideUIState() {
		this.mainCanvas.SetActive (false);
	}

	void OnCellSelected(object sender, HexGrid.CellEventArgs e) {
		if (e.cell.coordinates.Equals (this.coordinates) && !(this is AvatarCharacter)) { // The character is right on the cell
			if (e.cell.state == HexCell.CellState.StateSelected) { // Cell is selected, change to state of being selected
				displayUIState ();
			} else { // Cell is unselected, hide UI of state
				hideUIState ();
			}
		}
	}

	public void attack(Character enemy) {
		enemy.healthPoint -= this.attackDamage;
	}

	public bool ableToAttack(HexCoordinates enemyCoordinates) {
		if (this.coordinates.getManhattanDistance (enemyCoordinates) <= this.attackWidth)
			return true;
		return false;
	}

	public HexCell getCurrentCell() {
		return this.grid.getCellByCubeCoordinates (this.coordinates.X, this.coordinates.Y);
	}
}
