using UnityEngine;
using System.Collections;

public class BlockDevil : MonoBehaviour {
  public GameObject blockPrefab;
  public Vector3 rotationGuide = new Vector3(-1f,1f,0f);
  public Vector3 nextSpawnOffset;
  public Vector3 spawnOffset = new Vector3(0f,1f,0f);
  public float regularDropRate = 0.1f;
  public float fastDropRate = 0.8f;
  public float maxDropRate = 0.99f;
  public Color[] blockColors = new Color[] {Color.red, Color.red, Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.blue, Color.blue, Color.magenta};
  public Color borderColor = Color.grey;
  public int nextSpawnRow = 0;
  
  public IEnumerator spawnBorder(GridDevil gridder) {
    yield return new WaitForFixedUpdate();
    ChainDelegate f = o => {
        o.name = "border";
        o.transform.parent = transform;
        return true;
    };

    nextSpawnOffset = gridder.gridBaseLowerLeft;
    nextSpawnOffset.x = nextSpawnOffset.x - 1;
    GameObject border = makeBlock(gridder.grid_height -1, spawnOffset, borderColor);

    doAcrossChain(f, border);
    
    nextSpawnOffset.x = nextSpawnOffset.x + 1 + gridder.grid_width;
    border = makeBlock(gridder.grid_height - 1, spawnOffset, borderColor);
    doAcrossChain(f, border);
    
    nextSpawnOffset.y = nextSpawnOffset.y - 1;
    border = makeBlock(11, new Vector3(-1f,0f,0f), borderColor);
    doAcrossChain(f, border);
    
    reset(gridder.grid_height);
    yield return null;
  }

  public void reset(int startHeight) {
    nextSpawnOffset = Vector3.zero;
    nextSpawnOffset.y = startHeight;
    nextSpawnRow = 0;
  }

  public IEnumerator startBlock(LevelDevil level, ThingToDo nextThing) {
    yield return new WaitForFixedUpdate();
    GameObject block = makeBlock(3, spawnOffset, blockColors[nextSpawnRow]);
    float dropRate;
    bool vertical = true;
    Vector3 newPosition;
    int leftRightMove;
    GameObject top = getBlockParent(getBlockParent(getBlockParent(block)));
    GridDevil gridder = level.gridder;
    InputDevil player = level.player;

    nextSpawnRow = ( nextSpawnRow + 1 ) % gridder.grid_width;
    nextSpawnOffset.x = nextSpawnRow;

    while(gridder.checkRange(top.transform.position, 4, vertical ? Vector3.up : Vector3.right)) {
      dropRate = regularDropRate * level.speed;
      if (player.checkOngoing(Signals.DROP)) dropRate += fastDropRate;
      dropRate = Mathf.Clamp(dropRate, .1f, maxDropRate);

      
      top.transform.Translate(0f,-dropRate,0f);
      if (player.checkStarting(Signals.ROTATE) && (gridder.checkRange(top.transform.position, 4, !vertical ? Vector3.up : Vector3.right))) { 
        if (doAcrossChain(tryRotateChain, block)) {
          level.logger.log(2.5f, "rotate worked");
          vertical = !vertical;
        }
      }
      yield return null;
      
      leftRightMove = 0;
      if (player.checkStarting(Signals.RIGHT)) ++leftRightMove;
      if (player.checkStarting(Signals.LEFT)) --leftRightMove;
      if (leftRightMove == 0) continue;

      newPosition = top.transform.position;
      newPosition.x += leftRightMove;
      newPosition.y += 0.6f;
      if (gridder.checkRange(newPosition, 4, vertical ? Vector3.up : Vector3.right)) {
        top.transform.Translate(leftRightMove,0f,0f);
      }
    }
    
    if(doAcrossChain(gridder.fixBlock, block)) {
      nextThing = gridder.cleanupRows;
    }

    level.nextThing = nextThing;
    yield return null;
  }

  public bool tryRotateChain(GameObject o) {
    if(getBlockParent(o) == null) return true;

    o.transform.localPosition = new Vector3(o.transform.localPosition.y, o.transform.localPosition.x, o.transform.localPosition.z);
    return true;
  }  

  public bool doAcrossChain(ChainDelegate f, GameObject o) {
    GameObject last;
    while(o != null) {
      last = o;
      o = getBlockParent(o);
      if(!f(last)) return false;
    }
    return true;
  }

  public delegate bool ChainDelegate(GameObject first);

  public static GameObject getBlockParent(GameObject g) {
    if(g.transform.parent == null) return null;
    return g.transform.parent.gameObject;
  }

  public GameObject makeBlock(int subBlocks, Vector3 offsetBase, Color blockColor) {
    GameObject newblock = Object.Instantiate(blockPrefab, nextSpawnOffset + (offsetBase * subBlocks), transform.localRotation) as GameObject;
    newblock.GetComponent<Renderer>().material.color = blockColor;
    if(subBlocks > 0) {
      newblock.transform.parent = makeBlock(subBlocks-1, offsetBase, blockColor).transform;
    }
    return newblock;
  }
}
