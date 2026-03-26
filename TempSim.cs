using System;
using UnityEngine;

class Program {
    static void Main() {
        Console.WriteLine("Simulation started!");
        // We just test if we can access RiteManager.
        var rm = new RiteManager();
        rm.BaseRitePoints = 10;
        Console.WriteLine("Rite Points: " + rm.RitePoints);
    }
}
