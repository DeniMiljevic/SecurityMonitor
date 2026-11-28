namespace SecurityMonitor.Contracts.Agent;

public sealed record AgentPhoto(bool Succeeded, byte[]? Jpeg, string? CameraName, string? FailureReason)
{
    public static AgentPhoto Captured(byte[] jpeg, string cameraName) => new(true, jpeg, cameraName, null);

    public static AgentPhoto Failed(string reason) => new(false, null, null, reason);
}
