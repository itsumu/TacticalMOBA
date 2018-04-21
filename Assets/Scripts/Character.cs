using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour {
	public HexCoordinates coordinates;
	public HexGrid grid;
	public int movingRangeWidth;
	public bool isReadyToMove;
	public int healthPoint;
	public int attackDamage;
	public double attackWidth;
	public string characterName;
	public bool moveable;
	public GameObject mainCanvas;
	public Text textName;
	public Text textHP;

	public void Initialize(int cubeX, int cubeY, HexGrid grid, string name) {
		this.coordinates = new HexCoordinates (cubeX, cubeY);
		this.grid = grid;
		this.grid.SelectCell += switchMotion; // Bind event
		ownTheCell (this.grid.getCellByCubeCoordinates (cubeX, cubeY));
		this.moveToCoordinate (this.coordinates.X, this.coordinates.Y);
		this.movingRangeWidth = 2; // Default moving range
		this.isReadyToMove = false;
		this.healthPoint = 5;
		this.attackWidth = 1;
		this.attackDamage = 1;
		this.characterName = name;
		this.moveable = true;
	}

	// Cube coordinate
	public void moveToCoordinate(int x, int y) {
		this.transform.position = this.grid.getCellByCubeCoordinates (x, y).transform.position;
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

	void switchMotion(object sender, HexGrid.SelectCellEventArgs e) {
		if (!moveable)
			return;
		if (e.cell.coordinates.Equals (this.coordinates)) { // The character is right on the cell
			if (e.cell.state == HexCell.CellState.StateSelected) { // Cell is selected, change to state of being selected
				this.isReadyToMove = true;
				this.grid.switchRangeState (this.movingRangeWidth, HexCell.CellState.StateMoveRange); // Show ranges
				displayUIState ();
			} else { // Cell is unselected, cancel state of being selected
				this.isReadyToMove = false;
				this.grid.switchRangeState (this.movingRangeWidth, HexCell.CellState.StateDefault); // Unshow ranges
				e.cell.enableCell ();
				hideUIState ();
			}
		} else if (this.isReadyToMove) { // Character not on this cell, but is ready to move here
			dropTheCell (this.grid.getCellByCubeCoordinates (this.coordinates.X, this.coordinates.Y));
			moveToCoordinate (e.cell.coordinates.X, e.cell.coordinates.Y); // Move to new coordinate
			ownTheCell (e.cell);
			this.grid.switchRangeState (this.movingRangeWidth, HexCell.CellState.StateDefault); // Unshow ranges
			hideUIState ();
			this.isReadyToMove = false;
			e.cell.changeState (HexCell.CellState.StateDefault);
			e.cell.disableCell ();
			this.moveable = false; // Stay standby until game controller alert next turn
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
}
