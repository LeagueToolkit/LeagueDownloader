# LeagueDownloader
This is a tool that allows you to download and install any version from League of Legends using Riot's CDN.

[Get the latest version](https://github.com/LoL-Fantome/LeagueDownloader/releases/)

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

### List files
You can display a list of files from a specific project. Of course, there are filters you can use if you decide to only show files from a specific folder, revision or even if you want to show only one file to check if it exists. Here is how you can use all this:
```
list -n <project_name> [-v <project_version> -r <files_revision> -f <filter> -p <platform>]
```
By default, the project version ```-v``` is set to the latest one. 

As for the files revision, it is not set by default, which means the files revision will not be used to filter them. If you want only the files from a specific revision to show up in the list, you can specify this revision like this : ```-r 0.0.0.95```. If you want to only display files from the latest patch, use ```-r LATEST```.

You can also filter the list by deciding to only keep a specific folder or just a file from the entire list:
* Show files in the *LEVELS/Map1/* folder: ```-f LEVELS/Map1/```
* Show the file *LEVELS/Map1/env.ini* if it exists : ```-f LEVELS/Map1/env.ini```

And like before, the platform ```-p``` is set, by default, to live.

### Download files
You can easily download the files you want by using the *download* options. The usage is exactly the same as the *list* options except that you must display an output folder this time:
```
download -n <project_name> -o <output_folder> [-v <project_version> -r <files_revision> -f <filter> -p <platform>]
```
