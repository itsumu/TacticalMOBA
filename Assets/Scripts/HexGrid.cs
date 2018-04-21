using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour, IWeightedGraph<ICoordinates> {
	private HexCell[] cells;

	public HexCell cellSelected;
	public readonly int width = 6;
	public readonly int height = 6;
	public int moveRangeWidth = 0;
	public HexCell cellPrefab;
	public event EventHandler<SelectCellEventArgs> SelectCell;
	public static readonly HexCoordinates[] directions = new HexCoordinates[] { // Anticlockwise from left bottom
		new HexCoordinates(0, -1),
		new HexCoordinates(1, -1),
		new HexCoordinates(1, 0),
		new HexCoordinates(0, 1),
		new HexCoordinates(-1, 1),
		new HexCoordinates(-1, 0),
	};

	void Awake() {
		cells = new HexCell[height * width];
		this.transform.position = new Vector3 (- (width - 1) * HexMetrics.innerRadius, 
			- (3 * height / 4) * HexMetrics.outerRadius, this.transform.position.z); // Adjust position to make the grid look better
		for (int y = 0, i = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				createCell (x, y, i++);
			}
		}
	}

	public double getCost(ICoordinates startPoint, ICoordinates endPoint) {
		HexCoordinates endPointHex = endPoint as HexCoordinates;

		return (getCellByCubeCoordinates (endPointHex.X, endPointHex.Y).weight);
	}

	public IEnumerable<ICoordinates> getNeighbors (ICoordinates location) {
		HexCoordinates node = location as HexCoordinates;

		foreach (var direction in directions) {
			HexCoordinates neighbor = new HexCoordinates (node.X + direction.X, 
				                          node.Y + direction.Y);
			if (isLegalCubeCoordinates (neighbor) && !getCellByCubeCoordinates (neighbor.X, neighbor.Y).getDisabled ()) {
				yield return neighbor;
			} 
		}
	}
		
	// Create a basic cell
	void createCell(int x, int y, int index) {
		// Set position from offset coordinate
		Vector3 position;
		position.x = (x + y * 0.5f - y / 2) * (HexMetrics.innerRadius * 2f);
		position.y = y * (HexMetrics.outerRadius * 1.5f);
		position.z = 0;

		// Instantiate cells on scene
		HexCell cell = cells [index] = Instantiate (cellPrefab, this.transform, false);
		cell.Initiate ();
		cell.transform.localPosition = position;

		// Calculate cube coordinate from offset coordinate;
		cell.coordinates = HexCoordinates.transformOffsetToCube (x, y);
		
		// Bind events
		cell.SelectCell += tellPlayerCellSelect;
		cell.HoverOnCell += OnHoverOnCell;
		cell.HoverOffCell += OnHoverOffCell;
	}

	public HexCell[] getCells() {
		return this.cells;
	}

	private bool isLegalCubeCoordinates(HexCoordinates coordinates) {
		int offsetIndex = HexCoordinates.transformCubeToOffsetIndex (coordinates.X, coordinates.Y, width);
		return ((offsetIndex < width * height) && (offsetIndex >= 0));
	}

	public HexCell getCellByCubeCoordinates(int x, int y) {
		int offsetIndex = HexCoordinates.transformCubeToOffsetIndex (x, y, width);
		if (offsetIndex >= width * height || offsetIndex < 0) // Illegal index
			return null;
		return this.cells [offsetIndex];
	}

	// Show range of moveable cells and tell player to move or not
	void tellPlayerCellSelect(object sender, EventArgs e) {
		HexCell cell = sender as HexCell;
		if (cell.state == HexCell.CellState.StateSelected) {
			this.cellSelected = cell; // Record selected cell
		}
		if (SelectCell != null) {
			SelectCellEventArgs eventArgs = new SelectCellEventArgs (cell);
			SelectCell (this, eventArgs);
		}
		if (cell.state != HexCell.CellState.StateSelected) { // No longer selected, set it to null
			this.cellSelected = null;
		}
	}

	void OnHoverOnCell(object sender, EventArgs e) {
		HexCell cell = sender as HexCell;

		if (cellSelected != null && !cell.getDisabled ()
			&& !cellSelected.coordinates.Equals (cell.coordinates)) { // Hover for path showing
			drawPath (cellSelected.coordinates, cell.coordinates, HexCell.CellState.StatePath);
		}
	}

	void OnHoverOffCell(object sender, EventArgs e) {
		HexCell cell = sender as HexCell;

		if (cellSelected != null && !cell.getDisabled ()
			&& !cellSelected.coordinates.Equals (cell.coordinates)) { // Unhover for path unshowing
			drawPath (cellSelected.coordinates, cell.coordinates, HexCell.CellState.StateMoveRange);
		}
	}

	void drawPath(HexCoordinates start, HexCoordinates goal, HexCell.CellState cellState) {
		AStarSearch pathSearcher = new AStarSearch (this, start, goal);
		HexCoordinates node = pathSearcher.cameFrom [goal] as HexCoordinates;

		do {
			HexCell cell = getCellByCubeCoordinates (node.X, node.Y);
			cell.changeState (cellState);
			node = pathSearcher.cameFrom[node] as HexCoordinates;
		} while (!node.Equals (start));
		getCellByCubeCoordinates (start.X, start.Y).changeState (cellState);
	}

	public void switchRangeState(int rangeWidth, HexCell.CellState cellState) {
		// Set up range of moving state
		HexCoordinates coordinates = this.cellSelected.coordinates;
		for (int x = coordinates.X - rangeWidth; x <= coordinates.X + rangeWidth; x++) {
			for (int y = coordinates.Y - rangeWidth; y <= coordinates.Y + rangeWidth; y++) {
				for (int z = coordinates.Z - rangeWidth; z <= coordinates.Z + rangeWidth; z++) {
					if (x + y + z == 0) {
						HexCell cell = getCellByCubeCoordinates (x, y);
						if (cell != null) {
							if (cell == this.cellSelected && cellState == HexCell.CellState.StateMoveRange) {
								continue; // Selected cell shouldn't be set to move range state
							}
							if (cell.owner == null) { // Check cell's owner
								cell.changeState (cellState);
								if (cellState == HexCell.CellState.StateDefault) { // Hide ranges, disable them
									cell.disableCell ();
								}
							}
						}
					}
				}
			}
		}

		// todo: Set up range of attacking
	}

	public class SelectCellEventArgs: EventArgs {
		public HexCell cell;
		public SelectCellEventArgs(HexCell cell) {
			this.cell = cell;
		}
	}
}
