using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TileStatus
{
    UNVISITED,
    OPEN,
    CLOSED,
    IMPASSABLE,
    GOAL,
    START,
    PATH
};

public enum NeighbourTile
{
    TOP_TILE,
    RIGHT_TILE,
    BOTTOM_TILE,
    LEFT_TILE,
    NUM_OF_NEIGHBOUR_TILES
};

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject tilePanelPrefab;
    [SerializeField] private GameObject panelParent;
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject shipPrefab;
    [SerializeField] private Color[] colors;
    [SerializeField] private float baseTileCost = 1f;
    [SerializeField] private bool useManhattanHeuristic = true;
    [SerializeField] private float shipRotationSpeed;
    [SerializeField] private float shipMovementSpeed;
    [SerializeField] private GameObject[] mines;
    private GameObject[,] grid;
    private int rows = 12;
    private int columns = 16;
    private GameObject ship;

    public static GridManager Instance { get; private set; } // Static object of the class.

    void Awake()
    {
        if (Instance == null) // If the object/instance doesn't exist yet.
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Initialize()
    {
        ship = GameObject.FindGameObjectWithTag("Ship");
        BuildGrid();
        ConnectGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(!child.gameObject.activeSelf);
            panelParent.gameObject.SetActive(!panelParent.gameObject.activeSelf);
            instructionPanel.SetActive(!instructionPanel.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetTileStatuses();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        //SPAWN MINES
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    Vector2 gridPosition = GetGridPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        //    GameObject mineInst = GameObject.Instantiate(minePrefab, new Vector3(gridPosition.x, gridPosition.y, 0f), Quaternion.identity);
        //    Vector2 mineIndex = mineInst.GetComponent<NavigationObject>().GetGridIndex();
        //    grid[(int)mineIndex.y, (int)mineIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.IMPASSABLE);
        //   // mines.Add(mineInst);
        //    ConnectGrid();
        //}

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 gridPosition = GetGridPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            
            if (Vector2.Distance(GameObject.FindGameObjectWithTag("Planet").transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < .7f)
                return;
            foreach (GameObject mine in mines)
            {
                if (Vector2.Distance(mine.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < .7f)
                    return;
            }
            ship = GameObject.FindGameObjectWithTag("Ship");
            Vector2 tileIndex = ship.GetComponent<NavigationObject>().GetGridIndex();
            GetGrid()[(int)tileIndex.y, (int)tileIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.UNVISITED);
            Destroy(ship);
            ship = Instantiate(shipPrefab, new Vector3(gridPosition.x, gridPosition.y, 0f), Quaternion.identity);
            Vector2 shipIndex = ship.GetComponent<NavigationObject>().GetGridIndex();
            grid[(int)shipIndex.y, (int)shipIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.START);
            ConnectGrid();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector2 gridPosition = GetGridPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            
            if (Vector2.Distance(GameObject.FindGameObjectWithTag("Ship").transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < .7f)
                return;
            foreach (GameObject mine in mines)
            {
                if (Vector2.Distance(mine.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 1f)
                    return;
            }
            GameObject planet = GameObject.FindGameObjectWithTag("Planet");
            Vector2 tileIndex = planet.GetComponent<NavigationObject>().GetGridIndex();
            GetGrid()[(int)tileIndex.y, (int)tileIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.UNVISITED);
            Destroy(planet);
            planet = Instantiate(planetPrefab, new Vector3(gridPosition.x, gridPosition.y, 0f), Quaternion.identity);
            Vector2 planetIndex = planet.GetComponent<NavigationObject>().GetGridIndex();
            grid[(int)planetIndex.y, (int)planetIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.GOAL);
            ConnectGrid();

        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ship = GameObject.FindGameObjectWithTag("Ship");
            Vector2 shipIndecies = ship.GetComponent<NavigationObject>().GetGridIndex();
            PathNode start = grid[(int)shipIndecies.y, (int)shipIndecies.x].GetComponent<TileScript>().Node;

            GameObject planet = GameObject.FindGameObjectWithTag("Planet");
            Vector2 planetIndicies = planet.GetComponent<NavigationObject>().GetGridIndex();
            PathNode goal = grid[(int)planetIndicies.y, (int)planetIndicies.x].GetComponent<TileScript>().Node;

            //HERE IS PATH + MOVEMENT
            List<PathNode> path = PathManager.Instance.GetShortestPath(start, goal);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ship = GameObject.FindGameObjectWithTag("Ship");
            Vector2 shipIndecies = ship.GetComponent<NavigationObject>().GetGridIndex();
            PathNode start = grid[(int)shipIndecies.y, (int)shipIndecies.x].GetComponent<TileScript>().Node;

            GameObject planet = GameObject.FindGameObjectWithTag("Planet");
            Vector2 planetIndicies = planet.GetComponent<NavigationObject>().GetGridIndex();
            PathNode goal = grid[(int)planetIndicies.y, (int)planetIndicies.x].GetComponent<TileScript>().Node;

            //HERE IS PATH + MOVEMENT
            List<PathNode> path = PathManager.Instance.GetShortestPath(start, goal);
            if (path != null)
                StartCoroutine(MoveShipAlongPath(path));
            else
                Debug.LogWarning("Something with path");
        }

      
    }

    private void BuildGrid()
    {
        grid = new GameObject[rows, columns];
        int count = 0;
        float rowPos = 5.5f;
        for (int row = 0; row < rows; row++, rowPos--)
        {
            float colPos = -7.5f;
            for (int col = 0; col < columns; col++, colPos++)
            {
                GameObject tileInst = GameObject.Instantiate(tilePrefab, new Vector3(colPos, rowPos, 0f), Quaternion.identity);
                TileScript tileScript = tileInst.GetComponent<TileScript>();
                tileScript.SetColor(colors[System.Convert.ToInt32((count++ % 2 == 0))]);
                tileInst.transform.parent = transform;
                grid[row, col] = tileInst;
                // Instantiate a new TilePanel and link it to the Tile instance.
                GameObject panelInst = GameObject.Instantiate(tilePanelPrefab, tilePanelPrefab.transform.position, Quaternion.identity);
                panelInst.transform.SetParent(panelParent.transform);
                RectTransform panelTransform = panelInst.GetComponent<RectTransform>();
                panelTransform.localScale = Vector3.one;
                panelTransform.anchoredPosition = new Vector3(64f * col, -64f * row);
                tileScript.tilePanel = panelInst.GetComponent<TilePanelScript>();
                tileScript.Node = new PathNode(tileInst);
            }
            count--;
        }
        foreach(GameObject mine in mines)
        {
            Vector2 mineIndex = mine.GetComponent<NavigationObject>().GetGridIndex();
            grid[(int)mineIndex.y, (int)mineIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.IMPASSABLE);
        }
        // Set the tile under the ship to Start.
        ship = GameObject.FindGameObjectWithTag("Ship");
        Vector2 shipIndices = ship.GetComponent<NavigationObject>().GetGridIndex();
        grid[(int)shipIndices.y, (int)shipIndices.x].GetComponent<TileScript>().SetStatus(TileStatus.START);
        // Set the tile under the player to Goal and set tile costs.
        GameObject planet = GameObject.FindGameObjectWithTag("Planet");
        Vector2 planetIndices = planet.GetComponent<NavigationObject>().GetGridIndex();
        grid[(int)planetIndices.y, (int)planetIndices.x].GetComponent<TileScript>().SetStatus(TileStatus.GOAL);
        SetTileCosts(planetIndices);
    }

    public void ConnectGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                TileScript tileScript = grid[row, col].GetComponent<TileScript>();
                tileScript.ResetNeighnourConnectios();
                if (tileScript.status == TileStatus.IMPASSABLE) continue;

                if (row > 0) // Set top neighbour if tile is not in top row.
                {
                    if (!(grid[row - 1, col].GetComponent<TileScript>().status == TileStatus.IMPASSABLE))
                    {
                        tileScript.SetNeighbourTile((int)NeighbourTile.TOP_TILE, grid[row - 1, col]);
                        tileScript.Node.AddConnections(new PathConnection(tileScript.Node, grid[row - 1, col].GetComponent<TileScript>().Node,
                            Vector3.Distance(tileScript.transform.position, grid[row - 1, col].transform.position)));
                    }

                }
                if (col < columns - 1) // Set right neighbour if tile is not in rightmost row.
                {
                    if (!(grid[row, col + 1].GetComponent<TileScript>().status == TileStatus.IMPASSABLE))
                    {
                        tileScript.SetNeighbourTile((int)NeighbourTile.RIGHT_TILE, grid[row, col + 1]);
                        tileScript.Node.AddConnections(new PathConnection(tileScript.Node, grid[row, col + 1].GetComponent<TileScript>().Node,
                            Vector3.Distance(tileScript.transform.position, grid[row, col + 1].transform.position)));
                    }

                }
                if (row < rows - 1) // Set bottom neighbour if tile is not in bottom row.
                {
                    if (!(grid[row + 1, col].GetComponent<TileScript>().status == TileStatus.IMPASSABLE))
                    {
                        tileScript.SetNeighbourTile((int)NeighbourTile.BOTTOM_TILE, grid[row + 1, col]);
                        tileScript.Node.AddConnections(new PathConnection(tileScript.Node, grid[row + 1, col].GetComponent<TileScript>().Node,
                            Vector3.Distance(tileScript.transform.position, grid[row + 1, col].transform.position)));
                    }
                }
                if (col > 0) // Set left neighbour if tile is not in leftmost row.
                {
                    if (!(grid[row, col - 1].GetComponent<TileScript>().status == TileStatus.IMPASSABLE))
                    {
                        tileScript.SetNeighbourTile((int)NeighbourTile.LEFT_TILE, grid[row, col - 1]);
                        tileScript.Node.AddConnections(new PathConnection(tileScript.Node, grid[row, col - 1].GetComponent<TileScript>().Node,
                            Vector3.Distance(tileScript.transform.position, grid[row, col - 1].transform.position)));
                    }
                }
            }
        }
    }

    public GameObject[,] GetGrid()
    {
        return grid;
    }
    public Vector2 GetGridPosition(Vector2 worldPosition)
    {
        float xPos = Mathf.Floor(worldPosition.x) + 0.5f;
        float yPos = Mathf.Floor(worldPosition.y) + 0.5f;
        return new Vector2(xPos, yPos);
    }

    public void SetTileCosts(Vector2 targetIndices)
    {
        float distance = 0f;
        float dx = 0f;
        float dy = 0f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                TileScript tileScript = grid[row, col].GetComponent<TileScript>();
                if (useManhattanHeuristic)
                {
                    dx = Mathf.Abs(col - targetIndices.x);
                    dy = Mathf.Abs(row - targetIndices.y);
                    distance = dx + dy;
                }
                else // Euclidean.
                {
                    dx = targetIndices.x - col;
                    dy = targetIndices.y - row;
                    distance = Mathf.Sqrt(dx * dx + dy * dy);
                }
                float adjustedCost = distance * baseTileCost;
                tileScript.cost = adjustedCost;
                tileScript.tilePanel.costText.text = tileScript.cost.ToString("F1");
            }
        }
    }
    public void SetTileStatuses()
    {
        foreach (GameObject go in grid)
        {
            go.GetComponent<TileScript>().SetStatus(TileStatus.UNVISITED);
        }
        foreach (GameObject mine in mines)
        {
            Vector2 mineIndex = mine.GetComponent<NavigationObject>().GetGridIndex();
            grid[(int)mineIndex.y, (int)mineIndex.x].GetComponent<TileScript>().SetStatus(TileStatus.IMPASSABLE);
        }

        ship = GameObject.FindGameObjectWithTag("Ship");
        Vector2 shipIndicies = ship.GetComponent<NavigationObject>().GetGridIndex();
        grid[(int)shipIndicies.y, (int)shipIndicies.x].GetComponent<TileScript>().SetStatus(TileStatus.START);

        GameObject planet = GameObject.FindGameObjectWithTag("Planet");
        Vector2 planetIndicies = planet.GetComponent<NavigationObject>().GetGridIndex();
        grid[(int)planetIndicies.y, (int)planetIndicies.x].GetComponent<TileScript>().SetStatus(TileStatus.GOAL);
    }
    IEnumerator MoveShipAlongPath(List<PathNode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 startPosition = ship.transform.position;
            Vector3 endPosition = path[i + 1].Tile.transform.position;
            Vector3 direction = (endPosition - startPosition).normalized;

            // Рассчитываем угол для поворота в 2D
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90; // Смещение на 90 градусов, если необходимо
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            // Плавный поворот к следующей точке
            while (Quaternion.Angle(ship.transform.rotation, targetRotation) > 0.01f)
            {
                ship.transform.rotation = Quaternion.RotateTowards(ship.transform.rotation, targetRotation, shipRotationSpeed * Time.deltaTime);
                yield return null; // Или можно использовать new WaitForEndOfFrame(), но разницы нет
            }

            // Движение к следующей точке
            while (Vector3.Distance(ship.transform.position, endPosition) > 0.01f)
            {
                ship.transform.position = Vector3.MoveTowards(ship.transform.position, endPosition, shipMovementSpeed * Time.deltaTime);
                yield return null; // Для плавного движения
            }
        }
        // Завершающие действия после достижения пути
    }


}

