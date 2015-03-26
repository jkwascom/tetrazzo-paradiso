using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridDevil : MonoBehaviour {
  public int grid_width = 10;
  public int grid_height = 20;
  public int grid_height_fudge = 1;
  public Row[] grid;
  public GameObject emptyGridPrefab;
  public Vector3 gridBaseLowerLeft;
  public LogDevil logger;

  public IEnumerator initializeGrid () {
    logger.log(1f,"initializeGrid");
    grid = createLevelGrid();
    yield return null;
  }

  private Row[] createLevelGrid () {
    logger.log(1f,"createLevelGrid");
    int actual_grid_height = grid_height;
    Row[] newGrid = new Row[actual_grid_height];
    Vector3 rowBase = gridBaseLowerLeft;
    for(int r = 0; r < actual_grid_height; r = r + 1) {
      rowBase.y = r;
      Row newGridRow = new Row(this, rowBase);
      newGrid[r] = newGridRow;
    }
    return newGrid;
  }
  private delegate void RowCleaner();
  public void clearGrid() {
    for(int r = 0; r < grid_height; r = r + 1) {
        grid[r].clear();
    }
  }
  public IEnumerator cleanupRows(LevelDevil level, ThingToDo nextThing) {
    logger.log(1f,"cleanupRow");
    int completeRows = 0;
    nextThing = level.blocker.startBlock;

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
        logger.log(3f, "swap rows: {0} and {1}", r, targetRow);
        swapRows(r, targetRow);
        break;
      }
    }

    level.processClearedLines(completeRows);
    level.nextThing = nextThing;

    yield return null;
  }

  private void swapRows(int x, int y) {
    Row.swap(grid[x], grid[y]);
    Row r = grid[x];
    grid[x] = grid[y];
    grid[y] = r;
  }


  public bool checkRowCompletion(int rowIndex) {
    logger.log(0f,"checkRowCompletion, row: {0} filled: {1} width: {2}", rowIndex, grid[rowIndex].filledCells, grid_width);
    return (grid[rowIndex].filledCells == grid_width);
  }

  public bool isValidCell(Vector3 point) {
    int r = Mathf.FloorToInt(point.y - gridBaseLowerLeft.y);
    int c = (int)(point.x - gridBaseLowerLeft.x);
    if(r < 0 || r >= grid_height) return false;
    if(c < 0 || c >= grid_width) return false;
    return true;
  }

  public bool isValidCell(int r, int c) {
    logger.log(0f,"isValidCel: " + r + ", " + c);
    if(r < 0 || r >= grid_height) return false;
    if(c < 0 || c >= grid_width) return false;
    return true;
  }

  public bool fixBlock(GameObject fixee) {
    logger.log(1f,"fixBloc");
    Vector3 checkPoint = fixee.transform.position;
    checkPoint.y = checkPoint.y + 1;
    int row = Mathf.FloorToInt(checkPoint.y - gridBaseLowerLeft.y);
    int column = (int)(checkPoint.x - gridBaseLowerLeft.x);

    //ugly fudging until i fix fast dropping
    while(!(isEmptyCell(checkPoint))) {
      checkPoint.y = checkPoint.y + 1;

      if(!isValidCell(checkPoint)) {
        Transform t = fixee.transform;
        while(t != null) {
          fixee = t.gameObject;
          t = t.parent;
          Object.Destroy(fixee);
        }
        return false;
      }
    }

    logger.log(0f,"dropGridCell: {0}, {1}", row, column);
    grid[row].placeObject(fixee, column);
    return true;
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
    logger.log(0f,"isEmptyCell: " + point );
    int r = Mathf.FloorToInt(point.y - gridBaseLowerLeft.y);
    int c = (int)(point.x - gridBaseLowerLeft.x);

    if(!isValidCell(point)) return false;
    return grid[r].isEmptyCell(c);
  }

  public bool checkRange(Vector3 start, int range, Vector3 step) {
    logger.log(1f,"checkRang");
    if(range < 0) {
      range = range * -1;
      step = step * -1;
    }

    for(int i = 0; i < range; i++) {
      
      if(start.y >= grid_height) start.y = (float) (grid_height - 1);
      logger.log(0f,"iscr: " + start );
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
      filledCells = 0;
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
