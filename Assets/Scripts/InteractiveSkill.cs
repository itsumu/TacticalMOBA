using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The effect area is assumed to be a circle (can be modified)
public class InteractiveSkill : Action {
	public int effectWidth;
	public int damage;

	public InteractiveSkill() {
		this.actionPoint = 3;
		this.effectWidth = 1;
		this.damage = 2;
	}

	public void showEffectArea(HexGrid grid, HexCoordinates centerCoordinates) {
		var cells = grid.getCircleRange (centerCoordinates, effectWidth);
		foreach (var cell in cells) {
			cell.changeState (HexCell.CellState.StateEffectRange);
		}
	}

	public void settleDamageEffect(Character caster, HexGrid grid, HexCoordinates centerCoordinates) {
		var cells = grid.getCircleRange (centerCoordinates, effectWidth);
		foreach (var cell in cells) {
			if (cell.owner != null && !cell.owner.Equals (caster) && 
				!cell.owner.Equals (caster.originalCharacter)) {
				cell.owner.originalCharacter.healthPoint -= this.damage;
			}
		}
	}
}
