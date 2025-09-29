
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
git commit -m "Your commit message here"
git push
```

// todo how to init and pull to other directory ?

# Where to git Init
#### for each unity project perform git init in following directory:
```cs
git init at Assets/_/.git/     push to -> SPACE_UTIL repository
git init at Assets/.git/       push to -> #Name repository
```



# Fetch + clean previous:

```bash
# make a backup branch just in case
git branch backup-my-work

# switch to the branch you care about (main in your case)
git switch main

# fetch latest remote refs
git fetch origin

# force local branch to match remote branch exactly
git reset --hard origin/main

# remove untracked files and directories (use -fdx to also remove ignored files)
git clean -fd
```

---
#### Here’s what happens in your sequence:

```bash
git branch backup-my-work
```
- Creates a new branch **pointing to your current commit**.
- This is like a bookmark in Git history.
- It does **not** copy files somewhere; it just saves the commit reference.
- If you later reset `main`, the `backup-my-work` branch still points to your old commit, so you can switch back to it and recover everything.

```bash
git switch main
```
- Just moves you back to the `main` branch.
- After you run `git reset --hard origin/main` and `git clean -fd`, your `main` branch matches the remote, but the `backup-my-work` branch still contains all the old changes (even unpushed commits).
- If you **don’t** push `backup-my-work`, it will only exist in your local machine. If your local repo gets deleted, the backup branch is gone too.

If you want that backup saved on GitHub too, push it explicitly
```bash
git push origin backup-my-work
```



# Clone into a new folder (keeps the `.git` history) — easiest

`git clone` creates a new Git repository for you (it makes the directory, downloads the commits, and creates the `.git` folder).

```bash
git clone https://github.com/an-alch3mist/SPACE_UTIL.git .

# shallow clone (only latest commit)
git clone --depth 1 https://github.com/an-alch3mist/SPACE_UTIL.git /path/to/target-folder
```