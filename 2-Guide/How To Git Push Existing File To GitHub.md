
navigate inside folder containing index.html

### the first time:

```bash
git init
git remote add origin https://github.com/an-alch3mist/Loop-2025.git
git branch -M main
git add .
git commit -m "Initial commit: index.html"
git push -u origin main
```

### future update:
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


### the fetch + clean:

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