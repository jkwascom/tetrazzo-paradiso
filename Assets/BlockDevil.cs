using UnityEngine;
using System.Collections;

public class BlockDevil : MonoBehaviour {
  public GridDevil gridder;
  public InputDevil player;
  public GameObject blockPrefab;
  public Vector3 rotationGuide = new Vector3(-1f,1f,0f);
  //public Vector3 rotationGuide = new Vector3(0f,0f,90f);
  public Vector3 nextSpawnOffset;
  public Vector3 spawnOffset = new Vector3(0f,1f,0f);
  public float regularDropRate = 0.1f;
  public float fastDropRate = 0.8f;
  public Color[] blockColors = new Color[] {Color.red, Color.red, Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.blue, Color.blue, Color.magenta};
  public Color borderColor = Color.grey;
  public int nextSpawnRow = 0;
  
  public IEnumerator spawnBorder() {
    yield return new WaitForFixedUpdate();

    nextSpawnOffset = gridder.gridBaseLowerLeft;
    nextSpawnOffset.x = nextSpawnOffset.x - 1;
    GameObject border = makeBlock(20, spawnOffset, borderColor);
    doAcrossChain((o) => {
        o.name = "border";
        o.transform.parent = this.transform;
        }, border);
    
    nextSpawnOffset.x = nextSpawnOffset.x + 1 + gridder.grid_width;
    border = makeBlock(20, spawnOffset, borderColor);
    doAcrossChain((o) => {
        o.name = "border";
        o.transform.parent = this.transform;
        }, border);
    
    nextSpawnOffset.y = nextSpawnOffset.y - 1;
    border = makeBlock(11, new Vector3(-1f,0f,0f), borderColor);
    doAcrossChain((o) => {
        o.name = "border";
        o.transform.parent = this.transform;
        }, border);
    
    nextSpawnOffset = Vector3.zero;
    nextSpawnOffset.y = gridder.grid_height;
	  StartCoroutine(spawnBlock());
    yield return null;
  }

  public IEnumerator spawnBlock() {
    yield return new WaitForFixedUpdate();
    GameObject go = makeBlock(3, spawnOffset, blockColors[nextSpawnRow]);
    nextSpawnRow = ( nextSpawnRow + 1 ) % gridder.grid_width;
    nextSpawnOffset.x = nextSpawnRow;
    StartCoroutine(lowerBlock(go));
    yield return null;
  }

  public IEnumerator lowerBlock(GameObject block) {
    GameObject top = getBlockParent(getBlockParent(getBlockParent(block)));
    float dropRate;
    bool vertical = true;
    Vector3 newPosition;
    int leftRightMove;
    while(gridder.checkRange(top.transform.position, 4, vertical ? Vector3.up : Vector3.right)) {
      dropRate = player.checkOngoing(Signals.DROP) ? (fastDropRate + regularDropRate) : regularDropRate;
      top.transform.Translate(0f,-dropRate,0f);
      if (player.checkStarting(Signals.ROTATE) 
          && (gridder.checkRange(top.transform.position, 4, !vertical ? Vector3.up : Vector3.right))) {
        //Debug.Log("trying rotate");
        doAcrossChain((o) => {
          if(!o.Equals(top)) {
            o.transform.localPosition = new Vector3(o.transform.localPosition.y, o.transform.localPosition.x, o.transform.localPosition.z);
          }
        }, block);
        vertical = !vertical;
      }
      yield return null;
      
      leftRightMove = 0;
      if (player.checkStarting(Signals.RIGHT)) ++leftRightMove;
      if (player.checkStarting(Signals.LEFT)) --leftRightMove;
      if (leftRightMove == 0) continue;
      newPosition = top.transform.position;
      newPosition.x += leftRightMove;
      if (gridder.checkRange(newPosition, 4, vertical ? Vector3.up : Vector3.right)) {
        top.transform.Translate(leftRightMove,0f,0f);
      }
    }
    
    //Debug.Log("dropped all the way");
    doAcrossChain(gridder.fixBlock, block);
    yield return new WaitForSeconds(.1f);
	  StartCoroutine(spawnBlock());
  }

  public void doAcrossChain(ChainDelegate f, GameObject o) {
    GameObject last;
    while(o != null) {
      last = o;
      o = getBlockParent(o);
      f(last);
    }
  }

  public delegate void ChainDelegate(GameObject first);

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
