using UnityEngine;
using System.Collections;

public class GridDevil : MonoBehaviour {
	public int grid_width = 10;
	public int grid_height = 20;
  public Object[,] grid;
  public GameObject emptyGridPrefab;
  public Vector3 gridBaseLowerLeft;
	// Use this for initialization
	void Start () {
    grid = createGrid();
	}
	
  private Object[,] createGrid () {
    var newGrid = new Object[grid_width,grid_height];
    Vector3 gridCubeOffset = Vector3.zero;
    for(int r = 0; r < grid_width; r = r + 1) {
      for(int h = 0; h < grid_height; h = h + 1) {
        gridCubeOffset.x = r;
        gridCubeOffset.y = h;
        Quaternion qRot = transform.localRotation;
        Vector3 testPos = gridBaseLowerLeft + gridCubeOffset;
        Object testObj = Object.Instantiate(emptyGridPrefab, testPos, qRot);
        newGrid[r,h] = testObj;
      }
    }
    return newGrid;
  }
	// Update is called once per frame
	void Update () {
	
	}
}
