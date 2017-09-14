
#! /bin/sh

# Example install script for Unity3D project. See the entire example: https://github.com/JonathanPorta/ci-build

# This link changes from time to time. I haven't found a reliable hosted installer package for doing regular
# installs like this. You will probably need to grab a current link from: http://unity3d.com/get-unity/download/archive
echo 'Downloading from http://netstorage.unity3d.com/unity/3757309da7e7/MacEditorInstaller/Unity-5.2.2f1.pkg: '
curl -o Unity.pkg http://netstorage.unity3d.com/unity/3757309da7e7/MacEditorInstaller/Unity-5.2.2f1.pkg

echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /