﻿using System;
using UnityEngine;

[Serializable]
public class TileType
{
    public string name;
    public GameObject tileVisualPrefab;

    public int movementCost=1;
}
