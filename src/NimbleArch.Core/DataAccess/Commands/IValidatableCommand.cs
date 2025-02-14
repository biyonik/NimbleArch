using NimbleArch.SharedKernel.Validation.Base;

namespace NimbleArch.Core.DataAccess.Commands;

/// <summary>
/// Defines a command that requires validation.
/// </summary>
/// <remarks>
/// EN: Interface for commands that need to be validated before execution.
/// Integrates with the validation pipeline system.
///
/// TR: Yürütülmeden önce doğrulanması gereken komutlar için arayüz.
/// Doğrulama pipeline sistemi ile entegre çalışır.
/// </remarks>
public interface IValidatableCommand : ICommand
{
    /// <summary>
    /// Gets the validation group to use.
    /// </summary>
    ValidationGroup ValidationGroup { get; }
}