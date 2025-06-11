using Spectre.Console;

string? paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

if (paths != null)
{
	List<string> selectedPaths = new();
	List<Action> actions = new();
	
	while (true)
	{
		var entries = paths.Split(';');
		ShowList(entries);
		var action = ShowMenu();
		switch (action)
		{
			case ActionType.Add:
				var newPath = GetNewPath();
				if (!DoAdd(newPath))
				{
					Console.WriteLine("Failed to add");
					ShowMenu();	
				} 

				actions.Add(new Action(ActionType.Add, newPath));
				ShowList(paths.Split(';'));
				break;
			case ActionType.Edit:
				string path = GetPathToEdit(entries);
				DoEdit(path);	
				actions.Add(new Action(ActionType.Edit, $@"Edited: { path }"));
				Console.WriteLine();
				break;
			case ActionType.Remove:
				// AnsiConsole.Clear();
				selectedPaths = GetSelectedPaths(entries);
				foreach (var p in selectedPaths)
				{
					DoRemove(p);
					actions.Add(new Action(ActionType.Remove, p));
				}
				// AnsiConsole.Clear();	
				ShowList(paths.Split(';'));
				break;
			default:
				Environment.Exit(0);
				AnsiConsole.Clear();
				break;
		}
		
		var selected = AnsiConsole.Prompt(
			new SelectionPrompt<ActionType>()
			.Title("Environment variables have been modified: ")
			.AddChoices(new ActionType[]
			{
				ActionType.Continue, ActionType.View, ActionType.Save
			}));

		switch (selected)	
		{
			case ActionType.View:
				// AnsiConsole.Clear();
				AnsiConsole.Markup("[bold red]Pending Changes:\n[/]");
				ShowChanges(actions);
				AnsiConsole.Write("Press any key to continuee...");
				Console.ReadKey();
				// AnsiConsole.Clear();
				break;
			case ActionType.Save:
				Save();
				break;
			case ActionType.Continue:
			default:
				break;
		}
	}
}

/* *************************** */
/*          METHODS            */
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

string GetNewPath() => AnsiConsole.Prompt(new TextPrompt<string>("Enter new path: "));

bool DoAdd(string path)
{
	if (paths.Contains(path, StringComparison.OrdinalIgnoreCase))
		return false;
		
	paths = String.Concat(paths, path, ';');
	return true;
}

void Save()
{
	Environment.SetEnvironmentVariable("PATH", paths, EnvironmentVariableTarget.User);
}

void DoEdit(string oldPath)
{
	var newPath = GetNewPath();
	AnsiConsole.Markup($"[red]{ oldPath }[/] to [green]{ newPath }[/]");
	Console.ReadKey();
	
	if (!DoAdd(newPath)) Console.WriteLine("Already existing.");
	
	DoRemove(oldPath); 
}

void DoRemove(string path)
{
	var temp = paths.Split(';')
				.Where(p => !p.Equals(path, StringComparison.OrdinalIgnoreCase));
				
	paths = string.Join(';', temp);			
}

void ShowList(string[] paths)
{
	var table = new Table();
	var entries = paths.Where(e => !String.IsNullOrEmpty(e)).ToList();
	table.AddColumn("Id");
	table.AddColumn(new TableColumn(new Markup("[red]Path[/]")));
	for (int i = 1; i < entries.Count(); i++)
	{
		table.AddRow($"{ i }", entries[i]);
	}
		
	AnsiConsole.Write(table);
}

void ShowChanges(List<Action> actions)
{
	var table = new Table();
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

interface IOperation
{
	void ShowList();
	void ShowChanges();
	void Add(string path);
	void Edit(string oldPath, string newPath);
	void Remove(string path);
	void GetPaths();
}

/* *************************** */
/*   CLASSES, RECORDS, ENUMS   */
/*                             */
/* *************************** */

class Operation : IOperation
{
	public void GetPaths()
	{
        throw new NotImplementedException();
	}
	
    public void Add(string path)
    {
        throw new NotImplementedException();
    }

    public void Edit(string oldPath, string newPath)
    {
        throw new NotImplementedException();
    }

    public void Remove(string path)
    {
        throw new NotImplementedException();
    }

    public void ShowChanges()
    {
        throw new NotImplementedException();
    }

    public void ShowList()
    {
        throw new NotImplementedException();
    }
}

record Action(ActionType ActionType, string Path);

enum ActionType
{
	Add,
	Edit,
	Remove,
	Quit,
	Continue,
	Save,
	View
}
