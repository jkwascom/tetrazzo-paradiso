using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridDevil : MonoBehaviour {
  public int grid_width = 10;
  public int grid_height = 20;
  public Row[] grid;
  public GameObject emptyGridPrefab;
  public Vector3 gridBaseLowerLeft;
  public AudioSource[] clearClips;
  public LogDevil logger;

  public IEnumerator initializeGrid () {
    logger.log(1f,"initializeGrid");
    grid = createLevelGrid();
    yield return null;
  }

  private Row[] createLevelGrid () {
    logger.log(1f,"createLevelGrid");
    Row[] newGrid = new Row[grid_height];
    Vector3 rowBase = gridBaseLowerLeft;
    for(int r = 0; r < grid_height; r = r + 1) {
      rowBase.y = r;
      Row newGridRow = new Row(this, rowBase);
      newGrid[r] = newGridRow;
    }
    return newGrid;
  }
  private delegate void RowCleaner();
  public IEnumerator cleanupRows() {
    logger.log(1f,"cleanupRow");
    int completeRows = 0;

    for(int r = 0; r < grid_height; r = r + 1) {
      if (checkRowCompletion(r)) {
        completeRows += 1;
        grid[r].clear();
      }

      int targetRow = r+completeRows;
      while(targetRow > r && (targetRow < grid_height) && grid[targetRow].filledCells !=0) {
        if (checkRowCompletion(targetRow)) {
          completeRows += 1;
          grid[targetRow].clear();
          targetRow = r+completeRows;
          continue;
        }
        logger.log(3f, System.String.Format("swap rows: {0} and {1}", r, targetRow));
        swapRows(r, targetRow);
        break;
      }
    }
    clearClips[completeRows].Play();
    yield return null;
  }

  private void swapRows(int x, int y) {
    Row.swap(grid[x], grid[y]);
    Row r = grid[x];
    grid[x] = grid[y];
    grid[y] = r;
  }


  public bool checkRowCompletion(int rowIndex) {
    logger.log(1f,"checkRowCompletio");
    return (grid[rowIndex].filledCells == grid_width);
  }

  public bool isValidCell(int r, int c) {
    logger.log(1f,"isValidCel: " + r + ", " + c);
    if(r < 0 || r >= grid_height) return false;
    if(c < 0 || c >= grid_width) return false;
    return true;
  }

  public void fixBlock(GameObject fixee) {
    logger.log(1f,"fixBloc");
    int row = Mathf.FloorToInt(fixee.transform.position.y - gridBaseLowerLeft.y) + 1;
    int column = (int)(fixee.transform.position.x - gridBaseLowerLeft.x);
    grid[row].placeObject(fixee, column);
  }

  public void dropGridCell(int row, int column, int dropHeight) {
    logger.log(1f,"dropGridCel");
    grid[row].clearCell(column);
    if(isValidCell(row,column+dropHeight) && !grid[row].isEmptyCell(column+dropHeight)) {
      grid[row].placeObject(grid[column+dropHeight].getCell(row), column);
      Debug.Log("should have dropped something from " + row + "," + column + "+" + dropHeight);
    } 
  }

  public bool isEmptyCell(Vector3 point) {
    logger.log(1f,"isEmptyCell: " + point );
    int r = Mathf.FloorToInt(point.y - gridBaseLowerLeft.y);
    int c = (int)(point.x - gridBaseLowerLeft.x);

    r = Mathf.Clamp(r, -1, grid_height - 1);
    if(!isValidCell(r,c)) return false;
    return grid[r].isEmptyCell(c);
  }

  public bool checkRange(Vector3 start, int range, Vector3 step) {
    logger.log(1f,"checkRang");
    if(range < 0) {
      range = range * -1;
      step = step * -1;
    }

    for(int i = 0; i < range; i++) {
      
      if (!isEmptyCell(start)) {
        return false;
      }
      start = start + step;
    }

    return true;
  }

  public GameObject createGridCell(Vector3 cellPosition) {
    GameObject o = Object.Instantiate(emptyGridPrefab, cellPosition, transform.rotation) as GameObject;
    return o;
  }

  public class Row {
    private GameObject[] cells;
    private GameObject[] contents;
    private GridDevil parent;
    private int cellCount;
    private Transform baseTransform;
    public int filledCells = 0;
    public Row(GridDevil parent, Vector3 rowBase) {
      cellCount = parent.grid_width;
      cells = new GameObject[cellCount];
      contents = new GameObject[cellCount];
      Transform nextParent = parent.transform;
      for(int h = 0; h < parent.grid_width; ++h) {
        rowBase.x = h;
        cells[h] = parent.createGridCell(rowBase);
        cells[h].transform.parent = nextParent;
        nextParent = cells[h].transform;
      }
      baseTransform = cells[0].transform;
    }

    public GameObject getCell(int column) {
      return contents[column];
    }

    public void placeObject(GameObject g, int column) {
      Vector3 newPosition = cells[column].transform.position;
      newPosition.z = 0f;
      g.transform.position = newPosition;
      g.transform.parent = cells[column].transform;
      contents[column] = g;
      filledCells++;
    }

    public void clear() {
      for(int i = 0; i < cellCount; i++) {
        clearCell(i);
      }
    }

    public static void swap(Row x, Row y) {
      Vector3 pos = x.baseTransform.position;
      x.baseTransform.position = y.baseTransform.position;
      y.baseTransform.position = pos;
    }

    public void clearCell(int column) {
      GameObject g = contents[column];
      contents[column] = null;
      Object.Destroy(g);
      filledCells--;
    }

    public bool isEmptyCell(int c) {
      return contents[c] == null;
    }

  }
}
