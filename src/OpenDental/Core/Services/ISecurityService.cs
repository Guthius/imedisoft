using System;

namespace OpenDental.Core.Services;

public interface ISecurityService
{
    bool HasPermission(string permission);
    
    bool IsAuthorized(string permission);
    bool IsAuthorized(string permission, long foreignKey);
    bool IsAuthorized(string permission, DateTimeOffset asOf);
}