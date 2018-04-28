using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionExecutor {
	public static int maxPhaseCount = 3;
	public ActionContainer[] containersInPhases;

	public ActionExecutor() {
		this.containersInPhases = new ActionContainer[maxPhaseCount];
		for (int i = 0; i < maxPhaseCount; i++) {
			this.containersInPhases[i] = new ActionContainer();
		}
	}

	public void registerAction(int phaseIndex, Character caster, Action action,
		HexCoordinates targetCoordinates) {
		if (phaseIndex >= maxPhaseCount)
			return;
		this.containersInPhases [phaseIndex].addActionNode (caster, action, targetCoordinates);
	}

	// Return value indicates whether the phase is legal / has actions
	public bool executeActionsByPhase(int phaseIndex) {
		if (phaseIndex < 0 || phaseIndex >= maxPhaseCount)
			return false;
		
		var actionContainer = this.containersInPhases [phaseIndex];
		if (actionContainer.isEmpty ())
			return false;
		// Execute movements
		foreach (var movementNode in actionContainer.movementList) {
			var caster = movementNode.caster;
			Movement movement = movementNode.action as Movement;
			HexCoordinates targetCoordinates = movementNode.targetCoordinates;
			movement.moveToCoordinates (caster, caster.grid, targetCoordinates.X, targetCoordinates.Y);
		}

		// Execute interactive skills
		foreach (var interactiveSkillNode in actionContainer.interactiveSkillList) {
			var caster = interactiveSkillNode.caster;
			InteractiveSkill skill = interactiveSkillNode.action as InteractiveSkill;
			HexCoordinates targetCoordinates = interactiveSkillNode.targetCoordinates;
			skill.settleDamageEffect (caster, caster.grid, targetCoordinates);
		}
		actionContainer.clearContainer (); // Release registration
		return true;
	}

	// Execute all actions in every phase in one call
	public void executeAllActions() {
		for (int i = 0; i < maxPhaseCount; i++) {
			if (!executeActionsByPhase (i))
				return;
		}
	}
}

public class ActionContainer {
	public List<ActionNode> movementList;
	public List<ActionNode> interactiveSkillList;

	public ActionContainer() {
		// todo: add state skill list
		this.movementList = new List<ActionNode> ();
		this.interactiveSkillList = new List<ActionNode> ();
	}

	public void clearContainer() {
		this.movementList.Clear ();
		this.interactiveSkillList.Clear ();
	}

	// Whether all lists are empty
	public bool isEmpty() {
		return (this.movementList.Count == 0 && this.interactiveSkillList.Count == 0);
	}

	public void addActionNode(Character caster, Action action, 
		HexCoordinates targetCoordinates) {
		// todo: if action is a state skill

		if (action is Movement) {
			this.movementList.Add (new ActionNode(caster, action, targetCoordinates));
		}

		if (action is InteractiveSkill) {
			this.interactiveSkillList.Add (new ActionNode(caster, action, targetCoordinates));
		}
	}
}
	
public struct ActionNode {
	public Character caster;
	public Action action;
	public HexCoordinates targetCoordinates;

	public ActionNode(Character caster, Action action, HexCoordinates targetCoordinates) {
		this.caster = caster;
		this.action = action;
		this.targetCoordinates = targetCoordinates;
	}
}