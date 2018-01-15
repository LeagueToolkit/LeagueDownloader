# LeagueDownloader
This is a tool that allows you to download and install any version from League of Legends using Riot's CDN.

### Download & install a solution
A solution is usually composed of two projects: the main project and the "localization one" (e.g. *lol_game_client* which contains all game files and *lol_game_client_fr* which contains all french voices).

Here is how you can install a solution with LeagueDownloader:
```
solution -n <solution_name> -v <solution_version> -l <localization> -o <output_folder> [-p <platform> -d <deploy-mode>]
```
LeagueDownloader will download and install files from projects contained in the specified solution, version and localization in the specified output folder.

The paramater ```-p``` is set to ```live``` by default but you if you want to download files from the PBE, you can use ```-p pbe```.

The parameter ```-d``` allows you to override the deploy mode specified in the original game manifests. It can be used if you want all the downloaded files to be installed using a unique deploy mode. Here is what you would need to enter:

| Deploy mode                    | Parameter |
|--------------------------------|-----------|
| In deploy folder               | ```-d 0```|
| In deploy and solution folders | ```-d 4```|
| In managed files folder        | ```-d 5```|
| Uncompressed in RAF            | ```-d 6```|
| Compressed in RAF              | ```-d 22```|

### Download & install a single project
It is also possible to download a single project. The usage is quite similar to what we've just seen:
```
project -n <project_name> -v <project_version> -o <output_folder> [-p <platform> -d <deploy-mode>]
```

### Download files
WIP
