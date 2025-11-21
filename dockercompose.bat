@ECHO OFF

SET Tuat=src\Tuat
RMDIR /S /Q "%Tuat%\DeployLinux" >NUL

ECHO.
ECHO ** Publish TUAT for Linux
ECHO.
pushd %Tuat%
CALL publishLinux.bat >NUL
popd

ECHO.
ECHO ** Checking TUAT for Linux
ECHO.
IF NOT EXIST "%Tuat%\DeployLinux\Tuat.dll" (
  ECHO Camas Release build not found "%Tuat%\DeployLinux\Tuat.dll"
  GOTO ERROR
)

ECHO.
ECHO ** Creating docker image
ECHO.
docker compose down
docker build -f Dockerfile -t robertpeters/theultimateautomationtool:latest .
docker compose up -d --build

GOTO SUCCESS


:ERROR
ECHO.
ECHO !! ERROR - Error occured
GOTO END

:SUCCESS
GOTO END

:END
ECHO.
PAUSE