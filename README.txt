
SETTINGS
To override the default settings use CodeFetcher.ini, it should be place in the same directory as CodeFetcher.exe, setting below:

[Location]
; The directories you want to search, default is the directory where the CodeFetcher.exe is. 
; You can also specify a semicolon-separated list, i.e C:\Users\admin\Documents\Pictures;C:\Users\admin\Documents\Videos etc...
; Relative paths can also be used, they are relative to CodeFetcher.exe, i.e. Documents;Pictures or ..\Documents
Search Directory=C:\Users\admin\Documents\
; Where to store the lucene index, default is the SearchIndex directory where the CodeFetcher.exe is
Search Index=C:\TMP\Index
; If you want  the indexer to skip certain paths. The list must be semicolon-separated.
Paths To Skip=c:\$Recycle.Bin
; Search patterns separated by semicolons, the default is *.*
Search Patterns=*.doc;*.docx

[Index]
; The maximum file size to index in megabytes, default is 20mb
Max Size=20
; The maximum zip file to index in megabytes, default is 5mb
Zip Max Size=5

[Results]
; The maximum results to display, default is 200
Max Result=200

[Options]
; By default the indexes don't have full paths to ensure portability on USB's and in Dropboxes
; If you have multiple search paths each path will be searched to see if the file exists before opening
; To have full paths, i.e. C:\Users\Admin\Documents, set this to False
Portable Paths=False




LICENSE

Apache v2.0: http://www.apache.org/licenses/LICENSE-2.0.html

CREDITS

Application based on:  Dropout - Portable USB and Dropbox Search
  https://dropout.codeplex.com/

