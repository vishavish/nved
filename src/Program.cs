using Spectre.Console;

IEnvService op = new EnvService();
string? paths = op.GetPaths();

if (paths is not null)
{
    List<string> selectedPaths = new();
    List<Ops> actions = new();
    bool isQuit = false;
    ShowList(paths!.Split(';'));

    while (!isQuit)
    {
        var action = ShowMenu();
        switch (action)
        {
            case ActionType.Add:
                var newPath = GetNewPath();
                (string newPaths, bool res) = op.Add(paths, newPath);
                if (!res)
                {
                    AnsiConsole.WriteLine("Failed to add path.");
                    break;
                }
                paths = newPaths;
                actions.Add(new Ops(ActionType.Add, newPath));
                ShowList(paths.Split(';'));
                break;
            case ActionType.Edit:
                string path = GetPathToEdit(paths.Split(';'));
                AnsiConsole.Markup($"[bold green]SELECTED PATH: [[{path}]][/]\n");
                string pathToAdd = GetNewPath();
                AnsiConsole.Markup($"[bold red]{path}[/] -> [bold green]{pathToAdd}[/]\n");
                (string updatedPath, bool result) = op.Add(paths, pathToAdd);
                if (!result)
                {
                    AnsiConsole.WriteLine("Failed to update the path.");
                    break;
                }
                paths = op.Remove(updatedPath, path);
                actions.Add(new Ops(ActionType.Edit, $@"[bold red]{path}[/] -> [bold green]{pathToAdd}[/]"));
                AnsiConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                ShowList(paths.Split(';'));
                break;
            case ActionType.Remove:
                selectedPaths = GetSelectedPaths(paths.Split(';'));
                paths = op.Remove(paths.Split(';'), selectedPaths);
                selectedPaths.ForEach(p => actions.Add(new Ops(ActionType.Remove, p)));
                ShowList(paths.Split(';'));
                break;
            default:
                isQuit = true;
                break;
        }
    }

    if (!op.GetPaths()!.Equals(paths))
    {
        while (true)
        {
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<ActionType>()
                .Title("Environment variables have been modified. ")
                .AddChoices(new ActionType[]
                {
                    ActionType.View, ActionType.Save, ActionType.Quit
                }));

            switch (selected)
            {
                case ActionType.View:
                    AnsiConsole.Markup("[bold red]Pending Changes:\n[/]");
                    ShowChanges(actions);
                    AnsiConsole.Write("Press any key to continue...");
                    Console.ReadKey();
                    break;
                case ActionType.Save:
                    await AnsiConsole.Status()
                        .Spinner(Spinner.Known.Dots)
                        .SpinnerStyle(Style.Parse("green dim"))
                        .StartAsync("Saving changes...", async ctx =>
                        {
                            await Task.Run(() => op.Save(paths));
                        });
                    break;
                case ActionType.Quit:
                    Environment.Exit(0);
                    break;
            }
        }
    }
}

/* *************************** */
/*          FUNCTIONS          */
/*                             */
/* *************************** */

List<string> GetSelectedPaths(string[] paths)
{
    AnsiConsole.Markup("[bold yellow]User PATH Entries:[/]");
    return AnsiConsole.Prompt(
         new MultiSelectionPrompt<string>()
            .Title("Select a user path:")
            .NotRequired()
            .InstructionsText(
                "[grey](Press [blue]<space>[/] to select a path, " +
                "[green]<enter>[/] to accept)[/]")
            .AddChoices(paths.Where(e => !String.IsNullOrEmpty(e)))
    ) ?? new();
}

string GetPathToEdit(string[] paths)
{
    return AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Select action: ")
        .AddChoices(paths.Where(e => !String.IsNullOrEmpty(e))));
}

ActionType ShowMenu()
{
    return AnsiConsole.Prompt(
        new SelectionPrompt<ActionType>()
        .Title("Select action: ")
        .AddChoices(new ActionType[] {
            ActionType.Add, ActionType.Edit, ActionType.Remove, ActionType.Quit
        }));
}

string GetNewPath() => AnsiConsole.Prompt(new TextPrompt<string>("Enter new path: ").AllowEmpty()).Trim();

void ShowList(string[] paths)
{
    var entries = paths.Where(e => !String.IsNullOrEmpty(e)).ToList();
    if (!entries.Any())
    {
        AnsiConsole.Markup("[bold red]No paths available.[/]");
        return;
    }

    var table = new Table();
    table.Expand = true;
    table.AddColumn("Id");
    table.AddColumn(new TableColumn(new Markup("[red]Path[/]")));
    for (int i = 0; i < entries.Count(); i++)
        table.AddRow($"{i + 1}", entries[i]);

    AnsiConsole.Write(table);
}

void ShowChanges(List<Ops> actions)
{
    var table = new Table();
    table.Expand = true;
    table.AddColumn("Action");
    table.AddColumn(new TableColumn(new Markup("[red]Path[/]")));
    for (int i = 0; i < actions.Count(); i++)
        table.AddRow(actions[i].ActionType.ToString(), actions[i].Path);

    AnsiConsole.Write(table);
}

/* *************************** */
/*          INTERFACES         */
/*                             */
/* *************************** */

interface IEnvService
{
    (string, bool) Add(string path, string newPath);
    string Remove(string path, string pathToDelete);
    string Remove(string[] path, List<string> paths);
    string? GetPaths();
    void Save(string paths);
}

/* *************************** */
/*   CLASSES, RECORDS, ENUMS   */
/*                             */
/* *************************** */

class EnvService : IEnvService
{
    public string? GetPaths()      => Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

    public void Save(string paths) => Environment.SetEnvironmentVariable("PATH", paths, EnvironmentVariableTarget.User);

    public (string, bool) Add(string paths, string path)
    {
        if (paths.Contains(path, StringComparison.OrdinalIgnoreCase))
            return ("Path already exists.", false);

        paths = String.Join(';', new string[]{paths, path});
        return (paths, true);
    }

    public string Remove(string paths, string path)
    {
        var temp = paths.Split(';').Where(p => !p.Equals(path, StringComparison.OrdinalIgnoreCase));
        return string.Join(';', temp);
    }

    public string Remove(string[] path, List<string> paths)
    {
        var temp = path.Except(paths);
        return string.Join(';', temp.Select(p => p.ToString()));
    }
}

record Ops(ActionType ActionType, string Path);

enum ActionType
{
    Add,
    Edit,
    Remove,
    Quit,
    Continue,
    View,
    Save
}
