# Contributing

The Nspec test suite is written in NUnit. The test project is NSpecSpecs. Not to be confused with SampleSpecs which hosts numerous tests written in NSpec, some of which are intended to fail.

To run the NSpec test suite, you can use ncrunch or [Specwatchr](http://nspec.org/continuoustesting) which has support for `NUnit 2.5.9`. For Specwatchr, the `dotnet.watchr.rb` file contains a hard reference to the `2.5.9` binary which may need to be updated to your installed version. To do so, locate the following line:

    NUnitRunner.nunit_path = 'C:\program files (x86)\nunit 2.5.9\bin\net-2.0\nunit-console-x86.exe'

Otherwise you can get started by running the following commands:

    bundle install              (installs all required gems)
    rake                        (builds and runs unit tests)
    rake build                  (builds solution)
    rake spec                   (runs NSpecSpecs test suite with NUnit)
    rake samples [spec_name]    (runs spec_name in SampleSpecs with NSpecRunner)

If you have Resharper 6.1 there is a team-shared settings file in the repository. Please use the settings to format any new code you write.

Fork the project, make your changes, and then send a Pull Request.

## Branch housekeeping

If you are a direct contributor to the project, please keep an eye on your past development or features branches and think about archiving them once they're no longer needed. 
No worries, their commits will still be available under named tags, it's just that they will not pollute the branch list.

If you're running on a Windows OS, there's a batch script available at `scripts\archive-branch.bat`. Otherwise, the command sequence to run in a *nix shell is the following:

```bash
# Get local branch from remote, if needed
git checkout <your-branch-name>

# Go back to master
git checkout master

# Create local tag
git tag archive/<your-branch-name> <your-branch-name>

# Create remote tag
git push origin archive/<your-branch-name>

# Delete local branch
git branch -d <your-branch-name>

# Delete remote branch
git push origin --delete <your-branch-name>
```

If you need to later retrieve an archived branch, just run the following commands:

```bash
# Checkout archive tag
git checkout archive/<your-branch-name>

# (Re)Create branch
git checkout -b <some-branch-name>
```
