//-----------------------------------------------------------------------
// <copyright file="cMailSlot.cs" >
//     Copyright (c) 2011 Jean-Michel Julien.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;

namespace cMailSlot
{
    public sealed class MailSlot: IDisposable
    {
        private bool disposed = false;
        private SafeFileHandle handleValue = null;
        private FileStream fileStream = null;
        
        public enum ScopeType
    	{
            local,
            remote
    	}

        public enum SlotType
    	{
            reader,
            writer
    	}

        public uint MaxMessageSize {get; set;}
        public string Name {get; private set;}
        public ScopeType Scope {get; private set;}
        public SlotType Type {get; private set;}
        public string RemoteName {get; private set;}
        public string Filename {get; private set;}

        public FileStream FStream
        {
            get
            {
                    return fileStream;
            }
        }
        //public SafeFileHandle Handle
        //{
        //    get
        //    {
        //        // If the handle is valid,
        //        // return it.
        //        if (!handleValue.IsInvalid)
        //            return handleValue;
        //        else
        //            return null;
        //    }
        //}

        public bool IsReady
        {
            get
            {
                return ((handleValue != null) && (fileStream != null) && (!handleValue.IsInvalid));
            }
        }

        public uint IsMessageInSlot
        {
            get
            {
                uint nextMessageSize = 0;
                uint numMessages = 0;
                if (IsReady && Type == SlotType.reader)
                    GetMailslotInfo(handleValue, IntPtr.Zero, out nextMessageSize, out numMessages, IntPtr.Zero);
                return numMessages;
            }
        }

        public uint NextMessageSize
        {
            get
            {
                uint nextMessageSize = 0;
                uint numMessages = 0;
                if (IsReady && Type == SlotType.reader)
                    GetMailslotInfo(handleValue, IntPtr.Zero, out nextMessageSize, out numMessages, IntPtr.Zero);
                return nextMessageSize;
            }
        }

        // P/Invoke signatures for required API imports
        #region DLL Imports
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern SafeFileHandle CreateMailslot(string lpName,
                                            uint nMaxMessageSize,
                                            uint lReadTimeout,
                                            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern bool GetMailslotInfo(SafeFileHandle hMailslot,
                                           IntPtr lpMaxMessageSize,
                                           out uint lpNextSize,
                                           out uint lpMessageCount,
                                           IntPtr lpReadTimeout);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern SafeFileHandle CreateFile(
              string lpFileName,
              [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
              [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
              IntPtr SecurityAttributes,
              [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
              uint dwFlagsAndAttributes,
              IntPtr hTemplateFile
              );        
        #endregion

  
        //Default constructor should not be used
        private MailSlot(): this(@"DefaultMailSlot")
        {
        }

        public MailSlot(string name)
        {
            // arbitrary buffer length
            MaxMessageSize = 4096; 
            Open(name);
        }

        public MailSlot(string name, string remote)
        {
            // arbitrary buffer length
            MaxMessageSize = 4096; 
            Open(name, remote);
        }

        // To create server (reader); cannot be created remote
        public bool Open(string name)
        {
            //Do not reopen if we are disposed
            if (disposed)
                return false;
            
            //Want to close before we reopen
            Close();

            Name = name;
            Scope = ScopeType.local;
            Type = SlotType.reader;

            CreateMailSlotHandle();
            CreateFileStreamHandle();

            return IsReady;
        }

        // To create client (writer); remote value can be "computername", "domainname", or "*" for current domain. You can also pass null or empty string or "." in remote to consume local mailslot; 
        public bool Open(string name, string remote)
        {
            //Do not reopen if we are disposed
            if (disposed)
                return false;

            //Want to close before we reopen
            Close();

            Name = name;
            Scope = ScopeType.remote;
            Type = SlotType.writer;
            RemoteName = remote;

            if (remote == null || remote.Length == 0 || remote == @".")
            {
                RemoteName = @".";
                Scope = ScopeType.local;
            }

            CreateMailSlotHandle();
            CreateFileStreamHandle();

            return IsReady;
        }

        public void Close()
        {
            if (handleValue != null)
            {
                handleValue.Dispose();
                handleValue = null;
            }
            if (fileStream != null)
            {
                fileStream.Dispose();
                fileStream = null;
            }
        }

        private void CreateCanonicalSlotName()
        {
            StringBuilder ret = new StringBuilder(@"\\");
            switch (Scope) {
                case ScopeType.local:
                    ret.Append(@".\");
                    break;
                case ScopeType.remote:
                    ret.Append(RemoteName);
                    ret.Append(@"\");
                    break;
                default:
                    break;
            }
            ret.Append(@"mailslot\");
            ret.Append(Name);
            Filename = ret.ToString();
        }

        private void CreateMailSlotHandle()
        {

            CreateCanonicalSlotName();

            if (Type == SlotType.reader)
            {
                handleValue = CreateMailslot(Filename, 0, 0, IntPtr.Zero);
            }
            else if (Type == SlotType.writer)
            {
                // Try to open the file.
                handleValue = CreateFile(Filename, FileAccess.Write, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            }

            // If the handle is invalid,
            // get the last Win32 error 
            // and throw a Win32Exception.
            if (handleValue.IsInvalid)
            {
                handleValue = null;
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        private void CreateFileStreamHandle()
        {
            if ((handleValue != null) && (!handleValue.IsInvalid))
            {
                try
                {
                    if (Type == SlotType.reader)
                    {
                        fileStream = new FileStream(handleValue, FileAccess.Read);
                    }
                    else if (Type == SlotType.writer)
                    {
                        fileStream = new FileStream(handleValue, FileAccess.Write);
                    }
                }
                catch (Exception e)
                {
                    fileStream = null;
                    throw (e);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Close();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MailSlot()
        {
            Dispose(false);
        }
     }
}
