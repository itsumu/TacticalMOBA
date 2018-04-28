using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCharacter : Character {
	private int actionTakenCount;
	public event EventHandler CharacterSelected;

	public void Initialize(int cubeX, int cubeY, HexGrid grid, string name, Character parentCharacter,
		GameObject mainCanvas, Text textName, Text textHP, ActionExecutor actionExecutor) {
		Initialize (cubeX, cubeY, grid, name, mainCanvas, textName, textHP, actionExecutor);
		this.originalCharacter = parentCharacter;
		this.transform.position = parentCharacter.transform.position;
		this.actionTakenCount = 0;
		// Bind events
		this.grid.SelectCell += OnCellSelectedAvatar; 
		this.grid.HoverOnCell += OnCellHoverOn;
		this.grid.HoverOffCell += OnCellHoverOff;
	}
		
	// Receive action but not yet confirmed
	public void receiveAction(string actionName) {
		this.currentAction = this.availableActions [actionName];
		this.currentAction.showActionRange (this.grid);
		if (currentAction is Movement)
			this.isReadyToMove = true;
		if (currentAction is InteractiveSkill) 
			this.isReadyToAttack = true;
	}

	// Events 

	// Choose character or confirm actions
	void OnCellSelectedAvatar(object sender, HexGrid.CellEventArgs e) {
		if (!this.moveable)
			return;
		HexCell targetCell = e.cell;
		if (targetCell.coordinates.Equals (this.coordinates)) { // The character is right on the cell
			if (targetCell.state == HexCell.CellState.StateSelected) { // Cell is selected, change to state of being selected
				this.isSelected = true;
				CharacterSelected (this, EventArgs.Empty);
				// Disable other cells
				this.grid.disableCellsWithOwner ();
				targetCell.enableCell ();
			} else { // Cell is unselected, cancel state of being selected
				this.isSelected = false;
				this.isReadyToMove = false;
				this.isReadyToAttack = false;
				this.grid.hideRangeFromSelectedCell (this.movingRangeWidth); // Unshow ranges
				this.grid.enableCellsWithMoveableOwner ();
				targetCell.enableCell ();
			}
		} else if (this.isSelected){ // Character not on this cell but is selected (Confirm action of move or attack)
			this.originalCharacter.actionPoint -= this.currentAction.actionPoint;
			if (this.isReadyToMove) { // Character is ready to move here
				Movement actionMove = this.currentAction as Movement;
				this.actionExecutor.registerAction (this.actionTakenCount++, this.originalCharacter, 
					actionMove, targetCell.coordinates); // Register action in action calculator
				actionMove.moveToCoordinates (this, this.grid, 
					targetCell.coordinates.X, targetCell.coordinates.Y);
				targetCell.enableCell ();
				this.grid.hideRangeFromSelectedCell (actionMove.actionDistance); // Unshow ranges
				this.isReadyToMove = false;
				this.isSelected = false;
			} 
			if (this.isReadyToAttack) { // Character is ready to attack here
				InteractiveSkill actionSkill = this.currentAction as InteractiveSkill;
				this.actionExecutor.registerAction (this.actionTakenCount++, this, actionSkill,
					targetCell.coordinates);
				this.grid.recoverRangeState (actionSkill.effectWidth, targetCell.coordinates);
				this.grid.hideRangeFromSelectedCell (actionSkill.actionDistance); // Unshow ranges
				this.isReadyToAttack = false;
				this.isSelected = false;
				this.getCurrentCell ().changeState (HexCell.CellState.StateDefault);
			}
			targetCell.changeState (HexCell.CellState.StateDefault);

			if (this.originalCharacter.actionPoint <= 0) { // Character no longer able to move
				this.moveable = false;
				this.originalCharacter.moveable = false;
				this.isSelected = false;
				getCurrentCell ().disableCell ();
				this.actionTakenCount = 0;
				// Codes below should be modified or deleted if game is online
				this.grid.enableCellsWithMoveableOwner ();
			}
		}
	}

	// Show path or affect area
	void OnCellHoverOn(object sender, HexGrid.CellEventArgs e) {
		if (this.isReadyToMove) {
			this.grid.drawPath (this.grid.cellSelected.coordinates, e.cell.coordinates,
				HexCell.CellState.StatePath);
		}
		if (this.isReadyToAttack) {
			this.grid.switchRangeState ((this.currentAction as InteractiveSkill).effectWidth,
				HexCell.CellState.StateEffectRange, e.cell.coordinates);
		}
	}

	// Unshow path or affect area
	void OnCellHoverOff(object sender, HexGrid.CellEventArgs e) {
		if (this.isReadyToMove) {
			this.grid.drawPath (this.grid.cellSelected.coordinates, e.cell.coordinates,
				HexCell.CellState.StateMoveRange);
		}
		if (this.isReadyToAttack) {
			this.grid.recoverRangeState ((this.currentAction as InteractiveSkill).effectWidth,
				e.cell.coordinates);
		}
	}
		
}
