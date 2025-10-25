using AspNetCore.Boilerplate.Roslyn.Models;

namespace AspNetCore.Boilerplate.Roslyn.Components.Endpoint;

internal record ControllerInfo(HierarchyInfo Hierarchy, bool IsStatic);
