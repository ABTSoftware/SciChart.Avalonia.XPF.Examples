using System;

namespace SciChart.Examples.Demo.Lib.Bootstrapper
{
    public interface IBootstrapper
    {
        event EventHandler<EventArgs> WhenInit;
    }
}
