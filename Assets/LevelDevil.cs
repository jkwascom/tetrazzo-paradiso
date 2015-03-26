using UnityEngine;
using System.Collections;

public delegate IEnumerator ThingToDo(LevelDevil l, ThingToDo t);
public class LevelDevil : MonoBehaviour {
  public BlockDevil blocker;
  public GridDevil gridder;
  public InputDevil player;
  public ThingToDo nextThing;
  public LogDevil logger;
  public UnityEngine.UI.Text speedText;
  public UnityEngine.UI.Text scoreText;
  public UnityEngine.UI.Text cScoreText;
  public UnityEngine.UI.Text audioText;
  public UnityEngine.UI.Text linesText;
  public UnityEngine.UI.Text titleText;
  public Color goodColor;
  public Color badColor;
  public Color mehColor;
  public Color currentColor;
  public string titleString;
  public bool audioMuted = true;
  public int lineTarget = 100;
  public int linesRemaining;
  public int speed = 1;
  public int score = 0;
  public int maxScore = 0;
  public AudioSource[] clearClips;
  public int pointsPerBlock = 10;
  public Camera camera;

	void Start () {
	  StartCoroutine(startLevel());
	  StartCoroutine(updateUI());
	}

  public IEnumerator updateUI() {
    while(true) {
      speedText.text = speed.ToString();
      audioText.text = audioMuted ? "Muted" : "Obnoxious";
      linesText.text = linesRemaining.ToString();
      scoreText.text = score.ToString();
      cScoreText.text = (maxScore - score).ToString();
      titleText.text = titleString;
      titleText.color = currentColor;
      camera.backgroundColor = currentColor;
      yield return null;

      if(player.checkStarting(Signals.MUTE)) audioMuted = !audioMuted;
    }
	}

  public void updateValence(string s, Color c) {
      titleString = s;
      currentColor = c;
  }

  public IEnumerator startLevel() {
    currentColor = goodColor;
    yield return StartCoroutine(gridder.initializeGrid());
    yield return StartCoroutine(blocker.spawnBorder(gridder));
    StartCoroutine(runLevel(this, nextThing));
  }
	
  public void processClearedLines(int numberCleared) {
    //for right now, scoring assumes this is called each time a block chain lands
    maxScore += pointsPerBlock * 4;
    score += pointsPerBlock * numberCleared * gridder.grid_width;
    if (maxScore - score > pointsPerBlock * 4 * gridder.grid_width) {
      updateValence("TETRAZZO PURGATORIO", currentColor = mehColor);
    } else {
      updateValence("TETRAZZO PARADISO", currentColor = goodColor);
    }
    if(!audioMuted) clearClips[numberCleared].Play();
    linesRemaining = Mathf.Clamp((linesRemaining - numberCleared), 0, lineTarget);
  }

  public IEnumerator runLevel(LevelDevil l, ThingToDo ignoredThing) {
    updateValence("TETRAZZO PARADISO", currentColor = goodColor);
    linesRemaining = lineTarget;
    nextThing = blocker.startBlock;
    gridder.clearGrid();
    while(linesRemaining > 0) {
      speed = 1 + ((lineTarget - linesRemaining) / 10);
      logger.log(3f, "next step: {0}", nextThing.Method);
      yield return StartCoroutine(nextThing(this, loseGame));
    }
  }
	
  public IEnumerator loseGame(LevelDevil l, ThingToDo ignoredThing) {
    logger.log(4f, "lost!");
    updateValence("TETRAZZO INFERNO", currentColor = badColor);
    nextThing = runLevel; //this might eventually lead to a stack overflow but no one should be playing more than one game of this anyway
    yield return new WaitForSeconds(10f);
  }
}
