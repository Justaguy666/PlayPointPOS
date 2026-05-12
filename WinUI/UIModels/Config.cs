namespace WinUI.UIModels;

public class ConfigModel
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 3000;
    public bool RememberMe { get; set; }
}
