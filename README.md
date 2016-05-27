<img src="https://img.shields.io/appveyor/ci/ElanHasson/SSIS-Extensions/master.svg"/>

# SSIS-Extensions
A clone of https://ssisextensions.codeplex.com/ 


**SFTP, PGP and Zip Control Flow custom components**  
 A set of custom tasks to extend SSIS. Includes a SFTP task, PGP encryption task and zip/unzip task.  

Includes custom components to SFTP, Encrypt/Decrypt files using PGP and zip/unzip task for SSIS 2008 and SSIS 2012.

*   System Requirements:
    *   Microsoft SQL Server 2008
    *   Microsoft SQL Server 2012
    *   Microsoft Visual Studio 2012

*   SFTP component is entirely written in .NET and is based on the [http://sshnet.codeplex.com](http://sshnet.codeplex.com) SshNet library.
*   PGP component is based on the [http://www.mentalis.org/](http://www.mentalis.org/) Bouncy Castle library. Also uses the PGPEncryptionKeys class found here [http://blogs.microsoft.co.il/blogs/kim/archive/2009/01/23/pgp-zip-encrypted-files-with-c.aspx](http://blogs.microsoft.co.il/blogs/kim/archive/2009/01/23/pgp-zip-encrypted-files-with-c.aspx)
*   Zip is based on the [http://sharpdevelop.net/OpenSource/SharpZipLib/](http://sharpdevelop.net/OpenSource/SharpZipLib/) SharpZipLib.

*   This project can also help people figure out how to code a Custom SSIS Task using a Custom UI and Property Grid.

Note: I'd appreciate any feedback on the project, also let me know if you'd like more components.  

**Change Log**

*   May 27, 2016
    *   Added SQL 2014 Support and new installer for 2014 only. 

*   May 19, 2013
    *   Unified installer for SQL 2008R2 and 2012, both 32-bit and 64-bit
    *   Fixed issue with dll mismatch

*   Feb 16, 2013
    *   Uploaded the updated SQL 2008 R2 project
    *   Uploaded sample and documentation for SQL 2008 R2 project
    *   Comming soon - FTP component.

*   Feb 9, 2013
    *   Fixed a few bugs
    *   Added filtering ability to Zip task as well as SFTP task
    *   Coming soon - move changes to SQL 2008 R2 project.

*   Feb 3, 2013
    *   Updated the project for SQL 2012
    *   Filter files, local or remote, using FLEE [http://flee.codeplex.com/](http://flee.codeplex.com/)
    *   Removed dependency on Sharp SSH,
    *   SFTP uses SSHNET [http://sshnet.codeplex.com](https://ssisextensions.codeplex.com/wikipage?title=http%3a%2f%2fsshnet.codeplex.com&referringTitle=Home)
    *   Improved performance for SFTP

*   July 31, 2011
    *   Added Zip Task
    *   Added support for wildcards.
    *   Fixed: bug in SFTP task during validation.
    *   Fixed: getting error in SFTP task when receiving no files.
