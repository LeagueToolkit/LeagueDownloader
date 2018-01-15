# LeagueDownloader
This is a tool that allows you to download and install any version from League of Legends using Riot's CDN.

### Download & install a solution
A solution is usually composed of two projects: the main project and the "localization one" (e.g. *lol_game_client* which contains all game files and *lol_game_client_fr* which contains all french voices).

Here is how you can install a solution with LeagueDownloader:
```
solution -n <solution_name> -v <solution_version> -l <localization> -o <output_folder> [-p <platform>]
```
LeagueDownloader will download and install files from projects contained in the specified solution, version and localization in the specified output folder.
The paramater ```-p``` is set to ```live``` by default but you if you want to download files from the PBE, you can use ```-p pbe```.


### Download & install a single project
It is also possible to download a single project. The usage is quite similar to what we've just seen:
```
project -n <project_name> -v <project_version> -o <output_folder> [-p <platform>]
```

### Download files
WIP
