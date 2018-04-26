using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveSkill : Action {
	public int effectWidth;
	public int damage;

	public InteractiveSkill() {
		this.actionPoint = 3;
	}
}
