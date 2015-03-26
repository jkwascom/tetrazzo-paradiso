using UnityEngine;
using System.Collections;

public class LevelDevil : MonoBehaviour {
  public BlockDevil blocker;
  public GridDevil gridder;

	void Start () {
	  StartCoroutine(startLevel());
	}

  public IEnumerator startLevel() {
    yield return StartCoroutine(gridder.initializeGrid());
    yield return StartCoroutine(blocker.spawnBorder(gridder));
    StartCoroutine(runLevel());
  }
	
  public IEnumerator runLevel() {
    while(true) {
      yield return StartCoroutine(blocker.startBlock());
      yield return StartCoroutine(gridder.cleanupRows());
      yield return new WaitForSeconds(.1f);
    }
  }
}
