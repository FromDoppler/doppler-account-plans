using System.Runtime.CompilerServices;
using System;

namespace Doppler.AccountPlans.TimeCollector
{
    public interface ITimeCollector
    {
        string GetCsv();
        void ResetCollectors();
        IDisposable StartScopeCustom(string differentiator);
        IDisposable StartScope(
            string extraId = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = null);
        void CountException(
            Exception exception,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = null);
        void CountEvent(
            string extraId = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = null);
        void CountEventCustom(string differentiator);
    }
}
