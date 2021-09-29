using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : Singleton<SceneManager>
{
    Transform parentCanvas;

    string abs = "abcdefghijklmnop";

    [SerializeField]
    private Toggle whiteToggle;
    [SerializeField]
    private Toggle blackToggle;

    [SerializeField]
    private Toggle crossToggle;
    [SerializeField]
    private Toggle diagToggle;

    [SerializeField]
    private Toggle gameAgainsCPU;
    [SerializeField]
    private InputField topPlayerInput;
    [SerializeField]
    private InputField bottonPlayerInput;
    [SerializeField]
    private Text topPlayerName;
    [SerializeField]
    private Text bottonPlayerName;

    [SerializeField]
    private GameObject fieldUnit;
    private GameObject fieldUnitLocal;

    [SerializeField]
    private GameObject numbers;
    private GameObject numbersLocal;

    [SerializeField]
    private GameObject settingsPanel;
    [SerializeField]
    private GameObject resultsPanel;
    [SerializeField]
    private Text whiteResults;
    [SerializeField]
    private Text blackResults;

    private bool playerPlaysForWhite;
    private bool nextTurnWhites;

    [SerializeField]
    private GameObject solder;
    private GameObject solderLocal;
    private GameObject currentPickedSolderLocal;
    private Solder currentPickedSolderObject;

    private int boardSize = 8;
    private int soldersQnt = 9;

    public Dictionary<Vector2, Vector2Int> sceneCoordinates;
    public Dictionary<Vector2Int, Vector2> sceneCoordinatesInverse;
    public List<Vector2Int> boardCoordsForWinUp;
    public List<Vector2Int> boardCoordsForWinDown;
    public Dictionary<Vector2Int, Button> boardUnits;
    private Dictionary<Vector2Int, Solder> boardSoldersCoords;
    public List<Button> solderButtons;
    public List<GameObject> solderGO;

    private bool allPriorityStepsChecked;
    private bool allLessPriorityStepsChecked;
    private int nextSolderOfCPU;

    [HideInInspector]
    public Vector2 activatedSolderPos;

    //класс для фиксирования параметров игрока
    class Solder {
        public bool isPlayers;
        public bool isWhite;
        public Vector2Int solderCoords;
        public GameObject solderGameObject;
    };

    // Start is called before the first frame update
    void Start()
    {
        allPriorityStepsChecked = false;
        allLessPriorityStepsChecked = false;
        nextSolderOfCPU = 0;

        playerPlaysForWhite = true;
        whiteToggle.SetIsOnWithoutNotify(true);
        blackToggle.SetIsOnWithoutNotify(false);
        crossToggle.isOn = false;
        diagToggle.isOn = false;
        if (playerPlaysForWhite) nextTurnWhites = true;
        parentCanvas = GameObject.FindGameObjectWithTag("Respawn").transform;

        sceneCoordinates = new Dictionary<Vector2, Vector2Int>();
        sceneCoordinatesInverse = new Dictionary<Vector2Int, Vector2>();
        boardUnits = new Dictionary<Vector2Int, Button>();
        boardCoordsForWinUp = new List<Vector2Int>();
        boardCoordsForWinDown = new List<Vector2Int>();

        //координаты для определения хода компьютера
        //boardCoordsForWinDown1LineOfPriority = new List<Vector2Int>();
        //boardCoordsForWinDown2LineOfPriority = new List<Vector2Int>();
        //boardCoordsForWinDown3LineOfPriority = new List<Vector2Int>();

        boardSoldersCoords = new Dictionary<Vector2Int, Solder>();
        solderButtons = new List<Button>();
        solderGO = new List<GameObject>();

        topPlayerName.text = topPlayerInput.text;
        bottonPlayerName.text = bottonPlayerInput.text;

        setTheCoordinates();
        setTheBoard();
        setTheSolders();
        for (int i = 0; i < solderButtons.Count; i++)
        {
            solderButtons[i].interactable = false;
        }
    }

    //метод для начала новой игры и старта игры
    public void newGame(Text buttonText) {
        if (buttonText.text.Contains("Новая"))
        {
            buttonText.text = "Старт";
            settingsPanel.SetActive(true);
            resultsPanel.SetActive(false);
            boardSoldersCoords.Clear();
            solderButtons.Clear();
            resetExistingSoldersForNewGame();

            //дезактивация пешек перед стартом игры 
            for (int i = 0; i < solderButtons.Count; i++)
            {
                solderButtons[i].interactable = false;
            }
            //дезактивация всех активных полей доски 
            for (int x = 0; x < boardUnits.Count; x++)
            {
                boardUnits.Values.ToList()[x].interactable = false;
            }
            activatedSolderPos = new Vector2(-10000, -10000);
            currentPickedSolderLocal = null;
            currentPickedSolderObject = null;
        }
        else
        {
            allPriorityStepsChecked = false;
            allLessPriorityStepsChecked = false;
            nextSolderOfCPU = 0;
            topPlayerName.text = topPlayerInput.text;
            bottonPlayerName.text = bottonPlayerInput.text;
            buttonText.text = "Новая игра";
            settingsPanel.SetActive(false);
            for (int i = 0; i < solderButtons.Count; i++)
            {
                solderButtons[i].interactable = true;
            }
        }
    }

    //выбор цвета игрока
    public void changeTheColorOfPlayer() {
        if (playerPlaysForWhite)
        {
            whiteToggle.SetIsOnWithoutNotify(false);
            blackToggle.SetIsOnWithoutNotify(true);
            playerPlaysForWhite = false;
            nextTurnWhites = false;
        }
        else
        {
            whiteToggle.SetIsOnWithoutNotify(true);
            blackToggle.SetIsOnWithoutNotify(false);
            playerPlaysForWhite = true;
            nextTurnWhites = true;
        }

        boardSoldersCoords.Clear();
        solderButtons.Clear();
        resetExistingSoldersForNewGame();
        for (int i = 0; i < solderButtons.Count; i++)
        {
            solderButtons[i].interactable = false;
        }
    }

    //метод для создания коллекции координат сцены, которые будут соотвествовать квадратикам доски
    private void setTheCoordinates()
    {
        int xCoordinate = 0;
        int yCoordinate = -350;

        for (int j = 0; j < boardSize; j++)
        {
            for (int i = 0; i < boardSize; i++)
            {
                sceneCoordinates.Add(new Vector2(xCoordinate, yCoordinate), new Vector2Int(i, j));
                sceneCoordinatesInverse.Add(new Vector2Int(i, j), new Vector2(xCoordinate, yCoordinate));
                xCoordinate += 100;
            }
            xCoordinate = 0;
            yCoordinate += 100;
        }

        //сохранение координат для определения подбеды одного из игроков
        for (int j = 0; j < (int)Math.Sqrt(soldersQnt); j++)
        {
            for (int i = boardSize - (int)Math.Sqrt(soldersQnt); i < boardSize; i++)
            {
                boardCoordsForWinDown.Add(new Vector2Int(i, j));
            }
        }
        for (int j = boardSize - (int)Math.Sqrt(soldersQnt); j < boardSize; j++)
        {
            for (int i = 0; i < (int)Math.Sqrt(soldersQnt); i++)
            {
                boardCoordsForWinUp.Add(new Vector2Int(i, j));
            }
        }
    }

    //метод для расстановки квадратиков на старте игры
    private void setTheBoard() {
        for (int j = 0; j < boardSize; j++)
        {
            for (int i = 0; i < boardSize; i++)
            {
                fieldUnitLocal = Instantiate(fieldUnit);
                fieldUnitLocal.transform.SetParent(parentCanvas, false);
                fieldUnitLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];
                if (j % 2 == 0 && i % 2 == 0) fieldUnitLocal.GetComponent<Image>().color = new Color(1, 0.6f,0, 1);
                if (j % 2 != 0 && i % 2 != 0) fieldUnitLocal.GetComponent<Image>().color = new Color(1, 0.6f, 0, 1);
                boardUnits.Add(new Vector2Int(i, j), fieldUnitLocal.GetComponent<Button>());
            }
        }
        int numb = 1;
        for (int x = 0; x < boardSize; x++) {
            numbersLocal = Instantiate(numbers);
            numbersLocal.GetComponent<Text>().text = numb.ToString();
            numbersLocal.transform.SetParent(parentCanvas, false);
            numbersLocal.transform.localPosition = new Vector2(sceneCoordinatesInverse[new Vector2Int(0, x)].x - 70, sceneCoordinatesInverse[new Vector2Int(0, x)].y);

            numbersLocal = Instantiate(numbersLocal);
            numbersLocal.transform.SetParent(parentCanvas, false);
            numbersLocal.transform.localPosition = new Vector2(sceneCoordinatesInverse[new Vector2Int(0, x)].x + 770, sceneCoordinatesInverse[new Vector2Int(0, x)].y);

            numbersLocal = Instantiate(numbersLocal);
            numbersLocal.GetComponent<Text>().text = abs[numb - 1].ToString();
            numbersLocal.transform.SetParent(parentCanvas, false);
            numbersLocal.transform.localPosition = new Vector2(sceneCoordinatesInverse[new Vector2Int(x, 0)].x, sceneCoordinatesInverse[new Vector2Int(x, 0)].y - 80);

            numbersLocal = Instantiate(numbersLocal);
            numbersLocal.transform.SetParent(parentCanvas, false);
            numbersLocal.transform.localPosition = new Vector2(sceneCoordinatesInverse[new Vector2Int(x, 0)].x, sceneCoordinatesInverse[new Vector2Int(x, 0)].y + 780);
            numb++;
        }
    }

    //метод для расстановки пешек на старте игры
    private void setTheSolders()
    {
        if (playerPlaysForWhite)
        {
            solder.GetComponent<Image>().color = Color.white;
            for (int j = 0; j < (int)Math.Sqrt(soldersQnt); j++)
            {
                for (int i = boardSize - (int)Math.Sqrt(soldersQnt); i < boardSize; i++)
                {
                    solderLocal = Instantiate(solder);
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = true;
                    solderExm.isWhite = true;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                    solderGO.Add(solderLocal);
                }
            }
            solder.GetComponent<Image>().color = Color.black;
            for (int j = boardSize - (int)Math.Sqrt(soldersQnt); j < boardSize; j++)
            {
                for (int i = 0; i < (int)Math.Sqrt(soldersQnt); i++)
                {
                    solderLocal = Instantiate(solder);
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = false;
                    solderExm.isWhite = false;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                    solderGO.Add(solderLocal);
                }
            }
        }
        else {
            solder.GetComponent<Image>().color = Color.black;
            for (int j = 0; j < (int)Math.Sqrt(soldersQnt); j++)
            {
                for (int i = boardSize - (int)Math.Sqrt(soldersQnt); i < boardSize; i++)
                {
                    solderLocal = Instantiate(solder);
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = true;
                    solderExm.isWhite = false;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                    solderGO.Add(solderLocal);
                }
            }
            solder.GetComponent<Image>().color = Color.white;
            for (int j = boardSize - (int)Math.Sqrt(soldersQnt); j < boardSize; j++)
            {
                for (int i = 0; i < (int)Math.Sqrt(soldersQnt); i++)
                {
                    solderLocal = Instantiate(solder);
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = false;
                    solderExm.isWhite = true;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                    solderGO.Add(solderLocal);
                }
            }
        }
    }

    //перестановка существующих объектов пешек для начала новой игры
    private void resetExistingSoldersForNewGame() {
        if (playerPlaysForWhite)
        {
            int solderIndex = 0;
            for (int j = 0; j < (int)Math.Sqrt(soldersQnt); j++)
            {
                for (int i = boardSize - (int)Math.Sqrt(soldersQnt); i < boardSize; i++)
                {
                    solderLocal = solderGO[solderIndex];
                    solderLocal.GetComponent<Image>().color = Color.white;
                    solderIndex++;
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = true;
                    solderExm.isWhite = true;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                }
            }

            for (int j = boardSize - (int)Math.Sqrt(soldersQnt); j < boardSize; j++)
            {
                for (int i = 0; i < (int)Math.Sqrt(soldersQnt); i++)
                {
                    solderLocal = solderGO[solderIndex];
                    solderLocal.GetComponent<Image>().color = Color.black;
                    solderIndex++;
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = false;
                    solderExm.isWhite = false;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                }
            }
        }
        else
        {
            int solderIndex = 0;
            for (int j = 0; j < (int)Math.Sqrt(soldersQnt); j++)
            {
                for (int i = boardSize - (int)Math.Sqrt(soldersQnt); i < boardSize; i++)
                {
                    solderLocal = solderGO[solderIndex];
                    solderLocal.GetComponent<Image>().color = Color.black;
                    solderIndex++;
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = true;
                    solderExm.isWhite = false;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                }
            }
            for (int j = boardSize - (int)Math.Sqrt(soldersQnt); j < boardSize; j++)
            {
                for (int i = 0; i < (int)Math.Sqrt(soldersQnt); i++)
                {
                    solderLocal = solderGO[solderIndex];
                    solderLocal.GetComponent<Image>().color = Color.white;
                    solderIndex++;
                    solderLocal.transform.SetParent(parentCanvas, false);
                    solderLocal.transform.localPosition = sceneCoordinatesInverse[new Vector2Int(i, j)];

                    Solder solderExm = new Solder();
                    solderExm.isPlayers = false;
                    solderExm.isWhite = true;
                    solderExm.solderCoords = new Vector2Int(i, j);
                    solderExm.solderGameObject = solderLocal;
                    //boardSolders.Add(solderExm);
                    boardSoldersCoords.Add(new Vector2Int(i, j), solderExm);
                    solderButtons.Add(solderLocal.GetComponent<Button>());
                }
            }
        }
    }

    //активация вариантов хода для игрока при нажатии на пешку, вызывается со скрипта PlayerSolderCtrlr
    public void playerSolderPickMngr(GameObject solder)
    {

        Vector2 currentPos = solder.transform.localPosition; //получение координат сцены на которой стоит выбранная пешка
        Vector2Int boardPos = sceneCoordinates[currentPos]; //получение координат доски на которой стоит выбранная пешка
        currentPickedSolderObject = boardSoldersCoords[boardPos]; //получение ссылки на объект пешки и сохранение ее до хода


        //активация возможных ходов с учетом перехода хода
        if (currentPickedSolderObject.isWhite && nextTurnWhites)
        {
            //дезактивация всех активных полей доски 
            for (int x = 0; x < boardUnits.Count; x++)
            {
                boardUnits.Values.ToList()[x].interactable = false;
            }


            if (activatedSolderPos != currentPos)
            {
                //для определения возможности перепрыгивания
                List<Vector2Int> jumpOverStepsOnBoard = new List<Vector2Int>();

                currentPickedSolderLocal = solder;

                activatedSolderPos = currentPos;
                //активаци полей доски для пешек которые не на краях
                if (boardPos.x != 0 && boardPos.x != boardSize - 1 && boardPos.y != boardSize - 1 && boardPos.y != 0)
                {
                    for (int j = boardPos.y + 1; j > boardPos.y - 2; j--)
                    {
                        for (int i = boardPos.x - 1; i < boardPos.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                            if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite)
                                jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                        }
                    }
                }
                //активаци полей доски для пешек которые на краях
                else
                {
                    //активация полей доски для пешек которые не на углах
                    if (boardPos != new Vector2Int(0, 0) && boardPos != new Vector2Int(0, boardSize - 1) && boardPos != new Vector2Int(boardSize - 1, 0) && boardPos != new Vector2Int(boardSize - 1, boardSize - 1))
                    {
                        if (boardPos.y == 0)
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 1; j--)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos.y == boardSize - 1)
                        {
                            for (int j = boardPos.y - 1; j < boardPos.y + 1; j++)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos.x == 0)
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 2; j--)
                            {
                                for (int i = boardPos.x; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos.x == boardSize - 1)
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 2; j--)
                            {
                                for (int i = boardPos.x; i > boardPos.x - 2; i--)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                    }
                    //активация полей доски для пешек которые на углах
                    else
                    {
                        if (boardPos == new Vector2Int(0, 0))
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 1; j--)
                            {
                                for (int i = boardPos.x; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos == new Vector2Int(boardSize - 1, 0))
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 1; j--)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 1; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos == new Vector2Int(0, boardSize - 1))
                        {
                            for (int j = boardPos.y - 1; j < boardPos.y + 1; j++)
                            {
                                for (int i = boardPos.x; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos == new Vector2Int(boardSize - 1, boardSize - 1))
                        {
                            for (int j = boardPos.y - 1; j < boardPos.y + 1; j++)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 1; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && !boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                    }

                }

                //проверка возможности перепрыгивания
                if (crossToggle.isOn || diagToggle.isOn)
                {
                    for (int c = 0; c < jumpOverStepsOnBoard.Count; c++)
                    {
                        if (crossToggle.isOn)
                        {
                            //проверка вертикаль и горизонталь
                            if (jumpOverStepsOnBoard[c].x == boardPos.x && jumpOverStepsOnBoard[c].y < boardPos.y && jumpOverStepsOnBoard[c].y > 0 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y - 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y - 1)].interactable = true;
                            }

                            if (jumpOverStepsOnBoard[c].x == boardPos.x && jumpOverStepsOnBoard[c].y > boardPos.y && jumpOverStepsOnBoard[c].y < boardSize - 1 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y + 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y + 1)].interactable = true;
                            }

                            if (jumpOverStepsOnBoard[c].y == boardPos.y && jumpOverStepsOnBoard[c].x > boardPos.x && jumpOverStepsOnBoard[c].x < boardSize - 1 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y)].interactable = true;
                            }

                            if (jumpOverStepsOnBoard[c].y == boardPos.y && jumpOverStepsOnBoard[c].x < boardPos.x && jumpOverStepsOnBoard[c].x > 0 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y)].interactable = true;
                            }
                        }
                        if (diagToggle.isOn)
                        {
                            //дагональная проверка
                            if (jumpOverStepsOnBoard[c].x > boardPos.x && jumpOverStepsOnBoard[c].y > boardPos.y && jumpOverStepsOnBoard[c].y < boardSize - 1 && jumpOverStepsOnBoard[c].x < boardSize - 1 &&
                            !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y + 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y + 1)].interactable = true;
                            }
                            if (jumpOverStepsOnBoard[c].x < boardPos.x && jumpOverStepsOnBoard[c].y > boardPos.y && jumpOverStepsOnBoard[c].y < boardSize - 1 && jumpOverStepsOnBoard[c].x > 0 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y + 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y + 1)].interactable = true;
                            }
                            if (jumpOverStepsOnBoard[c].x > boardPos.x && jumpOverStepsOnBoard[c].y < boardPos.y && jumpOverStepsOnBoard[c].y > 0 && jumpOverStepsOnBoard[c].x < boardSize - 1 &&
                               !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y - 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y - 1)].interactable = true;
                            }
                            if (jumpOverStepsOnBoard[c].x < boardPos.x && jumpOverStepsOnBoard[c].y < boardPos.y && jumpOverStepsOnBoard[c].y > 0 && jumpOverStepsOnBoard[c].x > 0 &&
                              !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y - 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y - 1)].interactable = true;
                            }
                        }
                    }
                }
            }
            else
            {
                activatedSolderPos = new Vector2(-10000, -10000);
                currentPickedSolderLocal = null;
                currentPickedSolderObject = null;
            }
        }
        else if (!currentPickedSolderObject.isWhite && !nextTurnWhites)
        { //дезактивация всех активных полей доски 
            for (int x = 0; x < boardUnits.Count; x++)
            {
                boardUnits.Values.ToList()[x].interactable = false;
            }

            if (activatedSolderPos != currentPos)
            {
                //для определения возможности перепрыгивания
                List<Vector2Int> jumpOverStepsOnBoard = new List<Vector2Int>();

                currentPickedSolderLocal = solder;

                activatedSolderPos = currentPos;
                //активаци полей доски для пешек которые не на краях
                if (boardPos.x != 0 && boardPos.x != boardSize - 1 && boardPos.y != boardSize - 1 && boardPos.y != 0)
                {
                    for (int j = boardPos.y + 1; j > boardPos.y - 2; j--)
                    {
                        for (int i = boardPos.x - 1; i < boardPos.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                            if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                        }
                    }
                }
                //активаци полей доски для пешек которые на краях
                else
                {
                    //активация полей доски для пешек которые не на углах
                    if (boardPos != new Vector2Int(0, 0) && boardPos != new Vector2Int(0, boardSize - 1) && boardPos != new Vector2Int(boardSize - 1, 0) && boardPos != new Vector2Int(boardSize - 1, boardSize - 1))
                    {
                        if (boardPos.y == 0)
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 1; j--)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos.y == boardSize - 1)
                        {
                            for (int j = boardPos.y - 1; j < boardPos.y + 1; j++)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos.x == 0)
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 2; j--)
                            {
                                for (int i = boardPos.x; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos.x == boardSize - 1)
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 2; j--)
                            {
                                for (int i = boardPos.x; i > boardPos.x - 2; i--)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                    }
                    //активация полей доски для пешек которые на углах
                    else
                    {
                        if (boardPos == new Vector2Int(0, 0))
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 1; j--)
                            {
                                for (int i = boardPos.x; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos == new Vector2Int(boardSize - 1, 0))
                        {
                            for (int j = boardPos.y + 1; j > boardPos.y - 1; j--)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 1; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos == new Vector2Int(0, boardSize - 1))
                        {
                            for (int j = boardPos.y - 1; j < boardPos.y + 1; j++)
                            {
                                for (int i = boardPos.x; i < boardPos.x + 2; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                        else if (boardPos == new Vector2Int(boardSize - 1, boardSize - 1))
                        {
                            for (int j = boardPos.y - 1; j < boardPos.y + 1; j++)
                            {
                                for (int i = boardPos.x - 1; i < boardPos.x + 1; i++)
                                {
                                    if (new Vector2Int(i, j) != boardPos && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j))) boardUnits[new Vector2Int(i, j)].interactable = true;
                                    if (new Vector2Int(i, j) != boardPos && boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && boardSoldersCoords[new Vector2Int(i, j)].isWhite) jumpOverStepsOnBoard.Add(new Vector2Int(i, j));
                                }
                            }
                        }
                    }

                }


                //проверка возможности перепрыгивания
                if (crossToggle.isOn || diagToggle.isOn)
                {
                    for (int c = 0; c < jumpOverStepsOnBoard.Count; c++)
                    {
                        if (crossToggle.isOn)
                        {
                            //проверка вертикаль и горизонталь
                            if (jumpOverStepsOnBoard[c].x == boardPos.x && jumpOverStepsOnBoard[c].y < boardPos.y && jumpOverStepsOnBoard[c].y > 0 &&
                            !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y - 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y - 1)].interactable = true;
                            }

                            if (jumpOverStepsOnBoard[c].x == boardPos.x && jumpOverStepsOnBoard[c].y > boardPos.y && jumpOverStepsOnBoard[c].y < boardSize - 1 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y + 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x, jumpOverStepsOnBoard[c].y + 1)].interactable = true;
                            }

                            if (jumpOverStepsOnBoard[c].y == boardPos.y && jumpOverStepsOnBoard[c].x > boardPos.x && jumpOverStepsOnBoard[c].x < boardSize - 1 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y)].interactable = true;
                            }

                            if (jumpOverStepsOnBoard[c].y == boardPos.y && jumpOverStepsOnBoard[c].x < boardPos.x && jumpOverStepsOnBoard[c].x > 0 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y)].interactable = true;
                            }
                        }

                        if (diagToggle.isOn)
                        {
                            //дагональная проверка
                            if (jumpOverStepsOnBoard[c].x > boardPos.x && jumpOverStepsOnBoard[c].y > boardPos.y && jumpOverStepsOnBoard[c].y < boardSize - 1 && jumpOverStepsOnBoard[c].x < boardSize - 1 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y + 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y + 1)].interactable = true;
                            }
                            if (jumpOverStepsOnBoard[c].x < boardPos.x && jumpOverStepsOnBoard[c].y > boardPos.y && jumpOverStepsOnBoard[c].y < boardSize - 1 && jumpOverStepsOnBoard[c].x > 0 &&
                                !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y + 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y + 1)].interactable = true;
                            }
                            if (jumpOverStepsOnBoard[c].x > boardPos.x && jumpOverStepsOnBoard[c].y < boardPos.y && jumpOverStepsOnBoard[c].y > 0 && jumpOverStepsOnBoard[c].x < boardSize - 1 &&
                               !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y - 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x + 1, jumpOverStepsOnBoard[c].y - 1)].interactable = true;
                            }
                            if (jumpOverStepsOnBoard[c].x < boardPos.x && jumpOverStepsOnBoard[c].y < boardPos.y && jumpOverStepsOnBoard[c].y > 0 && jumpOverStepsOnBoard[c].x > 0 &&
                              !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y - 1)))
                            {
                                boardUnits[new Vector2Int(jumpOverStepsOnBoard[c].x - 1, jumpOverStepsOnBoard[c].y - 1)].interactable = true;
                            }
                        }
                    }
                }
            }
            else
            {
                activatedSolderPos = new Vector2(-10000, -10000);
                currentPickedSolderLocal = null;
                currentPickedSolderObject = null;
            }
        }
        else
        {
            activatedSolderPos = new Vector2(-10000, -10000);
            currentPickedSolderLocal = null;
            currentPickedSolderObject = null;
            //дезактивация всех активных полей доски 
            for (int x = 0; x < boardUnits.Count; x++)
            {
                boardUnits.Values.ToList()[x].interactable = false;
            }
        }

    }

    //нажатие на поле доски для того чтобы сделать ход
    public void makeAStep(Vector2 unitsScenePos) {
        Vector2Int unitsBoardPos = sceneCoordinates[unitsScenePos];

        //дезактивация всех активных полей доски 
        for (int x = 0; x < boardUnits.Count; x++)
        {
            boardUnits.Values.ToList()[x].interactable = false;
        }
        //переназначение параметров пешки в коллекции и перемещение ее на выбранную игроковм позицию
        boardSoldersCoords.Remove(currentPickedSolderObject.solderCoords);
        currentPickedSolderObject.solderCoords = unitsBoardPos;
        boardSoldersCoords.Add(unitsBoardPos, currentPickedSolderObject);
        currentPickedSolderLocal.transform.localPosition = unitsScenePos;

        currentPickedSolderLocal = null;
        currentPickedSolderObject = null;

        //переход хода
        if (nextTurnWhites) nextTurnWhites = false;
        else nextTurnWhites = true;

        ifSomeoneWon();

        //ход компютера если настройка включена и игрок уже не выиграл
        if (gameAgainsCPU.isOn && !resultsPanel.activeInHierarchy) CPUStepsCtrlr();
    }

    //контроллер ходов компьютера
    private void CPUStepsCtrlr()
    {
        List<Vector2Int> CPUSoldersCoords = new List<Vector2Int>();

        List<Vector2Int> possibleDiagonalMove = new List<Vector2Int>();
        List<Vector2Int> possibleOtherMove = new List<Vector2Int>();
        List<Vector2Int> possibleMinorDiagonalMove = new List<Vector2Int>();
        List<Vector2Int> possibleMinorOtherMove = new List<Vector2Int>();
        List<Vector2Int> badMove = new List<Vector2Int>();
        Solder randomSolderToMove = new Solder();

        //сначала выбор пешек которые еще не находятся на выигрышных позициях
        for (int i = 0; i < boardSoldersCoords.Count; i++)
        {
            if (!boardSoldersCoords.Values.ToList()[i].isPlayers)
            {
                CPUSoldersCoords.Add(boardSoldersCoords.Values.ToList()[i].solderCoords);
            }
        }

        randomSolderToMove = boardSoldersCoords[CPUSoldersCoords[nextSolderOfCPU]];

        //выбор полей доски для пешек которые не на краях 
        if (randomSolderToMove.solderCoords.x != 0 && randomSolderToMove.solderCoords.x != boardSize - 1 && randomSolderToMove.solderCoords.y != boardSize - 1 && randomSolderToMove.solderCoords.y != 0)
        {
            //приоритетные ходы
            for (int j = randomSolderToMove.solderCoords.y - 1; j < randomSolderToMove.solderCoords.y + 1; j++)
            {
                for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                {
                    if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                    {
                        if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                        else possibleOtherMove.Add(new Vector2Int(i, j));
                    }
                }
            }
        }
        else
        {
            //выбор полей доски для пешек которые не на углах
            if (randomSolderToMove.solderCoords != new Vector2Int(0, 0) && randomSolderToMove.solderCoords != new Vector2Int(0, boardSize - 1) && randomSolderToMove.solderCoords != new Vector2Int(boardSize - 1, 0)
                && randomSolderToMove.solderCoords != new Vector2Int(boardSize - 1, boardSize - 1))
            {
                if (randomSolderToMove.solderCoords.y == 0)
                {
                    for (int j = randomSolderToMove.solderCoords.y; j > randomSolderToMove.solderCoords.y - 1; j--)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
                else if (randomSolderToMove.solderCoords.y == boardSize - 1)
                {
                    for (int j = randomSolderToMove.solderCoords.y - 1; j < randomSolderToMove.solderCoords.y + 1; j++)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
                else if (randomSolderToMove.solderCoords.x == 0)
                {
                    for (int j = randomSolderToMove.solderCoords.y; j > randomSolderToMove.solderCoords.y - 2; j--)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
                else if (randomSolderToMove.solderCoords.x == boardSize - 1)
                {
                    for (int j = randomSolderToMove.solderCoords.y; j > randomSolderToMove.solderCoords.y - 2; j--)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i > randomSolderToMove.solderCoords.x - 1; i--)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
            }
            //выбор полей доски для пешек которые на углах
            else
            {
                if (randomSolderToMove.solderCoords == new Vector2Int(0, 0))
                {
                    for (int j = randomSolderToMove.solderCoords.y; j > randomSolderToMove.solderCoords.y - 1; j--)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
                else if (randomSolderToMove.solderCoords == new Vector2Int(0, boardSize - 1))
                {
                    for (int j = randomSolderToMove.solderCoords.y - 1; j < randomSolderToMove.solderCoords.y + 1; j++)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
                else if (randomSolderToMove.solderCoords == new Vector2Int(boardSize - 1, boardSize - 1))
                {
                    for (int j = randomSolderToMove.solderCoords.y - 1; j < randomSolderToMove.solderCoords.y + 1; j++)
                    {
                        for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 1; i++)
                        {
                            if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                            {
                                if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleDiagonalMove.Add(new Vector2Int(i, j));
                                else possibleOtherMove.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                }
            }

        }


        //выбор полей доски для пешек которые не на углах и не на краях для выбора неприоритетного хода (ход назад по диагонали или вверх или в лево)
        if (randomSolderToMove.solderCoords.x != 0 && randomSolderToMove.solderCoords.x != boardSize - 1 && randomSolderToMove.solderCoords.y != boardSize - 1 && randomSolderToMove.solderCoords.y != 0)
        {
            //неприоритетные ходы 
            for (int j = randomSolderToMove.solderCoords.y - 1; j < randomSolderToMove.solderCoords.y + 2; j++)
            {
                for (int i = randomSolderToMove.solderCoords.x - 1; i < randomSolderToMove.solderCoords.x + 2; i++)
                {
                    //тут отдельно исключается еще ход диагональю назад как худший ход
                    if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !possibleDiagonalMove.Contains(new Vector2Int(i, j)) && !possibleOtherMove.Contains(new Vector2Int(i, j))
                        && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)) && ((new Vector2Int(i, j) - randomSolderToMove.solderCoords) != new Vector2Int(-1, 1)))
                    {
                        if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleMinorDiagonalMove.Add(new Vector2Int(i, j));
                        else possibleMinorOtherMove.Add(new Vector2Int(i, j));
                    }
                }
            }
        }

        //выбор полей доски для пешек которые не на углах но на краях для выбора неприоритетного хода (ход назад по диагонали или вверх)
        if (randomSolderToMove.solderCoords != new Vector2Int(0, 0) && randomSolderToMove.solderCoords != new Vector2Int(0, boardSize - 1) && randomSolderToMove.solderCoords != new Vector2Int(boardSize - 1, 0)
            && randomSolderToMove.solderCoords != new Vector2Int(boardSize - 1, boardSize - 1))
        {
            if (randomSolderToMove.solderCoords.y == 0)
            {
                for (int j = randomSolderToMove.solderCoords.y + 1; j > randomSolderToMove.solderCoords.y; j--)
                {
                    for (int i = randomSolderToMove.solderCoords.x; i < randomSolderToMove.solderCoords.x + 2; i++)
                    {
                        if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                        {
                            if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleMinorDiagonalMove.Add(new Vector2Int(i, j));
                            else possibleMinorOtherMove.Add(new Vector2Int(i, j));
                        }
                    }
                }
            }
            else if (randomSolderToMove.solderCoords.x == boardSize - 1)
            {
                for (int j = randomSolderToMove.solderCoords.y; j > randomSolderToMove.solderCoords.y - 2; j--)
                {
                    for (int i = randomSolderToMove.solderCoords.x - 1; i > randomSolderToMove.solderCoords.x - 2; i--)
                    {
                        if (new Vector2Int(i, j) != randomSolderToMove.solderCoords && !boardSoldersCoords.Keys.ToList().Contains(new Vector2Int(i, j)))
                        {
                            if (i != randomSolderToMove.solderCoords.x && j != randomSolderToMove.solderCoords.y) possibleMinorDiagonalMove.Add(new Vector2Int(i, j));
                            else possibleMinorOtherMove.Add(new Vector2Int(i, j));
                        }
                    }
                }
            }
        }

        //осуществление приоритетного хода компьютера (диагональ вниз)
        if (!allPriorityStepsChecked) {
            if (possibleDiagonalMove.Count > 0)
            { //переназначение параметров пешки в коллекции и перемещение ее на выбранную игроковм позицию
                boardSoldersCoords.Remove(randomSolderToMove.solderCoords);
                randomSolderToMove.solderCoords = possibleDiagonalMove[UnityEngine.Random.Range(0, possibleDiagonalMove.Count)];
                boardSoldersCoords.Add(randomSolderToMove.solderCoords, randomSolderToMove);
                randomSolderToMove.solderGameObject.transform.localPosition = sceneCoordinatesInverse[randomSolderToMove.solderCoords];

                //переход хода
                if (nextTurnWhites) nextTurnWhites = false;
                else nextTurnWhites = true;

                ifSomeoneWon();

                nextSolderOfCPU = 0;
            }
            else
            {
                nextSolderOfCPU++;
                if (nextSolderOfCPU == soldersQnt)
                {
                    nextSolderOfCPU = 0;
                    allPriorityStepsChecked = true;
                    CPUStepsCtrlr();
                }
                else CPUStepsCtrlr();
            }
        }
        //осуществление менее приоритетного хода компьютера, вправо или вниз. 
        else if (!allLessPriorityStepsChecked) {
            if (possibleOtherMove.Count > 0)
            {
                //сначала проверка нет ли выигрышного хода среди неприоритетных ходов, обчыно это ход по диагонали
                for (int i = 0; i < possibleMinorDiagonalMove.Count; i++)
                {
                    if (boardCoordsForWinDown.Contains(possibleMinorDiagonalMove[i]) && !boardCoordsForWinDown.Contains(randomSolderToMove.solderCoords))
                    { //переназначение параметров пешки в коллекции и перемещение ее на выбранную игроковм позицию
                        boardSoldersCoords.Remove(randomSolderToMove.solderCoords);
                        randomSolderToMove.solderCoords = possibleMinorDiagonalMove[i];
                        boardSoldersCoords.Add(randomSolderToMove.solderCoords, randomSolderToMove);
                        randomSolderToMove.solderGameObject.transform.localPosition = sceneCoordinatesInverse[randomSolderToMove.solderCoords];

                        //переход хода
                        if (nextTurnWhites) nextTurnWhites = false;
                        else nextTurnWhites = true;

                        ifSomeoneWon();

                        //ресет проверки выгодного хода по всем пешкам
                        nextSolderOfCPU = 0;
                        allPriorityStepsChecked = false;

                        return;
                    }
                }

                //переназначение параметров пешки в коллекции и перемещение ее на выбранную игроковм позицию
                boardSoldersCoords.Remove(randomSolderToMove.solderCoords);
                randomSolderToMove.solderCoords = possibleOtherMove[UnityEngine.Random.Range(0, possibleOtherMove.Count)];
                boardSoldersCoords.Add(randomSolderToMove.solderCoords, randomSolderToMove);
                randomSolderToMove.solderGameObject.transform.localPosition = sceneCoordinatesInverse[randomSolderToMove.solderCoords];

                //переход хода
                if (nextTurnWhites) nextTurnWhites = false;
                else nextTurnWhites = true;

                ifSomeoneWon();

                //ресет проверки выгодного хода по всем пешкам
                nextSolderOfCPU = 0;
                allPriorityStepsChecked = false;
            }
            else
            {
                nextSolderOfCPU++;
                if (nextSolderOfCPU == soldersQnt)
                {
                    nextSolderOfCPU = 0;
                    allLessPriorityStepsChecked = true;
                    CPUStepsCtrlr();
                }
                else CPUStepsCtrlr();
            }
        }
        //осуществление неприоритетного хода компьютера, в основном при безвыходной ситуации
        else
        {
            if (possibleMinorDiagonalMove.Count > 0 || possibleMinorOtherMove.Count > 0)
            {
                if (possibleMinorDiagonalMove.Count > 0)
                { //переназначение параметров пешки в коллекции и перемещение ее на выбранную игроковм позицию
                    boardSoldersCoords.Remove(randomSolderToMove.solderCoords);
                    randomSolderToMove.solderCoords = possibleMinorDiagonalMove[UnityEngine.Random.Range(0, possibleMinorDiagonalMove.Count)];
                    boardSoldersCoords.Add(randomSolderToMove.solderCoords, randomSolderToMove);
                    randomSolderToMove.solderGameObject.transform.localPosition = sceneCoordinatesInverse[randomSolderToMove.solderCoords];

                    //переход хода
                    if (nextTurnWhites) nextTurnWhites = false;
                    else nextTurnWhites = true;

                    ifSomeoneWon();
                }
                else
                {
                    //переназначение параметров пешки в коллекции и перемещение ее на выбранную игроковм позицию
                    boardSoldersCoords.Remove(randomSolderToMove.solderCoords);
                    randomSolderToMove.solderCoords = possibleMinorOtherMove[UnityEngine.Random.Range(0, possibleMinorOtherMove.Count)];
                    boardSoldersCoords.Add(randomSolderToMove.solderCoords, randomSolderToMove);
                    randomSolderToMove.solderGameObject.transform.localPosition = sceneCoordinatesInverse[randomSolderToMove.solderCoords];

                    //переход хода
                    if (nextTurnWhites) nextTurnWhites = false;
                    else nextTurnWhites = true;

                    ifSomeoneWon();
                }
                nextSolderOfCPU = 0;
                allPriorityStepsChecked = false;
                allLessPriorityStepsChecked = false;
            }
            //выбор другой пешки если у текущей нет хода
            else
            {
                if (nextSolderOfCPU < soldersQnt - 1) nextSolderOfCPU++;
                else nextSolderOfCPU = 0;
                CPUStepsCtrlr();
            }
        }


    }

    //проверка победы одного из участников
    private void ifSomeoneWon() {
        if (playerPlaysForWhite) {
            int winWhiteCount = 0;
            int winBlackCount = 0;
            for (int i = 0; i < boardCoordsForWinUp.Count; i++) {
                if (boardSoldersCoords.Keys.ToList().Contains(boardCoordsForWinUp[i]) && boardSoldersCoords[boardCoordsForWinUp[i]].isWhite) {
                    winWhiteCount++;
                }
            }
            if (winWhiteCount == soldersQnt)
            {
                //Debug.Log("WhiteWon");
                whiteResults.text = "ПОБЕДА";
                whiteResults.color = Color.green;
                blackResults.text = "ПОРАЖЕНИЕ";
                blackResults.color = Color.red;
                for (int i = 0; i < solderButtons.Count; i++) {
                    solderButtons[i].interactable = false;
                }
                resultsPanel.SetActive(true);
            }
            for (int i = 0; i < boardCoordsForWinDown.Count; i++)
            {
                if (boardSoldersCoords.Keys.ToList().Contains(boardCoordsForWinDown[i]) && !boardSoldersCoords[boardCoordsForWinDown[i]].isWhite)
                {
                    winBlackCount++;
                }
            }
            if (winBlackCount == soldersQnt)
            {
                //Debug.Log("BlackWon");
                whiteResults.text = "ПОРАЖЕНИЕ";
                whiteResults.color = Color.red;
                blackResults.text = "ПОБЕДА";
                blackResults.color = Color.green;
                for (int i = 0; i < solderButtons.Count; i++)
                {
                    solderButtons[i].interactable = false;
                }
                resultsPanel.SetActive(true);
            }
        }
        else
        {
            int winWhiteCount = 0;
            int winBlackCount = 0;
            for (int i = 0; i < boardCoordsForWinUp.Count; i++)
            {
                if (boardSoldersCoords.Keys.ToList().Contains(boardCoordsForWinUp[i]) && !boardSoldersCoords[boardCoordsForWinUp[i]].isWhite)
                {
                    winBlackCount++;
                }
            }
            if (winBlackCount == soldersQnt)
            {
                //Debug.Log("BlackWon");
                whiteResults.text = "ПОРАЖЕНИЕ";
                whiteResults.color = Color.red;
                blackResults.text = "ПОБЕДА";
                blackResults.color = Color.green;
                for (int i = 0; i < solderButtons.Count; i++)
                {
                    solderButtons[i].interactable = false;
                }
                resultsPanel.SetActive(true);
            }
            for (int i = 0; i < boardCoordsForWinDown.Count; i++)
            {
                if (boardSoldersCoords.Keys.ToList().Contains(boardCoordsForWinDown[i]) && boardSoldersCoords[boardCoordsForWinDown[i]].isWhite)
                {
                    winWhiteCount++;
                }
            }
            if (winWhiteCount == soldersQnt)
            {
                //Debug.Log("WhiteWon");
                whiteResults.text = "ПОБЕДА";
                whiteResults.color = Color.green;
                blackResults.text = "ПОРАЖЕНИЕ";
                blackResults.color = Color.red;

                for (int i = 0; i < solderButtons.Count; i++)
                {
                    solderButtons[i].interactable = false;
                }
                resultsPanel.SetActive(true);
            }
        }
    }

    //закрыть игру
    public void closeTheGame() {
        Application.Quit();
    }
}
 