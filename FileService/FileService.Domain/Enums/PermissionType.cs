namespace FileService.Domain.Enums;

[Flags]
public enum PermissionType
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Share = 8,
    Download = 16,
    FullControl = Read | Write | Delete | Share | Download
}
