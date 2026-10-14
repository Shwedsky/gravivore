"""Wrap only package listing in a private copied SDK; query actual installed XML."""
import hashlib, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
MIRROR=ROOT.parents[1]/'.utmp/architecture-sdk55'
ORIGINAL=Path('C:/Program Files/Unity/Hub/Editor/6000.3.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK')
JAVA=ORIGINAL.parent/'OpenJDK'
assert MIRROR.is_relative_to(ROOT.parents[1]) and (MIRROR/'platforms/android-36/android.jar').is_file()
manager=MIRROR/'cmdline-tools/16.0/bin/sdkmanager.bat'
original=ORIGINAL/'cmdline-tools/16.0/bin/sdkmanager.bat'
before=hashlib.sha256(original.read_bytes()).hexdigest()
manager.write_text('@echo off\nsetlocal\nset "JAVA_HOME='+str(JAVA)+'"\nif "%~1"=="--list" goto installed\ncall "'+str(original)+'" %*\nexit /b %errorlevel%\n:installed\ncall "'+str(original)+'" --list_installed "--sdk_root='+str(MIRROR)+'"\nexit /b %errorlevel%\n')
assert before==hashlib.sha256(original.read_bytes()).hexdigest()
out=ROOT/'docs/history/implementation-passes/chapter01-architecture-production-v1/verification'
(out/'offline-sdk.json').write_text(json.dumps(dict(original=str(ORIGINAL),mirror=str(MIRROR),originalManagerSha256=before,originalManagerUnmodified=True,method='Official installed SDK copied privately; only --list delegates to official --list_installed with the real mirror package XML. No package/version data is fabricated, installed or downloaded.'),indent=2)+'\n')
print('Prepared private installed-package listing wrapper:',manager)
