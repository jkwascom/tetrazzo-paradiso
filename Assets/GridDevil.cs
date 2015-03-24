using UnityEngine;
using System.Collections;

public class GridDevil : MonoBehaviour {
	public int grid_width = 10;
	public int grid_height = 20;
  public GameObject[][] grid;
  public GameObject emptyGridPrefab;
  public Vector3 gridBaseLowerLeft;
  public BlockDevil blocker;
	// Use this for initialization
	void Start () {
    grid = createGrid();
	  StartCoroutine(blocker.spawnBorder());
	}
	
  private GameObject[][] createGrid () {
    var newGrid = new GameObject[grid_width][];
    for(int r = 0; r < grid_width; r = r + 1) {
      var newGridRow = new GameObject[grid_height];
      for(int h = 0; h < grid_height; h = h + 1) {
        newGridRow[h] = createGridCell(gridBaseLowerLeft + new Vector3(r, h, 0f));
      }
      newGrid[r] = newGridRow;
    }
    return newGrid;
  }

  private GameObject createGridCell(Vector3 cellPosition) {
    GameObject o = Object.Instantiate(emptyGridPrefab, cellPosition, transform.localRotation) as GameObject;
    o.transform.parent = this.transform;
    return o;
  }
  private delegate void RowCleaner();
  public IEnumerator cleanupRows() {
    bool rowIncomplete;
    
    for(int r = 0; r < grid_width; r = r + 1) {
      rowIncomplete = false;
      for(int h = 0; h < grid_height; h = h + 1) {
        if(!grid[r][h].name.Equals("blockcube(Clone)")) rowIncomplete = true;
      }
    }
    yield return null;
  }

  public void fixBlock(GameObject fixee) {
    //Debug.Log("fixing object [" + fixee + "]");
    //Debug.Log("initial object position [" + fixee.transform.position + "], local [" + fixee.transform.localPosition + "]");
    replaceGridObject(fixee);
    //Debug.Log("interstitial object position [" + fixee.transform.position + "], local [" + fixee.transform.localPosition + "]");
    fixee.transform.position = new Vector3(Mathf.FloorToInt(fixee.transform.position.x), Mathf.FloorToInt(fixee.transform.position.y+1), 0f);
    //Debug.Log("new object position [" + fixee.transform.position + "], local [" + fixee.transform.localPosition + "]");
  }

  public void replaceGridObject(GameObject g) {
    int gRow = Mathf.FloorToInt(g.transform.position.y - gridBaseLowerLeft.y) + 1;
    int gColumn = (int)(g.transform.position.x - gridBaseLowerLeft.x);
	
    //Debug.Log("fixing object [" + g + "] at location [" + gColumn + "," + gRow + "]");
    //Debug.Log("fixee [" + g + "], oldObject [" + grid[gColumn][gRow]+ "]");
    g.transform.parent = grid[gColumn][gRow].transform;
    grid[gColumn][gRow] = g;
	}

  public GameObject mapPointToGridObject(Vector3 point) {
    int gRow = Mathf.FloorToInt(point.y - gridBaseLowerLeft.y);
    int gColumn = (int)(point.x - gridBaseLowerLeft.x);
	
    //Debug.Log("mapped point [" + point + "] to location [" + gColumn + "," + gRow + "]");
    gRow = Mathf.Clamp(gRow, -1, grid_height - 1);
//    Debug.Log("clamped object [" + g + "] to location [" + gColumn + "," + gRow + "]");
    if (
        (gRow > grid_height - 1) || (gColumn > grid_width - 1)
        || (gRow < 0) || (gColumn < 0)) {
      return null;
    }
    return grid[gColumn][gRow];
	}

  public GameObject mapGameObjectToGrid(GameObject g) {
    int gRow = Mathf.FloorToInt(g.transform.position.y - gridBaseLowerLeft.y);
    int gColumn = (int)(g.transform.position.x - gridBaseLowerLeft.x);
	
  //  Debug.Log("mapped object [" + g + "] to location [" + gColumn + "," + gRow + "]");
    gRow = Mathf.Clamp(gRow, -1, grid_height - 1);
//    Debug.Log("clamped object [" + g + "] to location [" + gColumn + "," + gRow + "]");
    if (
        (gRow > grid_height - 1) || (gColumn > grid_width - 1)
        || (gRow < 0) || (gColumn < 0)) {
      return null;
    }
    return grid[gColumn][gRow];
	}


  public bool checkRange(Vector3 start, int range, Vector3 step) {
    if(range < 0) {
      range = range * -1;
      step = step * -1;
    }

    for(int i = 0; i < range; i++) {
      //Debug.Log("checking range: start[" + start + "], range [" + range + "], step[" + step + "]");
      GameObject gridObject = mapPointToGridObject(start);
      if ((gridObject == null) || !gridObject.name.Equals("gridcube(Clone)")) {
        //Debug.Log("invalid range: start[" + start + "], range [" + range + "], step[" + step + "]");
        return false;
      }
      start = start + step;
    }

//    Debug.Log("valid range start[" + start + "], range [" + range + "], step[" + step + "]");
    return true;
	}

  public bool canFall(GameObject faller) {
    GameObject gridObject = mapGameObjectToGrid(faller);
//    Debug.Log("faller [" + faller + "], gridObject [" + gridObject + "]");
    return (gridObject == null) ? false : gridObject.name.Equals("gridcube(Clone)");
	}
}
