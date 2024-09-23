# WinDurango.UI
[![Join our Discord](https://img.shields.io/discord/1280176159010848790?color=2c9510&label=WinDurango%20Discord&logo=Discord&logoColor=white)](https://discord.gg/mHN2BgH7MR)
[![View stargazers](https://img.shields.io/github/stars/WinDurango-project/WinDurango.UI)](https://github.com/WinDurango-project/WinDurango.UI/stargazers)
GUI for WinDurango, which is planned to allow for easy installing/patching among other random stuff I decide lmfao

> [!NOTE]
> This does nothing more than provide a GUI for easily registering and patching packages with WinDurango.   
> Additionally, I don't know much about C#... so the code is probably very messy.

# Roadmap

## Features
 - [ ] Compiling the DLLs and patching automatically
 - [ ] Allow for package removal from UI instead of just completely uninstalling, as well as allowing for any existing installed package to be added, and other install options... such as installing without symlinking, etc... So we could modify the install button to be a dropdown button with the options inside for the install stuff.
 - [ ] Resize content to fit to screen
 - [ ] Allow for search

## Bugs/Improvements
 - [X] Make the applist not go offscreen (lol)
 - [ ] Applist pages
 - [ ] Fix icon in the titlebar
 - [ ] Repo contributors on the about screen
 - [ ] Get Fluent Thin working
 - [ ] Add versioning to the InstalledPackages json (as in versioning the JSON file itself)
 - [ ] Make the Package stuff not rely on UI so much, handle that somewhere else.
 - [ ] Fix crash when installation errors
 - [ ] Cleanup, lots and lots of it.
