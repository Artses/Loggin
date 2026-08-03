using System.Runtime.InteropServices;

namespace Loggin.Desktop.Services;

public sealed class CredentialService
{
    private const string Target = "Artses.LogginDesktop.Jwt";
    private const int CredTypeGeneric = 1;
    private const int CredPersistLocalMachine = 2;

    public void SaveToken(string token)
    {
        var bytes = System.Text.Encoding.Unicode.GetBytes(token);
        var blob = Marshal.AllocCoTaskMem(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, blob, bytes.Length);
            var credential = new NativeCredential
            {
                Type = CredTypeGeneric,
                TargetName = Target,
                CredentialBlobSize = (uint)bytes.Length,
                CredentialBlob = blob,
                Persist = CredPersistLocalMachine,
                UserName = Environment.UserName
            };
            if (!CredWrite(ref credential, 0))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
        finally { Marshal.FreeCoTaskMem(blob); }
    }

    public string? ReadToken()
    {
        if (!CredRead(Target, CredTypeGeneric, 0, out var pointer))
            return null;
        try
        {
            var credential = Marshal.PtrToStructure<NativeCredential>(pointer);
            return Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
        }
        finally { CredFree(pointer); }
    }

    public void DeleteToken() => CredDelete(Target, CredTypeGeneric, 0);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string UserName;
    }

    [DllImport("advapi32", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);
    [DllImport("advapi32", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, int type, int flags, out IntPtr credential);
    [DllImport("advapi32", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, int type, int flags);
    [DllImport("advapi32", SetLastError = true)]
    private static extern void CredFree(IntPtr credential);
}
