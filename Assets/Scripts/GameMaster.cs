using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public enum TurnAction
{
    WAITING_START,
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

    public TurnState turnState = new TurnState();

    [SerializeField] TMPro.TMP_Text nextTurnText;
    [SerializeField] TMPro.TMP_Text gameOverText;
    [SerializeField] Image turnImage;
    [SerializeField] GameObject startGamePanel;

    [SerializeField] CinemachineVirtualCamera p1Cam;
    [SerializeField] CinemachineVirtualCamera p2Cam;
    [SerializeField] CinemachineVirtualCamera isometricCam;

    public List<List<Builder>> playerBuilders = new List<List<Builder>>();
    public List<Color> playerColors = new List<Color>();

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
        //StartNewGame();
        startGamePanel.gameObject.SetActive(true);
        turnState.action = TurnAction.WAITING_START;
        isometricCam.Priority = 100;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartNewGame()
    {
        BoardManager.Instance.InitializeBoard();
        turnState.playerTurn = 0;
        turnState.action = TurnAction.SELECT_PLAYER_SQUARES;
        isometricCam.Priority = -1;
        startGamePanel.gameObject.SetActive(false);
    }

    private void EndGame()
    {
        isometricCam.Priority = 100;
        p1Cam.Priority = 10;
        p2Cam.Priority = -1;
        BoardManager.Instance.InitializeBoard();
        turnState.playerTurn = 0;
        turnState.action = TurnAction.WAITING_START;

        // Delete players
        for (int i = 0; i < numPlayers; i++)
        {
            foreach (Builder builder in playerBuilders[i])
            {
                Destroy(builder.builderGameObject);
            }
            playerBuilders[i].Clear();
        }

        startGamePanel.gameObject.SetActive(true);
    }

    public void NextPlayer()
    {
        //StartCoroutine(ShowText());
        turnState.playerTurn = (turnState.playerTurn + 1)%2;

        if (turnState.action == TurnAction.MOVE)
        {
            if (CheckForGameOver())
            {
                // If player 1 cant move, player 2 wins and vice versa
                WonGame((turnState.playerTurn + 1) % 2);
            }
        }
        

        if (turnState.playerTurn == 0)
        {
            foreach (Builder builder in playerBuilders[0])
            {
                builder.builderGameObject.GetComponent<Outline>().enabled = true;
            }

            foreach (Builder builder in playerBuilders[1])
            {
                builder.builderGameObject.GetComponent<Outline>().enabled = false;
            }

            turnImage.color = Color.white;
            p1Cam.Priority = 10;
            p2Cam.Priority = -1;
        }
        else
        {
            foreach (Builder builder in playerBuilders[0])
            {
                builder.builderGameObject.GetComponent<Outline>().enabled = false;
            }

            foreach (Builder builder in playerBuilders[1])
            {
                builder.builderGameObject.GetComponent<Outline>().enabled = true;
            }

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

    public bool CheckForGameOver()
    {
        List<Vector2> availablePositions = new List<Vector2>();

        // Check if either of the builder can move
        foreach (Builder builder in playerBuilders[turnState.playerTurn])
        {
            BoardManager.Instance.ShowAvailableBoardPositions(builder.positionInBoard, ref availablePositions, true);
        }

        if (availablePositions.Count == 0)
            return true;

        return false;
    }

    public void WonGame(int player)
    {
        gameOverText.text = "Player " + (player + 1) + " wins!";
        if (playerColors.Count >= 2) gameOverText.color = playerColors[player];
        StartCoroutine(ShowText(gameOverText.gameObject, 2f));
    }

    public IEnumerator ShowText(GameObject textObj, float duration)
    {
        
        textObj.SetActive(true);
        yield return new WaitForSeconds(duration);
        textObj.SetActive(false);
        EndGame();
    }
}
