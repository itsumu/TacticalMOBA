using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour, IWeightedGraph<ICoordinates> {
	private HexCell[] cells;
	public HexCoordinates.MapType mapType;
	public Vector3 transformOffset;
	public HexCell cellSelected;
	public readonly int width = 9;
	public readonly int height = 9;
	public int moveRangeWidth = 0;
	public HexCell cellPrefab;
	public event EventHandler<CellEventArgs> SelectCell;
	public event EventHandler<CellEventArgs> HoverOnCell;
	public event EventHandler<CellEventArgs> HoverOffCell;
	public static readonly HexCoordinates[] directions = new HexCoordinates[] { // Anticlockwise from left bottom
		new HexCoordinates(0, -1),
		new HexCoordinates(1, -1),
		new HexCoordinates(1, 0),
		new HexCoordinates(0, 1),
		new HexCoordinates(-1, 1),
		new HexCoordinates(-1, 0),
	};

	void Awake() {
		this.transformOffset = new Vector3 (-(width + 3) * HexMetrics.innerRadius, 
			-(3 * height / 4) * HexMetrics.outerRadius, this.transform.position.z);
		this.mapType = HexCoordinates.MapType.Hexagon;
		cells = new HexCell[height * width];
		this.transform.position = this.transformOffset; // Adjust position to make the grid look better
		for (int y = 0, i = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				createCell (x, y, i++);
			}
		}
	}

	// Create a basic cell
	void createCell(int x, int y, int index) {
		// Set position from offset coordinate
		Vector3 position = getPositionByCoordinates (x, y);

		// Instantiate cells on scene
		HexCell cell = cells [index] = Instantiate (cellPrefab, this.transform, false);
		cell.Initiate ();
		cell.transform.localPosition = position;

		// Calculate cube coordinate from offset coordinate;
		cell.coordinates = HexCoordinates.transformOffsetToCube (x, y, this.mapType);

		if (!HexCoordinates.isValid (cell.coordinates.X, cell.coordinates.Y, width, this.mapType)) { // Hide redundants
			return;
		}
		cell.gameObject.SetActive (true);

		// Bind events
		cell.SelectCell += OnSelectCell;
		cell.HoverOnCell += OnHoverOnCell;
		cell.HoverOffCell += OnHoverOffCell;
	}

	// This coordinates can refer to either offset coordinates or hex coordinates in hexagon map
	public Vector3 getPositionByCoordinates(int x, int y) {
		Vector3 position;
		position.x = (x + y * 0.5f) * (HexMetrics.innerRadius * 2f);
		position.y = y * (HexMetrics.outerRadius * 1.5f);
		position.z = 0;
		return position;
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
		
	public HexCell[] getCells() {
		return this.cells;
	}

	private bool isLegalCubeCoordinates(HexCoordinates coordinates) {
		int offsetIndex = HexCoordinates.transformCubeToOffsetIndex (coordinates.X, coordinates.Y, width,
			this.mapType);
		return ((offsetIndex < width * height) && (offsetIndex >= 0));
	}

	public HexCell getCellByCubeCoordinates(int x, int y) {
		int offsetIndex = HexCoordinates.transformCubeToOffsetIndex (x, y, width,
			this.mapType);
		if (offsetIndex >= width * height || offsetIndex < 0) // Illegal index
			return null;
		return this.cells [offsetIndex];
	}

	public void drawPath(HexCoordinates start, HexCoordinates goal, HexCell.CellState cellState) {
		AStarSearch pathSearcher = new AStarSearch (this, start, goal);
		HexCoordinates node = pathSearcher.cameFrom [goal] as HexCoordinates;

		do {
			HexCell cell = getCellByCubeCoordinates (node.X, node.Y);
			cell.changeState (cellState);
			node = pathSearcher.cameFrom[node] as HexCoordinates;
		} while (!node.Equals (start));
		getCellByCubeCoordinates (start.X, start.Y).changeState (cellState);
	}

	public void showRangeFromSelectedCell(int rangeWidth) {
		switchRangeState (rangeWidth, HexCell.CellState.StateMoveRange, this.cellSelected.coordinates);
	}

	public void hideRangeFromSelectedCell(int rangeWidth) {
		switchRangeState (rangeWidth, HexCell.CellState.StateDefault, this.cellSelected.coordinates);
	}
		
	public List<HexCell> getCircleRange(HexCoordinates centerCoordinates, int rangeWidth) {
		List<HexCell> cells = new List<HexCell> ();
		for (int x = centerCoordinates.X - rangeWidth; x <= centerCoordinates.X + rangeWidth; x++) {
			for (int y = centerCoordinates.Y - rangeWidth; y <= centerCoordinates.Y + rangeWidth; y++) {
				for (int z = centerCoordinates.Z - rangeWidth; z <= centerCoordinates.Z + rangeWidth; z++) {
					if (x + y + z == 0) {
						HexCell cell = getCellByCubeCoordinates (x, y);
						cells.Add (cell);
					}
				}
			}
		}
		return cells;
	}

	public void switchRangeState(int rangeWidth, HexCell.CellState cellState, 
		HexCoordinates centerCoordinates) {
		// Set up range of moving state
		var cells = getCircleRange (centerCoordinates, rangeWidth);
		foreach (var cell in cells) {
			if (cell != null) {
				if (cell == this.cellSelected && cellState == HexCell.CellState.StateMoveRange) {
					continue; // Selected cell shouldn't be set to move range state
				}
				cell.changeState (cellState);
				if (cellState == HexCell.CellState.StateDefault && 
					cell.owner == null) { // Hide ranges, disable them
					cell.disableCell ();
				}
			}
		}

		// todo: Set up range of attacking
	}

	public void recoverRangeState(int rangeWidth, HexCoordinates centerCoordinates) {
		var cells = getCircleRange (centerCoordinates, rangeWidth);
		foreach (var cell in cells) {
			if (cell != null) {
				if (cell.owner == null) { // Check cell's owner
					cell.changeState (cell.lastState);
					if (cell.state == HexCell.CellState.StateDefault) { // Hide ranges, disable them
						cell.disableCell ();
					}
				}
			}
		}
	}

	List<HexCell> findCellsWithOwner() {
		List<HexCell> result = new List<HexCell>();

		foreach (var cell in this.cells) {
			if (cell.owner != null)
				result.Add (cell);
		}
		return result;
	}

	public void disableCellsWithOwner() {
		List<HexCell> cells = findCellsWithOwner ();

		foreach (var cell in cells) {
			cell.disableCell ();
		}
	}

	public void enableCellsWithMoveableOwner() {
		List<HexCell> cells = findCellsWithOwner ();

		foreach (var cell in cells) {
			if (cell.owner.moveable) {
				cell.enableCell ();
			}
		}
	}

	// Events

	void OnSelectCell(object sender, EventArgs e) {
		HexCell cell = sender as HexCell;
		if (cell.state == HexCell.CellState.StateSelected) {
			this.cellSelected = cell; // Record selected cell
		}
		if (SelectCell != null) {
			CellEventArgs eventArgs = new CellEventArgs (cell);
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
			HoverOnCell (this, new CellEventArgs (cell));
		}
	}

	void OnHoverOffCell(object sender, EventArgs e) {
		HexCell cell = sender as HexCell;

		if (cellSelected != null && !cell.getDisabled ()
			&& !cellSelected.coordinates.Equals (cell.coordinates)) { // Unhover for path unshowing
			HoverOffCell (this, new CellEventArgs (cell));
		}
	}

	public class CellEventArgs: EventArgs {
		public HexCell cell;
		public CellEventArgs(HexCell cell) {
			this.cell = cell;
		}
	}
}
