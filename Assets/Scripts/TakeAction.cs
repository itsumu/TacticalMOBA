using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeAction : MonoBehaviour {
	public event EventHandler TriggerClickButtonAction;

	public void Click() {
		TriggerClickButtonAction (this.GetComponentInParent<Button> (), EventArgs.Empty);
	}
}
