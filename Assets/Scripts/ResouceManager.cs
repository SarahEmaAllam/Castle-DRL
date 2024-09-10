using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceManager
{
    // Global Bricks variable shared by all agents
    public static int Bricks = 100;
    
    // Method to subtract Bricks (returns true if successful, false if not enough Bricks)
    public static bool SubtractBricks(int amount)
    {
        if (Bricks >= amount)
        {
            Bricks -= amount;
            return true;
        }
        else
        {
            Debug.LogError("Not enough Bricks to perform the action.");
            return false;
        }
    }
}
