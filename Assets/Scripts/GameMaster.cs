using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public enum TurnAction
{
    MOVE,
    BUILD,
    SELECT_PLAYER_SQUARES,
}

public class Builder
{
    public Vector2 positionInBoard;
    public GameObject builderGameObject;

    public Builder(Vector2 PositionInBoard, GameObject BuilderGameObject)
    {
        positionInBoard = PositionInBoard;
        builderGameObject = BuilderGameObject;
    }
}

public class TurnState
{
    public int playerTurn;
    public TurnAction action;
}

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance { get; private set; }

    private bool positionsAssigned = false;

    public TurnState turnState = new TurnState();

    [SerializeField] TMPro.TMP_Text nextTurnText;
    [SerializeField] Image turnImage;

    [SerializeField] CinemachineVirtualCamera p1Cam;
    [SerializeField] CinemachineVirtualCamera p2Cam;

    public List<List<Builder>> playerBuilders = new List<List<Builder>>();

    const int numPlayers = 2;


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

        // Initialize each List 
        for (int i = 0; i < numPlayers; i++)
        {
            playerBuilders.Add(new List<Builder>());
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        StartNewGame();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void StartNewGame()
    {
        BoardManager.Instance.InitializeBoard();
        positionsAssigned = false;
        turnState.playerTurn = 0;
        turnState.action = TurnAction.SELECT_PLAYER_SQUARES;
    }

    private void SelectPositions()
    {

    }

    public void NextPlayer()
    {
        //StartCoroutine(ShowText());
        turnState.playerTurn = (turnState.playerTurn + 1)%2;

        if (turnState.playerTurn == 0)
        {
            turnImage.color = Color.white;
            p1Cam.Priority = 10;
            p2Cam.Priority = -1;
        }
        else
        {
            turnImage.color = new Color(0.30f, 0.30f, 0.30f, 1);
            p1Cam.Priority = -1;
            p2Cam.Priority = 10;
        }

    }

    public void NextAction()
    {
        if (turnState.action == TurnAction.SELECT_PLAYER_SQUARES)
        {
            // If the last player has selected both builder positions, start the game
            if (playerBuilders[numPlayers - 1].Count == 2)
            {
                turnState.action = TurnAction.MOVE;
                NextPlayer();
                return;
            }
            // If not, move to the next player so he can select his builder positions.
            else
            {
                NextPlayer();
                return;
            }
        }

        if (turnState.action == TurnAction.MOVE)
        {
            turnState.action = TurnAction.BUILD;
        }
        else if (turnState.action == TurnAction.BUILD)
        {
            turnState.action = TurnAction.MOVE;
            NextPlayer();
        }
    }

    public IEnumerator ShowText()
    {
        nextTurnText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.25f);
        nextTurnText.gameObject.SetActive(false);
    }
}
