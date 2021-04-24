# **Contributing**
A guide for how to contribute to Quaver if you aren't familiar with Git, GitHub, source control, or open-source projects.


## **What you need**
- A GitHub account (https://github.com/Quaver/Quaver)
- A Git client
    - [GitKraken](https://www.gitkraken.com/) is a great multi-platform Git client, supporting Windows, Mac, and Linux.
    - Alternatively, the [Gitbash] (https://git-scm.com/downloads) command line is always an option.
- C# IDE (if you intend on modifying code)
    - Windows only: Microsoft [Visual Studio](https://visualstudio.microsoft.com/downloads/) 2017 or newer


## **GitHub Account Setup**

### **Forking**


The first step for new contributors is to fork the Quaver repository. Forking creates a copy of the repository that belongs to your GitHub account. They are free to make any changes to their repository, having no effect on the Quaver repository. At a later point they may ask to create a pull request to merge their changes to the Quaver repository. This does not put any code on your machine, so the next step is to start using git.

To fork, open the [Quaver repo page](https://github.com/Quaver/Quaver) and press the fork button on the top right of the screen.


### **Git setup**

Git is a source control tool used by many projects (including Quaver), while GitHub is a web interface for a Git repository. Git and GitHub are not the same thing. Git will allow you to manage your copy of the Quaver source code. This guide uses the Git command-line, for beginners it can be difficult, but these commands are consistent among all Git clients.

In the guide whenever you see $YOU, replace it with your GitHub username.


### **Git Configuration**
For GitHub to know who you are when using Git, type these two commands:
```bash
git config --global user.name $YOU
git config --global user.email you@example.com
```

### **Git Cloning**

The `git clone ` command is used to create a copy of a specific repository or branch within a repository.
Git is a distributed version control system. Maximize the advantages of a full repository on your own machine by cloning.
```bash
git clone --recurse-submodules https://github.com/Quaver/Quaver
cd Quaver
```
•   later use ` git push ` to share your branch with the remote repository
•	open a pull request to compare the changes with your collaborators
•	test and deploy as needed from the branch
•	merge into the master branch.

### **Git Remote**

You must configure a remote that points to the upstream repository in Git to sync changes you make in a fork with the original repository. This also allows you to sync changes made in the original repository with the fork.

- List the current configured remote repository for your fork.
```bash
git remote -v
> origin  https://github.com/YOUR_USERNAME/Quaver.git (fetch)
> origin  https://github.com/YOUR_USERNAME/Quaver.git (push)
```

- Specify a new remote upstream repository that will be synced with the fork.
```bash
git remote add upstream https://github.com/Quaver/Quaver.git
```

-Verify the new upstream repository you've specified for your fork.
```bash
git remote -v
> origin  https://github.com/YOUR_USERNAME/Quaver.git (fetch)
> origin  https://github.com/YOUR_USERNAME/Quaver.git (push)
> upstream  https://github.com/Quaver/Quaver.git (fetch)
> upstream  https://github.com/Quaver/Quaver.git (push)
```

# **Compiling**

## **Windows**

- If you'd like to edit or compile the source code using Visual Studio:
•	Open Quaver folder in your local machine using Visual Studio 2017 or newer. Then go to Quaver.API and find the Quaver.API.sln. Then Build → Build Solution (ctrl+Shift+B)./ or double click the Quaver.API.sln to open and build it.

### **Run the app**
•	Press F5 or the Start Debugging button in the Debug Toolbar, located above the code editor. 
•	If you want to run the unit tests suite,
        • Go to Quaver.API.Test- right click on anywhere in code section and click Run ALL Test.










