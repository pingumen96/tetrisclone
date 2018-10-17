using System;
using UnityEngine;

public class Piece : MonoBehaviour {
    public enum TetrominoType { I = 0, J, L, O, S, T, Z };
    public static short COLLISION_MATRIX_SIZE = 4;
    private System.Collections.Generic.List<Cube> cubes = new System.Collections.Generic.List<Cube>();
    private byte remainingCubes = 0;
    /*private static int[,,] collisionMatrices = new int[,,] {{{0, 0, 0, 0}, {0, 0, 0, 0}, {1, 1, 1, 1}, {0, 0, 0, 0}}, // I
                                                              { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 1, 1, 1}, {0, 0, 0, 1}}, // J
                                                              { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 1, 1, 1}, {0, 1, 0, 0}}, // L
                                                              { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 1, 1, 0}, {0, 1, 1, 0}}, // O
                                                              { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 1, 1}, {0, 1, 1, 0}}, // S
                                                              { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 1, 1, 1}, {0, 0, 1, 0}}, // T
                                                              { {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 1, 1, 0}, {0, 0, 1, 1}}};*/ // Z
    public TetrominoType tetrominoType;
    public enum MovementDirection { LEFT = 0, RIGHT, DOWN };
    public enum RotationDirection { CLOCKWISE, COUNTER_CLOCKWISE, NONE };
    public int[,] collisionMatrix = new int[COLLISION_MATRIX_SIZE, COLLISION_MATRIX_SIZE];

	// Use this for initialization
	void Start () {
        GetComponentsInChildren(cubes);

        foreach(Cube cube in cubes) {
            remainingCubes++;
            collisionMatrix[cube.position[0], cube.position[1]] = cube.GetInstanceID();
        }
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    public void OnCubeDestroyed() {
        remainingCubes--;

        if(remainingCubes == 0) {
            Destroy(gameObject);
        }
    }

    public void Move(MovementDirection movementDirection) {
        switch(movementDirection) {
            case MovementDirection.LEFT:
                transform.Translate(new Vector2(-1.0f, 0.0f), Space.World);
                break;
            case MovementDirection.RIGHT:
                transform.Translate(new Vector2(1.0f, 0.0f), Space.World);
                break;
            case MovementDirection.DOWN:
                transform.Translate(new Vector2(0.0f, -1.0f), Space.World);
                break;
        }
    }

    public RotationDirection GetNextRotationDirection() {
        if(tetrominoType == TetrominoType.I || tetrominoType == TetrominoType.S || tetrominoType == TetrominoType.Z) {
            if(transform.rotation.eulerAngles.z == 0.0f) {
                return RotationDirection.CLOCKWISE;
            } else {
                return RotationDirection.COUNTER_CLOCKWISE;
            }
        } else if(tetrominoType != TetrominoType.O) {
            return RotationDirection.CLOCKWISE;
        } else {
            return RotationDirection.NONE;
        }
    }

    public void Rotate() {
        switch(GetNextRotationDirection()) {
            case RotationDirection.CLOCKWISE:
                transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), -90.0f);
                break;
            case RotationDirection.COUNTER_CLOCKWISE:
                transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), 90.0f);
                break;
        }
    }

    public void RotateCollisionMatrix(RotationDirection direction) {
        if(direction == RotationDirection.NONE)
            return;

        // fare riferimento al file pezzi.xlsx presente nella cartella Docs per comprendere il codice che segue
        int[] R = new int[4];
        int[] Q = new int[4];
        int[] R2 = new int[2];

        R[0] = collisionMatrix[1, 2];
        R[1] = collisionMatrix[2, 3];
        R[2] = collisionMatrix[3, 2];
        R[3] = collisionMatrix[2, 1];
        Q[0] = collisionMatrix[1, 1];
        Q[1] = collisionMatrix[1, 3];
        Q[2] = collisionMatrix[3, 3];
        Q[3] = collisionMatrix[3, 1];
        R2[0] = collisionMatrix[0, 2];
        R2[1] = collisionMatrix[2, 0];

        if(direction == RotationDirection.COUNTER_CLOCKWISE) {
            collisionMatrix[0, 2] = 0;
            collisionMatrix[2, 0] = R2[0];

            collisionMatrix[2, 1] = R[0];
            collisionMatrix[3, 2] = R[3];
            collisionMatrix[2, 3] = R[2];
            collisionMatrix[1, 2] = R[1];

            collisionMatrix[3, 1] = Q[0];
            collisionMatrix[3, 3] = Q[3];
            collisionMatrix[1, 3] = Q[2];
            collisionMatrix[1, 1] = Q[1];
        } else if(direction == RotationDirection.CLOCKWISE) {
            collisionMatrix[0, 2] = R2[1];
            collisionMatrix[2, 0] = 0;

            collisionMatrix[1, 2] = R[3];
            collisionMatrix[2, 3] = R[0];
            collisionMatrix[3, 2] = R[1];
            collisionMatrix[2, 1] = R[2];

            collisionMatrix[1, 1] = Q[3];
            collisionMatrix[1, 3] = Q[0];
            collisionMatrix[3, 3] = Q[1];
            collisionMatrix[3, 1] = Q[2];
        }
    }
}
