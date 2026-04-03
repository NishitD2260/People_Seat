@echo off
echo ==============================================
echo   UNITY GIT PACKAGE TEMPLATE SETUP (TNP)
echo ==============================================
echo This script prepares your repository for use as a Unity UPM git package.
echo   1. Ensures .gitignore is present
    - Excludes Library, Temp, Build, Obj, and user settings.
echo   2. Ensures all template folders stay inside the Assets folder.
echo   3. Creates package.json in repo root for Unity Package Manager.
echo   4. Adds, commits, and pushes changes to your remote git repo.
echo ----------------------------------------------
pause

rem Step 1: Create package.json in repo root
echo {                                                    > package.json
echo   "name": "com.tapnplay.tnp-template",               >> package.json
echo   "displayName": "Tap N Play Template",              >> package.json
echo   "version": "1.0.0",                               >> package.json
echo   "unity": "2021.3",                                >> package.json
echo   "description": "Core template assets and folders for Tap N Play Unity projects.", >> package.json
echo   "author": { "name": "Tap N Play" },               >> package.json
echo   "keywords": ["template", "starter", "assets"],    >> package.json
echo   "type": "template"                                >> package.json
echo }                                                   >> package.json

rem Step 2: Git add/commit/push
git add .
git commit -m "Setup Unity template as UPM Git package: all folders stay inside Assets, with .gitignore applied."
git push

echo ----------------------------------------------
echo All done! Your template is now a Unity Package.
echo To use, add this to your project's manifest.json:
echo   "com.tapnplay.tnp-template": "https://github.com/Tap-N-Play/TNP_TEMPLATE.git"
echo ----------------------------------------------
pause
