Update Version
==========
* update `<Version>` value in `events-sdk-csharp.csproj`

Release to Nuget
==========
1. Create a new GitHub release with version that matches `<Version>` in `events-sdk-csharp.csproj`
2. The GitHub release action will publish to nuget automatically

Release to OpenUPM
==========
follow the instruction above to `Release to Nuget`. once the new version is available in Nuget and PR merged to main, run the following command in the root of the project:
```bash
sh upm_release.sh <directory>
```
NOTE: `<directory>` is a required folder to setup sandbox for release. it should be **outside** the project folder.

the script will setup a sandbox to pack the artifacts and create a `unity/<version>` tag on github. OpenUPM checks the `unity/<version>` tag periodically and create a release automatically.

Pre-release
==========
Pre-release is useful when testing code compatibility on Unity. To make a pre-release, update the version tag with a suffix of `-alpha.<v>` where `<v>` is the version number of this alpha release. The following is a list of valid pre-release versions:
* `2.0.0-alpha.1`
* `2.0.0-alpha.2`
* `2.0.0-alpha.12`

The rest of the pre-release progress is the same as a regular release.
