namespace FileService.Domain.Enums;

public enum FileStatus
{
    Uploading = 0,
    Scanning = 1,
    Encrypting = 2,
    Ready = 3,
    Quarantined = 4,
    Failed = 5
}
