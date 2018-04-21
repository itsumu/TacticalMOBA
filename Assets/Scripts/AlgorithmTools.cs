using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tuple<T1, T2> {
	public T1 First { get; private set; }
	public T2 Second { get; private set; }
	internal Tuple(T1 first, T2 second) {
		First = first;
		Second = second;
	}
}

public class PriorityQueue<T> {
	private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

	public int getCount() {
		return elements.Count;
	}

	public void enqueue(T item, double priority)
	{
		elements.Add(new Tuple<T, double>(item, priority));
	}

	public T dequeue() {
		int bestIndex = 0;

		for (int i = 0; i < elements.Count; i++) {
			if (elements[i].Second < elements[bestIndex].Second) {
				bestIndex = i;
			}
		}

		T bestItem = elements[bestIndex].First;
		elements.RemoveAt(bestIndex);
		return bestItem;
	}
}

public class AStarSearch {
	public Dictionary<ICoordinates, ICoordinates> cameFrom
		= new Dictionary<ICoordinates, ICoordinates>();
	public Dictionary<ICoordinates, double> costSoFar
		= new Dictionary<ICoordinates, double>();


	private double heuristicFunction(ICoordinates startPoint, ICoordinates endPoint) {
		return startPoint.getManhattanDistance (endPoint);
	}

	public AStarSearch(IWeightedGraph<ICoordinates> graph, ICoordinates start, ICoordinates goal) {
		PriorityQueue<ICoordinates> frontier = new PriorityQueue<ICoordinates> ();
		frontier.enqueue (start, 0);
		cameFrom [start] = start;
		costSoFar [start] = 0;

		while (frontier.getCount () > 0) {
			ICoordinates currentPosition = frontier.dequeue ();

			if (currentPosition.Equals (goal)) // Break loop when find goal
				break;
		
			foreach (var nextPosition in graph.getNeighbors (currentPosition)) {
				if (nextPosition == null)
					continue;
				double cost = costSoFar[currentPosition] 
					+ graph.getCost (currentPosition, nextPosition);

				if (!costSoFar.ContainsKey (nextPosition) 
					|| cost < costSoFar[nextPosition]) { // Enqueue when find new nearest path to nextPosition
					double priority = cost + heuristicFunction (currentPosition, nextPosition);
					frontier.enqueue (nextPosition, priority);
					cameFrom [nextPosition] = currentPosition;
					costSoFar [nextPosition] = cost;
				}
			}
		}
	}
}