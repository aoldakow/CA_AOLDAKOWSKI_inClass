using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CA_Manager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int res_x;
    public int res_y;

    public float size;

    public CA_Cell[] grid_cells;
    public int numCells;

    [Header("Visualization Settings")]
    public GameObject cell_obj;

    public Material matA;
    public Material matD;

    [Header("CA Settings")]
    [Range(0.0f,1.0f)]
    public float random_threshold;

    private bool[] states_current;
    private bool[] states_prev;

    public bool runCA;
    public float interval;
    private float timer;


    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        CreateGrid();
        VisualizeGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeAllCells();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            runCA = true;
        }

        if(runCA == true)
        {
            if(timer>=interval)
            {
                RunCA();
                timer = 0;
            }

            timer += Time.deltaTime;

        }
    }

    #region Grid

    public void CreateGrid()
    {
        numCells = res_x * res_y;
        grid_cells = new CA_Cell[numCells];
        states_current = new bool[numCells];
        states_prev = new bool[numCells];

        int counter = 0;

        for (int y = 0; y < res_y; y++)
        {
            var y_pos = y * size;
            for (int x = 0; x < res_x; x++)
            {
                var x_pos = x * size;

                CA_Cell cell = new CA_Cell();
                cell.ID = counter;
                cell.position = new Vector2(x_pos, y_pos);
                cell.normalized_coords = new Vector2(x, y);

                grid_cells[counter] = cell;
                counter++;
            }
        }

    }

    public void VisualizeGrid()
    {
        for (int i = 0; i < numCells; i++)
        {
            var cell = grid_cells[i];
            var pos = cell.position;

            //create the cell object
            GameObject new_cell = GameObject.Instantiate(cell_obj, transform);
            new_cell.transform.position = new Vector3(pos.x, pos.y, 0);
            new_cell.transform.localScale = new Vector3(size, size, size);
            new_cell.name = "cell_" + i.ToString();
            new_cell.GetComponent<MeshRenderer>().sharedMaterial = matD;
        }
    }

    #endregion

    #region CA

    public void RandomizeAllCells()
    {
        for (int i = 0; i < numCells; i++)
        {
            RandomizeCellState(i);
        }

        UpdateAllCellColors();
    }

    public void RandomizeCellState(int id)
    {
        float number = Random.Range(0.0f, 1.0f);
        if (number > random_threshold)
        {
            states_current[id] = true;
        }
        else
        {
            states_current[id] = false;
        }
    }

    public void UpdateAllCellColors()
    {
        for (int i = 0; i < numCells; i++)
        {
            UpdateSingleCellColor(i);
        }
    }

    public void UpdateSingleCellColor(int id)
    {
        if (states_current[id] == true)
        {
            transform.GetChild(id).GetComponent<MeshRenderer>().sharedMaterial = matA;
        }
        else
        {
            transform.GetChild(id).GetComponent<MeshRenderer>().sharedMaterial = matD;
        }
    }

    public void RunCA()
    {
        for(int i = 0; i < numCells; i++)
        {
            var state = states_current[i];
            var n_count = CountNeighbors(i);

            //cell is alive
            if(state==true)
            {
                if(n_count == 2 || n_count ==3)
                {
                    states_prev[i] = true;
                }
                else
                {
                    states_prev[i] = false;
                }
            }
            //cell is dead
            else
            {
                if(n_count == 3)
                {
                    states_prev[i] = true;
                }
                else
                {
                    states_prev[i] = false;
                }
            }
        }

        (states_current, states_prev) = (states_prev, states_current);
        UpdateAllCellColors();
    }

    public int CountNeighbors(int id)
    {
        var cell = grid_cells[id];
        var coords = cell.normalized_coords;
        int neighbor_counter = 0;

        for (int y = -1; y < 2; y++)
        {
            var ny = coords.y + y;
            //invalid y neighbor scenario
            if(ny<0 || ny>= res_y)
            {
                continue;
            }
            for(int x = -1; x < 2; x++)
            {
                var nx = coords.x + x;
                //invalid x neighbor scenario
                if(nx<0 || nx>=res_x)
                {
                    continue;
                }
                // not taking into account the cell itself in the neighbor count
                if(ny == 0 && nx == 0)
                {
                    continue;
                }

                //calculate neighbor index
                //y_coordinate * x_resolution + x_coordinate
                int n_id = (int)ny * res_x + (int)nx;
                if(states_current[n_id] == true)
                {
                    neighbor_counter++;
                }
            }
        }

        return neighbor_counter;
    }

    #endregion
}

