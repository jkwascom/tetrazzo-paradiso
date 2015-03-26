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
  public float speedFactor = 10f;
  public int score = 0;
  public int maxScore = 0;
  public AudioSource[] clearClips;
  public int pointsPerBlock = 10;
  public Camera camera;
  public bool gameRunning = false;

	void Start () {
    titleString = "Press Space to Start";
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
      if(!gameRunning) {
        if(player.checkOngoing(Signals.START)) {
          StartCoroutine(runLevel(this, nextThing));
        } else {
          Color c = titleText.color;
          c.a = 0;
          titleText.color = c;
          yield return new WaitForSeconds(0.1f);
          titleText.color = currentColor;
          yield return new WaitForSeconds(0.1f);
        }
      }
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
    gameRunning = true;
    updateValence("TETRAZZO PARADISO", currentColor = goodColor);
    linesRemaining = lineTarget;
    nextThing = blocker.startBlock;
    gridder.clearGrid();
    blocker.reset(gridder.grid_height);
    score = 0;
    maxScore = 0;
    while(gameRunning) {
      speed = 1 + (int)((((float)(lineTarget - linesRemaining) / (float)lineTarget)) * 10f);
      logger.log(3f, "next step: {0}", nextThing.Method);
      yield return StartCoroutine(nextThing(this, loseGame));
      if(linesRemaining <= 0) nextThing = winGame;
    }
  }
	
  public IEnumerator winGame(LevelDevil l, ThingToDo ignoredThing) {
    gameRunning = false;
    if(score == maxScore) {
      updateValence("PERFECT PARADISE", currentColor = Color.white);
    }
    yield return null;
  }
	
  public IEnumerator loseGame(LevelDevil l, ThingToDo ignoredThing) {
    logger.log(0f, "lost!");
    gameRunning = false;
    updateValence("TETRAZZO INFERNO", currentColor = badColor);
    nextThing = runLevel; //this might eventually lead to a stack overflow but no one should be playing more than one game of this anyway
    yield return null;
  }
}
