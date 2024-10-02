using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject player;
    public void Spawn(int [,] map ,int width,int height)
    {
        int nodeX = map.GetLength(0);
        Debug.Log(nodeX);
        int nodeY = map.GetLength(1);
        Debug.Log(nodeY);
        int randomX, randomY;
        while (true)
        {
            randomX = Random.Range(0, nodeX);
            randomY = Random.Range(0, nodeY);
            if (map[randomX, randomY] == 0)
            {
                break;
            }
        }
        Vector3 vr = new Vector3(randomX-(width/2), 0, randomY-(height/2));
        Instantiate(player);
        player.transform.position = vr;
        
    }
}
