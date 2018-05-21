# LeagueDownloader
LeagueDownloader is a command line tool that allows you to download and install any version from League of Legends using Riot's CDN.

[Get the latest version](https://github.com/LoL-Fantome/LeagueDownloader/releases/)

### Download & install a solution
A solution is usually composed of two projects: the main project and the "localization one" (e.g. *lol_game_client* which contains all game files and *lol_game_client_fr* which contains all french voices).

Here is how you can install a whole solution with LeagueDownloader:
```
solution -n <solution_name> -v <solution_version> -l <localization> -o <output_folder> [-u <cdn_url> -p <platform> -d <deploy-mode>]
```
LeagueDownloader will download and install files from projects contained in the specified solution, version and localization in the specified output folder.

The paramater ```-u``` allows you to use a custom CDN by specifying its base URL. By default, it is set to Riot's default CDN ```http://l3cdn.riotgames.com```

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
It is also possible to download a single project. The usage is similar to what we've just seen:
```
project -n <project_name> -v <project_version> -o <output_folder> [-u <cdn_url> -p <platform> -d <deploy-mode>]
```

### List files
You can display a list of files from a specific project. Of course, there are filters you can use if you decide to only show files from a specific folder, revision or even if you want to show only one file to check if it exists. Here is how you can use all this:
```
list -n <project_name> [-v <project_version> -r <files_revision> -f <filter> -u <cdn_url> -p <platform>]
```
By default, the project version ```-v``` is set to the latest one. 

As for the files revision, it is not set by default, which means the files revision will not be used to filter them. If you want only the files from a specific revision to show up in the list, you can specify this revision like this: ```-r 0.0.0.95```. If you want to only display files from the latest patch, use ```-r LATEST```.

You can also filter the list by deciding to only keep specific files. To do so, just specify a regular expression using the ```-f``` argument (e.g. if you only want room.nvr and room.wgeo files to appear in the list, you can use ```room.(wgeo|nvr)$```.

And like before, you can customize the CDN and the platform you want to fetch data from using the ```-u``` and ```-p``` arguments.

### Download files
You can easily download the files you want by using the *download* options. The usage is exactly the same as the *list* options except that you must display an output folder this time:
```
download -n <project_name> -o <output_folder> [-v <project_version> -r <files_revision> -f <filter> -u <cdn_url> -p <platform>]
```

Using this option, files will be downloaded in raw format in the specified output folder. That means they won't follow a RADS architecture (no packaging inside RAFs nor sorting by revision).

### Download multiple revisions of files
It also possible to download multiple revisions of files of your choice using the ```range-download``` option:
```
range-download -n <project_name> -o <output_folder> [-f <filter> -u <cdn_url> -p <platform> --start-revision <start_revision> --end-revision <end_revision> --ignore-older-files <ignore_older_files> --save-manifest <save_manifest>]
```

Like the ```download``` option, you need to specify the project name and the output folder, you can still customize the CDN URL and the platform and finally you can specify a regular expression to only select the files you want.
This option also comes with 4 new parameters:
* ```start-revision```: specify the revision from which you want to download your files (by default the start revision will be the first project revision ```0.0.0.0```).
* ```end-revision```: specify the revision until which you want to download your files (by default the end revision will be the latest revision of the specified project).
* ```ignore-older-files```: specify whether you want the files from an earlier revision than ```start-revision``` to be ignored (set to ```false``` by default).
* ```save_manifest```: specify whether you want the releasemanifest file from each revision of the specified project (from ```start-revision``` to ```end-revision```) to be saved (in addition to game files).

Let's say you want to download all the files related to Zed from the revision ```0.0.1.10``` to the latest. The command to use would look like something like this:
```
range-download -n lol_game_client -o "C:/Zed" -f "DATA/Characters/Zed" --start-revision 0.0.1.10
```
Letting ```ignore-older-files``` to its default value (```false```) will make the tool download earlier files if Zed files weren't changed by the revision ```0.0.1.10``` (so that you still get files that the game would have at version ```0.0.1.10``` even though they were revised earlier).

### Discord
Need some help to use this tool? Want to contribute? If you want to talk to the developers or other people in the community, join our discord server:

<table>
  <tbody>
    <tr>
      <td><img width=64 height=64 src="https://cdn.worldvectorlogo.com/logos/discord.svg"></td>
      <td><h1>https://discord.gg/SUHpgaF</h1></td>
    </tr>
  </tbody>
</table> 
