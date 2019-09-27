using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    //public Vector2 address = new Vector2 ();
    public int tileX;
    public int tileY;

    public List<Node> currentPath = null;


    private void Update ()
    {
        if ( currentPath != null )
        {
            int currentIndex = 0;
            while ( currentIndex < currentPath.Count -1 )
            {
                Vector3 start = currentPath[currentIndex].GetVector3 (-1);
                Vector3 end = currentPath[currentIndex+1].GetVector3 ( -1 );

                Debug.DrawLine (start, end, Color.grey);
                currentIndex++;
            }
        }
    }

}
