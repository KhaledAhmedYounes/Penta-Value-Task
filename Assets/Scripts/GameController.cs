using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // The maximum x value the player's position can have. Used in PlayerMovement
    public float xMax;

    // The maximum y value the player's position can have. Used in PlayerMovement
    public float yMax;

    // This is the number of rows. Obtained from the text file
    public int rows;

    // This is the number of columns. Obtained from the text file
    public int columns;

    // The location the player's starting position (the location in the array). Obtained from the text file
    public int startIndex;

    // The location the player's destination (the location in the array). Obtained from the text file
    public int endIndex;

    // This holds the different grid cells that will be used to build the grid
    [Header("Cell Prefabs")]
    public GameObject[] cellPrefabs;

    // Check if theplayer reached the desired destination or not
    public bool playerWon;

    // A reference to the UI Text that displays the number of tiles the player traversed
    public Text tileCounterText;

    // A reference to the PlayerMovement script
    public PlayerMovement playerMovement;

    // The name of the scene to be loaded when the player restarts the game (This is better than hard coding the scene name)
    public string sceneName;

    // The animator of the Win Screen. The screen is activted when the player wins
    public Animator winScreenAnimator;

    // The game object that holds the grid cells. The position of the cells will be relative to the parent
    public GameObject gridParent;

    // The player game object
    public GameObject player;

    // The cell the player must reach to win the game
    public GameObject winCell;

    // The cells that the player cannot traverse. Used in the PlayerMovement script
    public List<Vector2> dangerZones;

    // This will count the number of traversed cells. 
    // This is used instead of a counter so a cell doesn't get counted twice
    public List<Vector2> traversedCells;


    // The Vector2 position the player starts at
    public Vector2 startPosition;

    // The Vector2 the player is supposed to go to (This is how the player wins)
    public Vector2 endPosition;

    // The current Vector2 to place a grid cell. It gets incremented by 1 in the x and y axis over time
    public Vector2 cellPosistion;

    // The cells the player CAN traverse (The game glitches when it's not made public for some reason)
    [HideInInspector]
    public List<Vector2> safeZones;

    // A list of ALL the cells inside the grid. Used in the PlayerMovement script    
    public List<Vector2> cells;


    private string _gridSize;           // Holds the first line of the text file the game reads from. The first line specifies the grid size

    private string _gridContent;        // Holds the second line of the text file the game reads from. The second line specifies whether or not a cell can be walked on

    private string[] gridContent;       // Converted _gridContent into a string array and store it in a variable (It will be used by several functions)

    private string _startPosition;      // Holds the third line of the text file the game reads from. The third line specifies where the player starts and their destination



    // Use this for initialization
    void Start()
    {
        // Set the UI text to the number of cells traversed
        tileCounterText.text = "Tiles traversed: " + traversedCells.Count;
        // Read from the text file
        ReadFromTextFile();

        // Set player won to false
        playerWon = false;

        // The position of the first Grid Cell will be zero (relative to the Grid Parent)
        cellPosistion = Vector2.zero;

        // Set the player gameObject's parent to be the Grid Parent (To know where the player is insidethe grid)
        player.transform.SetParent(gridParent.transform);

        // Create a string array form _gridSize (Note the 'this' keyword)
        string[] _gridSize = this._gridSize.Split(' ');

        // Assign the values for rows and columns
        columns = Convert.ToInt32(_gridSize[0]);
        rows = Convert.ToInt32(_gridSize[1]);

        // Create a string array from _startPosition (Note the 'this' keyword)
        string[] _startPosition = this._startPosition.Split(' ');
        // Assign the values for startIndex and endIndex
        startIndex = Convert.ToInt32(_startPosition[0]);
        endIndex = Convert.ToInt32(_startPosition[1]);

        // Create a string array from _gridContent (Note that gridContent, without the underscore, is global because other methods use it)
        gridContent = _gridContent.Split(' ');

        // Place the Grid Parent in the center of the screen
        gridParent.transform.position = new Vector2(-1 * (columns / 2), rows / 2);

        // Calculate the maximum number of rows and column the player cannot exceed while navigating the grid
        xMax = columns;
        yMax = -1 * rows;

        // Create the Grid
        CreateGrid();

        // After creating the grid, make sure if the start point can be added in startIndex. If it can't, then add it in the first index
        // in the  safe zone list. This insures that there's ALWAYS a start point
        if (safeZones.Contains(cells[startIndex]))
        {
            startPosition = cells[startIndex];
        }
        else
        {
            startPosition = safeZones[0];
        }

        // After creating the grid, make sure if the end point can be added in endIndex. If it can't, then add it in the last index
        // in the safe zone list. This insures that there's ALWAYS an end point
        if (safeZones.Contains(cells[endIndex]))
        {
            endPosition = cells[endIndex];
        }
        else
        {
            endPosition = safeZones[safeZones.Count - 1];
        }

        // Create a GameObject for the winCell
        GameObject winCell = Instantiate(this.winCell) as GameObject;

        // Assign the winCell object's parent to the Grid Parent
        winCell.transform.SetParent(gridParent.transform);

        // Assign the local position of the winCell to the endPoint
        winCell.transform.localPosition = endPosition;

        // Set the winCell's rotaion to zero
        winCell.transform.localRotation = Quaternion.identity;

        // Set the player's starting position to startPostion
        playerMovement.newPosition = startPosition;
    }

    private void Update()
    {
        // Check if the player reached the endPosition
        if ((Vector2)player.transform.localPosition == endPosition)
        {
            Debug.Log("Player Won!");
            playerWon = true;
        }

        // Update the tile counter
        tileCounterText.text = "Tiles traversed: " + (traversedCells.Count - 1);

        // If the player hasn't won yet, hide the mouse cursor because it can be distracting.
        // Otherwise display the mouse cursor if the player won as well as play the win screen's animation
        if (playerWon)
        {
            Cursor.visible = true;
            winScreenAnimator.SetBool(StringValues.ANIMATION_PARAMETER_PLAYER_WON, true);
        }
        else
        {
            Cursor.visible = false;
        }
    }

    private void CreateGrid()
    {
        // Cycle through the gridContent array
        for (int i = 0; i < gridContent.Length; i++)
        {
            // Set index to zero by default. If it remains zero then we choose the first element in the Cell Prefabs array
            int index = 0;

            // If the current element in gridContent is equal to 1 then choose the second element
            if (gridContent[i] == "1")
            {
                index = 1;
                // Add the the Vector2 of that cell into the Danger Zones list (This will be used later to restrict the player's movements)
                dangerZones.Add(cellPosistion);
            }
            // Do a similar thing if the we find a 2
            else if (gridContent[i] == "2")
            {
                index = 2;
                dangerZones.Add(cellPosistion);
            }
            else if (gridContent[i] == "0")
            {
                // If the current element in gridContent is equal to 0 then add the Vector2 to the safe zones
                safeZones.Add(cellPosistion);
            }

            // After determining which cell prefab to choose, based on the index, place it at the current Vector2
            PlaceCellAt(cellPosistion, index);
            // After that add the current cell position to the cells
            cells.Add(cellPosistion);

            // Check if i+1 is greated than the number of columns specified in the text file. If it is, decrement             
            // the y-axis of the current cell position by one. If it's not then increment the x-axis by one
            if (i + 1 >= columns)
            {
                if ((i + 1) % columns == 0)
                {
                    cellPosistion.x = 0;
                    cellPosistion.y -= 1;
                }
                else
                {
                    cellPosistion.x += 1;
                }
            }
            else
            {
                cellPosistion.x += 1;
            }
        }
    }

    // This function takes a Vector2 to know where to place the grid cell, and an index to know what type of cell to choose from the cellPrefabs array
    private void PlaceCellAt(Vector2 position, int index)
    {
        // Instantiate a specific cell and store it in a GameObject object
        GameObject cell = Instantiate(cellPrefabs[index]) as GameObject;

        // Have that cell be a child of the Grid Parent
        cell.transform.SetParent(gridParent.transform);

        // Set the local position and NOT the global position because the Grid Parent will move to a different positon
        cell.transform.localPosition = position;

        // Set the rotation to zero
        cell.transform.localRotation = Quaternion.identity;
    }

    // Read the text file lecated in the Data folder (in the case of the build version) or the Assets folder (in the case of the Unity Editor)
    private void ReadFromTextFile()
    {
        // Get the directory for the Assests folder or the Game Data folder (depending on whether we're running a built version or not)
        string fileDirectory = Application.dataPath;

        // Read the text file by concatinating its name with the directory
        StreamReader reader = new StreamReader(fileDirectory + "\\Penta Value Task _In.txt");

        // This will store the individual lines in the text file
        List<string> lines = new List<string>();

        // Read the first line in the text file
        string line = reader.ReadLine();

        // Add the lines in the List and continue to read from the file 
        while (line != null)
        {
            lines.Add(line);
            line = reader.ReadLine();
        }

        // Try to assign the List of strings to the appropriate strings. If that doesn't work then display an error message in the console
        try
        {
            _gridSize = lines[0];
            _gridContent = lines[1];
            _startPosition = lines[2];
        }
        catch (Exception)
        {
            Debug.LogError("Seomthing went wrong");
        }
    }

    // This function is called when the player clicks the Play Again button
    public void PlayAgain()
    {
        // The scene name is specified in the inspector (better than hard coding the scene name)
        SceneManager.LoadScene(sceneName);
    }

    // Closes the game if the player click the Quit button
    public void QuitGame()
    {
        Application.Quit();
    }
}
