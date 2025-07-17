@echo off
set rootPath=%~dp0\..
echo %currentPath%
set frontPath=%rootPath%\Front\customapp
echo %frontPath%
cd %frontPath%
@REM call npm run build
robocopy "%frontPath%/dist" "%rootPath%/Packages/Front" /E
robocopy "%frontPath%/Publish" "%rootPath%/Packages/Front" web.config /E
cd %rootPath%\Back
 