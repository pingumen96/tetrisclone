using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {
    public int[] position = new int[2];
	// Use this for initialization
	void Start () {
        gameObject.name = GetInstanceID().ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void MoveDown() {
        transform.Translate(new Vector2(0.0f, -1.0f), Space.World);
    }

    private void OnDestroy() {
        /* da qua distruggere il pezzo se questo è l'ultimo cubo rimasto */
        /*if(GetComponentInParent<Transform>().GetComponentsInChildren<Transform>().GetLength(0) == 1) {
            Destroy(GetComponentInParent<Transform>().gameObject);
        }*/
        GetComponentInParent<Piece>().OnCubeDestroyed();
    }
}
