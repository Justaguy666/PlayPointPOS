using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.Converters;

public partial class IconConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, string> _pathDataCache = new();
    private static readonly Lazy<string?> _iconsDirectory = new(ResolveIconsDirectory);

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not IconState state)
            return null;

        string style = state.AlwaysFilled || state.IsSelected || state.IsHovered ? "filled" : "regular";
        string fileName = GetIconBaseName(state.Kind);
        string cacheKey = $"{fileName}|{state.Size}|{style}";

        if (_pathDataCache.TryGetValue(cacheKey, out var cachedPathData))
        {
            return CreateGeometry(cachedPathData);
        }

        string? iconsDirectory = _iconsDirectory.Value;
        if (string.IsNullOrWhiteSpace(iconsDirectory))
            return null;

        string? fullPath = ResolveIconPath(iconsDirectory, fileName, state.Size, style);
        if (fullPath is null)
            return null;

        try
        {
            string svgContent = File.ReadAllText(fullPath);
            var matches = Regex.Matches(
                svgContent,
                @"<path[^>]*\sd=(?:""([^""]*)""|'([^']*)')",
                RegexOptions.IgnoreCase
            );

            if (matches.Count == 0)
                return null;

            StringBuilder combinedData = new();
            foreach (Match match in matches)
            {
                string pathData = match.Groups[1].Success
                    ? match.Groups[1].Value
                    : match.Groups[2].Value;

                if (string.IsNullOrWhiteSpace(pathData))
                    continue;

                if (combinedData.Length > 0)
                    combinedData.Append(' ');

                combinedData.Append(pathData);
            }

            if (combinedData.Length == 0)
                return null;

            string combinedPathData = combinedData.ToString();
            _pathDataCache[cacheKey] = combinedPathData;
            return CreateGeometry(combinedPathData);
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

    private static string GetIconBaseName(IconKind kind) => kind switch
    {
        IconKind.Config => "ic_fluent_plug_disconnected",
        IconKind.Register => "ic_fluent_person_board_add",
        IconKind.Login => "ic_fluent_person_board",
        IconKind.Forget => "ic_fluent_scan_person",
        IconKind.Password => "ic_fluent_key_multiple",
        IconKind.Logout => "ic_fluent_inprivate_account",
        IconKind.Exit => "ic_fluent_arrow_exit",
        IconKind.Close => "ic_fluent_dismiss",
        IconKind.Dashboard => "ic_fluent_clover",
        IconKind.Area => "ic_fluent_table_multiple",
        IconKind.Game => "ic_fluent_chess",
        IconKind.Product => "ic_fluent_food_apple",
        IconKind.Food => "ic_fluent_food_egg",
        IconKind.Drink => "ic_fluent_drink_to_go",
        IconKind.Member => "ic_fluent_people",
        IconKind.History => "ic_fluent_history",
        IconKind.Settings => "ic_fluent_settings",
        IconKind.Dice => "ic_fluent_games",
        IconKind.Customer => "ic_fluent_person_heart",
        IconKind.Dinner => "ic_fluent_food",
        IconKind.BagOfCoins => "ic_fluent_coin_multiple",
        IconKind.Trending => "ic_fluent_fire",
        IconKind.Export => "ic_fluent_arrow_export_up",
        IconKind.Chart => "ic_fluent_chart_multiple",
        IconKind.Stat => "ic_fluent_book_number",
        IconKind.Target => "ic_fluent_target_arrow",
        IconKind.Table => "ic_fluent_desk_multiple",
        IconKind.Room => "ic_fluent_conference_room",
        IconKind.Calendar => "ic_fluent_calendar",
        IconKind.Clock => "ic_fluent_clock",
        IconKind.Up => "ic_fluent_arrow_up",
        IconKind.Down => "ic_fluent_arrow_down",
        IconKind.Add => "ic_fluent_add_square_multiple",
        IconKind.Delete => "ic_fluent_delete",
        IconKind.Grid => "ic_fluent_grid",
        IconKind.List => "ic_fluent_list_bar",
        IconKind.Update => "ic_fluent_edit",
        IconKind.Search => "ic_fluent_search",
        IconKind.Filter => "ic_fluent_options",
        IconKind.Paint => "ic_fluent_color",
        IconKind.Success => "ic_fluent_checkmark_square",
        IconKind.Warning => "ic_fluent_warning",
        IconKind.Error => "ic_fluent_error_circle",
        IconKind.Info => "ic_fluent_book_information",
        IconKind.More => "ic_fluent_more_circle",
        _ => "ic_fluent_question_circle_circle",
    };

    private static string? ResolveIconsDirectory()
    {
        string? packageRoot = TryGetPackageInstalledLocation();
        if (!string.IsNullOrWhiteSpace(packageRoot))
        {
            string? packagedDirectory = FindIconsDirectory(packageRoot);
            if (!string.IsNullOrWhiteSpace(packagedDirectory))
                return packagedDirectory;
        }

        return FindIconsDirectory(AppContext.BaseDirectory);
    }

    private static string? TryGetPackageInstalledLocation()
    {
        try
        {
            return Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        }
        catch
        {
            return null;
        }
    }

    private static string? FindIconsDirectory(string? startDirectory)
    {
        const string basePath = "Assets\\Icons";

        string? currentDirectory = startDirectory;
        while (!string.IsNullOrWhiteSpace(currentDirectory))
        {
            string candidate = Path.Combine(currentDirectory, basePath);
            if (Directory.Exists(candidate))
                return candidate;

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }

        return null;
    }

    private static string? ResolveIconPath(string iconsDirectory, string fileName, int size, string style)
    {
        string candidate = $"{fileName}_{size}_{style}.svg";

        string fullPath = Path.Combine(iconsDirectory, candidate);
        if (File.Exists(fullPath))
            return fullPath;

        string plainPath = Path.Combine(iconsDirectory, $"{fileName}.svg");
        if (File.Exists(plainPath))
            return plainPath;

        return null;
    }

    private static Geometry? CreateGeometry(string pathData)
    {
        if (string.IsNullOrWhiteSpace(pathData))
            return null;

        try
        {
            return (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData);
        }
        catch
        {
            return null;
        }
    }
}
