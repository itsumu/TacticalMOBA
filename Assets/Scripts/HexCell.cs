using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {
	private SpriteRenderer spriteRenderer;
	private bool disabled = true; // Whether disabled is determined by game state

	public enum CellState {
		StateSelected, // Different from pressed
		StateMoveRange,
		StatePath,
		StateDefault
	}
	public CellState state;
	public Sprite hexagonCyan;
	public Sprite hexagonWhite;
	public Sprite hexagonRed;
	public Sprite hexagonGrey;
	public Sprite hexagonGreen;
	public HexCoordinates coordinates; // Cube coordinates
	public event EventHandler SelectCell;
	public event EventHandler HoverOnCell;
	public event EventHandler HoverOffCell;
	public double weight;
	public Character owner;

	public void Initiate() {
		// Set up states
		this.state = CellState.StateDefault;
		this.weight = 1;
		this.owner = null;
	}

	// Use this for initialization
	void Start () {
		this.spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public void changeState(CellState state) {
		switch (state) {
		case CellState.StateSelected:
			this.spriteRenderer.sprite = this.hexagonCyan;
			break;
		case CellState.StateMoveRange:
			this.spriteRenderer.sprite = this.hexagonGreen;
			this.enableCell ();
			break;
		case CellState.StatePath:
			this.spriteRenderer.sprite = this.hexagonCyan;
			break;
		default:
			this.spriteRenderer.sprite = this.hexagonWhite;
			break;
		}
		this.state = state;
	}

	public void enableCell() {
		this.disabled = false;
	}

	public void disableCell() {
		this.disabled = true;
	}

	public bool getDisabled() {
		return this.disabled;
	}

	void OnMouseOver() {
		if (this.disabled) {
			this.spriteRenderer.sprite = this.hexagonRed;
		} else {
			this.spriteRenderer.sprite = this.hexagonGrey;
		}
		if (HoverOnCell != null)
			HoverOnCell (this, EventArgs.Empty);
	}

	void OnMouseExit() {
		this.changeState (this.state); // Return to original sprite
		if (HoverOffCell != null)
			HoverOffCell (this, EventArgs.Empty);
	}

	void OnMouseDown() {
		if (this.disabled) // No way to be pressed
			return;
		if (this.state == CellState.StateSelected) {
			this.changeState (CellState.StateDefault);
		} else if (this.state == CellState.StateDefault){
			this.changeState (CellState.StateSelected);
		}
		if (SelectCell != null) // Notify other class the cell is selected
			SelectCell (this, EventArgs.Empty);
	}
}
