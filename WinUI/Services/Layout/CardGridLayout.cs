namespace WinUI.Services.Layout;

public readonly record struct CardGridLayout(
    int Columns,
    double ItemWidth,
    double ItemHeight,
    int PageSize);
