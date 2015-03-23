using UnityEngine;
using System.Collections;

public class BlockDevil : MonoBehaviour {
  public GridDevil gridder;
  public GameObject blockPrefab;
  public Vector3 nextSpawnOffset;
  public Vector3 offsetBase = new Vector3(0f,1f,0f);
  public float dropRate = 0.1f;
  
	// Use this for initialization
	void Start () {
    nextSpawnOffset = Vector3.zero;
    nextSpawnOffset.y = gridder.grid_height;
	  StartCoroutine(spawnBlock());
	}

  public IEnumerator spawnBlock() {
    yield return new WaitForFixedUpdate();
    GameObject go = makeBlock(3);
    StartCoroutine(lowerBlock(go));
    yield return null;
  }

  public IEnumerator lowerBlock(GameObject block) {
    while(gridder.gridBaseLowerLeft.y < block.transform.position.y) {
      block.transform.Translate(0f,-dropRate,0f);
      yield return null;
    }
    Debug.Log("dropped all the way");
  }

  public GameObject makeBlock(int subBlocks) {
    GameObject newblock = Object.Instantiate(blockPrefab, nextSpawnOffset + (offsetBase * subBlocks), transform.localRotation) as GameObject;
    if(subBlocks > 0) {
      makeBlock(subBlocks-1).transform.parent = newblock.transform;
    }
    return newblock;
  }
	
	// Update is called once per frame
	void Update () {
	
	}
}
