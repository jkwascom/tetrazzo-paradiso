using UnityEngine;
using System.Collections;

public class LogDevil : MonoBehaviour {
  public float logLevel;
  public void error(float level, string s, params object[] formatArgs) {
    if(level > logLevel) {
      Debug.LogError(System.String.Format(s, formatArgs));
    }
  }
  public void log(float level, string s, params object[] formatArgs) {
    if(level > logLevel) {
      Debug.Log(System.String.Format(s, formatArgs));
    }
  }
}
