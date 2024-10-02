using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySTAT : MonoBehaviour
{
    public int ST;
    public int EHP;
    public int EAM;
    public int EDMG;
    public int ESPD;
    public void EnemyStat(int[] stat)
    {
        ST = stat[0];
        EHP = stat[1];
        EAM = stat[2];
        EDMG = stat[3];
        ESPD = stat[4];
    }
}
