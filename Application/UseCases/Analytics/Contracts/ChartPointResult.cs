using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UseCases.Analytics.Contracts;

public class ChartPointResult
{
    public DateTime Time { get; set; }
    public double Value { get; set; }
}
