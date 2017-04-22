using System;

namespace Titan.Bootstrap.Exit
{
    public interface IExitSignal
    {

        event EventHandler Exit;

    }
}