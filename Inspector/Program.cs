using Microsoft.Playwright;

var appProfile = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "TimeTracker", "ff-playwright");
Directory.CreateDirectory(appProfile);

Console.WriteLine("Apertura Firefox Playwright...");
var playwright = await Playwright.CreateAsync();
var browser = await playwright.Firefox.LaunchPersistentContextAsync(appProfile, new()
{
    Headless = false,
    ViewportSize = null,
});

var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();
await page.GotoAsync("https://intranet.isolutions.it/Ore/Ore_Dettaglio");

Console.WriteLine("\nFirefox aperto.");
Console.WriteLine("Se viene chiesto il login, autenticati nel browser.");
Console.WriteLine("Una volta che sei sulla pagina Ore_Dettaglio, premi ENTER qui...");
Console.ReadLine();

Console.WriteLine("URL attuale: " + page.Url);

var inputs = await page.QuerySelectorAllAsync("input:not([type=hidden]), select, textarea");
Console.WriteLine($"\n=== INPUT FIELDS ({inputs.Count}) ===");
foreach (var el in inputs)
{
    var id   = await el.GetAttributeAsync("id") ?? "-";
    var name = await el.GetAttributeAsync("name") ?? "-";
    var type = await el.GetAttributeAsync("type") ?? await el.EvaluateAsync<string>("e => e.tagName.toLowerCase()");
    var ph   = await el.GetAttributeAsync("placeholder") ?? "-";
    var cls  = (await el.GetAttributeAsync("class") ?? "").Split(' ')[0];
    Console.WriteLine($"  [{type,-10}] id={id,-40} name={name,-25} placeholder={ph,-20} class={cls}");
}

var labels = await page.QuerySelectorAllAsync("label, th");
Console.WriteLine($"\n=== LABELS ({labels.Count}) ===");
foreach (var lbl in labels)
{
    var txt = (await lbl.InnerTextAsync()).Trim().Replace("\n", " ");
    var forAttr = await lbl.GetAttributeAsync("for") ?? "-";
    if (txt.Length > 0 && txt.Length < 80)
        Console.WriteLine($"  for={forAttr,-35} text={txt}");
}

var buttons = await page.QuerySelectorAllAsync("button, input[type=submit], input[type=button]");
Console.WriteLine($"\n=== BUTTONS ({buttons.Count}) ===");
foreach (var btn in buttons)
{
    var id  = await btn.GetAttributeAsync("id") ?? "-";
    var txt = (await btn.InnerTextAsync()).Trim().Replace("\n", " ");
    var val = await btn.GetAttributeAsync("value") ?? "-";
    Console.WriteLine($"  id={id,-40} value={val,-20} text={txt}");
}

// Also get full page HTML for deeper inspection
var html = await page.ContentAsync();
var htmlPath = Path.Combine(Path.GetTempPath(), "ore_dettaglio.html");
File.WriteAllText(htmlPath, html);
Console.WriteLine($"\nHTML salvato in: {htmlPath}");

Console.WriteLine("\n=== PREMI ENTER PER CHIUDERE ===");
Console.ReadLine();
await browser.CloseAsync();
