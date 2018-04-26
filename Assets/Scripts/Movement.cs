using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : Action {
	public void moveToCoordinates(Character character, HexGrid grid, int x, int y) {
		character.transform.position = grid.getCellByCubeCoordinates (x, y).transform.position;
		character.coordinates = new HexCoordinates (x, y);
	}
}
