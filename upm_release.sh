#!/bin/bash

absolute_path() {
  DIR="$(echo "$(cd "$(dirname "$1")"; pwd)")"
  case $(basename $1) in
      ..) echo "$(dirname $DIR)";;
      .)  echo "$DIR";;
      *)  echo "$DIR/$(basename $1)";;
  esac
}

# checking if a directory is provided
if [ -z "$1" ] ; then
  echo "Please provide a directory to setup the sandbox. The directory should be outside events-sdk-csharp's directory"
  echo "Usage: $0 <directory to setup sandbox>"
  exit 1
fi
if ! [ -d "$1" ]; then
    echo "$1 does not exist."
    exit 1
fi
if [[ "$(absolute_path "$1")" = $PWD* ]]; then
    echo "Please provide a directory outside events-sdk-csharp's directory"
    exit 1
fi
cd "$1" || exit


echo "checking required tools..."

# checking required tools
if ! command -v git &> /dev/null
then
    echo "git could not be found"
    exit 1
fi
if ! command -v nuget &> /dev/null
then
    echo "nuget could not be found"
    exit 1
fi
if ! command -v jq &> /dev/null
then
    echo "jq could not be found"
    exit 1
fi

echo "looking for unity executable path..."
UNITY=$(find /Applications/Unity -type f -name 'Unity' | head -n 1)
echo "Unity executable found at $UNITY"
if [ -z "$UNITY" ]
then
      echo "unity executable is not found. make sure you have installed unity"
      exit
else
  echo "Unity executable found at $UNITY"
fi

echo "setting up release sandbox ..."
rm -rf sandbox
mkdir -m 777 sandbox
cd sandbox

# download events-sdk-csharp, so it's isolated
git clone https://github.com/ht-sdks/events-sdk-csharp.git
cd events-sdk-csharp || exit

echo "fetching the current version of project ..."
VERSION=$(grep '<Version>' events-sdk-csharp/events-sdk-csharp.csproj | sed "s@.*<Version>\(.*\)</Version>.*@\1@")
echo "copy README.md ..."
README=$(<README.md)
echo "releasing version $VERSION ..."

git checkout upm
cd ..

echo "packing ..."
if [ "$(jq -r '.version' events-sdk-csharp/package.json)" == $VERSION ]
then
  echo "$VERSION is the same as the current package version"
fi
# update version in package.json
echo "$(jq --arg VERSION "$VERSION" '.version=$VERSION' events-sdk-csharp/package.json)" > events-sdk-csharp/package.json
echo "$README" > events-sdk-csharp/README.md
# remove all files in Plugins folder recursively
rm -rf events-sdk-csharp/Plugins/*
# download events-sdk-csharp and its dependencies from nuget
nuget locals all -clear
nuget install Hightouch.Events.CSharp -Version "$VERSION" -OutputDirectory events-sdk-csharp/Plugins
# remove dependencies that are not required
declare -a deps=(events-sdk-csharp/Plugins/Hightouch.Events.CSharp.*)
for dir in events-sdk-csharp/Plugins/*; do
  if [ -d "$dir" ]; then
    in_deps=false
    for dep in "${deps[@]}"; do
        if [[ $dir == $dep ]]; then
            in_deps=true
            break
        fi
    done

    if [ $in_deps == false ]; then
        rm -rf "$dir"
    fi
  fi
done
# loop over all the libs and remove any non-netstandard1.3 libs
for dir in events-sdk-csharp/Plugins/*; do
  if [ -d "$dir" ]; then
    for lib in "$dir"/lib/*; do
      if [ "$lib" != "$dir/lib/netstandard1.3" ]; then
        echo $lib
        rm -rf "$lib"
      fi
    done
  fi
done

echo "generating meta files ..."
# launch unity to create a dummy head project
"$UNITY" -batchmode -quit -createProject dummy
# update the manifest of dummy head to import the package
echo "$(jq '.dependencies += {"com.hightouch.events.csharp": "file:../../events-sdk-csharp"}' dummy/Packages/manifest.json)" > dummy/Packages/manifest.json
# launch unity in quit mode to generate meta files
"$UNITY" -batchmode -quit -projectPath dummy

echo "releasing ..."
# commit all the changes
cd events-sdk-csharp || exit
git add .
git commit -m "prepare release $VERSION"
# create and push a new tag, openupm will pick up this new tag and release it automatically
git tag unity/"$VERSION"
git push && git push --tags
cd ..

echo "cleaning up"
# clean up sandbox
cd ..
rm -rf sandbox

echo "done!"



