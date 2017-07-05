# csharpmailslots

## Project Description

Windows Mailslots C# Wrapper is a simple CSharp class to consume Windows Mailslots. 
It shields you from using Win32 api with P/Invoke. 

Mailslot is an IPC mechanism that is just available on Windows. It communicates by broadcasting messages using datagrams. It is very simple to use and can be very useful but it lacks the ability to confirm the reception of a message. This is a drawback that most solutions cannot afford, leaving it as the least popular of all the IPC mechanism. Mailslots are fully documented here : [MSDN](http://msdn.microsoft.com/en-us/library/aa365576(v=vs.85).aspx).

Right now, probably due to the reason cited earlier, there is no wrapper in .Net to consume Mailslots other than using P/Invoke to reach the underlying Win32 api.

All the few projects that I found so far that wrap the win32 Mailslots api are higher level or full fledge librairies. They contain multiple classes that perform some client/server logic; read, write, async read etc. Also these projects read/write operations were all buffer oriented; I wanted to use streams.

[Documentation](https://github.com/KurdyMalloy/csharpmailslots/wiki/Documentation)
## Goal

The main goal was to keep it simple enough so it is enclosed in one file with the least dependencies as possible. I did not want to create a library; just a class that could be used as a component in a higher level library. If you really want a library out of this, just create a new library project and add the file to it; it should be that simple.

The class is versatile enough so that it can be used for a server (reader) or a client (writer) just based on the constructor parameters.

The class does not contain methods to write or read to the internal Mailslot; it gives access to a FileStream object that can be used in return by the consumer of the class to perform read or write operation.

## References

This is an application use of the class : [NadaConfig](https://github.com/KurdyMalloy/nadaconfig)

Some of my inspirations come from these articles :

For the Mailslots:
[http://www.codeproject.com/KB/miscctrl/CSharp_MailSlots.aspx](http://www.codeproject.com/KB/miscctrl/CSharp_MailSlots.aspx)  
[http://www.codeproject.com/KB/cpp/mailslots.aspx](http://www.codeproject.com/KB/cpp/mailslots.aspx)  
[http://ipclibrary.codeplex.com/](http://ipclibrary.codeplex.com/)  

For the streams:
[http://www.switchonthecode.com/tutorials/interprocess-communication-using-named-pipes-in-csharp](http://www.switchonthecode.com/tutorials/interprocess-communication-using-named-pipes-in-csharp)
