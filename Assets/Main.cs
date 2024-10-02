using System.Collections;
using System.Collections.Generic;
using UnityEngine;
static public class Constant
{
    public const int MAX = 10;
    public const int STAT = 4;
    public const int HIGH = 5;
    public const int PHP = 3000;
    public const int PAM = 5;
    public const int PDMG = 30;
    public const int PSPD = 2;
}
public class Main : MonoBehaviour
{
    private int[] player = new int[Constant.STAT + 1] { 0, Constant.PHP, Constant.PAM, Constant.PDMG, Constant.PSPD };
    int[,] monster1 = new int[Constant.MAX, Constant.STAT + 1];
    int[,] monster2 = new int[Constant.MAX, Constant.STAT + 1];
    public GameObject enemy;
    public int EHP;
    public int EAM;
    public int EDMG;
    public int ESPD;
    int[,] map;
    public int[] esArray;
    private bool isoutput = false;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
        PlayerCheck();

    }

    // Update is called once per frame
    public void Update()
    {
        Refresh();
        Do();
        Select();
        //Cross();
        Mutation();
        for (int i = 0; i < Constant.MAX; i++)
        {
            if (isoutput == false)
            {
                Debug.Log(monster1[i, 0]);
                if (((int)Mathf.Abs(player[0] - monster1[i, 0])) == 0)
                {
                    Debug.Log("!!");
                    /* for (int s = 0; s < Constant.MAX; s += 5)
                     {
                         for (int j = s; j < s + 5; j++)
                         {
                             for (int k = 1; k <= Constant.STAT; k++)
                             {

                             }
                         }
                     }*/
                     for(int x = 0; x < 5; x++)
                    {
                        Debug.Log(monster1[i, x]);
                    }
                    EHP = monster1[i, 1];
                    EAM = monster1[i, 2];
                    EDMG = monster1[i, 3];
                    ESPD = monster1[i, 4];
                    MapGenerator po = GameObject.Find("Map").GetComponent<MapGenerator>();
                    int nodeX = po.worldmap.GetLength(0);
                    int nodeY = po.worldmap.GetLength(1);
                    int randomX, randomY;
                    while (true)
                    {
                        randomX = Random.Range(0, nodeX);
                        randomY = Random.Range(0, nodeY);
                        if (po.worldmap[randomX, randomY] == 0)
                        {
                            break;
                        }
                    }
                    Vector3 vr = new Vector3(randomX - (po.width / 2), 0, randomY - (po.height / 2));
                    Instantiate(enemy);
                    enemy.transform.position = vr;
                    EnemySTAT es = GetComponent<EnemySTAT>();
                    esArray = new int[5];
                    for (int a = 0; a < Constant.STAT + 1; a++)
                    {
                        esArray[a] = monster1[i, a];
                        Debug.Log(esArray[a]);
                    }
                    GameObject.FindWithTag("Enemy").GetComponent<EnemySTAT>().EnemyStat(esArray);
                    isoutput = true;
                }
            }
        }
    }
    public void trans(int [,] map,int width,int height)
    {

    }
    void Reset()
    {
        for (int i = 0; i < Constant.MAX; i++)
        {
            monster1[i, 1] = Random.Range(100, 1000 + (player[1] / 10));
            monster1[i, 2] = Random.Range(0, 15) + player[2];
            monster1[i, 3] = Random.Range(10, 150 + player[3]);
            monster1[i, 4] = Random.Range(1, 5);
        }
    }
    void PlayerCheck()
    {
        player[0] = (int)(player[1] + player[1] * player[2] / 100) / player[3] * player[4];//(hp+hp*armor/100)/damage*speed;

    }
    void Refresh()
    {
        for (int i = 0; i < Constant.MAX; i++)
        {
            monster1[i, 0] = 0;
            monster2[i, 0] = 0;
        }
    }
    void Do()
    {
        int c = 0;
        int index1 = 0, index2 = 0;
        for (int i = 0; i < Constant.MAX; i++)
        {
            for (int j = 1; j <= Constant.STAT; j++)
            {
                monster2[i, j] = monster1[i, j];
            }
            monster2[i, 0] = (int)(monster2[i, 1] + monster2[i, 1] * monster2[i, 2] / 100) / monster2[i, 3] * monster2[i, 4];
        }
    }
    int Battle(int n)
    {
        int spd = 0;
        int[] v_player = new int[Constant.STAT + 1];
        int[] v_monster = new int[Constant.STAT + 1];
        for (int i = 0; i <= Constant.STAT; i++)
        {
            v_player[i] = player[i];
            v_monster[i] = monster2[n, i];
        }
        if (v_player[4] < v_monster[4])
        {
            spd = v_monster[4];
            while (true)
            {
                for (int i = 0; i < spd; i++)
                {
                    if (v_player[i] > 0)
                    {
                        v_player[1] = v_player[1] - (v_monster[3] - (v_monster[3] * v_player[2] / 100));
                        if (v_player[4] > i)
                        {
                            v_monster[1] = v_monster[1] - (v_player[3] - (v_player[3] * v_monster[2] / 100));
                            if (v_monster[1] <= 0)
                                return v_player[1];
                        }
                    }
                    else
                    {
                        return v_player[1];
                    }
                }
            }
        }
        else
        {
            spd = v_player[4];
            while (true)
            {
                for (int i = 0; i < spd; i++)
                {
                    v_monster[1] = v_monster[1] - (v_player[3] - (v_player[3] * v_monster[2] / 100));
                    if (v_monster[1] < 0)
                        return v_player[1];
                    if (v_monster[4] > i)
                    {
                        v_player[1] = v_player[1] - (v_player[3] - (v_player[3] * v_monster[2] / 100));
                        if (v_player[1] <= 0)
                            return v_player[1];
                    }
                }
            }
        }
        return v_player[1];
    }
    void Select()
    {
        int[,] temp = new int[Constant.HIGH, Constant.STAT + 1];
        int[] p_hp = new int[Constant.MAX];
        int[] q_hp = new int[Constant.MAX];
        int t_hp;
        int[] k_hp = new int[Constant.MAX];
        for (int i = 0; i < Constant.MAX; i++)
        {
            p_hp[i] = Battle(i);
            q_hp[i] = monster2[i, 0];
            k_hp[i] = i;
        }
        for (int i = 0; i < Constant.MAX; i++)
        {
            for (int j = 0; j < Constant.MAX-i-1; j++)
            {
                if ((int)Mathf.Abs(player[0] - q_hp[i]) > (int)Mathf.Abs(player[0] - q_hp[j + 1]))
                {
                    t_hp = q_hp[i];
                    q_hp[i] = q_hp[j + 1];
                    q_hp[j + 1] = t_hp;

                    t_hp = k_hp[j];
                    k_hp[j] = k_hp[j + 1];
                    k_hp[j + 1] = t_hp;
                }
            }
        }
        for (int i = 0; i < Constant.HIGH; i++)
        {
            for (int j = 0; j <= Constant.STAT; j++)
            {
                temp[i, j] = monster2[k_hp[i], j];
            }
        }
        int c = 0;
        int index1 = 0, index2 = 0;
        for (int i = 0; i < Constant.HIGH; i++)
        {
            for (int j = 0; j <= Constant.STAT; j++)
            {
                monster1[i, j] = temp[i, j];
            }
            monster1[i, 0] = (int)(monster1[i, 1] + monster1[i, 1] * monster1[i, 2] / 100) / monster1[i, 3] * monster1[i, 4];
        }
        for (int i = Constant.HIGH; i < Constant.MAX; i++)
        {
            index1 = Random.Range(0, Constant.HIGH - 1);
            index2 = Random.Range(0, Constant.HIGH - 1);
            for (int j = 1; j <= Constant.STAT; j++)
            {
                c = Random.Range(0, 1);
                if (c == 0)
                {
                    monster1[i, j] = temp[index1, j];
                }
                else
                {
                    monster1[i, j] = temp[index2, j];
                }
            }
            monster1[i, 0] = (int)(monster1[i, 1] + monster1[i, 1] * monster1[i, 2] / 100) / monster1[i, 3] * monster1[i, 4];

        }
    }
    void Cross()
    {
        int change = 0;
        int index1 = 0, index2 = 0;
        /*for (int i = 0; i < 1; i++)
        {
            for (int j = 0; i <= Constant.STAT; j++)
            {
                monster1[i, j] = monster1[i, j];
            }
        }*/
        for (int i = 1; i < Constant.MAX; i = i + 2)
        {
            for (int j = 1; j <= Constant.STAT / 2; j++)
            {
                monster1[i, j] = monster1[i + 1, j];
            }
            for (int j = Constant.STAT / 2; j <= Constant.STAT; j++)
            {
                monster1[i, j] = monster1[i + 1, j];
            }
            monster1[i, 0] = (int)(monster1[i, 1] + monster1[i, 1] * monster1[i, 2] / 100) / monster1[i, 3] * monster1[i, 4];
        }
    }
    void Mutation()
    {
        int mut = 0;
        for (int i = 0; i < Constant.MAX; i++)
        {
            for (int j = 0; j <= Constant.STAT; j++)
            {
                mut = Random.Range(0, 100);
                if (mut == 1)
                {
                    switch (j)
                    {
                        case 1:
                            monster1[i, j] = Random.Range(100, 1000 + (player[1] / 10));
                            break;
                        case 2:
                            monster1[i, j] = Random.Range(0, 15) + player[2];
                            break;
                        case 3:
                            monster1[i, j] = Random.Range(10, 150 + player[3]);
                            break;
                        case 4:
                            monster1[i, j] = Random.Range(1, 5);
                            break;
                    }
                }
            }
            monster1[i, 0] = (int)(monster1[i, 1] + monster1[i, 1] * monster1[i, 2] / 100) / monster1[i, 3] * monster1[i, 4];
        }
    }
}
