# LC-LCS-Upload (SSH)
Windows command line tool to upload LANCOM script files (*.lcs) to a LANCOM router device

## Features
- Upload a LCS script file to a LANCOM router device
- Drag and drop via desktop shortcut
- Define parameters like router adress, username, passwort as comments in the script file
    - Usage without any user interaction
    - Batch jobs for many routers
- Full command line interface
- Downloads LCF router configuration (*.lcf) as backup before uploading a script

## Installation

Download the latest ZIP archive from the [releases page](https://github.com/elpatron68/lc-lcs-upload/releases) and unzip it to a folder of you choice. The directory must be writeable, so don't use `"C:\Program Files (x86)"` or similar.

## Preparing script files
*LC-LCS-Upload* supports defining some device specific settings as comment lines in the LCS file. Using this enables you to upload scripts without any user interaction onto many devices.

Setting lines have to start with `#` as LANCOM comment line (see example).

Supported settings:
- `routeraddress:<IP adress or domain name>`
- `username:<specific username>` (defaults to `root`)
- `password:<password>`

### Example script file

```
# routeraddress:192.168.168.1
# username:root
# password:123654()Ab
#
lang English
flash No

set /Setup/SNMP/Administrator "Operator from hell"

flash Yes
exit
```


## Usage

### Command line usage

*LC-LCS-Upload* supports several command line parameters:

- `-i <input file>` - LANCOM Script file (*.lcs): **Required**
- `-a <router address>` - IP address or domain name of the LANCOM device: *Optional*
- `-u <username>` - Ssh username: *Optional*
- `-p <password>` - Ssh password: *Optional*
- `-w` - Wait for a keystroke after run (useful in *Desktop-shortcut-mode*): *Optional*
- `-b <path>` - Directory to be used for backups. If given, a LCF backup file will be downloaded before the script is uploaded: *Optional*
- `-h` Show help

```
LC-LCS-Upload v1.0
(c) 2018 m.busche@gmail.com
Free Open Source Software, see License.md

LC-LCS-Upload_SSH.exe {options}
Version: 1.0.0.0

The given arguments were incomplete:
  Required Argument 'input' is not given

Options:
    --input [-i]      : LANCOM script file (*.lcs)
    --address [-a]    : Router address (IP or DN)
    --username [-u]   : SSH username, default: root
    --password [-p]   : Ssh password
    --wait [-w]       : Wait for user after run
    --backup [-b]     : Download LCF backup in <path> before uploading the script
    --help [-h]       : Show help
```

### Desktop shortcut

For easy drag & drop handling of LCS script files, a desktop shortcut can be created. If should at least have the `-i` argument for the LCS file to be processed. You may probable add `-w` to see the output. note, that `-i` has to be the last command line switch as the file name will be added at the end.

![Desktop shortcut](https://github.com/elpatron68/lc-lcs-upload/raw/master/screenshots/LC-LCS-Upload_SSH.exe_shortcut.png "Desktop shortcut settings")


[![Video screencast](https://j.gifs.com/3231Np.gif)](https://www.youtube.com/watch?v=h_yv8S8wOhI)

### Priority of given settings

| Setting        | Environment   | Scriptfile  | Commandline | User input  |
| -------------- | :-----------: | :---------: | :---------: | ----------- |
| Address        | 1             | 2           | 3           | 4           |
| Username       | 1             | 2           | 3           | na          |
| Password       | 1             | 2           | 3           | 4           |

### Batch run

To process many files with a batch file, create your scripts and apply at least address and passwords into them. Create a Windows command file like this:

```
@echo off
for /F "tokens=*" %%A in ('type "*.lcs"') do LC-LCS-Upload_SSH.exe -b backup -i %%A
```

## License

*LC-LCS-Upload* is free open Source software (OSS), licensed under the MIT license. See [LICENSE.md](https://github.com/elpatron68/lc-lcs-upload/raw/master/LICENSE.md).

By using *LC-LCS-Upload* you have to accept the licenses of these third party components:

- [NLog](https://raw.githubusercontent.com/NLog/NLog/master/LICENSE.txt)
- [BurnSystems.CommandLine](https://opensource.org/licenses/MIT)
- [SSH.NET](https://github.com/sshnet/SSH.NET/blob/master/LICENSE)
