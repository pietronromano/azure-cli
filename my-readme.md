# DATE: 10-07-2025

# Visual Studio
- Install extension Azure CLI Tools 
- "Run Selected Text in Active Terminal"
    - CTRL|CMD+SHIFT+P -> GOTO Preferences: 
    - Open Keyboard Shortcuts -> Terminal -> Run Selected Text in Active Terminal

# MAC: Change prompt to just show working directory
PS1='%1~ $ '

# Add Git remote
## Create empty repository in github

## Init local git
git init
git add .
git commit -m "first local commit"
git branch -M main
git remote add origin https://github.com/pietronromano/azcli.git
git push -u origin main
