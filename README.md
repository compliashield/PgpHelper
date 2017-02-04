# PgpHelper
Command line tool to retrieve a PGP public key from a URL and encrypt a file or files in a directory

Required Arguments:
Argument 1: The URL of the PGP public key to encrypt the files
Argument 2: The path to the directory or file that you wish to encrypt. If a directory files within will be processed.

Optional Arguments:
/overwrite: If specified, the files will be overwritten.  If ommitted, the files that exist are skipped.
/deletesource: If specified, the source files will be deleted after they are encrypted.
--ext: If specified, must be followed by an extension value for the files that you wish to encrypt.

Usage:
1. Install the application.
2. Open a command prompt.
2. Navigate to the directory that contains the installed application.
   > cd C:\Program Files (x86)\CompliaShield\PgpHelper
3. Enter the desired command. For example:
   > PgpHelper https://datastore.compliashield.com/pgpkey/{{YOUR_COMPLIASHIELD_IDENTITY_GUID_HERE}}.asc "C:\My Files To Encrypt"
   
Examples: 
- Encrypt files at the path specified:
  > PgpHelper https://datastore.compliashield.com/pgpkey/{{YOUR_COMPLIASHIELD_IDENTITY_GUID_HERE}}.asc "C:\My Files To Encrypt"
  
- Encrypt files at the path specified and delete the source on complete:
  > PgpHelper https://datastore.compliashield.com/pgpkey/{{YOUR_COMPLIASHIELD_IDENTITY_GUID_HERE}}.asc "C:\My Files To Encrypt" /deletesource
  
- Encrypt files at the path specified that have the extension .txt, then delete the source on complete:
  > PgpHelper https://datastore.compliashield.com/pgpkey/{{YOUR_COMPLIASHIELD_IDENTITY_GUID_HERE}}.asc "C:\My Files To Encrypt" /deletesource --ext .txt
  
- Encrypt files at the path specified that have the extension .txt, overwrite the .gpg files that might exist already then delete the source on complete:
  > PgpHelper https://datastore.compliashield.com/pgpkey/{{YOUR_COMPLIASHIELD_IDENTITY_GUID_HERE}}.asc "C:\My Files To Encrypt" /deletesource /overwrite --ext .txt
