# c3d2obj

c3d2obj is a command line utility to convert Condor 2 .c3d files of Wavefront .obj files.

## Installation

No installation is needed. Just copy bin\Release\c3d2obj.exe to the folder with your .c3d files. 

You could put it elsewhere and add the path to it in the PATH environment variable.

## Usage

The program only runs under Windows. It's been tested on Windows 7 and Windows 10.

Open a Command shell or Windows PowerShell. cd to your working folder.

`c3d2obj -f <filename> | -v`

filename needs to be a .c3d file but you can leave off the extension.

-v for verbose mode

Note the | above shows the -v is optional. Don't type it in.
 
## Dependancies
The program needs Windows .NET Framework 4.5 or later. This is a standard part of Windows.

## Developement Environment
The code is in C# written with Visual Studio Express 2012.

## Credits
.NET Follower for the command line parser [Link](http://dotnetfollower.com/wordpress/2012/03/c-simple-command-line-arguments-parser/)

## License
[MIT](https://choosealicense.com/licenses/mit/)