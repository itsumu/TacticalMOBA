using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCharacter : Character {
	public event EventHandler CharacterSelected;

	public void Initialize(int cubeX, int cubeY, HexGrid grid, string name, Character parentCharacter,
		GameObject mainCanvas, Text textName, Text textHP) {
		Initialize (cubeX, cubeY, grid, name, mainCanvas, textName, textHP);
		this.originalCharacter = parentCharacter;
		this.transform.position = parentCharacter.transform.position;
		this.grid.SelectCell += OnCellSelectedAvatar; // Bind event
	}
		
	void OnCellSelectedAvatar(object sender, HexGrid.SelectCellEventArgs e) {
		if (!this.moveable)
			return;
		if (e.cell.coordinates.Equals (this.coordinates)) { // The character is right on the cell
			if (e.cell.state == HexCell.CellState.StateSelected) { // Cell is selected, change to state of being selected
				this.isSelected = true;
				CharacterSelected (this, EventArgs.Empty);
				// Disable other cells
				this.grid.disableCellsWithOwner ();
				e.cell.enableCell ();
			} else { // Cell is unselected, cancel state of being selected
				this.isSelected = false;
				this.isReadyToMove = false;
				this.isReadyToAttack = false;
				this.grid.hideRangeFromSelectedCell (this.movingRangeWidth); // Unshow ranges
				this.grid.enableCellsWithMoveableOwner ();
				e.cell.enableCell ();
			}
		} else if (isSelected){ // Character not on this cell but is selected (Confirm action of move or attack)
			this.originalCharacter.actionPoint -= this.currentAction.actionPoint;
			if (this.isReadyToMove) { // Character is ready to move here
				dropTheCell (getCurrentCell ());
				(this.availableActions ["移动"] as Movement).moveToCoordinates (this, this.grid, 
					e.cell.coordinates.X, e.cell.coordinates.Y);
				this.ownTheCell (e.cell);
				e.cell.enableCell ();
				this.grid.hideRangeFromSelectedCell (this.movingRangeWidth); // Unshow ranges
				this.isReadyToMove = false;
				e.cell.changeState (HexCell.CellState.StateDefault);
			} 
			if (this.isReadyToAttack) { // Character is ready to attack here
				// todo: ready to attack
				this.isReadyToAttack = false;
			}

			if (this.originalCharacter.actionPoint <= 0) { // Character no longer able to move
				this.moveable = false;
				this.originalCharacter.moveable = false;
				this.isSelected = false;
				getCurrentCell ().disableCell ();
				// Codes below should be modified or deleted if game is online
				this.grid.enableCellsWithMoveableOwner ();
			}
		}
	}

	public void takeAction(string actionName) {
		this.currentAction = this.availableActions [actionName];
		this.currentAction.showActionRange (this.grid);
		if (currentAction is Movement)
			this.isReadyToMove = true;
		if (currentAction is InteractiveSkill)
			this.isReadyToAttack = true;
	}
}
