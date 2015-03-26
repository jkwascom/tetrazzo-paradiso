using UnityEngine;
using System.Collections.Generic;

public enum Signals {
  DROP,
  ROTATE,
  LEFT,
  RIGHT,
  MUTE
}

public class InputDevil : MonoBehaviour {
  public Dictionary<Signals, List<KeyCode>> input;

  public void Start() {
    input = new Dictionary<Signals, List<KeyCode>>();
    input[Signals.DROP] = new List<KeyCode> { KeyCode.DownArrow, KeyCode.S};
    input[Signals.ROTATE] = new List<KeyCode> { KeyCode.UpArrow, KeyCode.W};
    input[Signals.LEFT] = new List<KeyCode> { KeyCode.LeftArrow, KeyCode.A};
    input[Signals.RIGHT] = new List<KeyCode> { KeyCode.RightArrow, KeyCode.D};
    input[Signals.MUTE] = new List<KeyCode> { KeyCode.M };
    Debug.Log("putting");
  }

	public bool checkStarting(Signals s) {
    Debug.Log("trying " + s);
    foreach (var item in input[s])
    {
      if(Input.GetKeyDown(item)) return true;
    }
    return false;
	}

	public bool checkOngoing(Signals s) {
    foreach (var item in input[s])
    {
      if(Input.GetKey(item)) return true;
    }
    return false;
	}
}
