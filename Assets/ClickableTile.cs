using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickableTile : MonoBehaviour {

    //public Vector2 tileAddress = new Vector2(0,0);
    public int tileX;
    public int tileY;


    [HideInInspector]
    public TileMap mapObject;

    public enum MAT_TYPE
    {
        NORMAL= 0,
        BLOCKED = 1,
        VISITED = 2,
        CHOSEN = 3
    }

    [SerializeField]
    List<Material> allMaterials;
    private Renderer myRenderer;

    [HideInInspector]
    public Text uiText;

    TileMap myMap;

    private void Start ()
    {
        myMap = FindObjectOfType<TileMap> ();
        myRenderer = GetComponentInChildren<Renderer> ();
        uiText = GetComponentInChildren<Text> ();
        uiText.text = ".";
        //uiText.text = string.Format ( "{0},{1}", tileX, tileY );
    }

    public void SetText (float cost)
    {
        uiText.text = cost < 999 ? string.Format ( "{0:0.#}", cost ) : "Infinity";
    }

    public void SetText ( string str )
    {
        uiText.text = str;
    }

    public void SetTileMat (MAT_TYPE type)
    {
        myRenderer.material = allMaterials[(int) type];
    }

    private void OnMouseUp ()
    {
        //Debug.Log ("click");
        if (myMap.useAStar)
            StartCoroutine ( mapObject.MoveSelectedUnitByAStar ( tileX, tileY ) );
        else
            StartCoroutine ( mapObject.MoveSelectedUnitTo ( tileX, tileY ) );
    }

}
