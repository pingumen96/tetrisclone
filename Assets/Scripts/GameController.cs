using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    public static GameController instance = null;

    private static readonly short GRID_COLS = 10, GRID_ROWS = 24;
    private readonly short[] STARTING_COLLISION_MATRIX_ORIGIN = { -2, 4 };
    private readonly Vector3 STARTING_POINT = new Vector3(0.5f, 23.5f, 0.0f);

    private int[,] grid = new int[GRID_ROWS + 1, GRID_COLS + 2];
    private short[] fallingPieceMatrixOrigin = new short[2];
    private Piece fallingPieceComponent;
    private Piece.TetrominoType currentPieceType;
    private Piece.TetrominoType nextPieceType;

    private bool gameOver = true, paused = false;
    private byte level = 0;
    private uint score = 0;
    private uint lines = 0;

    public GameObject[] pieceTemplates;
    public UIController uiController;
    public Transform piecesParent;

    /* REGOLE DI GIOCO, VANNO LASCIATE HARDCODED */
    private const int MAX_LEVEL = 10;
    private uint[] POINTS_PER_LINE = new uint[5] {0, 100, 250, 400, 550 };
    private float[] SPEED_DIVIDERS = new float[MAX_LEVEL + 1] {1.0f, 1.2f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f, 6.0f};
    //private int[] LEVEL_THRESHOLD = new int[MAX_LEVEL] { 1000, 5000, 15000, 30000, 50000, 80000, 120000, 170000, 240000, 350000 };
    private float fallingTime;

    // cagate per gestire i movimenti come vogliamo noi, con la "responsività" che vogliamo
    private byte lateralMovementWait = 0;
    private bool continuousLateralMovement = false;
    private byte downMovementWait = 0;
    // numero di frame che deve passare prima di fare un nuovo movimento laterale tenendo premuto
    private byte LATERAL_MOVEMENT_SYNC = 12;
    private byte DOWN_MOVEMENT_SYNC = 5;

    private uint CalculateScore(byte deletedLines) {
        return (uint)(POINTS_PER_LINE[deletedLines] * (1.0f + level / 10.0f));
    }

    void Start() {
        if(instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(gameObject);
        }

        // si limitano fps per non andare troppo veloci, se si va più lenti non succede nulla
        Application.targetFrameRate = 30;
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    public void StartGame() {
        // eliminare pezzi già esistenti se ci sono
        foreach(Transform piece in piecesParent.GetComponentInChildren<Transform>()) {
            Destroy(piece.gameObject);
        }

        // reset grid
        for(byte i = 0; i < GRID_ROWS; i++) {
            // si preparano i "confini" laterali della griglia
            grid[i, 0] = 1;
            grid[i, GRID_COLS + 1] = 1;

            for(byte j = 1; j <= GRID_COLS; j++) {
                grid[i, j] = 0;
            }
        }

        // si prepara il "fondo"
        for(byte i = 0; i < GRID_COLS + 2; i++) {
            grid[GRID_ROWS, i] = 1;
        }


        // reset valori
        
        score = lines = 0;
        level = 0;
		uiController.UpdateHUD(score, level, lines);
        uiController.SetGameMusicSpeed(1.0f);

        fallingTime = 1.0f / SPEED_DIVIDERS[level];

        uiController.PlayGameMusic();

        gameOver = false;
		paused = false;

        nextPieceType = GeneratePieceType();
        CreateNewPiece();
    }

    public void QuitGame() {
        Application.Quit();
    }


    private void Update() {
        Debug.Log(continuousLateralMovement);
        if(!gameOver && !paused) {
            // gestione input
            // fare refactoring su questa parte

            // se non si stanno premendo i tasti si "resettano" le variabili sugli spostamenti continui
            if(!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) {
                continuousLateralMovement = false;
                lateralMovementWait = 0;
            }

            // se ci si è spostati precedentemente e i tasti sono ancora premuti
            if(continuousLateralMovement) {
                // si aggiorna "attesa" dei frame giusti
                lateralMovementWait++;

                if(lateralMovementWait % LATERAL_MOVEMENT_SYNC == 0) {
                    if(Input.GetKey(KeyCode.LeftArrow) && !WillCollide(Piece.MovementDirection.LEFT)) {
                        fallingPieceMatrixOrigin[1]--;
                        fallingPieceComponent.Move(Piece.MovementDirection.LEFT);
                    } else if(Input.GetKey(KeyCode.RightArrow) && !WillCollide(Piece.MovementDirection.RIGHT)) {
                        fallingPieceMatrixOrigin[1]++;
                        fallingPieceComponent.Move(Piece.MovementDirection.RIGHT);
                    }
                }
                
            }

            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                continuousLateralMovement = true;
                lateralMovementWait = 0;
                if(!WillCollide(Piece.MovementDirection.LEFT)) {
                    fallingPieceMatrixOrigin[1]--;
                    fallingPieceComponent.Move(Piece.MovementDirection.LEFT);
                }
            } else if(Input.GetKeyDown(KeyCode.RightArrow)) {
                continuousLateralMovement = true;
                lateralMovementWait = 0;
                if(!WillCollide(Piece.MovementDirection.RIGHT)) {
                    fallingPieceMatrixOrigin[1]++;
                    fallingPieceComponent.Move(Piece.MovementDirection.RIGHT);
                }
            }
            

            // istruzioni che possono eseguirsi sempre
            if(Input.GetKeyDown(KeyCode.UpArrow)) {
                /* rotazione */
                if(!WillCollide(fallingPieceComponent.GetNextRotationDirection())) {
                    fallingPieceComponent.Rotate();
                }
            }

            if(Input.GetKeyDown(KeyCode.Escape)) {
                if(paused) {
                    ResumeGame();
                } else {
                    PauseGame();
                }
            }

            if(Input.GetKey(KeyCode.DownArrow)) {
                downMovementWait++;
                if(downMovementWait % DOWN_MOVEMENT_SYNC == 0) {
                    ManageMovements();
                }
                
            }
        }
    }

    void PauseGame() {
        paused = true;
        CancelInvoke("ManageMovements");
        uiController.ShowPauseMenu(true);
    }

    public void ResumeGame() {
        paused = false;
        InvokeRepeating("ManageMovements", 0.3f, fallingTime);
        uiController.ShowPauseMenu(false);
    }

    private Piece.TetrominoType GeneratePieceType() {
        return (Piece.TetrominoType)Random.Range(0, (int)Piece.TetrominoType.Z + 1);
    }

    void CreateNewPiece() {
        currentPieceType = nextPieceType;
        nextPieceType = GeneratePieceType();
        uiController.SetNextPiece(nextPieceType);
        GameObject fallingPiece = Instantiate(pieceTemplates[(int)currentPieceType], STARTING_POINT, Quaternion.identity, piecesParent);
        fallingPiece.SetActive(true);
        fallingPieceComponent = fallingPiece.GetComponent<Piece>();
        fallingPieceMatrixOrigin[0] = STARTING_COLLISION_MATRIX_ORIGIN[0];
        fallingPieceMatrixOrigin[1] = STARTING_COLLISION_MATRIX_ORIGIN[1];

        InvokeRepeating("ManageMovements", 1.0f, fallingTime);
    }

    private void EndGame() {
        gameOver = true;
        uiController.ShowGameOverMenu(true);
    }

    void ManageMovements() {
        if(!WillCollide(Piece.MovementDirection.DOWN)) {
            fallingPieceMatrixOrigin[0]++;
            fallingPieceComponent.Move(Piece.MovementDirection.DOWN);
        } else {
            // se si sovrappongono pezzi è game over a prescindere
            if(IsColliding(fallingPieceMatrixOrigin[0], fallingPieceMatrixOrigin[1])) {
                EndGame();
            }
            CancelInvoke("ManageMovements");
            UpdateGrid();
            if(!gameOver) {
                CreateNewPiece();
            }
        }
    }

    bool IsColliding(short matrixRow, short matrixCol) {
        for(int i = 0; i < Piece.COLLISION_MATRIX_SIZE; i++) {
            for(int j = 0; j < Piece.COLLISION_MATRIX_SIZE; j++) {
                // verifichiamo che il singolo quadrato si trovi dentro la griglia per controllare le opportune collisioni
                if(matrixRow + i >= 0 && matrixRow + i < GRID_ROWS + 1 && // controllo collisioni su righe
                    matrixCol + j >= 0 && matrixCol + j < GRID_COLS + 2 && // controllo collisioni su colonne
                    fallingPieceComponent.collisionMatrix[i, j] != 0 && grid[matrixRow + i, matrixCol + j] != 0) {
                    return true;
                }
            }
        }
        return false;
    }

    bool WillCollide(Piece.MovementDirection direction) {
        short matrixRow = fallingPieceMatrixOrigin[0], matrixCol = fallingPieceMatrixOrigin[1];

        switch(direction) {
            case Piece.MovementDirection.LEFT:
                matrixCol--;
                break;
            case Piece.MovementDirection.RIGHT:
                matrixCol++;
                break;
            case Piece.MovementDirection.DOWN:
                matrixRow++;
                break;
        }

        return IsColliding(matrixRow, matrixCol);
    }

    bool WillCollide(Piece.RotationDirection direction) {
        int[,] backupCollisionMatrix = new int[Piece.COLLISION_MATRIX_SIZE, Piece.COLLISION_MATRIX_SIZE];

        for(int i = 0; i < Piece.COLLISION_MATRIX_SIZE; i++) {
            for(int j = 0; j < Piece.COLLISION_MATRIX_SIZE; j++) {
                backupCollisionMatrix[i, j] = fallingPieceComponent.collisionMatrix[i, j];
            }
        }

        fallingPieceComponent.RotateCollisionMatrix(direction);
        if(IsColliding(fallingPieceMatrixOrigin[0], fallingPieceMatrixOrigin[1])) {
            /* si rimette la matrice di collisione originale */
            for(int i = 0; i < Piece.COLLISION_MATRIX_SIZE; i++) {
                for(int j = 0; j < Piece.COLLISION_MATRIX_SIZE; j++) {
                    fallingPieceComponent.collisionMatrix[i, j] = backupCollisionMatrix[i, j];
                }
            }
            return true;
        } else {
            return false;
        }
    }

    void UpdateGrid() {
        /* aggiorna la griglia di booleani ad ogni spostamento sulla base della matrice di collisione del pezzo che sta scendendo */
        for(int i = 0; i < Piece.COLLISION_MATRIX_SIZE; i++) {
            for(int j = 0; j < Piece.COLLISION_MATRIX_SIZE; j++) {
                if(fallingPieceComponent.collisionMatrix[i, j] != 0) {
                    if(i + fallingPieceMatrixOrigin[0] >= 0) {
                        grid[i + fallingPieceMatrixOrigin[0], j + fallingPieceMatrixOrigin[1]] = fallingPieceComponent.collisionMatrix[i, j];
                    } else {
                        Debug.Log("BASTA, STOP, FERMA TUTTO");
                        EndGame();
                    }
                }
            }
        }

        byte deletedLines = CheckForDeletion();
        score += CalculateScore(deletedLines);
        lines += deletedLines;

        if(deletedLines > 0) {
            uiController.PlaySoundEffect();
        }

        // si adatta il livello in base allo score
        /*if(level <= MAX_LEVEL && score >= LEVEL_THRESHOLD[level]) {
            fallingTime = 1.0f / SPEED_DIVIDERS[++level];
        }*/
        if(level < MAX_LEVEL && lines / 10 != level) {
            LevelUp();
        }
        
        uiController.UpdateHUD(score, level, lines);
    }

    private void LevelUp() {
        level = (byte)(lines / 10);
        fallingTime = 1.0f / SPEED_DIVIDERS[level];
        uiController.SetGameMusicSpeed(1.0f + Mathf.Log((level + 4) / 4.0f));
    }

    private byte CheckForDeletion() {
        bool flag;
        byte countDeleted = 0;
        for(int i = GRID_ROWS - 1; i >= 0; i--) {
            flag = true;
            for(int j = 1; j < GRID_COLS + 1; j++) {
                if(grid[i, j] == 0) flag = false;
            }
            if(flag) {
                // si elimina la riga e si incrementa la i per controllare quella successiva a seguito del shift
                DeleteRow(i++);
                countDeleted++;
            }
        }

        return countDeleted; // servirà per il calcolo del punteggio
    }

    void DeleteRow(int rowIndex) {
        for(int j = 1; j < GRID_COLS + 1; j++) {
            // si elimina il cubo attraverso il suo instanceId
            Destroy(GameObject.Find(grid[rowIndex, j].ToString()));
            for(int i = rowIndex; i > 0; i--) {
                grid[i, j] = grid[i - 1, j];

                // si sposta il cubo superiore verso il basso se esiste
                if(grid[i, j] != 0) {
                    GameObject.Find(grid[i, j].ToString()).GetComponent<Cube>().MoveDown();
                }
            }

            // si crea una nuova riga
            grid[0, j] = 0;
        }
    }
}
