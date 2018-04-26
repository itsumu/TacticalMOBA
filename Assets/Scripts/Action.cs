using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action {
	public int actionPoint;
	public int actionDistance;

	public Action() {
		this.actionPoint = 2;
		this.actionDistance = 2;
	}

	public void showActionRange(HexGrid grid) {
		grid.showRangeFromSelectedCell (actionDistance);
	}
}
