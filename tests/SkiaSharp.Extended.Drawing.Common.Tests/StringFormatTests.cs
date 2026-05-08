using System.Drawing;
using System.Drawing.Text;

namespace SkiaSharp.Extended.Drawing.Common.Tests;

public class StringFormatTests
{
    // --- Constructors ---

    [Fact]
    public void Constructor_Default()
    {
        using var sf = new StringFormat();
        Assert.Equal(StringAlignment.Near, sf.Alignment);
        Assert.Equal(StringAlignment.Near, sf.LineAlignment);
    }

    [Fact]
    public void Constructor_WithFlags()
    {
        using var sf = new StringFormat(StringFormatFlags.NoWrap);
        Assert.True((sf.FormatFlags & StringFormatFlags.NoWrap) != 0);
    }

    [Fact]
    public void Constructor_Copy()
    {
        using var original = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Far,
            Trimming = StringTrimming.EllipsisCharacter
        };
        using var copy = new StringFormat(original);
        Assert.Equal(StringAlignment.Center, copy.Alignment);
        Assert.Equal(StringAlignment.Far, copy.LineAlignment);
        Assert.Equal(StringTrimming.EllipsisCharacter, copy.Trimming);
    }

    // --- Properties ---

    [Fact]
    public void Alignment_GetSet()
    {
        using var sf = new StringFormat();
        sf.Alignment = StringAlignment.Center;
        Assert.Equal(StringAlignment.Center, sf.Alignment);
    }

    [Fact]
    public void LineAlignment_GetSet()
    {
        using var sf = new StringFormat();
        sf.LineAlignment = StringAlignment.Far;
        Assert.Equal(StringAlignment.Far, sf.LineAlignment);
    }

    [Fact]
    public void FormatFlags_GetSet()
    {
        using var sf = new StringFormat();
        sf.FormatFlags = StringFormatFlags.DirectionVertical;
        Assert.Equal(StringFormatFlags.DirectionVertical, sf.FormatFlags);
    }

    [Fact]
    public void Trimming_GetSet()
    {
        using var sf = new StringFormat();
        sf.Trimming = StringTrimming.Word;
        Assert.Equal(StringTrimming.Word, sf.Trimming);
    }

    [Fact]
    public void HotkeyPrefix_GetSet()
    {
        using var sf = new StringFormat();
        sf.HotkeyPrefix = HotkeyPrefix.Show;
        Assert.Equal(HotkeyPrefix.Show, sf.HotkeyPrefix);
    }

    // --- TabStops ---

    [Fact]
    public void SetTabStops_GetTabStops_Roundtrip()
    {
        using var sf = new StringFormat();
        sf.SetTabStops(10f, new float[] { 50, 100, 150 });
        var tabs = sf.GetTabStops(out float firstOffset);
        Assert.Equal(10f, firstOffset);
        Assert.Equal(3, tabs.Length);
        Assert.Equal(50f, tabs[0]);
    }

    [Fact]
    public void GetTabStops_Default_EmptyArray()
    {
        using var sf = new StringFormat();
        var tabs = sf.GetTabStops(out float firstOffset);
        Assert.Equal(0f, firstOffset);
        Assert.Empty(tabs);
    }

    // --- SetMeasurableCharacterRanges ---

    [Fact]
    public void SetMeasurableCharacterRanges_DoesNotThrow()
    {
        using var sf = new StringFormat();
        sf.SetMeasurableCharacterRanges(new CharacterRange[]
        {
            new CharacterRange(0, 5),
            new CharacterRange(5, 3)
        });
    }

    // --- DigitSubstitution ---

    [Fact]
    public void SetDigitSubstitution_StoresValues()
    {
        using var sf = new StringFormat();
        sf.SetDigitSubstitution(1033, StringDigitSubstitute.National);
        Assert.Equal(1033, sf.DigitSubstitutionLanguage);
        Assert.Equal(StringDigitSubstitute.National, sf.DigitSubstitutionMethod);
    }

    // --- Static formats ---

    [Fact]
    public void GenericDefault_IsNotNull()
    {
        var sf = StringFormat.GenericDefault;
        Assert.NotNull(sf);
    }

    [Fact]
    public void GenericTypographic_HasExpectedFlags()
    {
        var sf = StringFormat.GenericTypographic;
        Assert.True((sf.FormatFlags & StringFormatFlags.FitBlackBox) != 0);
    }

    // --- Clone ---

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        using var sf = new StringFormat { Alignment = StringAlignment.Far };
        using var clone = (StringFormat)sf.Clone();
        Assert.Equal(StringAlignment.Far, clone.Alignment);
        clone.Alignment = StringAlignment.Near;
        Assert.Equal(StringAlignment.Far, sf.Alignment);
    }

    // --- ToString ---

    [Fact]
    public void ToString_ContainsFormatFlags()
    {
        using var sf = new StringFormat(StringFormatFlags.NoWrap);
        Assert.Contains("FormatFlags", sf.ToString());
    }

    // --- Dispose ---

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var sf = new StringFormat();
        sf.Dispose();
    }
}
