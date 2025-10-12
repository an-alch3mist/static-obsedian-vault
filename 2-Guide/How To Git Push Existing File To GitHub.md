
navigate inside folder containing index.html
# From Scratch
#### the first time:

```bash
%% do not forget the .gitignore %%
git init
git remote add origin https://github.com/an-alch3mist/TopDownShooter-3D-Game-Alex-.git
git branch -M main
git add .
git commit -m "Initial commit: index.html"
git push -u origin main
```

```bash
# Ignore Assets/_/_secure directory when push is done via git
/_secure/
```
```cs
/* /Assets/_secure/_secure.cs */
public static class _secure
{
	public static string webhook_url = "https://discord.com/api/webhooks/...."
}
```
#### future update:
```bash
git add .
git commit -m "v0.5.2"
git push
```

// todo how to init and pull to other directory ? -> Done
### Where to git Init
#### for each unity project perform git init in following directory:
```cs
git init at Assets/_/.git/     push to -> SPACE_UTIL repository
git init at Assets/.git/       push to -> #Name repository
```

# Git Pull
### Hard reset new changes made in other local directory, linked to same remote repository
```bash
git restore .   # resets ALL tracked files in your working directory to the last committed state
git pull        # fetches and merges the latest changes from origin/main
```

# Git Clone (import the .git remote repository)

### Clone into a new folder (keeps the `.git` history)

`git clone` creates a new Git repository for you (it makes the directory, downloads the commits, and creates the `.git` folder).
```bash
# inside /Assets/_/
git clone https://github.com/an-alch3mist/SPACE_UTIL.git .

# shallow clone (only latest commit)
git clone --depth 1 https://github.com/an-alch3mist/SPACE_UTIL.git /path/to/target-folder
```