using System;
using System.Collections.Generic;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Suguri46b.Scripts.Localization;

/// <summary>
///     重复(X)阈值文本格式器：<c>{Var:repeatif:达成时显示的文本|未达成时的文本}</c>。
///     判定规则：变量当前值 &gt; 0 视为"已达成"。
///     与游戏内置 <c>show</c>（IfUpgraded）格式器同机制，使用 <see cref="IFormattingInfo.FormatAsChild" />
///     递归格式化所选分支，因此分支内可以嵌套其它占位符（如 <c>{ExtraDamage:diff()}</c>）。
///     未达成分支省略时（写成 <c>A|</c>）整行不显示。
/// </summary>
[RegisterSmartFormatter]
public class RepeatThresholdFormatter : IFormatter
{
    public string Name
    {
        get => "repeatif";
        set => throw new NotSupportedException("Setting the formatter name is not supported.");
    }

    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        IList<Format>? branches = formattingInfo.Format?.Split('|');
        if (branches == null || branches.Count == 0)
        {
            return false;
        }

        bool achieved = GetCurrentValue(formattingInfo.CurrentValue) > 0m;
        Format? chosen = achieved
            ? branches[0]
            : (branches.Count > 1 ? branches[1] : null);

        if (chosen != null)
        {
            formattingInfo.FormatAsChild(chosen, formattingInfo.CurrentValue);
        }
        return true;
    }

    /// <summary>取变量当前值：计算型变量返回其实时计算值（经 IConvertible）。</summary>
    private static decimal GetCurrentValue(object? value)
    {
        if (value == null)
        {
            return 0m;
        }
        if (value is decimal dec)
        {
            return dec;
        }
        if (value is IConvertible convertible)
        {
            try
            {
                return convertible.ToDecimal(CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0m;
            }
        }
        return 0m;
    }
}
