using Windows.UI;
using Microsoft.UI;

namespace WinUI.Resources;

/// <summary>
/// Centralized color definitions for the application.
/// Used in C# code where XAML resources are not available.
/// </summary>
public static class AppColors
{
    // Primary Brand Colors
    public static Color PrimaryOrange => AppResourceLookup.GetColor("PrimaryOrange", ColorHelper.FromArgb(0xFF, 0xFF, 0x6B, 0x35));
    public static Color PrimaryOrangeDark => AppResourceLookup.GetColor("PrimaryOrangeDark", ColorHelper.FromArgb(0xFF, 0xE5, 0x5A, 0x1F));
    public static Color PrimaryOrangeDarker => AppResourceLookup.GetColor("PrimaryOrangeDarker", ColorHelper.FromArgb(0xFF, 0xCC, 0x4D, 0x1A));
    public static Color PrimaryOrangeLight => AppResourceLookup.GetColor("PrimaryOrangeLight", ColorHelper.FromArgb(0xFF, 0xFF, 0xB4, 0x99));
    public static Color OrangeFocus => AppResourceLookup.GetColor("OrangeFocus", ColorHelper.FromArgb(0xFF, 0xFF, 0x7A, 0x30));
    public static Color OrangeLightVariant => AppResourceLookup.GetColor("OrangeLightVariant", ColorHelper.FromArgb(0xFF, 0xFF, 0x8C, 0x61));
    public static Color OrangePeachLight => AppResourceLookup.GetColor("OrangePeachLight", ColorHelper.FromArgb(0xFF, 0xFF, 0xF8, 0xF0));

    // Neutral Colors
    public static Color Transparent => AppResourceLookup.GetColor("TransparentColor", ColorHelper.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
    public static Color White => AppResourceLookup.GetColor("White", Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
    public static Color Black => AppResourceLookup.GetColor("Black", Color.FromArgb(0xFF, 0x1F, 0x1F, 0x1F));
    public static Color LightGray => AppResourceLookup.GetColor("LightGray", Color.FromArgb(0xFF, 0xD8, 0xD8, 0xD8));
    public static Color VeryLightGray => AppResourceLookup.GetColor("VeryLightGray", Color.FromArgb(0xFF, 0xFA, 0xFA, 0xFA));
    public static Color AlmostWhiteGray => AppResourceLookup.GetColor("AlmostWhiteGray", Color.FromArgb(0xFF, 0xF3, 0xF4, 0xF6));
    public static Color LighterGray => AppResourceLookup.GetColor("LighterGray", Color.FromArgb(0xFF, 0xE5, 0xE7, 0xEB));
    public static Color MediumGray => AppResourceLookup.GetColor("MediumGray", Color.FromArgb(0xFF, 0xB3, 0xB3, 0xB3));
    public static Color PlaceholderGray => AppResourceLookup.GetColor("PlaceholderGray", Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
    public static Color DarkGray => AppResourceLookup.GetColor("DarkGray", Color.FromArgb(0xFF, 0x55, 0x55, 0x55));
    public static Color SecondaryGray => AppResourceLookup.GetColor("SecondaryGray", Color.FromArgb(0xFF, 0xA0, 0xAE, 0xC0));

    // Title Bar Colors
    public static Color TitleBarColor => AppResourceLookup.GetColor("TitleBarColor", ColorHelper.FromArgb(0xFF, 0x2D, 0x37, 0x48));
    public static Color TitleBarButton => AppResourceLookup.GetColor("TitleBarButtonColor", ColorHelper.FromArgb(0xFF, 0x2D, 0x37, 0x48));
    public static Color TitleBarButtonHover => AppResourceLookup.GetColor("TitleBarButtonHover", ColorHelper.FromArgb(0x22, 0x2D, 0x37, 0x48));
    public static Color TitleBarButtonPressed => AppResourceLookup.GetColor("TitleBarButtonPressed", ColorHelper.FromArgb(0x44, 0x2D, 0x37, 0x48));

    // Status Colors
    public static Color ErrorRed => AppResourceLookup.GetColor("ErrorRed", Color.FromArgb(0xFF, 0xDC, 0x26, 0x26));
    public static Color ErrorRedLight => AppResourceLookup.GetColor("ErrorRedLight", Color.FromArgb(0xFF, 0xFE, 0xE2, 0xE2));
    public static Color SuccessGreen => AppResourceLookup.GetColor("SuccessGreen", Color.FromArgb(0xFF, 0x10, 0xB9, 0x81));
    public static Color SuccessGreenLight => AppResourceLookup.GetColor("SuccessGreenLight", Color.FromArgb(0xFF, 0xD1, 0xFA, 0xE5));
    public static Color WarningAmber => AppResourceLookup.GetColor("WarningAmber", Color.FromArgb(0xFF, 0xF5, 0x9E, 0x0B));
    public static Color WarningAmberLight => AppResourceLookup.GetColor("WarningAmberLight", Color.FromArgb(0xFF, 0xFE, 0xF3, 0xC7));
    public static Color InfoBlue => AppResourceLookup.GetColor("InfoBlue", Color.FromArgb(0xFF, 0x3B, 0x82, 0xF6));
    public static Color InfoBlueLight => AppResourceLookup.GetColor("InfoBlueLight", Color.FromArgb(0xFF, 0xDB, 0xEA, 0xFE));
    public static Color DashboardTrendPositive => AppResourceLookup.GetColor("DashboardTrendPositive", SuccessGreen);
    public static Color DashboardTrendNegative => AppResourceLookup.GetColor("DashboardTrendNegative", ErrorRed);
    public static Color DashboardStatCardPositiveTrendBackground => AppResourceLookup.GetColor("DashboardStatCardPositiveTrendBackground", SuccessGreenLight);
    public static Color DashboardStatCardNegativeTrendBackground => AppResourceLookup.GetColor("DashboardStatCardNegativeTrendBackground", ErrorRedLight);
    public static Color SettingsLogoutButtonBackground => AppResourceLookup.GetColor("SettingsLogoutButtonBackground", Color.FromArgb(0xFF, 0xF8, 0x71, 0x71));
    public static Color SettingsLogoutButtonBackgroundHover => AppResourceLookup.GetColor("SettingsLogoutButtonBackgroundHover", Color.FromArgb(0xFF, 0xDC, 0x26, 0x26));

    // Gradient Colors
    public static Color GradientStop1 => AppResourceLookup.GetColor("GradientStop1", Color.FromArgb(0xFF, 0xFF, 0x6B, 0x35));
    public static Color GradientStop2 => AppResourceLookup.GetColor("GradientStop2", Color.FromArgb(0xFF, 0xFF, 0xE5, 0xD9));
}
