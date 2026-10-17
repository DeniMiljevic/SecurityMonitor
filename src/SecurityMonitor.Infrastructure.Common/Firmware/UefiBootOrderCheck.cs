using SecurityMonitor.Core.Interfaces;

namespace SecurityMonitor.Infrastructure.Common.Firmware;

public sealed class UefiBootOrderCheck : IBootConfigurationCheck
{
    private readonly IUefiVariables _variables;

    public UefiBootOrderCheck(IUefiVariables variables) => _variables = variables;

    public Task<IReadOnlyList<string>> FindProblemsAsync(CancellationToken ct = default) =>
        Task.FromResult(FindProblems());

    private IReadOnlyList<string> FindProblems()
    {
        byte[]? bootOrderData = _variables.ReadGlobal("BootOrder");
        if (bootOrderData is null)
            return ["The UEFI BootOrder variable does not exist."];

        IReadOnlyList<ushort> bootOrder = UefiBootOrder.Parse(bootOrderData);

        foreach (ushort number in bootOrder)
        {
            byte[]? data = _variables.ReadGlobal(UefiBootOrder.VariableName(number));
            if (data is null)
                continue;

            UefiLoadOption entry = UefiLoadOption.Parse(data);
            if (!entry.IsActive)
                continue;

            if (!entry.IsWindowsBootManager)
                return [];

            return [$"'{entry.Description}' (Boot{number:X4}) is first in the UEFI boot order ({Format(bootOrder)}). " +
                   "A normal start skips the Linux login gate. To fix it, run 'bcdedit /enum firmware' as administrator " +
                   "and move the Linux entry first with 'bcdedit /set {fwbootmgr} displayorder {<Linux id>} /addfirst'."];
        }

        return [$"No active UEFI boot entry in the boot order ({Format(bootOrder)})."];
    }

    private static string Format(IReadOnlyList<ushort> order) =>
        string.Join(",", order.Select(number => number.ToString("X4")));
}
