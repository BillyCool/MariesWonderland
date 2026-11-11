using MudBlazor;

namespace MariesWonderland.Web;

/// <summary>
/// The admin tool's MudBlazor theme: marieswonderland.com's palette (cream on black, its light mode dark brown on stone), one
/// monospaced font and square corners. The rest of the look (panels, nav, tables, tabs) is in <c>wwwroot/css/admin.css</c>.
/// </summary>
public static class AdminTheme
{
    private const string Cream = "#e6e2cf";
    private const string Brown = "#2c2624";
    private const string Black = "#101010";
    private const string Stone = "#ebe8e4";

    private static readonly string[] Mono = ["IBM Plex Mono", "ui-monospace", "Cascadia Mono", "Consolas", "monospace"];

    public static MudTheme Create() => new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = Cream,
            PrimaryContrastText = Black,
            Secondary = "#bdb8a6",
            SecondaryContrastText = Black,
            Tertiary = Brown,
            Info = "#8fa8c8",
            InfoContrastText = Black,
            Success = "#9cc39a",
            SuccessContrastText = Black,
            Warning = "#d9b45a",
            WarningContrastText = Black,
            Error = "#e08b7a",
            ErrorContrastText = Black,
            Black = Black,
            Background = Black,
            BackgroundGray = "#1b1b1a",
            Surface = "#1c1c1b",
            AppbarBackground = Black,
            AppbarText = Cream,
            DrawerBackground = Black,
            DrawerText = Cream,
            DrawerIcon = Cream,
            TextPrimary = Cream,
            TextSecondary = "rgba(230,226,207,0.68)",
            TextDisabled = "rgba(230,226,207,0.32)",
            ActionDefault = Cream,
            ActionDisabled = "rgba(230,226,207,0.3)",
            ActionDisabledBackground = "rgba(230,226,207,0.1)",
            LinesDefault = "rgba(230,226,207,0.22)",
            LinesInputs = "rgba(230,226,207,0.55)",
            TableLines = "rgba(230,226,207,0.22)",
            TableHover = "rgba(230,226,207,0.07)",
            TableStriped = "rgba(230,226,207,0.03)",
            Divider = "rgba(230,226,207,0.22)",
            DividerLight = "rgba(230,226,207,0.1)",
            OverlayDark = "rgba(8,8,8,0.8)",
        },
        PaletteLight = new PaletteLight
        {
            Primary = Brown,
            PrimaryContrastText = Cream,
            Secondary = "#5d5650",
            SecondaryContrastText = Cream,
            Tertiary = Brown,
            Info = "#3f5f86",
            Success = "#3f6b3a",
            Warning = "#8a6516",
            Error = "#9a3a2f",
            Background = Stone,
            BackgroundGray = "#dfdcd7",
            Surface = "#d8d5d1",
            AppbarBackground = Stone,
            AppbarText = Brown,
            DrawerBackground = Stone,
            DrawerText = Brown,
            DrawerIcon = Brown,
            TextPrimary = Brown,
            TextSecondary = "rgba(44,38,36,0.7)",
            ActionDefault = Brown,
            LinesDefault = "rgba(44,38,36,0.22)",
            LinesInputs = "rgba(44,38,36,0.55)",
            TableLines = "rgba(44,38,36,0.22)",
            TableHover = "rgba(44,38,36,0.06)",
            Divider = "rgba(44,38,36,0.22)",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = Mono, FontSize = ".875rem", LetterSpacing = ".01em" },
            H4 = new H4Typography { FontFamily = Mono, FontSize = "1.5rem", FontWeight = "600", LetterSpacing = ".1em", TextTransform = "uppercase", LineHeight = "1.2" },
            H5 = new H5Typography { FontFamily = Mono, FontSize = "1.35rem", FontWeight = "500", LetterSpacing = ".02em" },
            H6 = new H6Typography { FontFamily = Mono, FontSize = ".82rem", FontWeight = "600", LetterSpacing = ".12em", TextTransform = "uppercase" },
            Subtitle1 = new Subtitle1Typography { FontFamily = Mono },
            Subtitle2 = new Subtitle2Typography { FontFamily = Mono },
            Body1 = new Body1Typography { FontFamily = Mono, FontSize = ".9rem" },
            Body2 = new Body2Typography { FontFamily = Mono, FontSize = ".84rem" },
            Button = new ButtonTypography { FontFamily = Mono, FontSize = ".74rem", FontWeight = "600", LetterSpacing = ".14em", TextTransform = "uppercase" },
            Caption = new CaptionTypography { FontFamily = Mono, FontSize = ".72rem", LetterSpacing = ".04em" },
            Overline = new OverlineTypography { FontFamily = Mono, FontSize = ".66rem", LetterSpacing = ".16em" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "0px",
            AppbarHeight = "56px",
            DrawerWidthLeft = "232px",
        },
    };
}
