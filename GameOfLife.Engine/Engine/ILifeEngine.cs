using System;


namespace TAlex.GameOfLife.Engine
{
    public interface ILifeEngine
    {
        byte this[int x, int y] { get; set; }
        int Population { get; }

        bool NextGeneration();
    }
}
