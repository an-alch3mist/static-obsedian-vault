Based on your actual folder structure (`smarket`, `schedule-1`, `btycoon`, `minemgl`), here's the exact step-by-step:

---

## Step 1 — Install Git LFS

Download from **git-lfs.com** and run the installer, then in any terminal:

```bash
git lfs install
```

> This is a one-time global setup. You only ever do this once per machine.

---

## Step 2 — Go into your root folder

```bash
cd C:\path\to\your\root-folder
```

If you already have a git repo here:

```bash
git status   # just confirm you're in the right place
```

If not initialised yet:

```bash
git init
git remote add origin https://github.com/yourname/your-repo.git
```

---

## Step 3 — Tell LFS what to track

Based on your file list, run these:

```bash
git lfs track "*.glb"
git lfs track "*.png"
git lfs track "*.json"
git lfs track "*.terrain"
```

> This writes rules into `.gitattributes`. You can open that file in Notepad to verify — it should look like:
> 
> ```
> *.glb filter=lfs diff=lfs merge=lfs -text
> *.png filter=lfs diff=lfs merge=lfs -text
> *.json filter=lfs diff=lfs merge=lfs -text
> ```

---

## Step 4 — Commit `.gitattributes` FIRST (critical)

```bash
git add .gitattributes
git commit -m "Configure Git LFS tracking"
```

> ⚠️ Never skip this. If you add files before committing `.gitattributes`, they go in as regular Git objects, not LFS — and GitHub will reject anything over 100MB.

---

## Step 5 — Bump LFS upload concurrency (speeds things up)

```bash
git config lfs.concurrenttransfers 8
```

> Default is 3. Setting it to 8 means 8 files upload in parallel instead of 3.

---

## Step 6 — Add and push ONE project at a time

Don't `git add .` everything at once — with files this large it'll eat your RAM. Do it per project folder:

### smarket first (has your two 1.8 GB monsters)

```bash
git add smarket/
git commit -m "Add smarket assets"
git push origin main
```

### then schedule-1

```bash
git add schedule-1/
git commit -m "Add schedule-1 assets"
git push origin main
```

### then btycoon

```bash
git add btycoon/
git commit -m "Add btycoon assets"
git push origin main
```

### then minemgl

```bash
git add minemgl/
git commit -m "Add minemgl assets"
git push origin main
```

> Each `git push` uploads LFS files to GitHub's LFS server separately from the Git history. You'll see a progress bar per file.

---

## Step 7 — Verify it worked

```bash
# See all files currently tracked by LFS
git lfs ls-files

# Check a specific file — should show a pointer, NOT binary garbage
cat "smarket/Assets/SceneHierarchyObject/Multiplayer.glb"
```

A correctly LFS-tracked file looks like this when you `cat` it:

```
version https://git-lfs.github.com/spec/v1
oid sha256:4d7a2...
size 2006123456
```

If it shows binary junk, the file wasn't tracked properly before adding.

---

## Step 8 — If a push fails halfway

LFS uploads can time out on large files. Just re-run:

```bash
git lfs push origin main --all
```

This resumes and only uploads what didn't make it — it won't re-upload files already there.

---

## What Your Normal Git Workflow Looks Like After This

Nothing changes day-to-day. You use Git exactly as before:

```bash
git add .
git commit -m "Update assets"
git push origin main
```

LFS handles the big files silently in the background. When someone clones your repo:

```bash
git clone https://github.com/yourname/repo.git
# LFS files download automatically
```

---

## Quick Cheat Sheet

```
SETUP (once)          git lfs install
TRACK TYPE            git lfs track "*.glb"
COMMIT CONFIG         git add .gitattributes && git commit
CHECK TRACKED         git lfs ls-files
PUSH NORMALLY         git add . → git commit → git push
RESUME FAILED PUSH    git lfs push origin main --all
CHECK A FILE          cat yourfile.glb   (should show pointer text)
```

---

## ⚠️ One Thing to Do Before You Start

Go to **github.com → your account → Settings → Billing** and add at least **1 LFS data pack ($5/month)** before pushing. Your top 2 files alone are 3.7 GB — the free 1 GB quota will fail on the very first push.