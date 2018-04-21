using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// This class is used only when cube coordinates is needed
public class HexCoordinates : ICoordinates {
	[SerializeField]
	private int x, y, z;

	public int X {
		get {
			return x;
		}
	}
	public int Y { 
		get {
			return y;
		}
	}
	public int Z { 
		get { 
			return -x - y;
		}
	}

	public HexCoordinates(int x, int y) {
		this.x = x;
		this.y = y;
		this.z = -x - y;
	}

	public override bool Equals (object obj) {
		HexCoordinates other = obj as HexCoordinates;
		return this.x == other.x && this.y == other.y;
	}

	public override int GetHashCode() {
		return x ^ y;
	}

	public static HexCoordinates transformOffsetToCube(int x, int y) {
		return new HexCoordinates (x - y / 2, y);
	}

	public static int transformCubeToOffsetIndex(int x, int y, int widthOfGrid) {
		x = x + y / 2;
		if (x < 0 || x >= widthOfGrid || y < 0 || y >= widthOfGrid) return -1; // Illegal coordinates
		return (y * widthOfGrid + x);
	}

	public double getManhattanDistance(ICoordinates endPoint) {
		HexCoordinates endPointHex = endPoint as HexCoordinates;

		return ((Math.Abs (endPointHex.X - this.x) + Math.Abs (endPointHex.Y - this.y) +
			Math.Abs (endPointHex.Z - this.z)) / 2);
	}
}
