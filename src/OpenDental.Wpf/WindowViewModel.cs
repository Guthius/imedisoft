using System;

namespace Imedisoft;

public abstract class WindowViewModel : ViewModel
{
    public Action<bool?>? Close { get; set; }
}