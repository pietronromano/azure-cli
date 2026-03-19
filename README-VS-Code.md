# VS Code Tips and Notes
# DATE: 18-03-2026

--- 

## Add Git remote
1. Create empty repository in github
2. Init local git
```bash
git init
git add .
git commit -m "first local commit"
git branch -M main
git remote add origin https://github.com/pietronromano/azcli.git
git push -u origin main
```
---
## MAC: Change zsh prompt to just show working directory
(NOTE this doesn't work on Bash, only Zsh)
PS1='%1~ $'

## VS Code: 
### Install extension Azure CLI Tools

### "Azure CLI: Run File in Terminal"
    - CTRL|CMD+SHIFT+P -> GOTO Preferences: 
    - Open Keyboard Shortcuts -> Terminal -> Azure CLI: Run File in Terminal 
### "Run Selected Text in Active Terminal"
    - CTRL|CMD+SHIFT+P -> GOTO Preferences: 
    - Open Keyboard Shortcuts -> Terminal -> Run Selected Text in Active Terminal

---

# Get environment variables from .env file
***## *NOTE! POSSIBLE ERROR**: 
"An environment file is configured but terminal environment injection is disabled." 

SOLUTION: Enable "python.terminal.useEnvFile" to use environment variables from .env files in terminals.

---

## Prevent VS Code from replacing the active file when opening new files
To prevent VS Code from replacing the active file when opening new files, you need to disable preview mode.
You can change this in your settings:
- Via Settings UI: Open Settings ("CMD+," [command and comma]), search for "enable preview", and uncheck:
  - Workbench › Editor: Enable Preview
  - Workbench › Editor: Enable Preview From Quick Open

Via settings.json: Add these lines:
- "workbench.editor.enablePreview": false,
- "workbench.editor.enablePreviewFromQuickOpen": false

---

## Debugger types in VS Code for .NET
For local build and debug, open the specific source code folder in VS Code (e.g. dotnet-service-bus folder) and ensure you have the right debugger type selected in your launch.json configuration.

There are two debugger types for .NET in VS Code:
- "**dotnet**" - The newer unified debugger (C# Dev Kit extension)
  - Does not support env property in launch.json
  - Uses launchSettings.json for environment variables
  - More integrated with modern .NET tooling
- "**coreclr**" - The legacy debugger (OmniSharp C# extension)
  - Does support env property directly in launch.json
  - More straightforward for setting environment variables
  - Still widely used and supported
  - If you want to set environment variables directly in launch.json, using "**coreclr**" is the better choice.

---

## Duplicate Workspace (Full New Window) 
This is the most common way to get a completely separate window for the same folder, including its own Explorer sidebar and debug sessions. It creates an "Untitled Workspace" which allows you to open the same folder path twice without conflicts.
- Command Palette: Press Ctrl+Shift+P (Windows/Linux) or Cmd+Shift+P (Mac).
- Search: Type "Duplicate Workspace" and select Workspaces: Duplicate As Workspace in New Window.
- Result: A new window opens with the same folder. Note that this technically creates an "Untitled Workspace" to bypass the restriction of opening the exact same folder path twice.

---
