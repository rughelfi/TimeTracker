using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

var playwright = await Playwright.CreateAsync();
var profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeTracker", "ff-inspector");
Directory.CreateDirectory(profilePath);

var browser = await playwright.Firefox.LaunchPersistentContextAsync(profilePath, new()
{
    Headless = false,
    ViewportSize = null,
});

var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();
await page.GotoAsync("https://intranet.isolutions.it/Ore/Ore_Dettaglio");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

Console.WriteLine("=== PAGE LOADED ===");
Console.WriteLine("URL: " + page.Url);

// Dump all input fields, selects, textareas
var inputs = await page.QuerySelectorAllAsync("input, select, textarea, [contenteditable]");
Console.WriteLine($"\nFound {inputs.Count} input elements:\n");
foreach (var el in inputs)
{
    var id = await el.GetAttributeAsync("id") ?? "";
    var name = await el.GetAttributeAsync("name") ?? "";
    var type = await el.GetAttributeAsync("type") ?? await el.EvaluateAsync<string>("e => e.tagName.toLowerCase()");
    var placeholder = await el.GetAttributeAsync("placeholder") ?? "";
    var cls = await el.GetAttributeAsync("class") ?? "";
    Console.WriteLine($"  tag={type,-12} id={id,-40} name={name,-30} placeholder={placeholder,-20} class={cls}");
}

// Also dump buttons
var buttons = await page.QuerySelectorAllAsync("button, input[type=submit], input[type=button], a[class*='btn']");
Console.WriteLine($"\nFound {buttons.Count} buttons/links:\n");
foreach (var btn in buttons)
{
    var id = await btn.GetAttributeAsync("id") ?? "";
    var txt = (await btn.InnerTextAsync()).Trim().Replace("\n"," ");
    var cls = await btn.GetAttributeAsync("class") ?? "";
    Console.WriteLine($"  id={id,-40} text={txt,-30} class={cls}");
}

Console.WriteLine("\n=== DONE — press Enter to close browser ===");
Console.ReadLine();
await browser.CloseAsync();
