using Diginsight.Runtime;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Configuration;

public static class HttpExtensions
{
    public static readonly HttpRequestOptionsKey<(Type Type, string MemberName)> InvocationOptionKey = new("Invocation");

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetInvocation(this HttpRequestMessage request, [CallerMemberName] string callerMemberName = "")
    {
        request.SetInvocation(RuntimeUtils.GetCallerType(), callerMemberName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetInvocation(this HttpRequestMessage request, Type type, string memberName)
    {
        request.Options.Set(InvocationOptionKey, (type, memberName));
    }



}