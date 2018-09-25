# UnityBuildHandler

A small script that makes generating distributables for various mobile platforms easier. It uses a set of preprocessor directives to determine which code should be run as well as chooses app icon assets based on the chosen platform (useful for Amazon Underground, which requires a special icon made specifically for it).

## Preprocessor Directives

* IOS - Marks code for iOS Applications
* AMAZON - Marks code for Android Applications distributed via Amazon
* UNDERGROUND - Marks code for Android Applications distributed via Amazon Underground
* GOOGLE - Marks code for Android Applications distributed via Google Play
* TEST_BUILD - Marks code that should only run on test builds

## Usage
1. Add the BuildHandler.cs script to your Unity project's Editor Folder.
2. Open the Build Handler window via Window->Build Handler.
3. Fill in your project name, keystore information if needed, application icon, versioning information, and whether this is a test build or not.
4. OPTIONAL: Use preprocessor directives to make certain code platform specific
5. Click on the button to generate the build. The build will be placed in [ProjectFolder]/Builds and will follow the naming convention [ProjectName]_[Version][Platform].

## Future
* Expand this from just handling mobile builds to handling all types of builds within Unity
* Expand what can be controlled from this window, ideally making it a one-stop shop for all build related tasks in Unity
* Redo the Editor Window to make it look better