using UnityEngine;
using System.Collections;

public class LogDevil : MonoBehaviour {
  public float logLevel;
  public void error(float level, string s) {
    if(level > logLevel) {
      Debug.LogError(s);
    }
  }
  public void log(float level, string s) {
    if(level > logLevel) {
      Debug.Log(s);
    }
  }
}
