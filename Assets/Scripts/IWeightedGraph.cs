using System.Collections;
using System.Collections.Generic;

public interface IWeightedGraph<Location> {
	double getCost(Location startPoint, Location endPoint);
	IEnumerable<Location> getNeighbors (Location location);
}

public interface ICoordinates {
	double getManhattanDistance (ICoordinates endPoint);
}
	