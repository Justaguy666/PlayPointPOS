using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UseCases.Analytics.Contracts.Enums;

public enum ChartRangeType
{
    Today, // last 24 hours
    ThisWeek,
    ThisMonth,
    ThisQuater,
    ThisYear
}
