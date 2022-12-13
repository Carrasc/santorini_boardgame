using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PositionState
{
    LVL0 = 0,
    LVL1 = 1,
    LVL2 = 2,
    LVL3 = 3,
    BLOCKED = -1
}

public class BoardSquare
{
    public PositionState state;
    public GameObject squareGameObject;
}

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    private GameObject currentSelectedPosition = null;
    private Builder currentSelectedBuilder = null;

    [Header("Players prefabs")]
    [SerializeField] private List<GameObject> playerPrefabs = new List<GameObject>();

    [Header("Structures parent GO")]
    [SerializeField] private GameObject structuresParent;

    [Header("Structures prefabs")]
    [SerializeField] private GameObject lvl1Prefab;
    [SerializeField] private GameObject lvl2Prefab;
    [SerializeField] private GameObject lvl3Prefab;
    [SerializeField] private GameObject blockedPrefab;

    [SerializeField] private List<GameObject> rows = new List<GameObject>();

    public BoardSquare[][] boardState;

    // When a builder is selecled, add the available move positions here
    private List<Vector2> availablePositions = new List<Vector2>();

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        InitializeBoard();
    }

    public void InitializeBoard()
    {
        boardState = new BoardSquare[rows.Count][];

        for (int i = 0; i < rows.Count; i++)
        {
            boardState[i] = new BoardSquare[rows[i].transform.childCount];
            for (int j = 0; j < rows[i].transform.childCount; j++)
            {
                // Initialize all positions at lvl 0 (nothing constructed)
                boardState[i][j] = new BoardSquare();
                boardState[i][j].state = PositionState.LVL0;
                boardState[i][j].squareGameObject = rows[i].transform.GetChild(j).gameObject;
            }
        }

        // Destroy all structures from previous rounds
        foreach (Transform child in structuresParent.transform)
        {
            Destroy(child.gameObject);
        }
        
    }

    public void SetSelectedPosition(GameObject square, int row, int column)
    {
        int playerTurn = GameMaster.Instance.turnState.playerTurn;

        if (GameMaster.Instance.turnState.action == TurnAction.WAITING_START)
        {
            
            return;
        }

        // SELECT BUILDER POSITION STAGE
        if (GameMaster.Instance.turnState.action == TurnAction.SELECT_PLAYER_SQUARES)
        {
            bool playerInSquare = false;
            // We cant be in other builder in the position, so check all builders positions 
            for (int players = 0; players < GameMaster.Instance.playerBuilders.Count; players++)
            {
                for (int builder = 0; builder < GameMaster.Instance.playerBuilders[players].Count; builder++)
                {
                    if (GameMaster.Instance.playerBuilders[players][builder].positionInBoard == new Vector2(row, column))
                    {
                        playerInSquare = true;
                        break;
                    }
                }
            }

            if (!playerInSquare)
            {
                // Asign the selected position to the player in turn
                GameMaster.Instance.playerBuilders[playerTurn].Add(
                    new Builder(new Vector2(row, column),
                    Instantiate(playerPrefabs[playerTurn],
                                square.transform.position,
                                Quaternion.LookRotation(playerTurn == 0 ? Vector3.forward : Vector3.left)))
                    ); // Look forward for player1 and left for player2
            }
            else
            {
                Debug.Log("There is a builder in this position already!");
            }
            

            if (GameMaster.Instance.playerBuilders[playerTurn].Count == 2)
            {
                GameMaster.Instance.NextAction();
            }

            return;
        }

        // MOVE A BUILDER TURN
        if (GameMaster.Instance.turnState.action == TurnAction.MOVE)
        {
            // If a builder is already selected, TRY MOVE to the block selected now (check if can move there or NOT)
            if (currentSelectedBuilder != null)
            {             
                if (availablePositions.Contains(new Vector2(row, column)))
                {
                    // We CAN move to the selected position
                    HideAvailableBoardPositions(ref availablePositions);
                    MoveBuilder(new Vector2(row, column));
                    return;
                }
                else
                {
                    Debug.Log("Cant move to position " + row + " - " + column);
                }
            }

            // If a builder is NOT already selected, Check for both positions on the board
            for (int i = 0; i < GameMaster.Instance.playerBuilders[playerTurn].Count; i++)
            {
                // The player clicked on a builder position to move him, select that builder and show available move positions
                if (GameMaster.Instance.playerBuilders[playerTurn][i].positionInBoard.x == row && GameMaster.Instance.playerBuilders[playerTurn][i].positionInBoard.y == column && currentSelectedBuilder == null)
                {
                    SelectBuilder(GameMaster.Instance.playerBuilders[playerTurn][i], square, true);
                    ShowAvailableBoardPositions(new Vector2(row, column), ref availablePositions);
                    break;
                }
                // If the clicks the same position, unselect the position
                else if (GameMaster.Instance.playerBuilders[playerTurn][i].positionInBoard.x == row && GameMaster.Instance.playerBuilders[playerTurn][i].positionInBoard.y == column && currentSelectedPosition == square)
                {
                    SelectBuilder(GameMaster.Instance.playerBuilders[playerTurn][i], square, false);
                    HideAvailableBoardPositions(ref availablePositions);
                    break;
                }     
            }
            
            return;
        }

        // BUILDING TURN
        if (GameMaster.Instance.turnState.action == TurnAction.BUILD)
        {
            // If the square selected is available to build
            if (availablePositions.Contains(new Vector2(row, column)))
            {
                HideAvailableBoardPositions(ref availablePositions);

                // Check what type of scructure should be built
                switch(boardState[row][column].state)
                {
                    case PositionState.LVL0:
                        Instantiate(lvl1Prefab, square.transform.position + new Vector3(0, 0.0625f + (0.125f * 0), 0), square.transform.rotation, structuresParent.transform);
                        boardState[row][column].state ++;
                        break;
                    case PositionState.LVL1:
                        Instantiate(lvl2Prefab, square.transform.position + new Vector3(0, 0.0625f + (0.125f * 1), 0), square.transform.rotation, structuresParent.transform);
                        boardState[row][column].state ++;
                        break;
                    case PositionState.LVL2:
                        Instantiate(lvl3Prefab, square.transform.position + new Vector3(0, 0.0625f + (0.125f * 2), 0), square.transform.rotation, structuresParent.transform);
                        boardState[row][column].state ++;
                        break;
                    case PositionState.LVL3:
                        Instantiate(blockedPrefab, square.transform.position + new Vector3(0, 0.0625f + (0.125f * 3), 0), square.transform.rotation, structuresParent.transform);
                        boardState[row][column].state = PositionState.BLOCKED;
                        break;
                    case PositionState.BLOCKED:                     
                        break;
                }

                GameMaster.Instance.NextAction();
            }
            return;
        }
    }

    private void SelectBuilder(Builder builder, GameObject square, bool state)
    {
        currentSelectedBuilder = state ? builder : null;
        currentSelectedPosition = state ? square : null;
        //builder.builderGameObject.GetComponent<Outline>().enabled = state;
        //builder.builderGameObject.GetComponent<BuilderSelectedAnimation>().enabled = state;
    }

    private List<Vector2> ShowAvailableBoardPositions(Vector2 builderPos, ref List<Vector2> availablePositions)
    {
        int from_i = 0;
        int from_j = 0;

        int to_i = 0;
        int to_j = 0;

        if (builderPos.x - 1 >= 0)
            from_i = (int)builderPos.x - 1;
        else
            from_i = 0;

        if (builderPos.x + 1 < rows.Count)
            to_i = (int)builderPos.x + 1;
        else
            to_i = rows.Count - 1;

        if (builderPos.y - 1 >= 0)
            from_j = (int)builderPos.y - 1;
        else
            from_j = 0;

        if (builderPos.y + 1 < rows[0].transform.childCount)
            to_j = (int)builderPos.y + 1;
        else
            to_j = rows[0].transform.childCount - 1;

        for (int i = from_i; i <= to_i; i++)
        {
            for (int j = from_j; j <= to_j; j++)
            {
                var vectorToAdd = new Vector2(i, j);
                bool availableSquare = true;
                
                // We cant move if there is other builder in the position, so check all builders positions 
                for (int players = 0; players < GameMaster.Instance.playerBuilders.Count; players++)
                {
                    for (int builder = 0; builder < GameMaster.Instance.playerBuilders[players].Count; builder++)
                    {
                        if (GameMaster.Instance.playerBuilders[players][builder].positionInBoard == vectorToAdd)
                        {
                            availableSquare = false;
                            break;
                        }
                    }
                }

                // We cant move more than 1 lvl UP at a time
                if (GameMaster.Instance.turnState.action == TurnAction.MOVE)
                {
                    // So if the difference of where the builder want to move MINUS where the builder is at is more than 1, we cant move there
                    if ((boardState[(int)vectorToAdd.x][(int)vectorToAdd.y].state - boardState[(int)builderPos.x][(int)builderPos.y].state) > 1)
                    {
                        availableSquare = false;
                    }
                }

                // We cant move or build if there is a BLOCKER at the top
                if ((boardState[(int)vectorToAdd.x][(int)vectorToAdd.y].state == PositionState.BLOCKED))
                {
                    availableSquare = false;
                }

                // If no other builder was in that position, add it as available
                if (availableSquare)
                {
                    availablePositions.Add(vectorToAdd);

                    // Different indicator depending if moving or building
                    if (GameMaster.Instance.turnState.action == TurnAction.MOVE)
                        boardState[i][j].squareGameObject.transform.GetChild(0).gameObject.SetActive(true);
                    else if (GameMaster.Instance.turnState.action == TurnAction.BUILD)
                        boardState[i][j].squareGameObject.transform.GetChild(1).gameObject.SetActive(true);
                } 
            }
        }

        return availablePositions;
    }

    private void HideAvailableBoardPositions(ref List<Vector2> availablePositions)
    {
        foreach(Vector2 position in availablePositions)
        {
            if (GameMaster.Instance.turnState.action == TurnAction.MOVE)
                boardState[(int)position.x][(int)position.y].squareGameObject.transform.GetChild(0).gameObject.SetActive(false);
            else if (GameMaster.Instance.turnState.action == TurnAction.BUILD)
                boardState[(int)position.x][(int)position.y].squareGameObject.transform.GetChild(1).gameObject.SetActive(false);

        }

        availablePositions.Clear();
    }

    private void MoveBuilder(Vector2 positionToMove)
    {
        float yOffset = 0;
        bool isWinningMove = false;
        // Move the actual transform of the character
        switch (boardState[(int)positionToMove.x][(int)positionToMove.y].state)
        {
            // 0.125 is the height of the cube
            case PositionState.LVL0:
                yOffset = 0f;
                break;
            case PositionState.LVL1:
                yOffset = (0.125f * 1);
                break;
            case PositionState.LVL2:
                yOffset = (0.125f * 2);
                break;
            case PositionState.LVL3:
                yOffset = (0.125f * 3);
                isWinningMove = true;
                break;
        }

        // Move the actual transform
        currentSelectedBuilder.builderGameObject.transform.position = boardState[(int)positionToMove.x][(int)positionToMove.y].squareGameObject.transform.position + new Vector3(0, yOffset, 0);

        // Update the builder position in the game logic
        currentSelectedBuilder.positionInBoard = new Vector2(positionToMove.x, positionToMove.y);

        // de-select the builder
        SelectBuilder(currentSelectedBuilder, null, false);
        GameMaster.Instance.NextAction();

        // If the player moved to a Lvl3 structure, end the game and dont show possible building squares
        if (isWinningMove)
        {
            GameMaster.Instance.WonGame(GameMaster.Instance.turnState.playerTurn);
            return;
        }

        // Show possible square positions for building
        ShowAvailableBoardPositions(GameMaster.Instance.playerBuilders[GameMaster.Instance.turnState.playerTurn][0].positionInBoard, ref availablePositions);
        ShowAvailableBoardPositions(GameMaster.Instance.playerBuilders[GameMaster.Instance.turnState.playerTurn][1].positionInBoard, ref availablePositions);
    }
}
